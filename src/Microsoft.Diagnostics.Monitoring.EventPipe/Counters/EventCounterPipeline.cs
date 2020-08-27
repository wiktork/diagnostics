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
    public sealed class EventCounterPipeline : EventSourcePipeline<EventPipeCounterPipelineSettings>
    {
        private IEnumerable<IMetricsLogger> _metricsLogger;
        private CounterFilter _filter;

        public EventCounterPipeline(DiagnosticsClient client,
            EventPipeCounterPipelineSettings settings,
            IEnumerable<IMetricsLogger> metricsLogger) : base(client, settings)
        {
            if (settings.CounterGroups.Length > 0)
            {
                _filter = new CounterFilter();
                foreach (var counterGroup in settings.CounterGroups)
                {
                    _filter.AddFilter(counterGroup.ProviderName, counterGroup.CounterNames);
                }
            }
            else
            {
                _filter = CounterFilter.AllCounters;
            }

            _metricsLogger = metricsLogger;
        }

        protected override DiagnosticsEventPipeProcessor CreateProcessor()
        {
            return new DiagnosticsEventPipeProcessor(PipeMode.Metrics, metricLoggers: _metricsLogger, metricIntervalSeconds: (int)Settings.RefreshInterval.TotalSeconds,
                metricFilter: _filter);
        }
    }
}
