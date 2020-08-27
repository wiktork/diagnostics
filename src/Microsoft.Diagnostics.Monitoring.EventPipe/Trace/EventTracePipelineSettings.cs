using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Diagnostics.Monitoring.EventPipe
{ 
    public class EventTracePipelineSettings : EventSourcePipelineSettings
    {
        public MonitoringSourceConfiguration Configuration { get; set; }
    }
}
