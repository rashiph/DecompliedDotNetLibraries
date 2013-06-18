namespace System.Workflow.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;

    [Serializable]
    internal class TrackingCallingState
    {
        private IList<string> callerActivityPathProxy;
        private Guid callerContextGuid;
        private Guid callerInstanceId;
        private Guid callerParentContextGuid;

        internal IList<string> CallerActivityPathProxy
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.callerActivityPathProxy;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.callerActivityPathProxy = value;
            }
        }

        public Guid CallerContextGuid
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.callerContextGuid;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.callerContextGuid = value;
            }
        }

        public Guid CallerParentContextGuid
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.callerParentContextGuid;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.callerParentContextGuid = value;
            }
        }

        public Guid CallerWorkflowInstanceId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.callerInstanceId;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.callerInstanceId = value;
            }
        }
    }
}

