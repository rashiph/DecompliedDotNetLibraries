namespace System.Xml.Serialization
{
    using System;
    using System.Xml.Schema;

    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
    public class XmlAttributeAttribute : Attribute
    {
        private string attributeName;
        private string dataType;
        private XmlSchemaForm form;
        private string ns;
        private System.Type type;

        public XmlAttributeAttribute()
        {
        }

        public XmlAttributeAttribute(string attributeName)
        {
            this.attributeName = attributeName;
        }

        public XmlAttributeAttribute(System.Type type)
        {
            this.type = type;
        }

        public XmlAttributeAttribute(string attributeName, System.Type type)
        {
            this.attributeName = attributeName;
            this.type = type;
        }

        public string AttributeName
        {
            get
            {
                if (this.attributeName != null)
                {
                    return this.attributeName;
                }
                return string.Empty;
            }
            set
            {
                this.attributeName = value;
            }
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

