namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;
    using System.Runtime.Serialization;
    using System.Xml;

    internal static class TransactionTraceRecord
    {
        public static void SerializeRecord(XmlWriter xmlWriter, object o)
        {
            new DataContractSerializer(o.GetType()).WriteObject(xmlWriter, o);
        }
    }
}

