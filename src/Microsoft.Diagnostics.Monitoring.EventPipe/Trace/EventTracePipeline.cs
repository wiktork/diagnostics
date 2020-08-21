using Microsoft.Diagnostics.Monitoring.Contracts;
using Microsoft.Diagnostics.NETCore.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Monitoring.EventPipe
{
    public class EventTracePipeline : EventSourcePipeline
    {
        private readonly DiagnosticsClient _client;
        private readonly EventTracePipelineSettings _settings;
        private readonly DiagnosticsEventPipeProcessor _pipeProcessor;

        public EventTracePipeline(DiagnosticsClient client, EventTracePipelineSettings settings, Func<Stream, CancellationToken, Task> streamAvailable)
        {
            _client = client;
            _settings = settings;
            _pipeProcessor = new DiagnosticsEventPipeProcessor(PipeMode.Nettrace, configuration: settings.Configuration, streamAvailable: streamAvailable);
        }
        
        protected override Task OnRun(CancellationToken token)
        {
            return _pipeProcessor.Process(_client, _settings.ProcessId, _settings.Duration, token);
        }

        protected override Task OnStop(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        protected override ValueTask OnDispose()
        {
            return _pipeProcessor.DisposeAsync();
        }
    }
}
