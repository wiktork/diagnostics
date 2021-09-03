// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Diagnostics.Tracing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Microsoft.Diagnostics.Monitoring.EventPipe.Triggers.Aspnet
{
    internal abstract class AspnetTrigger<TSettings> : IAspnetTraceEventTrigger, ITraceEventTrigger where TSettings : AspnetTriggerSettings
    {
        private const int ActivityStartId = 152;
        private const int ActivityStopId = 153;

        protected AspnetTrigger(TSettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));

            IncludePaths = new HashSet<string>(Settings.IncludePaths ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            ExcludePaths = new HashSet<string>(Settings.ExcludePaths ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
        }

        public IReadOnlyDictionary<string, IReadOnlyCollection<string>> GetProviderEventMap()
        {
            return new Dictionary<string, IReadOnlyCollection<string>>
            {
                {"Microsoft-Diagnostics-DiagnosticSource", new[]{ "Activity1/Start", "Activity1/Stop" } }
            };
        }

        public TSettings Settings { get; }

        //CONSIDER The current design is to trigger in aggregate across all url matches.
        //If we want per path triggering, we can simply setup more triggers.
        //CONSIDER Should we support wildcards or regexes?
        protected HashSet<string> IncludePaths { get; }

        protected HashSet<string> ExcludePaths { get; }

        protected virtual bool ActivityStart(DateTime timestamp, string path) => false;
        protected virtual bool ActivityStop(DateTime timestamp, string path, long durationTicks, int statusCode) => false;

        public bool HasSatisfiedCondition(TraceEvent traceEvent)
        {
            //We deconstruct the TraceEvent data to make it easy to write tests
            DateTime timeStamp = traceEvent.TimeStamp;
            ushort eventId = (ushort)traceEvent.ID;
            int? statusCode = null;
            long? duration = null;

            System.Collections.IList arguments = (System.Collections.IList)traceEvent.PayloadValue(2);
            string path = ExtractByIndex(arguments, 0);

            if ((ushort)traceEvent.ID == ActivityStopId)
            {
                statusCode = int.Parse(ExtractByIndex(arguments, 1));
                duration = long.Parse(ExtractByIndex(arguments, 2));
            }

            return ((IAspnetTraceEventTrigger)this).HasSatisfiedCondition(timeStamp, eventId, path, statusCode, duration);

        }

        bool IAspnetTraceEventTrigger.HasSatisfiedCondition(DateTime timestamp, ushort eventId, string path, int? statusCode, long? duration)
        {
            if (!CheckPathFilter(path))
            {
                //No need to update counts if the path is excluded.
                return false;
            }

            if (eventId == ActivityStartId)
            {
                return ActivityStart(timestamp, path);
            }
            else if (eventId == ActivityStopId)
            {
                return ActivityStop(timestamp, path, duration.Value, statusCode.Value);
            }
            return false;
        }

        private bool CheckPathFilter(string path)
        {
            if (IncludePaths.Count > 0)
            {
                return IncludePaths.Contains(path);
            }
            if (ExcludePaths.Count > 0)
            {
                return !ExcludePaths.Contains(path);
            }
            return true;
        }

        private static string ExtractByIndex(System.Collections.IList arguments, int index)
        {
            IEnumerable<KeyValuePair<string, object>> values = (IEnumerable<KeyValuePair<string, object>>)arguments[index];
            //The data is internally organized as two KeyValuePair entries,
            //The first entry is { Key, "KeyValue"}
            //The second is { Value, "Value"}
            //e.g.
            //{{ Key:"StatusCode", Value:"200" }}
            return (string)values.Last().Value;
        }
    }
}
