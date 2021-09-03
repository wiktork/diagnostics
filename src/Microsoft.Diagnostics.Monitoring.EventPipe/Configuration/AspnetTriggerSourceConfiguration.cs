// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using Microsoft.Diagnostics.NETCore.Client;

namespace Microsoft.Diagnostics.Monitoring.EventPipe
{
    [Flags]
    public enum AspnetTriggerActivity
    {
        Start = 0x1,
        Stop = 0x2
    }

    public sealed class AspnetTriggerSourceConfiguration : MonitoringSourceConfiguration
    {
        /// <summary>
        /// Filter string for trigger data. Note that even though each trigger typically uses start OR stop,
        /// collecting just one causes unusual behavior in data collection. In the future, we
        /// can also correlate starts and stops by collecting ActivityId.
        /// </summary>
        /// <remarks>
        /// IMPORTANT! We rely on these transformations to make sure we can access relevant data
        /// by index. The order must match the data extracted in the triggers.
        /// </remarks>
        private const string DiagnosticFilterString =
                "Microsoft.AspNetCore/Microsoft.AspNetCore.Hosting.HttpRequestIn.Start@Activity1Start:-" +
                    "Request.Path" +
                    "\r\n" +
                "Microsoft.AspNetCore/Microsoft.AspNetCore.Hosting.HttpRequestIn.Stop@Activity1Stop:-" +
                    "Request.Path" +
                    ";Response.StatusCode" +
                    ";ActivityDuration=*Activity.Duration.Ticks" +
                    "\r\n";

        public override IList<EventPipeProvider> GetProviders()
        {
            var providers = new List<EventPipeProvider>()
            {
                // Diagnostic source events
                new EventPipeProvider(DiagnosticSourceEventSource,
                        keywords: 0x1 | 0x2,
                        eventLevel: EventLevel.Verbose,
                        arguments: new Dictionary<string,string>
                        {
                            { "FilterAndPayloadSpecs", DiagnosticFilterString }
                        })
            };

            return providers;
        }
    }
}
