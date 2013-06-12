namespace System.ComponentModel.Design.Serialization
{
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited=false)]
    public sealed class DefaultSerializationProviderAttribute : Attribute
    {
        private string _providerTypeName;

        public DefaultSerializationProviderAttribute(string providerTypeName)
        {
            if (providerTypeName == null)
            {
                throw new ArgumentNullException("providerTypeName");
            }
            this._providerTypeName = providerTypeName;
        }

        public DefaultSerializationProviderAttribute(Type providerType)
        {
            if (providerType == null)
            {
                throw new ArgumentNullException("providerType");
            }
            this._providerTypeName = providerType.AssemblyQualifiedName;
        }

        public string ProviderTypeName
        {
            get
            {
                return this._providerTypeName;
            }
        }
    }
}

