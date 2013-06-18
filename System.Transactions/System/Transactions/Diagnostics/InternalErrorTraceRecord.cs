namespace System.Transactions.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Transactions;
    using System.Xml;

    internal class InternalErrorTraceRecord : TraceRecord
    {
        private string exceptionMessage;
        private static InternalErrorTraceRecord record = new InternalErrorTraceRecord();
        private string traceSource;

        internal static void Trace(string traceSource, string exceptionMessage)
        {
            lock (record)
            {
                record.traceSource = traceSource;
                record.exceptionMessage = exceptionMessage;
                DiagnosticTrace.TraceEvent(TraceEventType.Critical, "http://msdn.microsoft.com/2004/06/System/Transactions/InternalError", System.Transactions.SR.GetString("TraceInternalError"), record);
            }
        }

        internal override void WriteTo(XmlWriter xml)
        {
            TraceHelper.WriteTraceSource(xml, this.traceSource);
            xml.WriteElementString("ExceptionMessage", this.exceptionMessage);
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2004/03/Transactions/InternalErrorTraceRecord";
            }
        }
    }
}

