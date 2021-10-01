using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Diagnostics.Monitoring.EventPipe.Counters
{
    internal class CounterTracker
    {
        private class CounterState
        {
            public int IntervalSum { get; set; }

            public float ValueAggregation { get; set; }

            public float IntervalAggregation { get; set; }
        }

        private static readonly Dictionary<string, CounterState> _counters = new();

        public bool AddCounter(string provider, string name, int requestedInterval, int actualInterval)
        {
            if (!_counters.TryGetValue(name, out CounterState state))
            {
                state = new CounterState();
                _counters.Add(name, state);
            }

            //Works well for things like 10 -> 5 interval changes. Works poorly for 30 -> 25 since we don't have  good way
            //of actually firing on 30 intervals. We would only be allowed to scale the counter down by halves from the biggest counter, or upscale
            //the counter by multiples of the lowest counter value.
            //We could just fire a counter every 5 seconds.
            //Or force clients to use multiples of highest values (possibly snapping)
            //Or be aware of all startup counters, metrics, etc and find the lcd of all and adjust.
            //Any new coutners would have to be multiples of that interval.
            //OR Force all counter values to be relatively fixed.
            //OR A True mux. No matter the counter, sample every 5 seconds with 1 session.we broadcast the counters to sesssions as their interval
            //approaches. all data would have to be aggregated at every 5 second interval on a per provider per counter basis.
            return true;
        }
    }
}
