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
        private readonly CancellationTokenSource _disposeSource = new CancellationTokenSource();
        private object _lock = new object();
        private bool _isDisposed;
        private Task _runTask;
        private Task _stopTask;
        private Task _abortTask;

        protected abstract Task OnRun(CancellationToken token);

        protected virtual Task OnAbort() => Task.CompletedTask;

        protected virtual Task OnStop(CancellationToken token) => Task.CompletedTask;

        protected virtual ValueTask OnDispose() => default;

        public Task RunAsync(CancellationToken token)
        {
            Task runTask = null;
            lock (_lock)
            {
                ThrowIfDisposed();

                if (_runTask == null)
                {
                    _runTask = RunAsyncCore(token);
                }
                runTask = _runTask;
            }
            return _runTask;
        }

        private async Task RunAsyncCore(CancellationToken token)
        {
            using (var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(token, _disposeSource.Token))
            {
                try
                {
                    await OnRun(linkedSource.Token);
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
            Task stopTask = null;
            lock (_lock)
            {
                ThrowIfDisposed();
                if (_runTask == null)
                {
                    throw new PipelineException("Unable to stop unstarted pipeline");
                }
                if (_stopTask == null)
                {
                    _stopTask = StopAsyncCore(token);
                }
                stopTask = _stopTask;
            }
            return stopTask;
        }

        private async Task StopAsyncCore(CancellationToken token)
        {
            using (var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(token, _disposeSource.Token))
            {
                try
                {
                    await OnStop(linkedSource.Token);
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

        public async ValueTask DisposeAsync()
        {
            lock (_lock)
            {
                if (_isDisposed)
                {
                    return;
                }
                _isDisposed = true;
            }
            _disposeSource.Cancel();


            Task startTask = null;
            Task stopTask = null;
            Task abortTask = null;

            lock (_lock)
            {
                startTask = _runTask;
                stopTask = _stopTask;
                abortTask = _abortTask;
            }

            if (startTask != null)
            {
                try
                {
                    await startTask;
                }
                catch
                {
                }
                
            }
            if (stopTask != null)
            {
                try
                {
                    await stopTask;
                }
                catch
                {
                }
            }
            if (abortTask != null)
            {
                try
                {
                    await abortTask;
                }
                catch
                {
                }
            }

            await OnDispose();
            _disposeSource.Dispose();
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}
