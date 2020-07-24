using Graphs;
using Microsoft.Diagnostics.Monitoring.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Diagnostics.Monitoring.EventPipe
{
    public abstract class EventSourcePipeline : Pipeline
    {
        public EventSourcePipeline()
        {
        }

        protected override void OnAbort()
        {
        }

        protected override Task OnRun(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        protected override Task OnStop(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        protected override ValueTask OnDispose()
        {
            return base.OnDispose();
        }
    }
}
