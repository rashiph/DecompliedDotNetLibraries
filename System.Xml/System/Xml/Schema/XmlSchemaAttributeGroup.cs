namespace System.Xml.Schema
{
    using System;
    using System.Xml;
    using System.Xml.Serialization;

    public class XmlSchemaAttributeGroup : XmlSchemaAnnotated
    {
        private XmlSchemaAnyAttribute anyAttribute;
        private XmlSchemaObjectCollection attributes = new XmlSchemaObjectCollection();
        private XmlSchemaObjectTable attributeUses;
        private XmlSchemaAnyAttribute attributeWildcard;
        private string name;
        private XmlQualifiedName qname = XmlQualifiedName.Empty;
        private XmlSchemaAttributeGroup redefined;
        private int selfReferenceCount;

        internal override XmlSchemaObject Clone()
        {
            XmlSchemaAttributeGroup group = (XmlSchemaAttributeGroup) base.MemberwiseClone();
            if (XmlSchemaComplexType.HasAttributeQNameRef(this.attributes))
            {
                group.attributes = XmlSchemaComplexType.CloneAttributes(this.attributes);
                group.attributeUses = null;
            }
            return group;
        }

        internal void SetQualifiedName(XmlQualifiedName value)
        {
            this.qname = value;
        }

        [XmlElement("anyAttribute")]
        public XmlSchemaAnyAttribute AnyAttribute
        {
            get
            {
                return this.anyAttribute;
            }
            set
            {
                this.anyAttribute = value;
            }
        }

        [XmlElement("attributeGroup", typeof(XmlSchemaAttributeGroupRef)), XmlElement("attribute", typeof(XmlSchemaAttribute))]
        public XmlSchemaObjectCollection Attributes
        {
            get
            {
                return this.attributes;
            }
        }

        [XmlIgnore]
        internal XmlSchemaObjectTable AttributeUses
        {
            get
            {
                if (this.attributeUses == null)
                {
                    this.attributeUses = new XmlSchemaObjectTable();
                }
                return this.attributeUses;
            }
        }

        [XmlIgnore]
        internal XmlSchemaAnyAttribute AttributeWildcard
        {
            get
            {
                return this.attributeWildcard;
            }
            set
            {
                this.attributeWildcard = value;
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
                return this.qname;
            }
        }

        [XmlIgnore]
        internal XmlSchemaAttributeGroup Redefined
        {
            get
            {
                return this.redefined;
            }
            set
            {
                this.redefined = value;
            }
        }

        [XmlIgnore]
        public XmlSchemaAttributeGroup RedefinedAttributeGroup
        {
            get
            {
                return this.redefined;
            }
        }

        [XmlIgnore]
        internal int SelfReferenceCount
        {
            get
            {
                return this.selfReferenceCount;
            }
            set
            {
                this.selfReferenceCount = value;
            }
        }
    }
}

