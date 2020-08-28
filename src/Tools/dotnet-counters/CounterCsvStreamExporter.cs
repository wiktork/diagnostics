﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Diagnostics.Monitoring.EventPipe;
using System;
using System.IO;
using System.Text;

namespace Microsoft.Diagnostics.Monitoring
{
    public class CounterCsvStreamExporter : IMetricsLogger
    {
        private StringBuilder builder;
        private int flushLength = 10_000; // Arbitrary length to flush
        private StreamWriter _outputWriter;
        private bool _firstOutput = true;

        public CounterCsvStreamExporter(Stream outputStream)
        {
            _outputWriter = new StreamWriter(outputStream);
        }

        public void PipelineStarted()
        {
            builder = new StringBuilder();
            builder.AppendLine("Timestamp,Provider,Counter Name,Counter Type,Mean/Increment");
        }

        public void CounterPayloadReceived(string providerName, ICounterPayload payload)
        {
            if (_firstOutput)
            {
                builder = new StringBuilder();
                builder.AppendLine("Timestamp,Provider,Counter Name,Counter Type,Mean/Increment");
                _firstOutput = false;
            }

            if (builder.Length > flushLength)
            {
                FlushToStream();
            }
            builder.Append(DateTime.UtcNow.ToString() + ",");
            builder.Append(providerName + ",");
            builder.Append(payload.GetDisplay() + ",");
            builder.Append(payload.GetCounterType() + ",");
            builder.Append(payload.GetValue() + "\n");
        }

        public void PipelineStopped()
        {
            FlushToStream();
        }

        private void FlushToStream()
        {
            if (builder != null)
            {
                _outputWriter.Write(builder.ToString());
                _outputWriter.Flush();
                builder.Clear();
            }
        }

        public void LogMetrics(Metric metric)
        {
            CounterPayloadReceived(metric.Namespace, metric);
        }

        public void Dispose()
        {
            FlushToStream();
            _outputWriter.Dispose();
        }
    }
}
