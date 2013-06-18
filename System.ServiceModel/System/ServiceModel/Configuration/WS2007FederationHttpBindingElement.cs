namespace System.ServiceModel.Configuration
{
    using System;
    using System.ServiceModel;

    public class WS2007FederationHttpBindingElement : WSFederationHttpBindingElement
    {
        public WS2007FederationHttpBindingElement() : this(null)
        {
        }

        public WS2007FederationHttpBindingElement(string name) : base(name)
        {
        }

        protected override Type BindingElementType
        {
            get
            {
                return typeof(WS2007FederationHttpBinding);
            }
        }
    }
}

