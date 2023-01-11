using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Internal.Common.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Tools.Counters
{
    internal abstract class CounterMonitorSource : IDisposable
    {
        public abstract Task<int> Start();

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected abstract void Dispose(bool disposing);
    }

    internal sealed class DiagnosticClientCounterMonitorSource : CounterMonitorSource
    {
        private DiagnosticsClient _diagnosticsClient;
        private EventPipeSession _session;
        private CancellationToken _token;
        private Func<EventProxy> _callback;
        private int processId _processId;

        public DiagnosticClientCounterMonitorSource(Func<EventProxy> callback, CancellationToken cancellationToken, int processId)
        {
            DiagnosticsClientBuilder builder = new DiagnosticsClientBuilder("dotnet-counters", 10);
            using (DiagnosticsClientHolder holder = await builder.Build(ct, _processId, diagnosticPort, showChildIO: false, printLaunchCommand: false))
        }

        public override Task<int> Start()
        {
        }

        protected override void Dispose(bool disposing)
        {
        }
    }
}
