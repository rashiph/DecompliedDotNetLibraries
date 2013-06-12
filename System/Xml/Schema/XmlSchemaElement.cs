namespace System.Xml.Schema
{
    using System;
    using System.ComponentModel;
    using System.Xml;
    using System.Xml.Serialization;

    public class XmlSchemaElement : XmlSchemaParticle
    {
        private XmlSchemaDerivationMethod block = XmlSchemaDerivationMethod.None;
        private XmlSchemaDerivationMethod blockResolved;
        private XmlSchemaObjectCollection constraints;
        private string defaultValue;
        private SchemaElementDecl elementDecl;
        private XmlSchemaType elementType;
        private XmlSchemaDerivationMethod final = XmlSchemaDerivationMethod.None;
        private XmlSchemaDerivationMethod finalResolved;
        private string fixedValue;
        private XmlSchemaForm form;
        private bool hasAbstractAttribute;
        private bool hasNillableAttribute;
        private bool isAbstract;
        private bool isLocalTypeDerivationChecked;
        private bool isNillable;
        private string name;
        private XmlQualifiedName qualifiedName = XmlQualifiedName.Empty;
        private XmlQualifiedName refName = XmlQualifiedName.Empty;
        private XmlQualifiedName substitutionGroup = XmlQualifiedName.Empty;
        private XmlSchemaType type;
        private XmlQualifiedName typeName = XmlQualifiedName.Empty;

        internal override XmlSchemaObject Clone()
        {
            return this.Clone(null);
        }

        internal XmlSchemaObject Clone(XmlSchema parentSchema)
        {
            XmlSchemaElement element = (XmlSchemaElement) base.MemberwiseClone();
            element.refName = this.refName.Clone();
            element.substitutionGroup = this.substitutionGroup.Clone();
            element.typeName = this.typeName.Clone();
            element.qualifiedName = this.qualifiedName.Clone();
            XmlSchemaComplexType type = this.type as XmlSchemaComplexType;
            if ((type != null) && type.QualifiedName.IsEmpty)
            {
                element.type = (XmlSchemaType) type.Clone(parentSchema);
            }
            element.constraints = null;
            return element;
        }

        internal void SetBlockResolved(XmlSchemaDerivationMethod value)
        {
            this.blockResolved = value;
        }

        internal void SetElementType(XmlSchemaType value)
        {
            this.elementType = value;
        }

        internal void SetFinalResolved(XmlSchemaDerivationMethod value)
        {
            this.finalResolved = value;
        }

        internal void SetQualifiedName(XmlQualifiedName value)
        {
            this.qualifiedName = value;
        }

        internal XmlReader Validate(XmlReader reader, XmlResolver resolver, XmlSchemaSet schemaSet, ValidationEventHandler valEventHandler)
        {
            if (schemaSet != null)
            {
                XmlReaderSettings readerSettings = new XmlReaderSettings {
                    ValidationType = ValidationType.Schema,
                    Schemas = schemaSet
                };
                readerSettings.ValidationEventHandler += valEventHandler;
                return new XsdValidatingReader(reader, resolver, readerSettings, this);
            }
            return null;
        }

        [DefaultValue(0x100), XmlAttribute("block")]
        public XmlSchemaDerivationMethod Block
        {
            get
            {
                return this.block;
            }
            set
            {
                this.block = value;
            }
        }

        [XmlIgnore]
        public XmlSchemaDerivationMethod BlockResolved
        {
            get
            {
                return this.blockResolved;
            }
        }

        [XmlElement("unique", typeof(XmlSchemaUnique)), XmlElement("key", typeof(XmlSchemaKey)), XmlElement("keyref", typeof(XmlSchemaKeyref))]
        public XmlSchemaObjectCollection Constraints
        {
            get
            {
                if (this.constraints == null)
                {
                    this.constraints = new XmlSchemaObjectCollection();
                }
                return this.constraints;
            }
        }

        [DefaultValue((string) null), XmlAttribute("default")]
        public string DefaultValue
        {
            get
            {
                return this.defaultValue;
            }
            set
            {
                this.defaultValue = value;
            }
        }

        internal SchemaElementDecl ElementDecl
        {
            get
            {
                return this.elementDecl;
            }
            set
            {
                this.elementDecl = value;
            }
        }

        [XmlIgnore]
        public XmlSchemaType ElementSchemaType
        {
            get
            {
                return this.elementType;
            }
        }

        [Obsolete("This property has been deprecated. Please use ElementSchemaType property that returns a strongly typed element type. http://go.microsoft.com/fwlink/?linkid=14202"), XmlIgnore]
        public object ElementType
        {
            get
            {
                if (this.elementType == null)
                {
                    return null;
                }
                if (this.elementType.QualifiedName.Namespace == "http://www.w3.org/2001/XMLSchema")
                {
                    return this.elementType.Datatype;
                }
                return this.elementType;
            }
        }

        [DefaultValue(0x100), XmlAttribute("final")]
        public XmlSchemaDerivationMethod Final
        {
            get
            {
                return this.final;
            }
            set
            {
                this.final = value;
            }
        }

        [XmlIgnore]
        public XmlSchemaDerivationMethod FinalResolved
        {
            get
            {
                return this.finalResolved;
            }
        }

        [XmlAttribute("fixed"), DefaultValue((string) null)]
        public string FixedValue
        {
            get
            {
                return this.fixedValue;
            }
            set
            {
                this.fixedValue = value;
            }
        }

        [DefaultValue(0), XmlAttribute("form")]
        public XmlSchemaForm Form
        {
            get
            {
                return this.form;
            }
            set
            {
                this.form = value;
            }
        }

        [XmlIgnore]
        internal bool HasAbstractAttribute
        {
            get
            {
                return this.hasAbstractAttribute;
            }
        }

        internal bool HasConstraints
        {
            get
            {
                return ((this.constraints != null) && (this.constraints.Count > 0));
            }
        }

        [XmlIgnore]
        internal bool HasDefault
        {
            get
            {
                return ((this.defaultValue != null) && (this.defaultValue.Length > 0));
            }
        }

        [XmlIgnore]
        internal bool HasNillableAttribute
        {
            get
            {
                return this.hasNillableAttribute;
            }
        }

        [DefaultValue(false), XmlAttribute("abstract")]
        public bool IsAbstract
        {
            get
            {
                return this.isAbstract;
            }
            set
            {
                this.isAbstract = value;
                this.hasAbstractAttribute = true;
            }
        }

        internal bool IsLocalTypeDerivationChecked
        {
            get
            {
                return this.isLocalTypeDerivationChecked;
            }
            set
            {
                this.isLocalTypeDerivationChecked = value;
            }
        }

        [XmlAttribute("nillable"), DefaultValue(false)]
        public bool IsNillable
        {
            get
            {
                return this.isNillable;
            }
            set
            {
                this.isNillable = value;
                this.hasNillableAttribute = true;
            }
        }

        [DefaultValue(""), XmlAttribute("name")]
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
        internal override string NameString
        {
            get
            {
                return this.qualifiedName.ToString();
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

        [XmlAttribute("ref")]
        public XmlQualifiedName RefName
        {
            get
            {
                return this.refName;
            }
            set
            {
                this.refName = (value == null) ? XmlQualifiedName.Empty : value;
            }
        }

        [XmlElement("simpleType", typeof(XmlSchemaSimpleType)), XmlElement("complexType", typeof(XmlSchemaComplexType))]
        public XmlSchemaType SchemaType
        {
            get
            {
                return this.type;
            }
            set
            {
                this.type = value;
            }
        }

        [XmlAttribute("type")]
        public XmlQualifiedName SchemaTypeName
        {
            get
            {
                return this.typeName;
            }
            set
            {
                this.typeName = (value == null) ? XmlQualifiedName.Empty : value;
            }
        }

        [XmlAttribute("substitutionGroup")]
        public XmlQualifiedName SubstitutionGroup
        {
            get
            {
                return this.substitutionGroup;
            }
            set
            {
                this.substitutionGroup = (value == null) ? XmlQualifiedName.Empty : value;
            }
        }
    }
}

