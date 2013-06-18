namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.Diagnostics;
    using System.Runtime.Serialization;
    using System.Xml;

    [DataContract(Name="ComPlusInstanceReleased")]
    internal class ComPlusInstanceReleasedSchema : TraceRecord
    {
        [DataMember(Name="appid")]
        private Guid appid;
        [DataMember(Name="clsid")]
        private Guid clsid;
        [DataMember(Name="InstanceID")]
        private int instanceID;
        private const string schemaId = "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusInstanceReleasedTraceRecord";

        public ComPlusInstanceReleasedSchema(Guid appid, Guid clsid, int instanceID)
        {
            this.appid = appid;
            this.clsid = clsid;
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
                return "http://schemas.microsoft.com/2006/08/ServiceModel/ComPlusInstanceReleasedTraceRecord";
            }
        }
    }
}

