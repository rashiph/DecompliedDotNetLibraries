namespace System.ServiceModel.Configuration
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    public abstract class BindingElementExtensionElement : ServiceModelExtensionElement
    {
        protected BindingElementExtensionElement()
        {
        }

        public virtual void ApplyConfiguration(BindingElement bindingElement)
        {
            if (bindingElement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bindingElement");
            }
        }

        protected internal abstract BindingElement CreateBindingElement();
        protected internal virtual void InitializeFrom(BindingElement bindingElement)
        {
            if (bindingElement == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("bindingElement");
            }
            if (bindingElement.GetType() != this.BindingElementType)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("bindingElement", System.ServiceModel.SR.GetString("ConfigInvalidTypeForBindingElement", new object[] { this.BindingElementType.ToString(), bindingElement.GetType().ToString() }));
            }
        }

        public abstract System.Type BindingElementType { get; }
    }
}

