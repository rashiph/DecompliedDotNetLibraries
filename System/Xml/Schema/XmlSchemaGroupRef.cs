namespace System.Xml.Schema
{
    using System;
    using System.Xml;
    using System.Xml.Serialization;

    public class XmlSchemaGroupRef : XmlSchemaParticle
    {
        private XmlSchemaGroupBase particle;
        private XmlSchemaGroup refined;
        private XmlQualifiedName refName = XmlQualifiedName.Empty;

        internal void SetParticle(XmlSchemaGroupBase value)
        {
            this.particle = value;
        }

        [XmlIgnore]
        public XmlSchemaGroupBase Particle
        {
            get
            {
                return this.particle;
            }
        }

        [XmlIgnore]
        internal XmlSchemaGroup Redefined
        {
            get
            {
                return this.refined;
            }
            set
            {
                this.refined = value;
            }
        }

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

