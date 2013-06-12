namespace System.Xml.Schema
{
    using System;
    using System.Xml.Serialization;

    public class XmlSchemaInclude : XmlSchemaExternal
    {
        private XmlSchemaAnnotation annotation;

        public XmlSchemaInclude()
        {
            base.Compositor = System.Xml.Schema.Compositor.Include;
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
    }
}

