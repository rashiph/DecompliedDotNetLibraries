namespace System.Transactions.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Transactions;
    using System.Xml;

    internal class ConfiguredDefaultTimeoutAdjustedTraceRecord : TraceRecord
    {
        private static ConfiguredDefaultTimeoutAdjustedTraceRecord record = new ConfiguredDefaultTimeoutAdjustedTraceRecord();
        private string traceSource;

        internal static void Trace(string traceSource)
        {
            lock (record)
            {
                record.traceSource = traceSource;
                DiagnosticTrace.TraceEvent(TraceEventType.Warning, "http://msdn.microsoft.com/2004/06/System/Transactions/ConfiguredDefaultTimeoutAdjusted", System.Transactions.SR.GetString("TraceConfiguredDefaultTimeoutAdjusted"), record);
            }
        }

        internal override void WriteTo(XmlWriter xml)
        {
            TraceHelper.WriteTraceSource(xml, this.traceSource);
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2004/03/Transactions/ConfiguredDefaultTimeoutAdjustedTraceRecord";
            }
        }
    }
}

