namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.Diagnostics;
    using System.Runtime.Serialization;
    using System.Xml;

    [DataContract(Name="ComPlusTxProxySchema")]
    internal class ComPlusTxProxySchema : TraceRecord
    {
        [DataMember(Name="appid")]
        private Guid appid;
        [DataMember(Name="clsid")]
        private Guid clsid;
        [DataMember(Name="InstanceID")]
        private int instanceID;
        private const string schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusTxProxyTxTraceRecord";
        [DataMember(Name="TransactionID")]
        private Guid transactionID;

        public ComPlusTxProxySchema(Guid appid, Guid clsid, Guid transactionID, int instanceID)
        {
            this.appid = appid;
            this.clsid = clsid;
            this.transactionID = transactionID;
            this.instanceID = instanceID;
        }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusTxProxyTxTraceRecord";
            }
        }
    }
}

