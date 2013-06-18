namespace System.Runtime
{
    using System;
    using System.Runtime.CompilerServices;

    internal static class WaitCallbackActionItem
    {
        internal static bool ShouldUseActivity
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return <ShouldUseActivity>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                <ShouldUseActivity>k__BackingField = value;
            }
        }
    }
}

