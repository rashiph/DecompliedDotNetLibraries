namespace Microsoft.Build.Tasks
{
    using System;

    internal enum UnificationReason
    {
        DidntUnify,
        FrameworkRetarget,
        BecauseOfBindingRedirect
    }
}

