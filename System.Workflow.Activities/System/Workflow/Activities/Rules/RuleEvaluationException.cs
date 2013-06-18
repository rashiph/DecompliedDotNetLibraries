namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;

    [Serializable]
    public class RuleEvaluationException : RuleException, ISerializable
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RuleEvaluationException()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RuleEvaluationException(string message) : base(message)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected RuleEvaluationException(SerializationInfo serializeInfo, StreamingContext context) : base(serializeInfo, context)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RuleEvaluationException(string message, Exception ex) : base(message, ex)
        {
        }
    }
}

