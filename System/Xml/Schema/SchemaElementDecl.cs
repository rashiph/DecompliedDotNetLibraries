namespace System.Xml.Schema
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Xml;

    internal sealed class SchemaElementDecl : SchemaDeclBase, IDtdAttributeListInfo
    {
        private XmlSchemaAnyAttribute anyAttribute;
        private Dictionary<XmlQualifiedName, SchemaAttDef> attdefs;
        private XmlSchemaDerivationMethod block;
        private CompiledIdentityConstraint[] constraints;
        private System.Xml.Schema.ContentValidator contentValidator;
        private List<IDtdDefaultAttributeInfo> defaultAttdefs;
        internal static readonly SchemaElementDecl Empty = new SchemaElementDecl();
        private bool hasNonCDataAttribute;
        private bool hasRequiredAttribute;
        private bool isAbstract;
        private bool isIdDeclared;
        private bool isNillable;
        private bool isNotationDeclared;
        private Dictionary<XmlQualifiedName, XmlQualifiedName> prohibitedAttributes;
        private XmlSchemaElement schemaElement;

        internal SchemaElementDecl()
        {
            this.attdefs = new Dictionary<XmlQualifiedName, SchemaAttDef>();
            this.prohibitedAttributes = new Dictionary<XmlQualifiedName, XmlQualifiedName>();
        }

        internal SchemaElementDecl(XmlSchemaDatatype dtype)
        {
            this.attdefs = new Dictionary<XmlQualifiedName, SchemaAttDef>();
            this.prohibitedAttributes = new Dictionary<XmlQualifiedName, XmlQualifiedName>();
            base.Datatype = dtype;
            this.contentValidator = System.Xml.Schema.ContentValidator.TextOnly;
        }

        internal SchemaElementDecl(XmlQualifiedName name, string prefix) : base(name, prefix)
        {
            this.attdefs = new Dictionary<XmlQualifiedName, SchemaAttDef>();
            this.prohibitedAttributes = new Dictionary<XmlQualifiedName, XmlQualifiedName>();
        }

        internal void AddAttDef(SchemaAttDef attdef)
        {
            this.attdefs.Add(attdef.Name, attdef);
            if ((attdef.Presence == SchemaDeclBase.Use.Required) || (attdef.Presence == SchemaDeclBase.Use.RequiredFixed))
            {
                this.hasRequiredAttribute = true;
            }
            if ((attdef.Presence == SchemaDeclBase.Use.Default) || (attdef.Presence == SchemaDeclBase.Use.Fixed))
            {
                if (this.defaultAttdefs == null)
                {
                    this.defaultAttdefs = new List<IDtdDefaultAttributeInfo>();
                }
                this.defaultAttdefs.Add(attdef);
            }
        }

        internal void CheckAttributes(Hashtable presence, bool standalone)
        {
            foreach (SchemaAttDef def in this.attdefs.Values)
            {
                if (presence[def.Name] == null)
                {
                    if (def.Presence == SchemaDeclBase.Use.Required)
                    {
                        throw new XmlSchemaException("Sch_MissRequiredAttribute", def.Name.ToString());
                    }
                    if ((standalone && def.IsDeclaredInExternal) && ((def.Presence == SchemaDeclBase.Use.Default) || (def.Presence == SchemaDeclBase.Use.Fixed)))
                    {
                        throw new XmlSchemaException("Sch_StandAlone", string.Empty);
                    }
                }
            }
        }

        internal SchemaElementDecl Clone()
        {
            return (SchemaElementDecl) base.MemberwiseClone();
        }

        internal static SchemaElementDecl CreateAnyTypeElementDecl()
        {
            return new SchemaElementDecl { Datatype = DatatypeImplementation.AnySimpleType.Datatype };
        }

        internal SchemaAttDef GetAttDef(XmlQualifiedName qname)
        {
            SchemaAttDef def;
            if (this.attdefs.TryGetValue(qname, out def))
            {
                return def;
            }
            return null;
        }

        IDtdAttributeInfo IDtdAttributeListInfo.LookupAttribute(string prefix, string localName)
        {
            SchemaAttDef def;
            XmlQualifiedName key = new XmlQualifiedName(localName, prefix);
            if (this.attdefs.TryGetValue(key, out def))
            {
                return def;
            }
            return null;
        }

        IEnumerable<IDtdDefaultAttributeInfo> IDtdAttributeListInfo.LookupDefaultAttributes()
        {
            return this.defaultAttdefs;
        }

        IDtdAttributeInfo IDtdAttributeListInfo.LookupIdAttribute()
        {
            foreach (SchemaAttDef def in this.attdefs.Values)
            {
                if (def.TokenizedType == XmlTokenizedType.ID)
                {
                    return def;
                }
            }
            return null;
        }

        internal XmlSchemaAnyAttribute AnyAttribute
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

        internal Dictionary<XmlQualifiedName, SchemaAttDef> AttDefs
        {
            get
            {
                return this.attdefs;
            }
        }

        internal XmlSchemaDerivationMethod Block
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

        internal CompiledIdentityConstraint[] Constraints
        {
            get
            {
                return this.constraints;
            }
            set
            {
                this.constraints = value;
            }
        }

        internal System.Xml.Schema.ContentValidator ContentValidator
        {
            get
            {
                return this.contentValidator;
            }
            set
            {
                this.contentValidator = value;
            }
        }

        internal IList<IDtdDefaultAttributeInfo> DefaultAttDefs
        {
            get
            {
                return this.defaultAttdefs;
            }
        }

        internal bool HasDefaultAttribute
        {
            get
            {
                return (this.defaultAttdefs != null);
            }
        }

        internal bool HasNonCDataAttribute
        {
            get
            {
                return this.hasNonCDataAttribute;
            }
            set
            {
                this.hasNonCDataAttribute = value;
            }
        }

        internal bool HasRequiredAttribute
        {
            get
            {
                return this.hasRequiredAttribute;
            }
            set
            {
                this.hasRequiredAttribute = value;
            }
        }

        internal bool IsAbstract
        {
            get
            {
                return this.isAbstract;
            }
            set
            {
                this.isAbstract = value;
            }
        }

        internal bool IsIdDeclared
        {
            get
            {
                return this.isIdDeclared;
            }
            set
            {
                this.isIdDeclared = value;
            }
        }

        internal bool IsNillable
        {
            get
            {
                return this.isNillable;
            }
            set
            {
                this.isNillable = value;
            }
        }

        internal bool IsNotationDeclared
        {
            get
            {
                return this.isNotationDeclared;
            }
            set
            {
                this.isNotationDeclared = value;
            }
        }

        internal Dictionary<XmlQualifiedName, XmlQualifiedName> ProhibitedAttributes
        {
            get
            {
                return this.prohibitedAttributes;
            }
        }

        internal XmlSchemaElement SchemaElement
        {
            get
            {
                return this.schemaElement;
            }
            set
            {
                this.schemaElement = value;
            }
        }

        bool IDtdAttributeListInfo.HasNonCDataAttributes
        {
            get
            {
                return this.hasNonCDataAttribute;
            }
        }

        string IDtdAttributeListInfo.LocalName
        {
            get
            {
                return base.Name.Name;
            }
        }

        string IDtdAttributeListInfo.Prefix
        {
            get
            {
                return base.Prefix;
            }
        }
    }
}

