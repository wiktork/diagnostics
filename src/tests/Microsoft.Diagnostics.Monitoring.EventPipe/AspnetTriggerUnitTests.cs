using Microsoft.Diagnostics.Monitoring.EventPipe.Triggers.Aspnet;
using Microsoft.Diagnostics.Monitoring.EventPipe.Triggers.Pipelines;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.Tracing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Diagnostics.Monitoring.EventPipe.UnitTests
{
    public class AspnetTriggerUnitTests
    {
        private const int ActivityStartId = 152;
        private const int ActivityStopId = 153;
        private readonly ITestOutputHelper _outputHelper;

        public AspnetTriggerUnitTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public void TestAspnetRequestCount()
        {
            AspnetRequestCountTriggerSettings settings = new()
            {
                IncludePaths = new[] { "/" },
                RequestCount = 3,
                SlidingWindowDuration = TimeSpan.FromMinutes(1)
            };

            AspnetRequestCountTriggerFactory factory = new();
            var trigger = (AspnetRequestCountTrigger)factory.Create(settings);

            PayloadGenerator generator = new();

            var s1 = generator.CreateEvent(DateTime.UtcNow);

            // These should not trigger anything because they are not included
            var s2 = generator.CreateEvent(s1.Timestamp + TimeSpan.FromSeconds(30), "/notIncluded");
            var s3 = generator.CreateEvent(s2.Timestamp, "/notIncluded");
            var s4 = generator.CreateEvent(s2.Timestamp, "/notIncluded");

            var s5 = generator.CreateEvent(s2.Timestamp);

            //Pushes the first event out of sliding window
            var s6 = generator.CreateEvent(s2.Timestamp + TimeSpan.FromSeconds(40));

            var s7 = generator.CreateEvent(s6.Timestamp + TimeSpan.FromSeconds(0.5));

            ValidateTriggers(trigger, s1, s2, s3, s4, s5, s6, s7);
        }

        [Fact]
        public void TestAspnetRequestCountExclusions()
        {
            AspnetRequestCountTriggerSettings settings = new()
            {
                ExcludePaths = new[] {"/"},
                RequestCount = 3,
                SlidingWindowDuration = TimeSpan.FromMinutes(1)
            };

            AspnetRequestCountTriggerFactory factory = new();
            var trigger = (AspnetRequestCountTrigger)factory.Create(settings);

            PayloadGenerator generator = new();

            // These should not trigger anything because they are excluded
            var s1 = generator.CreateEvent(DateTime.UtcNow);
            var s2 = generator.CreateEvent(s1.Timestamp + TimeSpan.FromSeconds(1));
            var s3 = generator.CreateEvent(s2.Timestamp);
            var s4 = generator.CreateEvent(s2.Timestamp);

            var s5 = generator.CreateEvent(s2.Timestamp, "/notExcluded");
            var s6 = generator.CreateEvent(s2.Timestamp, "/notExcluded");
            var s7 = generator.CreateEvent(s6.Timestamp + TimeSpan.FromSeconds(10), "/notExcluded");

            ValidateTriggers(trigger, s1, s2, s3, s4, s5, s6, s7);
        }

        [Fact]
        public void TestAspnetDuration()
        {
            AspnetRequestDurationTriggerSettings settings = new()
            {
                RequestCount = 3,
                RequestDuration = TimeSpan.FromSeconds(3),
                SlidingWindowDuration = TimeSpan.FromMinutes(1),
            };

            AspnetRequestDurationTriggerFactory factory = new();
            var trigger = (AspnetRequestDurationTrigger)factory.Create(settings);

            PayloadGenerator generator = new();

            var s1 = generator.CreateEvent(DateTime.UtcNow);
            var e1 = generator.CreateEvent(s1, TimeSpan.FromSeconds(3.1).Ticks);

            var s2 = generator.CreateEvent(s1.Timestamp + TimeSpan.FromSeconds(1));
            var e2 = generator.CreateEvent(s2, TimeSpan.FromSeconds(10).Ticks);

            //does not exceed duration
            var s3 = generator.CreateEvent(s1.Timestamp + TimeSpan.FromSeconds(1));
            var e3 = generator.CreateEvent(s3, TimeSpan.FromSeconds(1).Ticks);

            //pushes the sliding window past all prevoius events
            var s4 = generator.CreateEvent(s1.Timestamp + TimeSpan.FromMinutes(5));
            var e4 = generator.CreateEvent(s4, TimeSpan.FromSeconds(15).Ticks);

            var s5 = generator.CreateEvent(s4.Timestamp + TimeSpan.FromSeconds(1));
            var e5 = generator.CreateEvent(s5, TimeSpan.FromSeconds(10).Ticks);

            var s6 = generator.CreateEvent(s4.Timestamp + TimeSpan.FromSeconds(1));
            var e6 = generator.CreateEvent(s6, TimeSpan.FromSeconds(20).Ticks);

            //Reflects actual ordering
            ValidateTriggers(trigger, s1, s2, s3, e3, e1, e2, s4, s5, s6, e5, e4, e6);
        }

        [Fact]
        public void TestAspnetStatus()
        {
            AspnetRequestStatusTriggerSettings settings = new()
            {
                RequestCount = 3,
                StatusCodeFilter = "520;521;400-500",
                SlidingWindowDuration = TimeSpan.FromMinutes(1),
            };

            AspnetRequestStatusTriggerFactory factory = new();
            var trigger = (AspnetRequestStatusTrigger)factory.Create(settings);

            PayloadGenerator generator = new();

            var s1 = generator.CreateEvent(DateTime.UtcNow);
            var e1 = generator.CreateEvent(s1, TimeSpan.FromSeconds(3.1).Ticks, statusCode: 404);

            var s2 = generator.CreateEvent(s1.Timestamp + TimeSpan.FromSeconds(1));
            var e2 = generator.CreateEvent(s2, TimeSpan.FromSeconds(10).Ticks, statusCode: 420);

            //does not meet status code
            var s3 = generator.CreateEvent(s1.Timestamp + TimeSpan.FromSeconds(1));
            var e3 = generator.CreateEvent(s3, TimeSpan.FromSeconds(1).Ticks);

            //pushes the sliding window past all prevoius events
            var s4 = generator.CreateEvent(s1.Timestamp + TimeSpan.FromMinutes(5));
            var e4 = generator.CreateEvent(s4, TimeSpan.FromSeconds(15).Ticks, statusCode: 520);

            var s5 = generator.CreateEvent(s4.Timestamp + TimeSpan.FromSeconds(1));
            var e5 = generator.CreateEvent(s5, TimeSpan.FromSeconds(10).Ticks, statusCode: 521);

            //does not meet status code
            var s6 = generator.CreateEvent(s4.Timestamp + TimeSpan.FromSeconds(1));
            var e6 = generator.CreateEvent(s5, TimeSpan.FromSeconds(10).Ticks);

            var s7 = generator.CreateEvent(s4.Timestamp + TimeSpan.FromSeconds(1));
            var e7 = generator.CreateEvent(s7, TimeSpan.FromSeconds(20).Ticks, statusCode: 404);

            //Reflects actual ordering
            ValidateTriggers(trigger, s1, s2, s3, e3, e1, e2, s4, s5, s6, s7, e5, e4, e6, e7);
        }

        [Fact(/*Skip = "Only useful for development purposes"*/)]
        public async Task DebugAspnetTrigger()
        {
            var process = Process.GetProcessesByName("iisexpress");
            DiagnosticsClient c = new DiagnosticsClient(process[0].Id);

            var callback = (TraceEvent e) =>
            {
                _outputHelper.WriteLine(e.EventName);
            };

            await using EventPipeTriggerPipeline<AspnetRequestStatusTriggerSettings> pipeline = new(c, new EventPipeTriggerPipelineSettings<AspnetRequestStatusTriggerSettings>
            {
                Configuration = new AspnetTriggerSourceConfiguration(),
                Duration = TimeSpan.FromMinutes(10),
                TriggerFactory = new AspnetRequestStatusTriggerFactory(),
                TriggerSettings = new AspnetRequestStatusTriggerSettings()
                {
                    RequestCount = 5,
                    SlidingWindowDuration = TimeSpan.FromMinutes(1),
                    StatusCodeFilter = "200",
                    IncludePaths = new[] {"/", "/Privacy", "/Missing"}
                }
            }, callback);

            await pipeline.RunAsync(default);
        }

        private static void ValidateTriggers(IAspnetTraceEventTrigger requestTrigger, params SimulatedTraceEvent[] events)
        {
            for (int i = 0; i < events.Length; i++)
            {
                bool shouldSatisfy = i == events.Length - 1;
                bool validTriggerResult = (shouldSatisfy == requestTrigger.HasSatisfiedCondition(
                        events[i].Timestamp,
                        events[i].Id,
                        events[i].Path,
                        events[i].StatusCode,
                        events[i].Duration));

                Assert.True(validTriggerResult, $"Failed at index {i}");
            }
        }

        private sealed class PayloadGenerator
        {
            public SimulatedTraceEvent CreateEvent(DateTime timestamp, string path = "/")
            {
                return new SimulatedTraceEvent { Timestamp = timestamp, Id = ActivityStartId, Path = path };
            }

            public SimulatedTraceEvent CreateEvent(SimulatedTraceEvent previousEvent, long duration, int statusCode = 200, string path = null)
            {
                Assert.NotNull(previousEvent);
                return new SimulatedTraceEvent
                {
                    Timestamp = previousEvent.Timestamp + TimeSpan.FromTicks(duration),
                    Path = path ?? previousEvent.Path,
                    Duration = duration,
                    Id = ActivityStopId,
                    StatusCode = statusCode
                };
            }
        }

        private sealed class SimulatedTraceEvent
        {
            public DateTime Timestamp { get; set; }
            public ushort Id { get; set; }

            public string Path { get; set; }

            public int? StatusCode { get; set; }

            public long? Duration { get; set; }
        }

    }
}
