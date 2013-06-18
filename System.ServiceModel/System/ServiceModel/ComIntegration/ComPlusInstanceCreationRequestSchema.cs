namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.Diagnostics;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.Xml;

    [DataContract(Name="ComPlusInstanceCreationRequest")]
    internal class ComPlusInstanceCreationRequestSchema : TraceRecord
    {
        [DataMember(Name="appid")]
        private Guid appid;
        [DataMember(Name="clsid")]
        private Guid clsid;
        [DataMember(Name="From")]
        private Uri from;
        [DataMember(Name="IncomingTransactionID")]
        private Guid incomingTransactionID;
        [DataMember(Name="RequestingIdentity")]
        private string requestingIdentity;
        private const string schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusInstanceCreationRequestTraceRecord";

        public ComPlusInstanceCreationRequestSchema(Guid appid, Guid clsid, Uri from, Guid incomingTransactionID, string requestingIdentity)
        {
            this.from = from;
            this.appid = appid;
            this.clsid = clsid;
            this.incomingTransactionID = incomingTransactionID;
            this.requestingIdentity = requestingIdentity;
        }

        public override string ToString()
        {
            return System.ServiceModel.SR.GetString("ComPlusInstanceCreationRequestSchema", new object[] { this.from.ToString(), this.appid.ToString(), this.clsid.ToString(), this.incomingTransactionID.ToString(), this.requestingIdentity });
        }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        internal override string EventId
        {
            get
            {
                return "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusInstanceCreationRequestTraceRecord";
            }
        }
    }
}

