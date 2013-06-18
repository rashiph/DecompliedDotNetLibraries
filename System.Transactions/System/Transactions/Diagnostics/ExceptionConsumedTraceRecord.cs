namespace System.Transactions.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Transactions;
    using System.Xml;

    internal class ExceptionConsumedTraceRecord : TraceRecord
    {
        private Exception exception;
        private static ExceptionConsumedTraceRecord record = new ExceptionConsumedTraceRecord();
        private string traceSource;

        internal static void Trace(string traceSource, Exception exception)
        {
            lock (record)
            {
                record.traceSource = traceSource;
                record.exception = exception;
                DiagnosticTrace.TraceEvent(TraceEventType.Verbose, "http://msdn.microsoft.com/2004/06/System/Transactions/ExceptionConsumed", System.Transactions.SR.GetString("TraceExceptionConsumed"), record);
            }
        }

        internal override void WriteTo(XmlWriter xml)
        {
            TraceHelper.WriteTraceSource(xml, this.traceSource);
            xml.WriteElementString("ExceptionMessage", this.exception.Message);
            xml.WriteElementString("ExceptionStack", this.exception.StackTrace);
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2004/03/Transactions/ExceptionConsumedTraceRecord";
            }
        }
    }
}

