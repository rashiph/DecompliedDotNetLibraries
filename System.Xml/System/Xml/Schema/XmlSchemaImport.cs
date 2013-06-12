namespace System.Xml.Schema
{
    using System;
    using System.Xml.Serialization;

    public class XmlSchemaImport : XmlSchemaExternal
    {
        private XmlSchemaAnnotation annotation;
        private string ns;

        public XmlSchemaImport()
        {
            base.Compositor = System.Xml.Schema.Compositor.Import;
        }

        internal override void AddAnnotation(XmlSchemaAnnotation annotation)
        {
            this.annotation = annotation;
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

        [XmlAttribute("namespace", DataType="anyURI")]
        public string Namespace
        {
            get
            {
                return this.ns;
            }
            set
            {
                this.ns = value;
            }
        }
    }
}

