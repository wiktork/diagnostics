using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Monitoring.EventPipe
{
    public class EventGCPipeline : EventSourcePipeline
    {
        protected override Task OnRun(CancellationToken token)
        {
        }

        protected override Task OnStop(CancellationToken token)
        {
        }
    }
}
