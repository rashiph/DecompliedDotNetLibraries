namespace System.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.Class, Inherited=true)]
    public sealed class TypeDescriptionProviderAttribute : Attribute
    {
        private string _typeName;

        public TypeDescriptionProviderAttribute(string typeName)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }
            this._typeName = typeName;
        }

        public TypeDescriptionProviderAttribute(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            this._typeName = type.AssemblyQualifiedName;
        }

        public string TypeName
        {
            get
            {
                return this._typeName;
            }
        }
    }
}

