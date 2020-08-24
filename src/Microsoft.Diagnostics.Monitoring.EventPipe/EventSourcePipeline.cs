using Graphs;
using Microsoft.Diagnostics.Monitoring.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Monitoring.EventPipe
{
    public abstract class EventSourcePipeline : Pipeline
    {
        private Lazy<DiagnosticsEventPipeProcessor> _processor;

        public EventSourcePipeline()
        {
            _processor = new Lazy<DiagnosticsEventPipeProcessor>(CreateProcessor);
        }

        protected abstract DiagnosticsEventPipeProcessor CreateProcessor();

        protected override Task OnRun(CancellationToken token)
        {
            _processor.Value.Process()
        }
    }
}
