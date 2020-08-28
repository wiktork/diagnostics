using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Monitoring.EventPipe
{
    public class EventLogsPipeline : EventSourcePipeline<EventLogsPipelineSettings>
    {
        private readonly ILoggerFactory _factory;
        public EventLogsPipeline(DiagnosticsClient client, EventLogsPipelineSettings settings, ILoggerFactory factory) 
            : base(client, settings)
        {
            _factory = factory;
        }

        protected override DiagnosticsEventPipeProcessor CreateProcessor()
        {
            return new DiagnosticsEventPipeProcessor(PipeMode.Logs, loggerFactory: _factory, logsLevel: Settings.LogLevel);
        }
    }
}
