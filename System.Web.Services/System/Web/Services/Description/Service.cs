namespace System.Web.Services.Description
{
    using System;
    using System.Runtime;
    using System.Web.Services.Configuration;
    using System.Xml.Serialization;

    [XmlFormatExtensionPoint("Extensions")]
    public sealed class Service : NamedItem
    {
        private ServiceDescriptionFormatExtensionCollection extensions;
        private System.Web.Services.Description.ServiceDescription parent;
        private PortCollection ports;

        internal void SetParent(System.Web.Services.Description.ServiceDescription parent)
        {
            this.parent = parent;
        }

        [XmlIgnore]
        public override ServiceDescriptionFormatExtensionCollection Extensions
        {
            get
            {
                if (this.extensions == null)
                {
                    this.extensions = new ServiceDescriptionFormatExtensionCollection(this);
                }
                return this.extensions;
            }
        }

        [XmlElement("port")]
        public PortCollection Ports
        {
            get
            {
                if (this.ports == null)
                {
                    this.ports = new PortCollection(this);
                }
                return this.ports;
            }
        }

        public System.Web.Services.Description.ServiceDescription ServiceDescription
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.parent;
            }
        }
    }
}

