using Graphs;
using Microsoft.Diagnostics.Monitoring.Contracts;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Monitoring.EventPipe
{
    public abstract class EventSourcePipeline<T> : Pipeline where T : EventSourcePipelineSettings
    {
        private readonly Lazy<DiagnosticsEventPipeProcessor> _processor;
        public DiagnosticsClient Client { get; }
        public T Settings { get; }

        protected EventSourcePipeline(DiagnosticsClient client, T settings)
        {
            _processor = new Lazy<DiagnosticsEventPipeProcessor>(CreateProcessor);
            Settings = settings;
            Client = client;
        }

        protected abstract DiagnosticsEventPipeProcessor CreateProcessor();

        protected override Task OnRun(CancellationToken token)
        {
            return _processor.Value.Process(Client, Settings.ProcessId, Settings.Duration, token);
        }

        protected override ValueTask OnDispose()
        {
            return _processor.Value.DisposeAsync();
        }

        protected override Task OnStop(CancellationToken token)
        {
            Task stoppingTask = Task.Run(() => _processor.Value.StopProcessing(), token);

            var taskCompletionSource = new TaskCompletionSource<bool>();

            var src = new TaskCompletionSource<T>();
            token.Register(() => src.SetCanceled());
            return Task.WhenAny(stoppingTask, src.Task).Unwrap();
        }
    }
}
