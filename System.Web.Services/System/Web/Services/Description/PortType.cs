namespace System.Web.Services.Description
{
    using System;
    using System.Runtime;
    using System.Web.Services.Configuration;
    using System.Xml.Serialization;

    [XmlFormatExtensionPoint("Extensions")]
    public sealed class PortType : NamedItem
    {
        private ServiceDescriptionFormatExtensionCollection extensions;
        private OperationCollection operations;
        private System.Web.Services.Description.ServiceDescription parent;

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

        [XmlElement("operation")]
        public OperationCollection Operations
        {
            get
            {
                if (this.operations == null)
                {
                    this.operations = new OperationCollection(this);
                }
                return this.operations;
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

