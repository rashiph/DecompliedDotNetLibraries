namespace System.ComponentModel.Design
{
    using System;
    using System.ComponentModel;

    public abstract class TypeDescriptionProviderService
    {
        protected TypeDescriptionProviderService()
        {
        }

        public abstract TypeDescriptionProvider GetProvider(object instance);
        public abstract TypeDescriptionProvider GetProvider(Type type);
    }
}

