namespace System.Workflow.Runtime
{
    using System;
    using System.Messaging;
    using System.Runtime;
    using System.Runtime.Serialization;

    [Serializable]
    internal class QueueException : InvalidOperationException
    {
        [NonSerialized]
        private MessageQueueErrorCode errorCode;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected QueueException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public QueueException(string message, MessageQueueErrorCode errorCode) : base(message)
        {
            this.errorCode = errorCode;
        }

        public MessageQueueErrorCode ErrorCode
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.errorCode;
            }
        }
    }
}

