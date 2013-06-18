namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    internal sealed class NonCorrelatedProvider : ICorrelationProvider
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal NonCorrelatedProvider()
        {
        }

        bool ICorrelationProvider.IsInitializingMember(Type interfaceType, string memberName, object[] methodArgs)
        {
            return true;
        }

        ICollection<CorrelationProperty> ICorrelationProvider.ResolveCorrelationPropertyValues(Type interfaceType, string methodName, object[] methodArgs, bool provideInitializerTokens)
        {
            return null;
        }
    }
}

