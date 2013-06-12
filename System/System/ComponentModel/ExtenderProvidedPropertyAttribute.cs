namespace System.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.All)]
    public sealed class ExtenderProvidedPropertyAttribute : Attribute
    {
        private PropertyDescriptor extenderProperty;
        private IExtenderProvider provider;
        private Type receiverType;

        internal static ExtenderProvidedPropertyAttribute Create(PropertyDescriptor extenderProperty, Type receiverType, IExtenderProvider provider)
        {
            return new ExtenderProvidedPropertyAttribute { extenderProperty = extenderProperty, receiverType = receiverType, provider = provider };
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            ExtenderProvidedPropertyAttribute attribute = obj as ExtenderProvidedPropertyAttribute;
            return ((((attribute != null) && attribute.extenderProperty.Equals(this.extenderProperty)) && attribute.provider.Equals(this.provider)) && attribute.receiverType.Equals(this.receiverType));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return (this.receiverType == null);
        }

        public PropertyDescriptor ExtenderProperty
        {
            get
            {
                return this.extenderProperty;
            }
        }

        public IExtenderProvider Provider
        {
            get
            {
                return this.provider;
            }
        }

        public Type ReceiverType
        {
            get
            {
                return this.receiverType;
            }
        }
    }
}

