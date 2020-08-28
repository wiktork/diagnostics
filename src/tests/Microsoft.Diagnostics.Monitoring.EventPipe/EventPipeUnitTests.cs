// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Diagnostics.NETCore.Client;
using Microsoft.Diagnostics.NETCore.Client.UnitTests;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions;

namespace Microsoft.Diagnostics.Monitoring.EventPipe.UnitTests
{
    public class EventPipeUnitTests
    {
        private readonly ITestOutputHelper _output;

        public EventPipeUnitTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private sealed class TestMetricsLogger : IMetricsLogger
        {
            private readonly ITestOutputHelper _output;
            private Dictionary<string, Metric> _metrics = new Dictionary<string, Metric>();

            public TestMetricsLogger(ITestOutputHelper output)
            {
                _output = output;
            }

            public void Dispose()
            {
            }

            public IEnumerable<Metric> Metrics => _metrics.Values;

            public void LogMetrics(Metric metric)
            {
                _metrics[string.Concat(metric.Namespace, "_", metric.Name)] = metric;
            }
        }

        [SkippableFact]
        public async Task TestCounterEventPipeline()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                throw new SkipTestException("Unstable test on OSX");
            }

            var logger = new TestMetricsLogger(_output);
            var expectedCounters = new[] { "cpu-usage", "working-set" };
            string expectedProvider = "System.Runtime";

            await using (var testExecution = StartTraceeProcess("CounterRemoteTest"))
            {
                //TestRunner should account for start delay to make sure that the diagnostic pipe is available.

                var client = new DiagnosticsClient(testExecution.TestRunner.Pid);

                EventCounterPipeline pipeline = new EventCounterPipeline(client, new EventPipeCounterPipelineSettings
                {
                    Duration = TimeSpan.FromSeconds(10),
                    CounterGroups = new[]
                    {
                        new EventPipeCounterGroup
                        {
                            ProviderName = expectedProvider,
                            CounterNames = expectedCounters
                        }
                    },
                    ProcessId = testExecution.TestRunner.Pid,
                    RefreshInterval = TimeSpan.FromSeconds(1)
                }, new[] { logger });

                Task pipelineTask = pipeline.RunAsync(CancellationToken.None);

                //Add a small delay to make sure diagnostic processor had a chance to initialize
                await Task.Delay(1000);
                //Send signal to proceed with event collection
                testExecution.Start();

                try
                {
                    await pipelineTask;
                }
                finally
                {
                    await pipeline.DisposeAsync();
                }
            }

            Assert.True(logger.Metrics.Any());

            var actualMetrics = logger.Metrics.Select(m => m.Name).OrderBy(m => m);

            Assert.Equal(expectedCounters, actualMetrics);
            Assert.True(logger.Metrics.All(m => string.Equals(m.Namespace, expectedProvider)));
        }

        private static void Validate(IDictionary<string, JsonElement> values, params (string key, object value)[] expectedValues)
        {
            Assert.NotNull(values);
            foreach(var expectedValue in expectedValues)
            {
                Assert.True(values.TryGetValue(expectedValue.key, out JsonElement value));
                //TODO For now this will always be a string
                Assert.Equal(expectedValue.value, value.GetString());
            }
        }

        private RemoteTestExecution StartTraceeProcess(string loggerCategory)
        {
            return RemoteTestExecution.StartProcess(CommonHelper.GetTraceePath("EventPipeTracee") + " " + loggerCategory, _output);
        }

        private sealed class LoggerTestResult
        {
            public string Category { get; set; }
            public string LogLevel { get; set; }
            public string EventId { get; set; }
            public string Message { get; set; }
            public IDictionary<string, JsonElement> Arguments { get; set; }
            public IDictionary<string, JsonElement> Scopes { get; set; }
        }
    }
}
