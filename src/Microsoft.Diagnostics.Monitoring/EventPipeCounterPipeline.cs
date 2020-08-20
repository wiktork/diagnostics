using Microsoft.Diagnostics.Monitoring.Contracts;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Monitoring
{
    public class EventPipeCounterPipelineSettings
    {
        public int ProcessId { get; set; }
        public EventPipeCounterGroup[] CounterGroups { get; set; }
        public TimeSpan RefreshInterval { get; set; }

        public TimeSpan Duration { get; set; }
    }

    public class EventPipeCounterGroup
    {
        public string ProviderName { get; set; }
        public string[] CounterNames { get; set; }
    }
}
