namespace System.Xml.Serialization
{
    using System;

    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
    public class SoapElementAttribute : Attribute
    {
        private string dataType;
        private string elementName;
        private bool nullable;

        public SoapElementAttribute()
        {
        }

        public SoapElementAttribute(string elementName)
        {
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
            }
        }
    }
}

