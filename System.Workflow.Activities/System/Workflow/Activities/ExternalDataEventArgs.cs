namespace System.Workflow.Activities
{
    using System;
    using System.Runtime;
    using System.Workflow.Runtime;

    [Serializable]
    public class ExternalDataEventArgs : EventArgs
    {
        private IPendingWork batchworkHandler;
        private object batchworkItem;
        private string identity;
        private Guid instanceId;
        private bool waitForIdle;

        public ExternalDataEventArgs() : this(Guid.Empty, null, null, false)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ExternalDataEventArgs(Guid instanceId) : this(instanceId, null, null, false)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ExternalDataEventArgs(Guid instanceId, IPendingWork workHandler, object workItem) : this(instanceId, workHandler, workItem, false)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ExternalDataEventArgs(Guid instanceId, IPendingWork workHandler, object workItem, bool waitForIdle)
        {
            this.instanceId = instanceId;
            this.batchworkHandler = workHandler;
            this.batchworkItem = workItem;
            this.waitForIdle = waitForIdle;
        }

        public string Identity
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.identity;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.identity = value;
            }
        }

        public Guid InstanceId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.instanceId;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.instanceId = value;
            }
        }

        public bool WaitForIdle
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.waitForIdle;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.waitForIdle = value;
            }
        }

        public IPendingWork WorkHandler
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.batchworkHandler;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.batchworkHandler = value;
            }
        }

        public object WorkItem
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.batchworkItem;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.batchworkItem = value;
            }
        }
    }
}

