namespace System.Xml.Serialization
{
    using System;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method | AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple=true)]
    public class XmlIncludeAttribute : Attribute
    {
        private System.Type type;

        public XmlIncludeAttribute(System.Type type)
        {
            this.type = type;
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

