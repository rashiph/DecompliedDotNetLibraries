namespace System.Activities
{
    using System;
    using System.Runtime.CompilerServices;

    public delegate void CompletionCallback<TResult>(NativeActivityContext context, System.Activities.ActivityInstance completedInstance, TResult result);
}

