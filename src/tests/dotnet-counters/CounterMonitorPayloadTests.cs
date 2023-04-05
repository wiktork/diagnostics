// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.IO;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tools;
using Microsoft.Diagnostics.Tools.Counters;
using Microsoft.Diagnostics.Tools.Counters.Exporters;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsTCPIP;
using Xunit;
using Xunit.Abstractions;

namespace DotnetCounters.UnitTests
{
    /// <summary>
    /// These test the various internal logic in CounterMonitor
    /// </summary>
    public class CounterMonitorPayloadTests
    {


        [Fact]
        public async Task TestBasicEvent()
        {
            StringWriter writer = new StringWriter();

            var monitor = new CounterMonitor(new TestCounterMonitorSourceFactory());

            string path = Path.ChangeExtension(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()), "json");

            var result = await monitor.Collect(CancellationToken.None, new List<string> {"System.Runtime"}, null, new TestConsole(), 1, 5, CountersExportFormat.json, path, null, null, false, 1, 1, TimeSpan.FromSeconds(5));

            return;

        }

        private sealed class TestConsole : IConsole
        {
            private readonly TestStandardStreamWriter _outWriter;
            private readonly TestStandardStreamWriter _errorWriter;

            private sealed class TestStandardStreamWriter : IStandardStreamWriter
            {
                private StringWriter _writer = new();
                public void Write(string value) => _writer.Write(value);
                public void WriteLine(string value) => _writer.WriteLine(value);
            }

            public TestConsole()
            {
                _outWriter = new TestStandardStreamWriter();
                _errorWriter = new TestStandardStreamWriter();
            }

            public IStandardStreamWriter Out => _outWriter;

            public bool IsOutputRedirected => true;

            public IStandardStreamWriter Error => _errorWriter;

            public bool IsErrorRedirected => true;

            public bool IsInputRedirected => false;
        }

        private sealed class TestCounterMonitorSourceFactory : CounterMonitorSourceFactory
        {
            public override CounterMonitorSource Create(Action<EventProxy> callback, ICounterRenderer renderer, int processId, string diagnosticPort, bool resumeRuntime, CancellationToken cancellationToken)
            {
                return new TestCounterMonitorSource(callback);
            }
        }

        private sealed class TestCounterMonitorSource : CounterMonitorSource
        {
            private Action<EventProxy> _callback;

            public TestCounterMonitorSource(Action<EventProxy> callback)
            {
                _callback = callback;
            }

            public override Task<int> Start(EventPipeProvider[] providers)
            {
                return Task.FromResult(0);
            }
            protected override void Dispose(bool disposing)
            {
            }
            public override void Stop()
            {
            }
        }

        private sealed class TestEventProxy : EventProxy
        {
            private List<object> _payload;

            public TestEventProxy(string providerName, string eventName, IList<object> payload, DateTime? timestamp)
            {
                ProviderName = providerName;
                EventName = eventName;
                _payload = payload.ToList();
                TimeStamp = timestamp ?? DateTime.UtcNow;
            }

            public override DateTime TimeStamp { get; }

            public override string ProviderName { get; }

            public override string EventName { get; }

            public override object PayloadValue(int index) => _payload[index];
        }
    }
}
