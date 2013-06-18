namespace System.ServiceModel.Configuration
{
    using System;
    using System.ServiceModel;

    public class WS2007HttpBindingElement : WSHttpBindingElement
    {
        public WS2007HttpBindingElement() : this(null)
        {
        }

        public WS2007HttpBindingElement(string name) : base(name)
        {
        }

        protected override Type BindingElementType
        {
            get
            {
                return typeof(WS2007HttpBinding);
            }
        }
    }
}

