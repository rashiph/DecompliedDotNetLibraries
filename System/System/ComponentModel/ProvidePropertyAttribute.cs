namespace System.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
    public sealed class ProvidePropertyAttribute : Attribute
    {
        private readonly string propertyName;
        private readonly string receiverTypeName;

        public ProvidePropertyAttribute(string propertyName, string receiverTypeName)
        {
            this.propertyName = propertyName;
            this.receiverTypeName = receiverTypeName;
        }

        public ProvidePropertyAttribute(string propertyName, Type receiverType)
        {
            this.propertyName = propertyName;
            this.receiverTypeName = receiverType.AssemblyQualifiedName;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            ProvidePropertyAttribute attribute = obj as ProvidePropertyAttribute;
            return (((attribute != null) && (attribute.propertyName == this.propertyName)) && (attribute.receiverTypeName == this.receiverTypeName));
        }

        public override int GetHashCode()
        {
            return (this.propertyName.GetHashCode() ^ this.receiverTypeName.GetHashCode());
        }

        public string PropertyName
        {
            get
            {
                return this.propertyName;
            }
        }

        public string ReceiverTypeName
        {
            get
            {
                return this.receiverTypeName;
            }
        }

        public override object TypeId
        {
            get
            {
                return (base.GetType().FullName + this.propertyName);
            }
        }
    }
}

