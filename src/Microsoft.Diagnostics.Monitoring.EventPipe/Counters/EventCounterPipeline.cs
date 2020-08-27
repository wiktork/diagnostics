using Graphs;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsTCPIP;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
            CounterFilter filter;
            if (settings.CounterGroups.Length > 0)
            {
                filter = new CounterFilter();
                foreach (var counterGroup in settings.CounterGroups)
                {
                    filter.AddFilter(counterGroup.ProviderName, counterGroup.CounterNames);
                }
            }
            else
            {
                filter = CounterFilter.AllCounters;
            }


            _pipeProcessor = new DiagnosticsEventPipeProcessor(PipeMode.Metrics, metricLoggers: metricsLogger, metricIntervalSeconds: (int)settings.RefreshInterval.TotalSeconds,
                metricFilter: filter);

            _settings = settings;
            _diagnosticsClient = client;
        }
         
        protected override Task OnRun(CancellationToken token)
        {
            return _pipeProcessor.Process(_diagnosticsClient, 0, _settings.Duration, token);
        }

        protected override Task OnStop(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        protected override async ValueTask OnDispose()
        {
            await _pipeProcessor.DisposeAsync();
        }

    }
}
