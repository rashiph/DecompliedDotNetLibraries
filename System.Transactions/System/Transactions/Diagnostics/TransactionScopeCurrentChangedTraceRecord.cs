namespace System.Transactions.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Transactions;
    using System.Xml;

    internal class TransactionScopeCurrentChangedTraceRecord : TraceRecord
    {
        private TransactionTraceIdentifier currentTxTraceId;
        private static TransactionScopeCurrentChangedTraceRecord record = new TransactionScopeCurrentChangedTraceRecord();
        private TransactionTraceIdentifier scopeTxTraceId;
        private string traceSource;

        internal static void Trace(string traceSource, TransactionTraceIdentifier scopeTxTraceId, TransactionTraceIdentifier currentTxTraceId)
        {
            lock (record)
            {
                record.traceSource = traceSource;
                record.scopeTxTraceId = scopeTxTraceId;
                record.currentTxTraceId = currentTxTraceId;
                DiagnosticTrace.TraceEvent(TraceEventType.Warning, "http://msdn.microsoft.com/2004/06/System/Transactions/TransactionScopeCurrentTransactionChanged", System.Transactions.SR.GetString("TraceTransactionScopeCurrentTransactionChanged"), record);
            }
        }

        internal override void WriteTo(XmlWriter xml)
        {
            TraceHelper.WriteTraceSource(xml, this.traceSource);
            TraceHelper.WriteTxId(xml, this.scopeTxTraceId);
            TraceHelper.WriteTxId(xml, this.currentTxTraceId);
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2004/03/Transactions/TransactionScopeCurrentChangedTraceRecord";
            }
        }
    }
}

