namespace System.ServiceModel.Configuration
{
    using System;

    public class MexHttpBindingElement : MexBindingElement<WSHttpBinding>
    {
        public MexHttpBindingElement() : this(null)
        {
        }

        public MexHttpBindingElement(string name) : base(name)
        {
        }
    }
}

