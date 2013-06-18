namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class CorrelationInitializer
    {
        internal CorrelationInitializer()
        {
        }

        internal CorrelationInitializer Clone()
        {
            CorrelationInitializer initializer = this.CloneCore();
            if (this.CorrelationHandle != null)
            {
                initializer.CorrelationHandle = (InArgument<System.ServiceModel.Activities.CorrelationHandle>) InArgument.CreateReference(this.CorrelationHandle, this.ArgumentName);
            }
            return initializer;
        }

        internal abstract CorrelationInitializer CloneCore();

        internal string ArgumentName { get; set; }

        [DefaultValue((string) null)]
        public InArgument<System.ServiceModel.Activities.CorrelationHandle> CorrelationHandle { get; set; }
    }
}

