namespace System.Xml.Schema
{
    using System;
    using System.Xml;

    internal sealed class SchemaAttDef : SchemaDeclBase, IDtdDefaultAttributeInfo, IDtdAttributeInfo
    {
        private bool defaultValueChecked;
        private string defExpanded;
        public static readonly SchemaAttDef Empty = new SchemaAttDef();
        private bool hasEntityRef;
        private int lineNum;
        private int linePos;
        private Reserve reserved;
        private XmlSchemaAttribute schemaAttribute;
        private int valueLineNum;
        private int valueLinePos;

        private SchemaAttDef()
        {
        }

        public SchemaAttDef(XmlQualifiedName name) : base(name, null)
        {
        }

        public SchemaAttDef(XmlQualifiedName name, string prefix) : base(name, prefix)
        {
        }

        internal void CheckXmlSpace(IValidationEventHandling validationEventHandling)
        {
            if (((base.datatype.TokenizedType == XmlTokenizedType.ENUMERATION) && (base.values != null)) && (base.values.Count <= 2))
            {
                string str = base.values[0].ToString();
                if (base.values.Count == 2)
                {
                    string str2 = base.values[1].ToString();
                    if (((str == "default") || (str2 == "default")) && ((str == "preserve") || (str2 == "preserve")))
                    {
                        return;
                    }
                }
                else
                {
                    switch (str)
                    {
                        case "default":
                        case "preserve":
                            return;
                    }
                }
            }
            validationEventHandling.SendEvent(new XmlSchemaException("Sch_XmlSpace", string.Empty), XmlSeverityType.Error);
        }

        internal SchemaAttDef Clone()
        {
            return (SchemaAttDef) base.MemberwiseClone();
        }

        internal bool DefaultValueChecked
        {
            get
            {
                return this.defaultValueChecked;
            }
        }

        internal string DefaultValueExpanded
        {
            get
            {
                if (this.defExpanded == null)
                {
                    return string.Empty;
                }
                return this.defExpanded;
            }
            set
            {
                this.defExpanded = value;
            }
        }

        internal bool HasEntityRef
        {
            get
            {
                return this.hasEntityRef;
            }
            set
            {
                this.hasEntityRef = value;
            }
        }

        internal int LineNumber
        {
            get
            {
                return this.lineNum;
            }
            set
            {
                this.lineNum = value;
            }
        }

        internal int LinePosition
        {
            get
            {
                return this.linePos;
            }
            set
            {
                this.linePos = value;
            }
        }

        internal Reserve Reserved
        {
            get
            {
                return this.reserved;
            }
            set
            {
                this.reserved = value;
            }
        }

        internal XmlSchemaAttribute SchemaAttribute
        {
            get
            {
                return this.schemaAttribute;
            }
            set
            {
                this.schemaAttribute = value;
            }
        }

        bool IDtdAttributeInfo.IsDeclaredInExternal
        {
            get
            {
                return base.IsDeclaredInExternal;
            }
        }

        bool IDtdAttributeInfo.IsNonCDataType
        {
            get
            {
                return (this.TokenizedType != XmlTokenizedType.CDATA);
            }
        }

        bool IDtdAttributeInfo.IsXmlAttribute
        {
            get
            {
                return (this.Reserved != Reserve.None);
            }
        }

        int IDtdAttributeInfo.LineNumber
        {
            get
            {
                return this.LineNumber;
            }
        }

        int IDtdAttributeInfo.LinePosition
        {
            get
            {
                return this.LinePosition;
            }
        }

        string IDtdAttributeInfo.LocalName
        {
            get
            {
                return base.Name.Name;
            }
        }

        string IDtdAttributeInfo.Prefix
        {
            get
            {
                return base.Prefix;
            }
        }

        string IDtdDefaultAttributeInfo.DefaultValueExpanded
        {
            get
            {
                return this.DefaultValueExpanded;
            }
        }

        object IDtdDefaultAttributeInfo.DefaultValueTyped
        {
            get
            {
                return base.DefaultValueTyped;
            }
        }

        int IDtdDefaultAttributeInfo.ValueLineNumber
        {
            get
            {
                return this.ValueLineNumber;
            }
        }

        int IDtdDefaultAttributeInfo.ValueLinePosition
        {
            get
            {
                return this.ValueLinePosition;
            }
        }

        internal XmlTokenizedType TokenizedType
        {
            get
            {
                return base.Datatype.TokenizedType;
            }
            set
            {
                base.Datatype = XmlSchemaDatatype.FromXmlTokenizedType(value);
            }
        }

        internal int ValueLineNumber
        {
            get
            {
                return this.valueLineNum;
            }
            set
            {
                this.valueLineNum = value;
            }
        }

        internal int ValueLinePosition
        {
            get
            {
                return this.valueLinePos;
            }
            set
            {
                this.valueLinePos = value;
            }
        }

        internal enum Reserve
        {
            None,
            XmlSpace,
            XmlLang
        }
    }
}

