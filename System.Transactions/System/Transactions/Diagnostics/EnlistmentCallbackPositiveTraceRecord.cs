namespace System.Transactions.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Transactions;
    using System.Xml;

    internal class EnlistmentCallbackPositiveTraceRecord : TraceRecord
    {
        private EnlistmentCallback callback;
        private EnlistmentTraceIdentifier enTraceId;
        private static EnlistmentCallbackPositiveTraceRecord record = new EnlistmentCallbackPositiveTraceRecord();
        private string traceSource;

        internal static void Trace(string traceSource, EnlistmentTraceIdentifier enTraceId, EnlistmentCallback callback)
        {
            lock (record)
            {
                record.traceSource = traceSource;
                record.enTraceId = enTraceId;
                record.callback = callback;
                DiagnosticTrace.TraceEvent(TraceEventType.Verbose, "http://msdn.microsoft.com/2004/06/System/Transactions/EnlistmentCallbackPositive", System.Transactions.SR.GetString("TraceEnlistmentCallbackPositive"), record);
            }
        }

        internal override void WriteTo(XmlWriter xml)
        {
            TraceHelper.WriteTraceSource(xml, this.traceSource);
            TraceHelper.WriteEnId(xml, this.enTraceId);
            xml.WriteElementString("EnlistmentCallback", this.callback.ToString());
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2004/03/Transactions/EnlistmentCallbackPositiveTraceRecord";
            }
        }
    }
}

