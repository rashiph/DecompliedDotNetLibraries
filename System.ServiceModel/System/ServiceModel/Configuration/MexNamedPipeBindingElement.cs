namespace System.ServiceModel.Configuration
{
    using System;

    public class MexNamedPipeBindingElement : MexBindingElement<CustomBinding>
    {
        public MexNamedPipeBindingElement() : this(null)
        {
        }

        public MexNamedPipeBindingElement(string name) : base(name)
        {
        }
    }
}

