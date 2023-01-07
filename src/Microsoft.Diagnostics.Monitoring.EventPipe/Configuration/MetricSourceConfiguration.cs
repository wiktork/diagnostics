// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing.Parsers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Microsoft.Diagnostics.Monitoring.EventPipe
{
    public sealed class MetricEventPipeProvider
    {
        public string Provider { get; set; }

        public float IntervalSeconds { get; set; }
    }

    public sealed class MetricSourceConfiguration : MonitoringSourceConfiguration
    {
        public static readonly string[] DefaultProviders = new[] { SystemRuntimeEventSourceName, MicrosoftAspNetCoreHostingEventSourceName, GrpcAspNetCoreServer };
        private readonly IList<EventPipeProvider> _eventPipeProviders;

        public string SessionId { get; private set; }

        public MetricSourceConfiguration(float metricIntervalSeconds, IEnumerable<string> customProviderNames) : this(
            metricIntervalSeconds, customProviderNames?.Any() == true ? CreateProviders(metricIntervalSeconds, customProviderNames) :
            CreateProviders(metricIntervalSeconds, DefaultProviders))
        {
        }

        private static IEnumerable<MetricEventPipeProvider> CreateProviders(float metricIntervalSeconds, IEnumerable<string> customProviderNames) =>
            customProviderNames.Select(provider => new MetricEventPipeProvider
            {
                Provider = provider,
                IntervalSeconds = metricIntervalSeconds
            });

        public MetricSourceConfiguration(float defaultIntervalSeconds, IEnumerable<MetricEventPipeProvider> customProviderNames)
        {
            RequestRundown = false;
            if (customProviderNames == null)
            {
                throw new ArgumentNullException(nameof(customProviderNames));
            }

            _eventPipeProviders = customProviderNames.Select((MetricEventPipeProvider provider) => new EventPipeProvider(provider.Provider,
               EventLevel.Informational,
               (long)ClrTraceEventParser.Keywords.None,
               new Dictionary<string, string>()
               {
                    { "EventCounterIntervalSec", provider.IntervalSeconds.ToString(CultureInfo.InvariantCulture) }
               })).ToList();
        }

        public MetricSourceConfiguration(float metricIntervalSeconds, IEnumerable<string> customProviderNames, int maxHistograms, int maxTimeSeries) : this(metricIntervalSeconds, customProviderNames)
        {
            const long TimeSeriesValues = 0x2;
            StringBuilder metrics = new StringBuilder();
            foreach (string provider in customProviderNames)
            {
                if (metrics.Length != 0)
                {
                    metrics.Append(",");
                }

                metrics.Append(provider);
            }

            SessionId = Guid.NewGuid().ToString();

            EventPipeProvider metricsEventSourceProvider =
                new EventPipeProvider("System.Diagnostics.Metrics", EventLevel.Informational, TimeSeriesValues,
                    new Dictionary<string, string>()
                    {
                        { "SessionId", SessionId },
                        { "Metrics", metrics.ToString() },
                        { "RefreshInterval", metricIntervalSeconds.ToString() },
                        { "MaxTimeSeries", maxTimeSeries.ToString() },
                        { "MaxHistograms", maxHistograms.ToString() }
                    }
                );

            _eventPipeProviders = _eventPipeProviders.Append(metricsEventSourceProvider).ToArray();
        }

        public override IList<EventPipeProvider> GetProviders() => _eventPipeProviders;
    }
}
