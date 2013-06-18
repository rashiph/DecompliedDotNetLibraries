namespace System.ServiceModel.Configuration
{
    using System;
    using System.ServiceModel.Channels;

    public class PnrpPeerResolverElement : BindingElementExtensionElement
    {
        protected internal override BindingElement CreateBindingElement()
        {
            return new PnrpPeerResolverBindingElement();
        }

        public override System.Type BindingElementType
        {
            get
            {
                return typeof(PnrpPeerResolverBindingElement);
            }
        }
    }
}

