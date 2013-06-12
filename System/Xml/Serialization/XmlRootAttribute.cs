namespace System.Xml.Serialization
{
    using System;

    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Interface | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Class)]
    public class XmlRootAttribute : Attribute
    {
        private string dataType;
        private string elementName;
        private string ns;
        private bool nullable;
        private bool nullableSpecified;

        public XmlRootAttribute()
        {
            this.nullable = true;
        }

        public XmlRootAttribute(string elementName)
        {
            this.nullable = true;
            this.elementName = elementName;
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

        internal string Key
        {
            get
            {
                return (((this.ns == null) ? string.Empty : this.ns) + ":" + this.ElementName + ":" + this.nullable.ToString());
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
    }
}

