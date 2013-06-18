namespace System.Transactions.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Transactions;
    using System.Xml;

    internal class EnlistmentNotificationCallTraceRecord : TraceRecord
    {
        private EnlistmentTraceIdentifier enTraceId;
        private NotificationCall notCall;
        private static EnlistmentNotificationCallTraceRecord record = new EnlistmentNotificationCallTraceRecord();
        private string traceSource;

        internal static void Trace(string traceSource, EnlistmentTraceIdentifier enTraceId, NotificationCall notCall)
        {
            lock (record)
            {
                record.traceSource = traceSource;
                record.enTraceId = enTraceId;
                record.notCall = notCall;
                DiagnosticTrace.TraceEvent(TraceEventType.Verbose, "http://msdn.microsoft.com/2004/06/System/Transactions/EnlistmentNotificationCall", System.Transactions.SR.GetString("TraceEnlistmentNotificationCall"), record);
            }
        }

        internal override void WriteTo(XmlWriter xml)
        {
            TraceHelper.WriteTraceSource(xml, this.traceSource);
            TraceHelper.WriteEnId(xml, this.enTraceId);
            xml.WriteElementString("NotificationCall", this.notCall.ToString());
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2004/03/Transactions/EnlistmentNotificationCallTraceRecord";
            }
        }
    }
}

