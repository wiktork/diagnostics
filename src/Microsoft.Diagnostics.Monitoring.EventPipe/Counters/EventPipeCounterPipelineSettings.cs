using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Diagnostics.Monitoring.EventPipe
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
