namespace System.Workflow.Activities
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class EventDeliveryFailedException : SystemException
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public EventDeliveryFailedException()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public EventDeliveryFailedException(string message) : base(message)
        {
        }

        private EventDeliveryFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public EventDeliveryFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

