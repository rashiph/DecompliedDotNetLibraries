namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    internal class InstanceHandleReference
    {
        internal InstanceHandleReference(System.Runtime.DurableInstancing.InstanceHandle instanceHandle)
        {
            this.InstanceHandle = instanceHandle;
        }

        internal void Cancel()
        {
            this.InstanceHandle = null;
        }

        internal System.Runtime.DurableInstancing.InstanceHandle InstanceHandle
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<InstanceHandle>k__BackingField;
            }
            [CompilerGenerated]
            private set
            {
                this.<InstanceHandle>k__BackingField = value;
            }
        }
    }
}

