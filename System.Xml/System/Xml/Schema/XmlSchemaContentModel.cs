namespace System.Xml.Schema
{
    using System;
    using System.Xml.Serialization;

    public abstract class XmlSchemaContentModel : XmlSchemaAnnotated
    {
        protected XmlSchemaContentModel()
        {
        }

        [XmlIgnore]
        public abstract XmlSchemaContent Content { get; set; }
    }
}

