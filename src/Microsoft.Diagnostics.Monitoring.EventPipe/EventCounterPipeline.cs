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

        public EventCounterPipeline(DiagnosticsClient client,
            EventPipeCounterPipelineSettings settings,
            IEnumerable<IMetricsLogger> metricsLogger)
        {
            _pipeProcessor = new DiagnosticsEventPipeProcessor(PipeMode.Metrics, metricLoggers: metricsLogger, metricIntervalSeconds: (int)settings.RefreshInterval.TotalSeconds);
            _settings = settings;
        }

        protected override Task OnRun(CancellationToken token)
        {
            return _pipeProcessor.Process(_settings.ProcessId, Timeout.InfiniteTimeSpan, token);
        }

        protected override async ValueTask OnDispose()
        {
            await _pipeProcessor.DisposeAsync();
        }

    }
}
