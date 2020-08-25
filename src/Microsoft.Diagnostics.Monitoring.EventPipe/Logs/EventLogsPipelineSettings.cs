using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Diagnostics.Monitoring.EventPipe
{
    public class EventLogsPipelineSettings : EventSourcePipelineSettings
    {
        public LogLevel LogLevel { get; set; }
    }
}
