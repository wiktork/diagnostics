using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Diagnostics.Monitoring.EventPipe
{ 
    public class EventTracePipelineSettings
    {
        public int ProcessId { get; set; }

        public TimeSpan Duration { get; set; }

        public MonitoringSourceConfiguration Configuration { get; set; }
    }
}
