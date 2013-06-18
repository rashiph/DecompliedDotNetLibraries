namespace System.ServiceModel.Configuration
{
    using System;

    public class MexTcpBindingElement : MexBindingElement<CustomBinding>
    {
        public MexTcpBindingElement() : this(null)
        {
        }

        public MexTcpBindingElement(string name) : base(name)
        {
        }
    }
}

