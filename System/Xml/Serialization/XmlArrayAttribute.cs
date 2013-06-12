namespace System.Xml.Serialization
{
    using System;
    using System.Xml;
    using System.Xml.Schema;

    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple=false)]
    public class XmlArrayAttribute : Attribute
    {
        private string elementName;
        private XmlSchemaForm form;
        private string ns;
        private bool nullable;
        private int order;

        public XmlArrayAttribute()
        {
            this.order = -1;
        }

        public XmlArrayAttribute(string elementName)
        {
            this.order = -1;
            this.elementName = elementName;
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
    }
}

