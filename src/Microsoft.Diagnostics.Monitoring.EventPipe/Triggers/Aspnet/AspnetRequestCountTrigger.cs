// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Diagnostics.Tracing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Diagnostics.Monitoring.EventPipe.Triggers.Aspnet
{
    internal class AspnetRequestCountTrigger : AspnetTrigger<AspnetRequestCountTriggerSettings>
    {
        private SlidingWindow _window;

        public AspnetRequestCountTrigger(AspnetRequestCountTriggerSettings settings) : base(settings)
        {
            _window = new SlidingWindow(settings.SlidingWindowDuration);
        }

        protected override bool ActivityStart(DateTime timestamp, string path)
        {
            _window.AddDataPoint(timestamp);
            if (_window.Count >= Settings.RequestCount)
            {
                //Reset trigger?
                _window.Clear();
                return true;
            }

            return base.ActivityStart(timestamp, path);
        }
    }
}
