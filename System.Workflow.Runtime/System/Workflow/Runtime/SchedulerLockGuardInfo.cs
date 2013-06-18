namespace System.Workflow.Runtime
{
    using System;
    using System.Runtime;

    internal sealed class SchedulerLockGuardInfo
    {
        private object eventInfo;
        private WorkflowEventInternal eventType;
        private object sender;

        internal SchedulerLockGuardInfo(object _sender, WorkflowEventInternal _eventType)
        {
            this.sender = _sender;
            this.eventType = _eventType;
            this.eventInfo = null;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal SchedulerLockGuardInfo(object _sender, WorkflowEventInternal _eventType, object _eventInfo) : this(_sender, _eventType)
        {
            this.eventInfo = _eventInfo;
        }

        internal object EventInfo
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.eventInfo;
            }
        }

        internal WorkflowEventInternal EventType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.eventType;
            }
        }

        internal object Sender
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.sender;
            }
        }
    }
}

