namespace System.Activities
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class WorkflowApplicationException : Exception
    {
        private Guid instanceId;
        private const string InstanceIdName = "instanceId";

        public WorkflowApplicationException() : base(System.Activities.SR.DefaultWorkflowApplicationExceptionMessage)
        {
        }

        public WorkflowApplicationException(string message) : base(message)
        {
        }

        protected WorkflowApplicationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            this.instanceId = (Guid) info.GetValue("instanceId", typeof(Guid));
        }

        public WorkflowApplicationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public WorkflowApplicationException(string message, Guid instanceId) : base(message)
        {
            this.instanceId = instanceId;
        }

        public WorkflowApplicationException(string message, Guid instanceId, Exception innerException) : base(message, innerException)
        {
            this.instanceId = instanceId;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("instanceId", this.instanceId);
        }

        public Guid InstanceId
        {
            get
            {
                return this.instanceId;
            }
        }
    }
}

