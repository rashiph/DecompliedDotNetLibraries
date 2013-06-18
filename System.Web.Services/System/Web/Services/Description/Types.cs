namespace System.Web.Services.Description
{
    using System;
    using System.Web.Services.Configuration;
    using System.Xml.Schema;
    using System.Xml.Serialization;

    [XmlFormatExtensionPoint("Extensions")]
    public sealed class Types : DocumentableItem
    {
        private ServiceDescriptionFormatExtensionCollection extensions;
        private XmlSchemas schemas;

        internal bool HasItems()
        {
            return (((this.schemas != null) && (this.schemas.Count > 0)) || ((this.extensions != null) && (this.extensions.Count > 0)));
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

        [XmlElement("schema", typeof(XmlSchema), Namespace="http://www.w3.org/2001/XMLSchema")]
        public XmlSchemas Schemas
        {
            get
            {
                if (this.schemas == null)
                {
                    this.schemas = new XmlSchemas();
                }
                return this.schemas;
            }
        }
    }
}

