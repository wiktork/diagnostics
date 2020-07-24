using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Monitoring.Contracts
{
    public abstract class Pipeline : IPipeline, IAsyncDisposable
    {
        private enum PipelineState
        {
            Unstarted = 0,
            Running,
            Stopping,
            Stopped,
            Disposed,
        }

        private readonly CancellationTokenSource _disposeSource = new CancellationTokenSource();
        private int _state = (int)PipelineState.Unstarted;

        protected abstract void OnAbort();

        protected abstract Task OnRun(CancellationToken token);

        protected abstract Task OnStop(CancellationToken token);

        protected virtual ValueTask OnDispose() => default;

        public async Task RunAsync(CancellationToken token)
        {
            TransitionState(PipelineState.Running, allowedOldStates: PipelineState.Unstarted);
            using (var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(token, _disposeSource.Token))
            using (var registration = linkedSource.Token.Register(Abort, useSynchronizationContext: false))
            {
                await OnRun(linkedSource.Token);
            }
        }

        public async Task StopAsync(CancellationToken token = default)
        {
            TransitionState(PipelineState.Stopping, PipelineState.Unstarted, PipelineState.Running);
            using (var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(token, _disposeSource.Token))
            using (var registration = linkedSource.Token.Register(Abort, useSynchronizationContext: false))
            {
                try
                {
                    await OnStop(linkedSource.Token);
                }
                finally
                {
                    TransitionState(PipelineState.Stopped, PipelineState.Stopping);
                }
            }
        }

        public void Abort()
        {
            //Should this be async?
            TransitionState(PipelineState.Stopping, PipelineState.Unstarted, PipelineState.Running, PipelineState.Stopping);
            OnAbort();
            TransitionState(PipelineState.Stopped, PipelineState.Unstarted, PipelineState.Running, PipelineState.Stopping);
        }

        private void TransitionState(PipelineState newState, params PipelineState[] allowedOldStates)
        {
            PipelineState? oldState = null;
            bool transitioned = false;

            foreach(PipelineState allowedOldState in allowedOldStates)
            {
                oldState = (PipelineState)Interlocked.CompareExchange(ref _state, value: (int)newState, comparand: (int)allowedOldState);
                if (oldState == allowedOldState)
                {
                    transitioned = true;
                    break;
                }
            }

            if (!transitioned)
            {
                throw new PipelineException($"Unexpected state transition from {oldState.Value} to {newState}");
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref _state, (int)PipelineState.Disposed, (int)PipelineState.Disposed) != (int)PipelineState.Disposed)
            {
                _disposeSource.Cancel();

                //TODO Should we await outstanding operations here, or do we push that responsibility to the OnDispose method?
                try
                {
                    await OnDispose();
                }
                finally
                {
                    _disposeSource.Dispose();
                }
            }
        }
    }
}
