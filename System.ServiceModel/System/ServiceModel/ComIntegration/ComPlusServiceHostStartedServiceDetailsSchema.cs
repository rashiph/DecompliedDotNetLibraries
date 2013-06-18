namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Runtime.Serialization;
    using System.Web.Services.Description;
    using System.Xml;

    [DataContract(Name="ComPlusServiceHostStartedServiceDetails")]
    internal class ComPlusServiceHostStartedServiceDetailsSchema : ComPlusServiceHostSchema
    {
        [DataMember(Name="ServiceDescription")]
        private WsdlWrapper wsdlWrapper;

        public ComPlusServiceHostStartedServiceDetailsSchema(Guid appid, Guid clsid, ServiceDescription wsdl) : base(appid, clsid)
        {
            this.wsdlWrapper = new WsdlWrapper(wsdl);
        }

        internal override void WriteTo(XmlWriter xmlWriter)
        {
            ComPlusTraceRecord.SerializeRecord(xmlWriter, this);
        }

        internal override string EventId
        {
            get
            {
                return base.BuildEventId("ComPlusServiceHostStartedServiceDetails");
            }
        }
    }
}

