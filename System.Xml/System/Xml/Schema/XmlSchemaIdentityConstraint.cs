namespace System.Xml.Schema
{
    using System;
    using System.Xml;
    using System.Xml.Serialization;

    public class XmlSchemaIdentityConstraint : XmlSchemaAnnotated
    {
        private CompiledIdentityConstraint compiledConstraint;
        private XmlSchemaObjectCollection fields = new XmlSchemaObjectCollection();
        private string name;
        private XmlQualifiedName qualifiedName = XmlQualifiedName.Empty;
        private XmlSchemaXPath selector;

        internal void SetQualifiedName(XmlQualifiedName value)
        {
            this.qualifiedName = value;
        }

        [XmlIgnore]
        internal CompiledIdentityConstraint CompiledConstraint
        {
            get
            {
                return this.compiledConstraint;
            }
            set
            {
                this.compiledConstraint = value;
            }
        }

        [XmlElement("field", typeof(XmlSchemaXPath))]
        public XmlSchemaObjectCollection Fields
        {
            get
            {
                return this.fields;
            }
        }

        [XmlAttribute("name")]
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        [XmlIgnore]
        internal override string NameAttribute
        {
            get
            {
                return this.Name;
            }
            set
            {
                this.Name = value;
            }
        }

        [XmlIgnore]
        public XmlQualifiedName QualifiedName
        {
            get
            {
                return this.qualifiedName;
            }
        }

        [XmlElement("selector", typeof(XmlSchemaXPath))]
        public XmlSchemaXPath Selector
        {
            get
            {
                return this.selector;
            }
            set
            {
                this.selector = value;
            }
        }
    }
}

