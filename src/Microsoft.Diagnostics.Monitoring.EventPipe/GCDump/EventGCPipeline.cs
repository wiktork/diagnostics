using Graphs;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing.Parsers.Clr;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Monitoring.EventPipe
{
    public class EventGCPipeline : EventSourcePipeline<EventGCPipelineSettings>
    {
        private readonly MemoryGraph _gcGraph;

        public EventGCPipeline(DiagnosticsClient client, EventGCPipelineSettings settings, MemoryGraph gcGraph) : base(client, settings)
        {
            _gcGraph = gcGraph;
        }

        protected override DiagnosticsEventPipeProcessor CreateProcessor()
        {
            return new DiagnosticsEventPipeProcessor(PipeMode.GCDump, gcGraph: _gcGraph);
        }
    }
}
