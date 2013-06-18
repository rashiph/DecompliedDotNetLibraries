namespace System.ServiceModel.Configuration
{
    using System;

    public sealed class ClearBehaviorElement : BehaviorExtensionElement
    {
        protected internal override object CreateBehavior()
        {
            return null;
        }

        public override Type BehaviorType
        {
            get
            {
                return null;
            }
        }
    }
}

