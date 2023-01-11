using Microsoft.Diagnostics.Tracing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Diagnostics.Tools.Counters
{
    internal abstract class EventProxy
    {
        public abstract DateTime TimeStamp { get; }
        public abstract object PayloadValue(int index);

        public T PayloadValue<T>(int index) => (T)PayloadValue(index);

        public abstract string ProviderName { get; }

        public abstract string EventName { get; }
    }

    internal sealed class TraceEventProxy : EventProxy
    {
        private readonly TraceEvent _traceEvent;

        public TraceEventProxy(TraceEvent traceEvent)
        {
            _traceEvent = traceEvent ?? throw new ArgumentNullException(nameof(traceEvent));
        }

        public override object PayloadValue(int index) => _traceEvent.PayloadValue(index);

        public override DateTime TimeStamp => _traceEvent.TimeStamp;

        public override string ProviderName => _traceEvent.ProviderName;

        public override string EventName => _traceEvent.EventName;
    }
}
