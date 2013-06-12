namespace System.Xml.Schema
{
    using System;
    using System.Xml;
    using System.Xml.Serialization;

    public class XmlSchemaAttributeGroupRef : XmlSchemaAnnotated
    {
        private XmlQualifiedName refName = XmlQualifiedName.Empty;

        [XmlAttribute("ref")]
        public XmlQualifiedName RefName
        {
            get
            {
                return this.refName;
            }
            set
            {
                this.refName = (value == null) ? XmlQualifiedName.Empty : value;
            }
        }
    }
}

