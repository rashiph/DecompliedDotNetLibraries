namespace System.Xml.Serialization
{
    using System;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Class)]
    public class XmlTypeAttribute : Attribute
    {
        private bool anonymousType;
        private bool includeInSchema;
        private string ns;
        private string typeName;

        public XmlTypeAttribute()
        {
            this.includeInSchema = true;
        }

        public XmlTypeAttribute(string typeName)
        {
            this.includeInSchema = true;
            this.typeName = typeName;
        }

        public bool AnonymousType
        {
            get
            {
                return this.anonymousType;
            }
            set
            {
                this.anonymousType = value;
            }
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

