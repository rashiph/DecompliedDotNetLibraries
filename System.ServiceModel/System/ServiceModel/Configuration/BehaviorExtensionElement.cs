namespace System.ServiceModel.Configuration
{
    using System;

    public abstract class BehaviorExtensionElement : ServiceModelExtensionElement
    {
        protected BehaviorExtensionElement()
        {
        }

        protected internal abstract object CreateBehavior();

        public abstract Type BehaviorType { get; }
    }
}

