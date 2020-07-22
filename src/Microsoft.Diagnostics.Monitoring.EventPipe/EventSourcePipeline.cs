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
    public class EventSourcePipeline : Pipeline
    {
        private readonly MemoryGraph _gcGraph;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IEnumerable<IMetricsLogger> _metricLoggers;
        private readonly PipeMode _mode;
        private readonly int _metricIntervalSeconds;
        private readonly LogLevel _logsLevel;

        public EventSourcePipeline(
                    PipeMode mode,
                    ILoggerFactory loggerFactory = null,
                    IEnumerable<IMetricsLogger> metricLoggers = null,
                    int metricIntervalSeconds = 10,
                    MemoryGraph gcGraph = null,
                    LogLevel logsLevel = LogLevel.Debug)
        {
            _metricLoggers = metricLoggers ?? Enumerable.Empty<IMetricsLogger>();
            _mode = mode;
            _loggerFactory = loggerFactory;
            _gcGraph = gcGraph;
            _metricIntervalSeconds = metricIntervalSeconds;
            _logsLevel = logsLevel;
        }

        protected override void OnAbort()
        {
        }

        protected override Task OnRun(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        protected override Task OnStop(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        protected override ValueTask OnDispose()
        {

            return base.OnDispose();
        }
    }
}
