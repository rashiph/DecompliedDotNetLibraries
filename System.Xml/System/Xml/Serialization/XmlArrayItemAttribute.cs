namespace System.Xml.Serialization
{
    using System;
    using System.Xml.Schema;

    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple=true)]
    public class XmlArrayItemAttribute : Attribute
    {
        private string dataType;
        private string elementName;
        private XmlSchemaForm form;
        private int nestingLevel;
        private string ns;
        private bool nullable;
        private bool nullableSpecified;
        private System.Type type;

        public XmlArrayItemAttribute()
        {
        }

        public XmlArrayItemAttribute(string elementName)
        {
            this.elementName = elementName;
        }

        public XmlArrayItemAttribute(System.Type type)
        {
            this.type = type;
        }

        public XmlArrayItemAttribute(string elementName, System.Type type)
        {
            this.elementName = elementName;
            this.type = type;
        }

        public string DataType
        {
            get
            {
                if (this.dataType != null)
                {
                    return this.dataType;
                }
                return string.Empty;
            }
            set
            {
                this.dataType = value;
            }
        }

        public string ElementName
        {
            get
            {
                if (this.elementName != null)
                {
                    return this.elementName;
                }
                return string.Empty;
            }
            set
            {
                this.elementName = value;
            }
        }

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

        public bool IsNullable
        {
            get
            {
                return this.nullable;
            }
            set
            {
                this.nullable = value;
                this.nullableSpecified = true;
            }
        }

        internal bool IsNullableSpecified
        {
            get
            {
                return this.nullableSpecified;
            }
        }

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

        public int NestingLevel
        {
            get
            {
                return this.nestingLevel;
            }
            set
            {
                this.nestingLevel = value;
            }
        }

        public System.Type Type
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
    }
}

