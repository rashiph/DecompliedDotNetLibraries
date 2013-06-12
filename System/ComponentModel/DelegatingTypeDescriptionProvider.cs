namespace System.ComponentModel
{
    using System;
    using System.Collections;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    internal sealed class DelegatingTypeDescriptionProvider : TypeDescriptionProvider
    {
        private Type _type;

        internal DelegatingTypeDescriptionProvider(Type type)
        {
            this._type = type;
        }

        public override object CreateInstance(IServiceProvider provider, Type objectType, Type[] argTypes, object[] args)
        {
            return this.Provider.CreateInstance(provider, objectType, argTypes, args);
        }

        public override IDictionary GetCache(object instance)
        {
            return this.Provider.GetCache(instance);
        }

        public override ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
        {
            return this.Provider.GetExtendedTypeDescriptor(instance);
        }

        protected internal override IExtenderProvider[] GetExtenderProviders(object instance)
        {
            return this.Provider.GetExtenderProviders(instance);
        }

        public override string GetFullComponentName(object component)
        {
            return this.Provider.GetFullComponentName(component);
        }

        public override Type GetReflectionType(Type objectType, object instance)
        {
            return this.Provider.GetReflectionType(objectType, instance);
        }

        public override Type GetRuntimeType(Type objectType)
        {
            return this.Provider.GetRuntimeType(objectType);
        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            return this.Provider.GetTypeDescriptor(objectType, instance);
        }

        public override bool IsSupportedType(Type type)
        {
            return this.Provider.IsSupportedType(type);
        }

        internal TypeDescriptionProvider Provider
        {
            get
            {
                return TypeDescriptor.GetProviderRecursive(this._type);
            }
        }
    }
}

