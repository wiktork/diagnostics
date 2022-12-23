// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Microsoft.Diagnostics.Monitoring.EventPipe
{
    internal class CounterPayload : ICounterPayload
    {
        public CounterPayload(DateTime timestamp,
            string provider,
            string name,
            string displayName,
            string unit,
            double value,
            CounterType counterType,
            float interval,
            string metadata)
        {
            Timestamp = timestamp;
            Name = name;
            DisplayName = displayName;
            Unit = unit;
            Value = value;
            CounterType = counterType;
            Provider = provider;
            Interval = interval;
            Metadata = metadata;
            EventType = EventType.Gauge;
        }

        // Copied from dotnet-counters
        public CounterPayload(string providerName, string name, string displayName, string displayUnits, string metadata, double value, DateTime timestamp, string type, EventType eventType)
        {
            Provider = providerName;
            Name = name;
            Metadata = metadata;
            Value = value;
            Timestamp = timestamp;
            CounterType = (CounterType)Enum.Parse(typeof(CounterType), type);
            EventType = eventType;
        }

        public string Name { get; }

        public string DisplayName { get; protected set; }

        public string Unit { get; }

        public double Value { get; }

        public DateTime Timestamp { get; }

        public float Interval { get; }

        public CounterType CounterType { get; }

        public string Provider { get; }

        public string Metadata { get; }

        public EventType EventType { get; set; }

        public virtual bool IsMeter => false;
    }

    internal class GaugePayload : CounterPayload
    {
        public GaugePayload(string providerName, string name, string displayName, string displayUnits, string metadata, double value, DateTime timestamp) :
            base(providerName, name, displayName, displayUnits, metadata, value, timestamp, "Metric", EventType.Gauge)
        {
            // In case these properties are not provided, set them to appropriate values.
            string counterName = string.IsNullOrEmpty(displayName) ? name : displayName;
            DisplayName = !string.IsNullOrEmpty(displayUnits) ? $"{counterName} ({displayUnits})" : counterName;
        }

        public override bool IsMeter => true;
    }

    internal class InstrumentationStartedPayload : CounterPayload
    {
        public InstrumentationStartedPayload(string providerName, string name, DateTime dateTime)
            : base(providerName, name, string.Empty, string.Empty, null, 0.0, dateTime, "Metric", EventType.InstrumentationStarted)
        {
        }

        public override bool IsMeter => true;
    }

    internal class CounterEndedPayload : CounterPayload
    {
        public CounterEndedPayload(string providerName, string name, string displayName, DateTime timestamp)
            : base(providerName, name, displayName, string.Empty, null, 0.0, timestamp, "Metric", EventType.CounterEnded)
        {

        }

        public override bool IsMeter => true;
    }

    internal class RatePayload : CounterPayload
    {
        public RatePayload(string providerName, string name, string displayName, string displayUnits, string metadata, double value, double intervalSecs, DateTime timestamp) :
            base(providerName, name, displayName, displayUnits, metadata, value, timestamp, "Rate", EventType.Rate)
        {
            // In case these properties are not provided, set them to appropriate values.
            string counterName = string.IsNullOrEmpty(displayName) ? name : displayName;
            string unitsName = string.IsNullOrEmpty(displayUnits) ? "Count" : displayUnits;
            string intervalName = intervalSecs.ToString() + " sec";
            DisplayName = $"{counterName} ({unitsName} / {intervalName})";
        }

        public override bool IsMeter => true;
    }

    internal class PercentilePayload : CounterPayload
    {
        public PercentilePayload(string providerName, string name, string displayName, string displayUnits, string metadata, double val, DateTime timestamp) :
            base(providerName, name, displayName, displayUnits, metadata, val, timestamp, "Metric", EventType.Histogram)
        {
            // In case these properties are not provided, set them to appropriate values.
            string counterName = string.IsNullOrEmpty(displayName) ? name : displayName;
            DisplayName = !string.IsNullOrEmpty(displayUnits) ? $"{counterName} ({displayUnits})" : counterName;
        }

        public override bool IsMeter => true;
    }

    internal class ErrorPayload : CounterPayload
    {
        public ErrorPayload(string errorMessage, DateTime timestamp, ErrorType errorType = ErrorType.NonFatal) :
            base(string.Empty, string.Empty, string.Empty, string.Empty, null, 0.0, timestamp, "Metric", EventType.Error)
        {
            ErrorMessage = errorMessage;
            ErrorType = errorType;
        }

        public string ErrorMessage { get; }

        public ErrorType ErrorType { get; }
    }

    // If keep this, should probably put it somewhere else
    internal enum EventType : int
    {
        Rate,
        Gauge,
        Histogram,
        Error,
        InstrumentationStarted,
        CounterEnded
    }

    internal enum ErrorType : int
    {
        NonFatal,
        TracingError,
        SessionStartupError
    }
}