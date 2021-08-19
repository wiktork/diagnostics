using Microsoft.Diagnostics.Tracing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Diagnostics.Monitoring.EventPipe.Triggers
{
    internal class AspnetTriggerSettings { }

    internal class AspnetTriggerProtoFactory : ITraceEventTriggerFactory<AspnetTriggerSettings>
    {
        public ITraceEventTrigger Create(AspnetTriggerSettings settings)
        {
            return new AspnetTriggerProto();
        }
    }

    internal class AspnetTriggerProto : ITraceEventTrigger
    {
        private readonly Dictionary<string, IEnumerable<string>> _providerMap;

        public AspnetTriggerProto()
        {
            _providerMap = new()
            {
                { "Microsoft-Diagnostics-DiagnosticSource", null }
            };
        }

        public IDictionary<string, IEnumerable<string>> GetProviderEventMap()
        {
            return _providerMap;
        }

        public bool HasSatisfiedCondition(TraceEvent traceEvent)
        {
            return true;
        }
    }
}
