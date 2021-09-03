// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Diagnostics.Monitoring.EventPipe.Triggers.Aspnet
{
    internal class AspnetRequestCountTriggerFactory : ITraceEventTriggerFactory<AspnetRequestCountTriggerSettings>
    {
        public ITraceEventTrigger Create(AspnetRequestCountTriggerSettings settings) => new AspnetRequestCountTrigger(settings);
    }

    internal class AspnetRequestDurationTriggerFactory : ITraceEventTriggerFactory<AspnetRequestDurationTriggerSettings>
    {
        public ITraceEventTrigger Create(AspnetRequestDurationTriggerSettings settings) => new AspnetRequestDurationTrigger(settings);
    }

    internal class AspnetRequestStatusTriggerFactory : ITraceEventTriggerFactory<AspnetRequestStatusTriggerSettings>
    {
        public ITraceEventTrigger Create(AspnetRequestStatusTriggerSettings settings) => new AspnetRequestStatusTrigger(settings);
    }
}
