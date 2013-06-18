namespace System.ServiceModel.Configuration
{
    using System;
    using System.ServiceModel.Description;

    public sealed class SynchronousReceiveElement : BehaviorExtensionElement
    {
        protected internal override object CreateBehavior()
        {
            return new SynchronousReceiveBehavior();
        }

        public override Type BehaviorType
        {
            get
            {
                return typeof(SynchronousReceiveBehavior);
            }
        }
    }
}

