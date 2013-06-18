namespace System.Workflow.Activities.Rules
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Workflow.ComponentModel.Compiler;

    [Serializable]
    public class RuleSetValidationException : RuleException, ISerializable
    {
        private ValidationErrorCollection m_errors;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RuleSetValidationException()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RuleSetValidationException(string message) : base(message)
        {
        }

        protected RuleSetValidationException(SerializationInfo serializeInfo, StreamingContext context) : base(serializeInfo, context)
        {
            if (serializeInfo == null)
            {
                throw new ArgumentNullException("serializeInfo");
            }
            this.m_errors = (ValidationErrorCollection) serializeInfo.GetValue("errors", typeof(ValidationErrorCollection));
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RuleSetValidationException(string message, Exception ex) : base(message, ex)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RuleSetValidationException(string message, ValidationErrorCollection errors) : base(message)
        {
            this.m_errors = errors;
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("errors", this.m_errors);
        }

        public ValidationErrorCollection Errors
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_errors;
            }
        }
    }
}

