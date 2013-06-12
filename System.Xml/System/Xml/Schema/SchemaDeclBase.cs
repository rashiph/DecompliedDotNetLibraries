namespace System.Xml.Schema
{
    using System;
    using System.Collections.Generic;
    using System.Xml;

    internal abstract class SchemaDeclBase
    {
        protected XmlSchemaDatatype datatype;
        protected string defaultValueRaw;
        protected object defaultValueTyped;
        protected bool isDeclaredInExternal;
        protected long maxLength;
        protected long minLength;
        protected XmlQualifiedName name;
        protected string prefix;
        protected Use presence;
        protected XmlSchemaType schemaType;
        protected List<string> values;

        protected SchemaDeclBase()
        {
            this.name = XmlQualifiedName.Empty;
        }

        protected SchemaDeclBase(XmlQualifiedName name, string prefix)
        {
            this.name = XmlQualifiedName.Empty;
            this.name = name;
            this.prefix = prefix;
            this.maxLength = -1L;
            this.minLength = -1L;
        }

        internal void AddValue(string value)
        {
            if (this.values == null)
            {
                this.values = new List<string>();
            }
            this.values.Add(value);
        }

        internal bool CheckEnumeration(object pVal)
        {
            return (((this.datatype.TokenizedType != XmlTokenizedType.NOTATION) && (this.datatype.TokenizedType != XmlTokenizedType.ENUMERATION)) || this.values.Contains(pVal.ToString()));
        }

        internal bool CheckValue(object pVal)
        {
            return (((this.presence != Use.Fixed) && (this.presence != Use.RequiredFixed)) || ((this.defaultValueTyped != null) && this.datatype.IsEqual(pVal, this.defaultValueTyped)));
        }

        internal XmlSchemaDatatype Datatype
        {
            get
            {
                return this.datatype;
            }
            set
            {
                this.datatype = value;
            }
        }

        internal string DefaultValueRaw
        {
            get
            {
                if (this.defaultValueRaw == null)
                {
                    return string.Empty;
                }
                return this.defaultValueRaw;
            }
            set
            {
                this.defaultValueRaw = value;
            }
        }

        internal object DefaultValueTyped
        {
            get
            {
                return this.defaultValueTyped;
            }
            set
            {
                this.defaultValueTyped = value;
            }
        }

        internal bool IsDeclaredInExternal
        {
            get
            {
                return this.isDeclaredInExternal;
            }
            set
            {
                this.isDeclaredInExternal = value;
            }
        }

        internal long MaxLength
        {
            get
            {
                return this.maxLength;
            }
            set
            {
                this.maxLength = value;
            }
        }

        internal long MinLength
        {
            get
            {
                return this.minLength;
            }
            set
            {
                this.minLength = value;
            }
        }

        internal XmlQualifiedName Name
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

        internal string Prefix
        {
            get
            {
                if (this.prefix != null)
                {
                    return this.prefix;
                }
                return string.Empty;
            }
            set
            {
                this.prefix = value;
            }
        }

        internal Use Presence
        {
            get
            {
                return this.presence;
            }
            set
            {
                this.presence = value;
            }
        }

        internal XmlSchemaType SchemaType
        {
            get
            {
                return this.schemaType;
            }
            set
            {
                this.schemaType = value;
            }
        }

        internal List<string> Values
        {
            get
            {
                return this.values;
            }
            set
            {
                this.values = value;
            }
        }

        internal enum Use
        {
            Default,
            Required,
            Implied,
            Fixed,
            RequiredFixed
        }
    }
}

