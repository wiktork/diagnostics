using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Diagnostics.Monitoring.EventPipe.Triggers.Aspnet
{
    internal interface IAspnetTraceEventTrigger
    {
        bool HasSatisfiedCondition(DateTime timestamp, ushort eventId, string path, int? statusCode, long? duration);
    }
}
