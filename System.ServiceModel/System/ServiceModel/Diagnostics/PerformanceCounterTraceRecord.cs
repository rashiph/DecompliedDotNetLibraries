namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Runtime.Diagnostics;
    using System.Xml;

    internal class PerformanceCounterTraceRecord : TraceRecord
    {
        private string categoryName;
        private string instanceName;
        private string perfCounterName;

        internal PerformanceCounterTraceRecord(string perfCounterName) : this(null, perfCounterName, null)
        {
        }

        internal PerformanceCounterTraceRecord(string categoryName, string perfCounterName) : this(categoryName, perfCounterName, null)
        {
        }

        internal PerformanceCounterTraceRecord(string categoryName, string perfCounterName, string instanceName)
        {
            this.categoryName = categoryName;
            this.perfCounterName = perfCounterName;
            this.instanceName = instanceName;
        }

        internal override void WriteTo(XmlWriter writer)
        {
            if (!string.IsNullOrEmpty(this.categoryName))
            {
                writer.WriteElementString("PerformanceCategoryName", this.categoryName);
            }
            writer.WriteElementString("PerformanceCounterName", this.perfCounterName);
            if (!string.IsNullOrEmpty(this.instanceName))
            {
                writer.WriteElementString("InstanceName", this.instanceName);
            }
        }

        internal override string EventId
        {
            get
            {
                return base.BuildEventId("PerformanceCounter");
            }
        }
    }
}

