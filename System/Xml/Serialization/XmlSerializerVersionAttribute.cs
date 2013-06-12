namespace System.Xml.Serialization
{
    using System;

    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class XmlSerializerVersionAttribute : Attribute
    {
        private string mvid;
        private string ns;
        private string serializerVersion;
        private System.Type type;

        public XmlSerializerVersionAttribute()
        {
        }

        public XmlSerializerVersionAttribute(System.Type type)
        {
            this.type = type;
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

        public string ParentAssemblyId
        {
            get
            {
                return this.mvid;
            }
            set
            {
                this.mvid = value;
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

        public string Version
        {
            get
            {
                return this.serializerVersion;
            }
            set
            {
                this.serializerVersion = value;
            }
        }
    }
}

