using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
        private object _lock = new object();
        private PipelineState _state = PipelineState.Unstarted;
        private Task _runTask;
        private Task _stopTask;
        private Task _abortTask;

        protected abstract Task OnRun(CancellationToken token);

        protected virtual Task OnAbort() => Task.CompletedTask;

        protected virtual Task OnStop(CancellationToken token) => Task.CompletedTask;

        protected virtual ValueTask OnDispose() => default;

        public Task RunAsync(CancellationToken token)
        {
            lock (_lock)
            {
                ThrowIfDisposed();

                if (_runTask != null)
                {
                    return _runTask;
                }
            }
            //THREADING BUG HERE!!!
            Task runTask = RunAsyncCore(token);
            lock (_lock)
            {
                _runTask = runTask;
            }
            return _runTask;
        }

        private async Task RunAsyncCore(CancellationToken token)
        {
            using (var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(token, _disposeSource.Token))
            {
                try
                {
                    try
                    {
                        TransitionState(PipelineState.Running, true, PipelineState.Unstarted);
                        await OnRun(linkedSource.Token);
                    }
                    finally
                    {
                        TransitionState(PipelineState.Stopped, false, PipelineState.Running, PipelineState.Stopping);
                    }
                }
                catch (OperationCanceledException)
                {
                    await Abort();
                    throw;
                }
            }
        }

        public Task StopAsync(CancellationToken token = default)
        {
            lock (_lock)
            {
                ThrowIfDisposed();
                if (_stopTask != null)
                {
                    return _stopTask;
                }

                Task stopTask = StopAsyncCore(token);
                lock(_lock)
                {
                    _stopTask = stopTask;
                }
                return _stopTask;
            }
        }

        private async Task StopAsyncCore(CancellationToken token)
        {
            using (var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(token, _disposeSource.Token))
            {
                try
                {
                    try
                    {
                        TransitionState(PipelineState.Stopping, true, PipelineState.Running);
                        await OnStop(linkedSource.Token);
                    }
                    finally
                    {
                        TransitionState(PipelineState.Stopped, false, PipelineState.Stopping);
                    }
                }
                catch (OperationCanceledException)
                {
                    await Abort();
                    throw;
                }
            }
        }

        private async Task Abort()
        {
            Task abortTask = null;
            lock (_lock)
            {
                if (_abortTask == null)
                {
                    _abortTask = OnAbort();
                }
                abortTask = _abortTask;
            }
            await abortTask;
        }

        private void TransitionState(PipelineState newState, bool throwOnFailure, params PipelineState[] allowedOldStates)
        {
            lock(_lock)
            {
                if (allowedOldStates.Contains(_state))
                {
                    _state = newState;
                }
                else if (throwOnFailure)
                {
                    throw new PipelineException($"Unable to transition from {_state} to {newState}");
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            lock (_lock)
            {
                if (_state == PipelineState.Disposed)
                {
                    return;
                }
                _state = PipelineState.Disposed;
            }
            _disposeSource.Cancel();


            try
            {
                Task startTask = null;
                Task stopTask = null;

                lock (_lock)
                {
                    startTask = _runTask;
                    stopTask = _stopTask;
                }

                if (startTask != null)
                {
                    await startTask;
                }
                if (stopTask != null)
                {
                    await stopTask;
                }
            }
            catch
            {
            }
            finally
            {
                await OnDispose();
                _disposeSource.Dispose();
            }
        }

        private void ThrowIfDisposed()
        {
            if (_state == PipelineState.Disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}
