namespace System.Xml.Schema
{
    using System;
    using System.Xml;
    using System.Xml.Serialization;

    public class XmlSchemaAnnotated : XmlSchemaObject
    {
        private XmlSchemaAnnotation annotation;
        private string id;
        private XmlAttribute[] moreAttributes;

        internal override void AddAnnotation(XmlSchemaAnnotation annotation)
        {
            this.annotation = annotation;
        }

        internal override void SetUnhandledAttributes(XmlAttribute[] moreAttributes)
        {
            this.moreAttributes = moreAttributes;
        }

        [XmlElement("annotation", typeof(XmlSchemaAnnotation))]
        public XmlSchemaAnnotation Annotation
        {
            get
            {
                return this.annotation;
            }
            set
            {
                this.annotation = value;
            }
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

