namespace System.ServiceModel.Configuration
{
    using System;

    public class MexHttpsBindingElement : MexBindingElement<WSHttpBinding>
    {
        public MexHttpsBindingElement() : this(null)
        {
        }

        public MexHttpsBindingElement(string name) : base(name)
        {
        }
    }
}

