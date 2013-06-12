namespace System.Xml.Schema
{
    using System;
    using System.Xml;
    using System.Xml.Serialization;

    public class XmlSchemaAppInfo : XmlSchemaObject
    {
        private XmlNode[] markup;
        private string source;

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

