namespace System.Transactions.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Transactions;
    using System.Xml;

    internal class TransactionScopeCreatedTraceRecord : TraceRecord
    {
        private static TransactionScopeCreatedTraceRecord record = new TransactionScopeCreatedTraceRecord();
        private string traceSource;
        private TransactionScopeResult txScopeResult;
        private TransactionTraceIdentifier txTraceId;

        internal static void Trace(string traceSource, TransactionTraceIdentifier txTraceId, TransactionScopeResult txScopeResult)
        {
            lock (record)
            {
                record.traceSource = traceSource;
                record.txTraceId = txTraceId;
                record.txScopeResult = txScopeResult;
                DiagnosticTrace.TraceEvent(TraceEventType.Information, "http://msdn.microsoft.com/2004/06/System/Transactions/TransactionScopeCreated", System.Transactions.SR.GetString("TraceTransactionScopeCreated"), record);
            }
        }

        internal override void WriteTo(XmlWriter xml)
        {
            TraceHelper.WriteTraceSource(xml, this.traceSource);
            TraceHelper.WriteTxId(xml, this.txTraceId);
            xml.WriteElementString("TransactionScopeResult", this.txScopeResult.ToString());
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2004/03/Transactions/TransactionScopeCreatedTraceRecord";
            }
        }
    }
}

