namespace System.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public class AttributeProviderAttribute : Attribute
    {
        private string _propertyName;
        private string _typeName;

        public AttributeProviderAttribute(string typeName)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }
            this._typeName = typeName;
        }

        public AttributeProviderAttribute(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            this._typeName = type.AssemblyQualifiedName;
        }

        public AttributeProviderAttribute(string typeName, string propertyName)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }
            this._typeName = typeName;
            this._propertyName = propertyName;
        }

        public string PropertyName
        {
            get
            {
                return this._propertyName;
            }
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

