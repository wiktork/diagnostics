// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tools;
using Microsoft.Diagnostics.Tools.Counters;
using Microsoft.Diagnostics.Tools.Counters.Exporters;
using Xunit;

namespace DotnetCounters.UnitTests
{
    /// <summary>
    /// These test the various internal logic in CounterMonitor
    /// </summary>
    public class CounterMonitorPayloadTests
    {
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
