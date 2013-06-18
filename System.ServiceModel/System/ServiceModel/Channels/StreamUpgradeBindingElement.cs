namespace System.ServiceModel.Channels
{
    using System;

    public abstract class StreamUpgradeBindingElement : BindingElement
    {
        protected StreamUpgradeBindingElement()
        {
        }

        protected StreamUpgradeBindingElement(StreamUpgradeBindingElement elementToBeCloned) : base(elementToBeCloned)
        {
        }

        public abstract StreamUpgradeProvider BuildClientStreamUpgradeProvider(BindingContext context);
        public abstract StreamUpgradeProvider BuildServerStreamUpgradeProvider(BindingContext context);
    }
}

