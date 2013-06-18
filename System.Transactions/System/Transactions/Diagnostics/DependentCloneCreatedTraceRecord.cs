namespace System.Transactions.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Transactions;
    using System.Xml;

    internal class DependentCloneCreatedTraceRecord : TraceRecord
    {
        private DependentCloneOption option;
        private static DependentCloneCreatedTraceRecord record = new DependentCloneCreatedTraceRecord();
        private string traceSource;
        private TransactionTraceIdentifier txTraceId;

        internal static void Trace(string traceSource, TransactionTraceIdentifier txTraceId, DependentCloneOption option)
        {
            lock (record)
            {
                record.traceSource = traceSource;
                record.txTraceId = txTraceId;
                record.option = option;
                DiagnosticTrace.TraceEvent(TraceEventType.Information, "http://msdn.microsoft.com/2004/06/System/Transactions/DependentCloneCreated", System.Transactions.SR.GetString("TraceDependentCloneCreated"), record);
            }
        }

        internal override void WriteTo(XmlWriter xml)
        {
            TraceHelper.WriteTraceSource(xml, this.traceSource);
            TraceHelper.WriteTxId(xml, this.txTraceId);
            xml.WriteElementString("DependentCloneOption", this.option.ToString());
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2004/03/Transactions/DependentCloneCreatedTraceRecord";
            }
        }
    }
}

