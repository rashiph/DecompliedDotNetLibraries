namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.ServiceModel.Channels;

    public abstract class MexBindingBindingCollectionElement<TStandardBinding, TBindingConfiguration> : StandardBindingCollectionElement<TStandardBinding, TBindingConfiguration> where TStandardBinding: Binding where TBindingConfiguration: StandardBindingElement, new()
    {
        protected MexBindingBindingCollectionElement()
        {
        }

        protected internal override bool TryAdd(string name, Binding binding, System.Configuration.Configuration config)
        {
            return false;
        }
    }
}

