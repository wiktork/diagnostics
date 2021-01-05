using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Diagnostics.Monitoring.RestServer
{
    internal sealed class ApiAuthenticationOptions
    {
        public const string ConfigurationKey = "ApiAuthentication";

        public string ApiKeyHash { get; set; }
    }
}
