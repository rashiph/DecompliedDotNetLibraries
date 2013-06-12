namespace System.Xml.Serialization
{
    using System;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Class, AllowMultiple=false)]
    public sealed class XmlSerializerAssemblyAttribute : Attribute
    {
        private string assemblyName;
        private string codeBase;

        public XmlSerializerAssemblyAttribute() : this(null, null)
        {
        }

        public XmlSerializerAssemblyAttribute(string assemblyName) : this(assemblyName, null)
        {
        }

        public XmlSerializerAssemblyAttribute(string assemblyName, string codeBase)
        {
            this.assemblyName = assemblyName;
            this.codeBase = codeBase;
        }

        public string AssemblyName
        {
            get
            {
                return this.assemblyName;
            }
            set
            {
                this.assemblyName = value;
            }
        }

        public string CodeBase
        {
            get
            {
                return this.codeBase;
            }
            set
            {
                this.codeBase = value;
            }
        }
    }
}

