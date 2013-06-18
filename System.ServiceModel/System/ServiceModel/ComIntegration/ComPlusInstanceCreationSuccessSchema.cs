namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.Serialization;
    using System.Xml;

    [DataContract(Name="ComPlusInstanceCreationSuccess")]
    internal class ComPlusInstanceCreationSuccessSchema : ComPlusInstanceCreationRequestSchema
    {
        [DataMember(Name="InstanceID")]
        private int instanceID;
        private const string schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusInstanceCreationSuccessTraceRecord";

        public ComPlusInstanceCreationSuccessSchema(Guid appid, Guid clsid, Uri from, Guid incomingTransactionID, string requestingIdentity, int instanceID) : base(appid, clsid, from, incomingTransactionID, requestingIdentity)
        {
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
                return "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusInstanceCreationSuccessTraceRecord";
            }
        }
    }
}

