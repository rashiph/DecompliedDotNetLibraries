namespace System.Xml.Serialization
{
    using System;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Class)]
    public class SoapTypeAttribute : Attribute
    {
        private bool includeInSchema;
        private string ns;
        private string typeName;

        public SoapTypeAttribute()
        {
            this.includeInSchema = true;
        }

        public SoapTypeAttribute(string typeName)
        {
            this.includeInSchema = true;
            this.typeName = typeName;
        }

        public SoapTypeAttribute(string typeName, string ns)
        {
            this.includeInSchema = true;
            this.typeName = typeName;
            this.ns = ns;
        }

        public bool IncludeInSchema
        {
            get
            {
                return this.includeInSchema;
            }
            set
            {
                this.includeInSchema = value;
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

        public string TypeName
        {
            get
            {
                if (this.typeName != null)
                {
                    return this.typeName;
                }
                return string.Empty;
            }
            set
            {
                this.typeName = value;
            }
        }
    }
}

