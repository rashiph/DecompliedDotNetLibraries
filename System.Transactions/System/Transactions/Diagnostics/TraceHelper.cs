namespace System.Transactions.Diagnostics
{
    using System;
    using System.Globalization;
    using System.Transactions;
    using System.Xml;

    internal static class TraceHelper
    {
        internal static void WriteEnId(XmlWriter writer, EnlistmentTraceIdentifier enId)
        {
            writer.WriteStartElement("EnlistmentTraceIdentifier");
            writer.WriteElementString("ResourceManagerId", enId.ResourceManagerIdentifier.ToString());
            WriteTxId(writer, enId.TransactionTraceId);
            writer.WriteElementString("EnlistmentIdentifier", enId.EnlistmentIdentifier.ToString(CultureInfo.CurrentCulture));
            writer.WriteEndElement();
        }

        internal static void WriteTraceSource(XmlWriter writer, string traceSource)
        {
            writer.WriteElementString("TraceSource", traceSource);
        }

        internal static void WriteTxId(XmlWriter writer, TransactionTraceIdentifier txTraceId)
        {
            writer.WriteStartElement("TransactionTraceIdentifier");
            if (txTraceId.TransactionIdentifier != null)
            {
                writer.WriteElementString("TransactionIdentifier", txTraceId.TransactionIdentifier);
            }
            else
            {
                writer.WriteElementString("TransactionIdentifier", "");
            }
            int cloneIdentifier = txTraceId.CloneIdentifier;
            if (cloneIdentifier != 0)
            {
                writer.WriteElementString("CloneIdentifier", cloneIdentifier.ToString(CultureInfo.CurrentCulture));
            }
            writer.WriteEndElement();
        }
    }
}

