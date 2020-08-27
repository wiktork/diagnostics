using Graphs;
using Microsoft.Diagnostics.NETCore.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Monitoring.EventPipe
{
    public class EventGCPipeline : EventSourcePipeline
    {
        private DiagnosticsEventPipeProcessor _pipeProcessor;
        private EventGCPipelineSettings _settings;
        private DiagnosticsClient _client;

        public EventGCPipeline(DiagnosticsClient client, EventGCPipelineSettings settings, MemoryGraph gcGraph)
        {
            _pipeProcessor = new DiagnosticsEventPipeProcessor(PipeMode.GCDump, gcGraph: gcGraph);
            _client = client;
            _settings = settings;
        }

        protected override Task OnRun(CancellationToken token)
        {
            return _pipeProcessor.Process(_client, _settings.ProcessId, _settings.Duration, token);
        }

        protected override Task OnStop(CancellationToken token)
        {
            _pipeProcessor.StopProcessing();
            return Task.CompletedTask;
        }

        protected override ValueTask OnDispose()
        {
            return _pipeProcessor.DisposeAsync();
        }
    }
}
