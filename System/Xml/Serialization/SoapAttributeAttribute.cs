namespace System.Xml.Serialization
{
    using System;

    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
    public class SoapAttributeAttribute : Attribute
    {
        private string attributeName;
        private string dataType;
        private string ns;

        public SoapAttributeAttribute()
        {
        }

        public SoapAttributeAttribute(string attributeName)
        {
            this.attributeName = attributeName;
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

