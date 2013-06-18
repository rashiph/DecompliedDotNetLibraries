namespace System.Workflow.Runtime
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    [Serializable]
    public class WorkflowOwnershipException : Exception
    {
        private Guid _instanceId;

        public WorkflowOwnershipException() : base(ExecutionStringManager.WorkflowOwnershipException)
        {
        }

        public WorkflowOwnershipException(Guid instanceId) : base(ExecutionStringManager.WorkflowOwnershipException)
        {
            this.InstanceId = instanceId;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public WorkflowOwnershipException(string message) : base(message)
        {
        }

        public WorkflowOwnershipException(Guid instanceId, string message) : base(message)
        {
            this.InstanceId = instanceId;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        protected WorkflowOwnershipException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this._instanceId = (Guid) info.GetValue("__instanceId__", typeof(Guid));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public WorkflowOwnershipException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public WorkflowOwnershipException(Guid instanceId, string message, Exception innerException) : base(message, innerException)
        {
            this.InstanceId = instanceId;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("__instanceId__", this._instanceId);
        }

        public Guid InstanceId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._instanceId;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._instanceId = value;
            }
        }
    }
}

