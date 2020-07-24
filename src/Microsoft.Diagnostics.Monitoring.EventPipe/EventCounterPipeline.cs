using Graphs;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsTCPIP;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Monitoring.EventPipe
{
    public sealed class EventCounterPipeline : EventSourcePipeline
    {
        private DiagnosticsEventPipeProcessor _pipeProcessor;
        private EventPipeCounterPipelineSettings _settings;
        private DiagnosticsClient _diagnosticsClient;

        public EventCounterPipeline(DiagnosticsClient client,
            EventPipeCounterPipelineSettings settings,
            IEnumerable<IMetricsLogger> metricsLogger)
        {
            _pipeProcessor = new DiagnosticsEventPipeProcessor(PipeMode.Metrics, metricLoggers: metricsLogger, metricIntervalSeconds: (int)settings.RefreshInterval.TotalSeconds);
            _settings = settings;
            _diagnosticsClient = client;
        }
         
        protected override Task OnRun(CancellationToken token)
        {
            //TODO Fixup duration; should not always be infinite
            return _pipeProcessor.Process(_diagnosticsClient, Timeout.InfiniteTimeSpan, token);
        }

        protected override Task OnStop(CancellationToken token)
        {
            //TODO Stop apis on EventPipeEventSource.
            return Task.CompletedTask;
           
        }

        protected override async ValueTask OnDispose()
        {
            await _pipeProcessor.DisposeAsync();
        }

    }
}
