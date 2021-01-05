// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Diagnostics.Monitoring;
using Microsoft.Diagnostics.Monitoring.RestServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Tools.Monitor
{
    internal sealed class DiagnosticsMonitorCommandHandler
    {
        private const string ConfigPrefix = "DotnetMonitor_";
        private const string SettingsFileName = "settings.json";
        private const string ProductFolderName = "dotnet-monitor";

        // Location where shared dotnet-monitor configuration is stored.
        // Windows: "%ProgramData%\dotnet-monitor
        // Other: /etc/dotnet-monitor
        private static readonly string SharedConfigDirectoryPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), ProductFolderName) :
            Path.Combine("/etc", ProductFolderName);

        private static readonly string SharedSettingsPath = Path.Combine(SharedConfigDirectoryPath, SettingsFileName);

        // Location where user's dotnet-monitor configuration is stored.
        // Windows: "%USERPROFILE%\.dotnet-monitor"
        // Other: "%XDG_CONFIG_HOME%/dotnet-monitor" OR "%HOME%/.config/dotnet-monitor"
        private static readonly string UserConfigDirectoryPath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "." + ProductFolderName) :
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ProductFolderName);

        private static readonly string UserSettingsPath = Path.Combine(UserConfigDirectoryPath, SettingsFileName);

        public async Task<int> Start(CancellationToken token, IConsole console, string[] urls, string[] metricUrls, bool metrics, string diagnosticPort)
        {
            //CONSIDER The console logger uses the standard AddConsole, and therefore disregards IConsole.
            using IHost host = CreateHostBuilder(console, urls, metricUrls, metrics, diagnosticPort).Build();
            await host.RunAsync(token);
            return 0;
        }

        public IHostBuilder CreateHostBuilder(IConsole console, string[] urls, string[] metricUrls, bool metrics, string diagnosticPort)
        {
            if (metrics)
            {
                urls = urls.Concat(metricUrls).ToArray();
            }

            return Host.CreateDefaultBuilder()
                .UseContentRoot(AppContext.BaseDirectory) // Use the application root instead of the current directory
                .ConfigureAppConfiguration((IConfigurationBuilder builder) =>
                {
                    //Note these are in precedence order.
                    ConfigureEndpointInfoSource(builder, diagnosticPort);
                    if (metrics)
                    {
                        ConfigureMetricsEndpoint(builder, metricUrls);
                    }

                    builder.AddJsonFile(UserSettingsPath, optional: true, reloadOnChange: true);
                    builder.AddJsonFile(SharedSettingsPath, optional: true, reloadOnChange: true);

                    builder.AddKeyPerFile(SharedConfigDirectoryPath, optional: true);
                    builder.AddEnvironmentVariables(ConfigPrefix);
                })
                .ConfigureServices((HostBuilderContext context, IServiceCollection services) =>
                {
                    //TODO Many of these service additions should be done through extension methods

                    //Add support for Authentication and Authorization.
                    AuthenticationBuilder authBuilder = services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = AuthConstants.ApiKeySchema;
                        options.DefaultChallengeScheme = AuthConstants.ApiKeySchema;
                    })
                    .AddScheme<ApiKeyAuthenticationHandlerOptions, ApiKeyAuthenticationHandler>(AuthConstants.ApiKeySchema, _ => { });

                    var authSchemas = new List<string> { AuthConstants.ApiKeySchema };

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        //On Windows add Negotiate package. This will use NTLM to perform Windows Authentication.
                        authBuilder.AddNegotiate();
                        authSchemas.Add(AuthConstants.NegotiateSchema);
                    }

                    //Apply Authorization Policy for NTLM. Without Authorization, any user with a valid login/password will be authorized. We only
                    //want to authorize the same user that is running dotnet-monitor, at least for now.
                    //Note this policy applies to both Authorization schemas.
                    services.AddAuthorization(authOptions => {
                        authOptions.AddPolicy(AuthConstants.PolicyName, (builder) =>
                        {
                            builder.AddRequirements(new AuthorizedUserRequirement());
                            builder.RequireAuthenticatedUser();
                            builder.AddAuthenticationSchemes(authSchemas.ToArray());
                        });
                    });
                    services.AddSingleton<IAuthorizationHandler, UserAuthorizationHandler>();

                    services.Configure<DiagnosticPortOptions>(context.Configuration.GetSection(DiagnosticPortOptions.ConfigurationKey));
                    services.AddSingleton<IEndpointInfoSource, FilteredEndpointInfoSource>();
                    services.AddHostedService<FilteredEndpointInfoSourceHostedService>();
                    services.AddSingleton<IDiagnosticServices, DiagnosticServices>();
                    services.ConfigureEgress(context.Configuration);
                    if (metrics)
                    {
                        services.ConfigureMetrics(context.Configuration);
                    }
                    services.AddSingleton<ExperimentalToolLogger>();
                    services.ConfigureApiKeyConfiguration(context.Configuration);
                })
                .ConfigureLogging(builder =>
                {
                    // Always allow the experimental tool message to be logged
                    ExperimentalToolLogger.AddLogFilter(builder);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseUrls(urls);
                    webBuilder.UseStartup<Startup>();
                });
        }

        private static void ConfigureMetricsEndpoint(IConfigurationBuilder builder, string[] metricEndpoints)
        {
            builder.AddInMemoryCollection(new Dictionary<string, string>
            {
                {MakeKey(MetricsOptions.ConfigurationKey, nameof(MetricsOptions.Endpoints)), string.Join(';', metricEndpoints)},
                {MakeKey(MetricsOptions.ConfigurationKey, nameof(MetricsOptions.Enabled)), true.ToString()},
                {MakeKey(MetricsOptions.ConfigurationKey, nameof(MetricsOptions.UpdateIntervalSeconds)), 10.ToString()},
                {MakeKey(MetricsOptions.ConfigurationKey, nameof(MetricsOptions.MetricCount)), 3.ToString()}
            });
        }

        private static void ConfigureEndpointInfoSource(IConfigurationBuilder builder, string diagnosticPort)
        {
            DiagnosticPortConnectionMode connectionMode = string.IsNullOrEmpty(diagnosticPort) ? DiagnosticPortConnectionMode.Connect : DiagnosticPortConnectionMode.Listen;
            builder.AddInMemoryCollection(new Dictionary<string, string>
            {
                {MakeKey(DiagnosticPortOptions.ConfigurationKey, nameof(DiagnosticPortOptions.ConnectionMode)), connectionMode.ToString()},
                {MakeKey(DiagnosticPortOptions.ConfigurationKey, nameof(DiagnosticPortOptions.EndpointName)), diagnosticPort}
            });
        }

        private static string MakeKey(string parent, string child)
        {
            return FormattableString.Invariant($"{parent}:{child}");
        }
    }
}
