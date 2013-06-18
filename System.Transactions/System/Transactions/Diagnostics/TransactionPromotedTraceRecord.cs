namespace System.Transactions.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Transactions;
    using System.Xml;

    internal class TransactionPromotedTraceRecord : TraceRecord
    {
        private TransactionTraceIdentifier distTxTraceId;
        private TransactionTraceIdentifier localTxTraceId;
        private static TransactionPromotedTraceRecord record = new TransactionPromotedTraceRecord();
        private string traceSource;

        internal static void Trace(string traceSource, TransactionTraceIdentifier localTxTraceId, TransactionTraceIdentifier distTxTraceId)
        {
            lock (record)
            {
                record.traceSource = traceSource;
                record.localTxTraceId = localTxTraceId;
                record.distTxTraceId = distTxTraceId;
                DiagnosticTrace.TraceEvent(TraceEventType.Information, "http://msdn.microsoft.com/2004/06/System/Transactions/TransactionPromoted", System.Transactions.SR.GetString("TraceTransactionPromoted"), record);
            }
        }

        internal override void WriteTo(XmlWriter xml)
        {
            TraceHelper.WriteTraceSource(xml, this.traceSource);
            xml.WriteStartElement("LightweightTransaction");
            TraceHelper.WriteTxId(xml, this.localTxTraceId);
            xml.WriteEndElement();
            xml.WriteStartElement("PromotedTransaction");
            TraceHelper.WriteTxId(xml, this.distTxTraceId);
            xml.WriteEndElement();
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2004/03/Transactions/TransactionPromotedTraceRecord";
            }
        }
    }
}

