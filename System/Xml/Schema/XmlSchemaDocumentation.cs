namespace System.Xml.Schema
{
    using System;
    using System.Xml;
    using System.Xml.Serialization;

    public class XmlSchemaDocumentation : XmlSchemaObject
    {
        private string language;
        private static XmlSchemaSimpleType languageType = DatatypeImplementation.GetSimpleTypeFromXsdType(new XmlQualifiedName("language", "http://www.w3.org/2001/XMLSchema"));
        private XmlNode[] markup;
        private string source;

        [XmlAttribute("xml:lang")]
        public string Language
        {
            get
            {
                return this.language;
            }
            set
            {
                this.language = (string) languageType.Datatype.ParseValue(value, null, null);
            }
        }

        [XmlAnyElement, XmlText]
        public XmlNode[] Markup
        {
            get
            {
                return this.markup;
            }
            set
            {
                this.markup = value;
            }
        }

        [XmlAttribute("source", DataType="anyURI")]
        public string Source
        {
            get
            {
                return this.source;
            }
            set
            {
                this.source = value;
            }
        }
    }
}

