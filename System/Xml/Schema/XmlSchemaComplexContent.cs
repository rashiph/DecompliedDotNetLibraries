namespace System.Xml.Schema
{
    using System;
    using System.Xml.Serialization;

    public class XmlSchemaComplexContent : XmlSchemaContentModel
    {
        private XmlSchemaContent content;
        private bool hasMixedAttribute;
        private bool isMixed;

        [XmlElement("extension", typeof(XmlSchemaComplexContentExtension)), XmlElement("restriction", typeof(XmlSchemaComplexContentRestriction))]
        public override XmlSchemaContent Content
        {
            get
            {
                return this.content;
            }
            set
            {
                this.content = value;
            }
        }

        [XmlIgnore]
        internal bool HasMixedAttribute
        {
            get
            {
                return this.hasMixedAttribute;
            }
        }

        [XmlAttribute("mixed")]
        public bool IsMixed
        {
            get
            {
                return this.isMixed;
            }
            set
            {
                this.isMixed = value;
                this.hasMixedAttribute = true;
            }
        }
    }
}

