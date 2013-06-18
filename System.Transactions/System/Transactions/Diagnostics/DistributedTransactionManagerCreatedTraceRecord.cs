namespace System.Transactions.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Transactions;
    using System.Xml;

    internal class DistributedTransactionManagerCreatedTraceRecord : TraceRecord
    {
        private string nodeName;
        private static DistributedTransactionManagerCreatedTraceRecord record = new DistributedTransactionManagerCreatedTraceRecord();
        private Type tmType;
        private string traceSource;

        internal static void Trace(string traceSource, Type tmType, string nodeName)
        {
            lock (record)
            {
                record.traceSource = traceSource;
                record.tmType = tmType;
                record.nodeName = nodeName;
                DiagnosticTrace.TraceEvent(TraceEventType.Verbose, "http://msdn.microsoft.com/2004/06/System/Transactions/TransactionManagerCreated", System.Transactions.SR.GetString("TraceTransactionManagerCreated"), record);
            }
        }

        internal override void WriteTo(XmlWriter xml)
        {
            TraceHelper.WriteTraceSource(xml, this.traceSource);
            xml.WriteElementString("TransactionManagerType", this.tmType.ToString());
            xml.WriteStartElement("TransactionManagerProperties");
            xml.WriteElementString("DistributedTransactionManagerName", this.nodeName);
            xml.WriteEndElement();
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2004/03/Transactions/TransactionManagerCreatedTraceRecord";
            }
        }
    }
}

