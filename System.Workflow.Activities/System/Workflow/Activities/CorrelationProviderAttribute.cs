namespace System.Workflow.Activities
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
    internal sealed class CorrelationProviderAttribute : Attribute
    {
        private Type correlationProviderType;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal CorrelationProviderAttribute(Type correlationProviderType)
        {
            this.correlationProviderType = correlationProviderType;
        }

        internal Type CorrelationProviderType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.correlationProviderType;
            }
        }
    }
}

