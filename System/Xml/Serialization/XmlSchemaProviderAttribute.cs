namespace System.Xml.Serialization
{
    using System;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Struct | AttributeTargets.Class)]
    public sealed class XmlSchemaProviderAttribute : Attribute
    {
        private bool any;
        private string methodName;

        public XmlSchemaProviderAttribute(string methodName)
        {
            this.methodName = methodName;
        }

        public bool IsAny
        {
            get
            {
                return this.any;
            }
            set
            {
                this.any = value;
            }
        }

        public string MethodName
        {
            get
            {
                return this.methodName;
            }
        }
    }
}

