namespace System.ServiceModel.Configuration
{
    using System;
    using System.ServiceModel.Channels;

    public abstract class MexBindingElement<TStandardBinding> : StandardBindingElement where TStandardBinding: Binding
    {
        protected MexBindingElement(string name) : base(name)
        {
        }

        protected override void OnApplyConfiguration(Binding binding)
        {
        }

        protected override System.Type BindingElementType
        {
            get
            {
                return typeof(TStandardBinding);
            }
        }
    }
}

