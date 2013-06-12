namespace System.Xml.Schema
{
    using System;
    using System.Xml;
    using System.Xml.Serialization;

    public class XmlSchemaAnnotation : XmlSchemaObject
    {
        private string id;
        private XmlSchemaObjectCollection items = new XmlSchemaObjectCollection();
        private XmlAttribute[] moreAttributes;

        internal override void SetUnhandledAttributes(XmlAttribute[] moreAttributes)
        {
            this.moreAttributes = moreAttributes;
        }

        [XmlAttribute("id", DataType="ID")]
        public string Id
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }

        [XmlIgnore]
        internal override string IdAttribute
        {
            get
            {
                return this.Id;
            }
            set
            {
                this.Id = value;
            }
        }

        [XmlElement("documentation", typeof(XmlSchemaDocumentation)), XmlElement("appinfo", typeof(XmlSchemaAppInfo))]
        public XmlSchemaObjectCollection Items
        {
            get
            {
                return this.items;
            }
        }

        [XmlAnyAttribute]
        public XmlAttribute[] UnhandledAttributes
        {
            get
            {
                return this.moreAttributes;
            }
            set
            {
                this.moreAttributes = value;
            }
        }
    }
}

