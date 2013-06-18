namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;

    internal interface ICorrelationProvider
    {
        bool IsInitializingMember(Type interfaceType, string memberName, object[] methodArgs);
        ICollection<CorrelationProperty> ResolveCorrelationPropertyValues(Type interfaceType, string memberName, object[] methodArgs, bool provideInitializerTokens);
    }
}

