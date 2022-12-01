using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Monitoring.EventPipe;
using Microsoft.Diagnostics.Tools.Counters.Exporters;

namespace Microsoft.Diagnostics.Tools.Counters
{
    internal abstract class CounterRendererAdapter : ICountersLogger, ICounterRenderer
    {
        public abstract void CounterPayloadReceived(CounterPayload payload, bool paused);

        public abstract void CounterStopped(CounterPayload payload);

        public abstract void EventPipeSourceConnected();

        public abstract void Initialize();

        public void Log(List<ICounterPayload> counter)
        {
        }

        public Task PipelineStarted()
        {
            Initialize();
            return Task.CompletedTask;
        }

        public Task PipelineStopped()
        {
            Stop();
            return Task.CompletedTask;
        }

        public void SetErrorText(string errorText)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void ToggleStatus(bool paused)
        {
            throw new NotImplementedException();
        }
    }
}
