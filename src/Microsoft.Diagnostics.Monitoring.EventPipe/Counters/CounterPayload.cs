// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Diagnostics.Monitoring.EventPipe
{
    internal class CounterPayload : ICounterPayload
    {
        private string m_Name;
        private double m_Value;
        private string m_DisplayName;
        private string m_DisplayUnits;
        private string m_Provider;

        public CounterPayload(string provider, IDictionary<string, object> payloadFields)
        {
            m_Name = payloadFields["Name"].ToString();
            m_Value = (double)payloadFields["Mean"];
            m_DisplayName = payloadFields["DisplayName"].ToString();
            m_DisplayUnits = payloadFields["DisplayUnits"].ToString();
            m_Provider = provider;

            // In case these properties are not provided, set them to appropriate values.
            m_DisplayName = m_DisplayName.Length == 0 ? m_Name : m_DisplayName;
        }

        public string GetName()
        {
            return m_Name;
        }

        public double GetValue()
        {
            return m_Value;
        }

        public string GetDisplay()
        {
            if (m_DisplayUnits.Length > 0)
            {
                return $"{m_DisplayName} ({m_DisplayUnits})";
            }
            return $"{m_DisplayName}";
        }

        public string GetCounterType()
        {
            return "Metric";
        }

        public string GetProvider() => m_Provider;

        public string GetDisplayName()
        {
            throw new NotImplementedException();
        }

        public string GetUnit()
        {
            throw new NotImplementedException();
        }

        public DateTime GetTimestamp()
        {
            throw new NotImplementedException();
        }
    }

    internal class IncrementingCounterPayload : ICounterPayload
    {
        private string m_Name;
        private double m_Value;
        private string m_DisplayName;
        private string m_Interval;
        private string m_DisplayUnits;
        private string m_Provider;

        public IncrementingCounterPayload(string provider, IDictionary<string, object> payloadFields, int interval)
        {
            m_Name = payloadFields["Name"].ToString();
            m_Value = (double)payloadFields["Increment"];
            m_DisplayName = payloadFields["DisplayName"].ToString();
            m_DisplayUnits = payloadFields["DisplayUnits"].ToString();
            m_Interval = interval.ToString() + " sec";
            m_Provider = provider;
            // In case these properties are not provided, set them to appropriate values.
            m_DisplayName = m_DisplayName.Length == 0 ? m_Name : m_DisplayName;
        }

        public string GetName()
        {
            return m_Name;
        }

        public double GetValue()
        {
            return m_Value;
        }

        public string GetDisplay()
        {
            return $"{m_DisplayName} ({GetDisplayUnits()} / {m_Interval})";
        }

        public string GetCounterType()
        {
            return "Rate";
        }

        private string GetDisplayUnits()
        {
            if (m_DisplayUnits.Length == 0) return "Count";
            return m_DisplayUnits;
        }

        public string GetProvider() => m_Provider;

        public string GetDisplayName()
        {
            throw new NotImplementedException();
        }

        public string GetUnit()
        {
            throw new NotImplementedException();
        }

        public DateTime GetTimestamp()
        {
            throw new NotImplementedException();
        }
    }
}
