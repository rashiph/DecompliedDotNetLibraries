namespace System.Workflow.ComponentModel
{
    using System;
    using System.Runtime;

    [Serializable]
    internal sealed class ActivityExecutionContextInfo
    {
        private int completedOrderId = -1;
        private Guid contextGuid = Guid.Empty;
        private int contextId = -1;
        private PersistFlags flags;
        private int parentContextId = -1;
        private string qualifiedID = string.Empty;

        internal ActivityExecutionContextInfo(string qualifiedName, int contextId, Guid contextGuid, int parentContextId)
        {
            this.qualifiedID = qualifiedName;
            this.contextId = contextId;
            this.contextGuid = contextGuid;
            this.parentContextId = parentContextId;
        }

        public override bool Equals(object obj)
        {
            ActivityExecutionContextInfo info = obj as ActivityExecutionContextInfo;
            return ((info != null) && this.ContextGuid.Equals(info.ContextGuid));
        }

        public override int GetHashCode()
        {
            return this.contextGuid.GetHashCode();
        }

        internal void SetCompletedOrderId(int completedOrderId)
        {
            this.completedOrderId = completedOrderId;
        }

        public string ActivityQualifiedName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.qualifiedID;
            }
        }

        public int CompletedOrderId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.completedOrderId;
            }
        }

        public Guid ContextGuid
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.contextGuid;
            }
        }

        internal int ContextId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.contextId;
            }
        }

        internal PersistFlags Flags
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.flags;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.flags = value;
            }
        }

        internal int ParentContextId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.parentContextId;
            }
        }
    }
}

