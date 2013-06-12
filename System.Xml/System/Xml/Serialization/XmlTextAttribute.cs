namespace System.Xml.Serialization
{
    using System;

    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
    public class XmlTextAttribute : Attribute
    {
        private string dataType;
        private System.Type type;

        public XmlTextAttribute()
        {
        }

        public XmlTextAttribute(System.Type type)
        {
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

