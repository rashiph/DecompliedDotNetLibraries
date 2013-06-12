namespace System.Xml.Schema
{
    using System;
    using System.Xml;
    using System.Xml.Serialization;

    public class XmlSchemaKeyref : XmlSchemaIdentityConstraint
    {
        private XmlQualifiedName refer = XmlQualifiedName.Empty;

        [XmlAttribute("refer")]
        public XmlQualifiedName Refer
        {
            get
            {
                return this.refer;
            }
            set
            {
                this.refer = (value == null) ? XmlQualifiedName.Empty : value;
            }
        }
    }
}

