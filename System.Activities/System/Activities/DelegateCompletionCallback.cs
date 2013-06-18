namespace System.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public delegate void DelegateCompletionCallback(NativeActivityContext context, System.Activities.ActivityInstance completedInstance, IDictionary<string, object> outArguments);
}

