namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    internal class InstanceNormalEvent : InstancePersistenceEvent
    {
        private HashSet<InstanceHandle> boundHandles;
        private HashSet<InstanceHandle> pendingHandles;

        internal InstanceNormalEvent(InstancePersistenceEvent persistenceEvent) : base(persistenceEvent.Name)
        {
            this.boundHandles = new HashSet<InstanceHandle>();
            this.pendingHandles = new HashSet<InstanceHandle>();
        }

        internal HashSet<InstanceHandle> BoundHandles
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.boundHandles;
            }
        }

        internal bool IsSignaled
        {
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.<IsSignaled>k__BackingField;
            }
            [CompilerGenerated, TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.<IsSignaled>k__BackingField = value;
            }
        }

        internal HashSet<InstanceHandle> PendingHandles
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.pendingHandles;
            }
        }
    }
}

