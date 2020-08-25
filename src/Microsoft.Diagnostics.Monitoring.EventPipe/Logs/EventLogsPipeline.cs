using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Monitoring.EventPipe
{
    public class EventLogsPipeline : EventSourcePipeline
    {
        private readonly DiagnosticsClient _client;
        private readonly EventLogsPipelineSettings _settings;
        private readonly DiagnosticsEventPipeProcessor _pipeProcessor;

        public EventLogsPipeline(DiagnosticsClient client, EventLogsPipelineSettings settings, ILoggerFactory factory)
        {
            _client = client;
            _settings = settings;
            _pipeProcessor = new DiagnosticsEventPipeProcessor(PipeMode.Logs, loggerFactory: factory, logsLevel: settings.LogLevel);
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
