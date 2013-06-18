namespace System.Transactions.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Transactions;
    using System.Xml;

    internal class InvalidOperationExceptionTraceRecord : TraceRecord
    {
        private string exceptionMessage;
        private static InvalidOperationExceptionTraceRecord record = new InvalidOperationExceptionTraceRecord();
        private string traceSource;

        internal static void Trace(string traceSource, string exceptionMessage)
        {
            lock (record)
            {
                record.traceSource = traceSource;
                record.exceptionMessage = exceptionMessage;
                DiagnosticTrace.TraceEvent(TraceEventType.Error, "http://msdn.microsoft.com/2004/06/System/Transactions/InvalidOperationException", System.Transactions.SR.GetString("TraceInvalidOperationException"), record);
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
                return "http://schemas.microsoft.com/2004/03/Transactions/InvalidOperationExceptionTraceRecord";
            }
        }
    }
}

