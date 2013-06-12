namespace System.Xml.Schema
{
    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;

    public class XmlSchemaXPath : XmlSchemaAnnotated
    {
        private string xpath;

        [XmlAttribute("xpath"), DefaultValue("")]
        public string XPath
        {
            get
            {
                return this.xpath;
            }
            set
            {
                this.xpath = value;
            }
        }
    }
}

