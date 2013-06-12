namespace System.Xml.Schema
{
    using System;
    using System.Xml.Serialization;

    public class XmlSchemaSimpleContent : XmlSchemaContentModel
    {
        private XmlSchemaContent content;

        [XmlElement("restriction", typeof(XmlSchemaSimpleContentRestriction)), XmlElement("extension", typeof(XmlSchemaSimpleContentExtension))]
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
    }
}

