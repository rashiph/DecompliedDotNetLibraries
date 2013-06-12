namespace System.Xml.Serialization
{
    using System;
    using System.Xml;
    using System.Xml.Schema;

    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple=true)]
    public class XmlElementAttribute : Attribute
    {
        private string dataType;
        private string elementName;
        private XmlSchemaForm form;
        private string ns;
        private bool nullable;
        private bool nullableSpecified;
        private int order;
        private System.Type type;

        public XmlElementAttribute()
        {
            this.order = -1;
        }

        public XmlElementAttribute(string elementName)
        {
            this.order = -1;
            this.elementName = elementName;
        }

        public XmlElementAttribute(System.Type type)
        {
            this.order = -1;
            this.type = type;
        }

        public XmlElementAttribute(string elementName, System.Type type)
        {
            this.order = -1;
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

        public int Order
        {
            get
            {
                return this.order;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(Res.GetString("XmlDisallowNegativeValues"), "Order");
                }
                this.order = value;
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

