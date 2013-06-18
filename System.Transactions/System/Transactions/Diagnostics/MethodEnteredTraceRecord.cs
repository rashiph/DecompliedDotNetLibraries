namespace System.Transactions.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Transactions;
    using System.Xml;

    internal class MethodEnteredTraceRecord : TraceRecord
    {
        private string methodName;
        private static MethodEnteredTraceRecord record = new MethodEnteredTraceRecord();
        private string traceSource;

        internal static void Trace(string traceSource, string methodName)
        {
            lock (record)
            {
                record.traceSource = traceSource;
                record.methodName = methodName;
                DiagnosticTrace.TraceEvent(TraceEventType.Verbose, "http://msdn.microsoft.com/2004/06/System/Transactions/MethodEntered", System.Transactions.SR.GetString("TraceMethodEntered"), record);
            }
        }

        internal override void WriteTo(XmlWriter xml)
        {
            TraceHelper.WriteTraceSource(xml, this.traceSource);
            xml.WriteElementString("MethodName", this.methodName);
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2004/03/Transactions/MethodEnteredTraceRecord";
            }
        }
    }
}

