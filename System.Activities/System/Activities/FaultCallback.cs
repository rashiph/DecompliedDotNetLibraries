namespace System.Activities
{
    using System;
    using System.Runtime.CompilerServices;

    public delegate void FaultCallback(NativeActivityFaultContext faultContext, Exception propagatedException, System.Activities.ActivityInstance propagatedFrom);
}

