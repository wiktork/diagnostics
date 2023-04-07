// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tools.Counters.Exporters;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftAntimalwareAMFilter;
using Microsoft.Internal.Common.Utils;

namespace Microsoft.Diagnostics.Tools.Counters
{
    internal abstract class CounterMonitorSourceFactory
    {
        public abstract CounterMonitorSource Create(Action<TraceEvent> callback, ICounterRenderer renderer, int processId, string diagnosticPort, bool resumeRuntime, CancellationToken cancellationToken);
    }

    internal abstract class CounterMonitorSource : IDisposable
    {
        public abstract Task<int> Start(EventPipeProvider[] providers);

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected abstract void Dispose(bool disposing);

        public abstract void Stop();
    }

    internal sealed class DiagnosticClientCounterMonitorSourceFactory : CounterMonitorSourceFactory
    {
        public override CounterMonitorSource Create(Action<TraceEvent> callback, ICounterRenderer renderer, int processId, string diagnosticPort, bool resumeRuntime, CancellationToken cancellationToken)
        {
            return new DiagnosticClientCounterMonitorSource(callback, renderer, processId, diagnosticPort, resumeRuntime, cancellationToken);
        }
    }

    internal sealed class DiagnosticClientCounterMonitorSource : CounterMonitorSource
    {
        private DiagnosticsClient _diagnosticsClient;
        private ICounterRenderer _renderer;
        private EventPipeSession _session;
        private CancellationToken _token;
        private Action<TraceEvent> _callback;
        private string _diagnosticPort;
        private int _processId;
        private bool _resumeRuntime;

        public DiagnosticClientCounterMonitorSource(Action<TraceEvent> callback,
            ICounterRenderer renderer,
            int processId, string diagnosticPort, bool resumeRuntime,
            CancellationToken cancellationToken)
        {
            _callback = callback;
            _token = cancellationToken;
            _processId = processId;
            _diagnosticPort = diagnosticPort;
            _resumeRuntime = resumeRuntime;
            _renderer = renderer;
        }

        public override async Task<int> Start(EventPipeProvider[] providers)
        {
            DiagnosticsClientBuilder builder = new("dotnet-counters", 10);
            using (DiagnosticsClientHolder holder = await builder.Build(_token, _processId, _diagnosticPort, showChildIO: false, printLaunchCommand: false).ConfigureAwait(true))
            {
                _diagnosticsClient = holder.Client;
                _session = _diagnosticsClient.StartEventPipeSession(providers, false, 10);
                if (_resumeRuntime) {
                    try {
                        _diagnosticsClient.ResumeRuntime();
                    }
                    catch (UnsupportedCommandException) {
                        // Noop if the command is unknown since the target process is most likely a 3.1 app.
                    }
                }
                EventPipeEventSource source = new(_session.EventStream);
                source.Dynamic.All += _callback;
                _renderer.EventPipeSourceConnected();
                source.Process();
            }

            return 0;
        }

        public override void Stop()
        {
            _session?.Stop();
        }

        protected override void Dispose(bool disposing)
        {
        }
    }
}
