namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;

    [Serializable]
    public class RuleException : Exception, ISerializable
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RuleException()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RuleException(string message) : base(message)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected RuleException(SerializationInfo serializeInfo, StreamingContext context) : base(serializeInfo, context)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RuleException(string message, Exception ex) : base(message, ex)
        {
        }
    }
}

