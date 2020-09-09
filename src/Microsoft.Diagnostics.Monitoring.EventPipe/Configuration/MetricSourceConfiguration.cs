﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing.Parsers;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Microsoft.Diagnostics.Monitoring.EventPipe
{
    public sealed class MetricSourceConfiguration : MonitoringSourceConfiguration
    {
        private readonly IEnumerable<string> _customProviderNames;

        public MetricSourceConfiguration(int metricIntervalSeconds, IEnumerable<string> customProviderNames)
        {
            MetricIntervalSeconds = metricIntervalSeconds.ToString(CultureInfo.InvariantCulture);
            _customProviderNames = customProviderNames;
        }

        private string MetricIntervalSeconds { get; }

        public override IList<EventPipeProvider> GetProviders()
        {
            IEnumerable<string> providers = null;
            if (_customProviderNames.Any())
            {
                providers = _customProviderNames;
            }
            else
            {
                providers = new[] { SystemRuntimeEventSourceName, MicrosoftAspNetCoreHostingEventSourceName, GrpcAspNetCoreServer };
            }

            return providers.Select((string provider) => new EventPipeProvider(provider,
               EventLevel.Informational,
               (long)ClrTraceEventParser.Keywords.None,
                   new Dictionary<string, string>() {
                        { "EventCounterIntervalSec", MetricIntervalSeconds } 
                   }
               )).ToList();
        }
    }
}
