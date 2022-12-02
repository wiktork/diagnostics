using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Monitoring.EventPipe;
using Microsoft.Diagnostics.Tools.Counters.Exporters;

namespace Microsoft.Diagnostics.Tools.Counters
{
    public abstract class CounterRendererAdapter : ICountersLogger, ICounterRenderer
    {
        public abstract void CounterPayloadReceived(CounterPayload payload, bool paused);

        public abstract void CounterStopped(CounterPayload payload);

        public abstract void EventPipeSourceConnected();

        public abstract void Initialize();

        public void Log(List<ICounterPayload> counter)
        {
            ICounterPayload payload = counter[0];
            //Check types
            if (payload is ErrorPayload error)
            {
                SetErrorText(error.ErrorMessage);
            }
            /*else if (payload.EventType == EventType.EndGauge)
            {
                // Probably occurs when the instrument callback throws an exception (possibly coupled with an error event)
                CounterStopped(payload):
            }
            */
            else
            {
                CounterPayloadReceived((CounterPayload)payload, PausedCommandSet);
            }
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

        public abstract void SetErrorText(string errorText);

        public abstract void Stop();

        public abstract void ToggleStatus(bool paused);

        public Task OnEventSourceAvailable()
        {
            EventPipeSourceConnected();
            return Task.CompletedTask;
        }

        public bool PausedCommandSet { get; set; }
    }
}
