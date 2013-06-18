namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class WorkflowValidationFailedException : Exception
    {
        private ValidationErrorCollection errors;

        public WorkflowValidationFailedException() : base(SR.GetString("Error_WorkflowLoadValidationFailed"))
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public WorkflowValidationFailedException(string message) : base(message)
        {
        }

        private WorkflowValidationFailedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            this.errors = (ValidationErrorCollection) info.GetValue("errors", typeof(ValidationErrorCollection));
            if (this.errors == null)
            {
                throw new SerializationException(SR.GetString("Error_SerializationInsufficientState"));
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public WorkflowValidationFailedException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public WorkflowValidationFailedException(string message, ValidationErrorCollection errors) : base(message)
        {
            if (errors == null)
            {
                throw new ArgumentNullException("errors");
            }
            this.errors = XomlCompilerHelper.MorphIntoFriendlyValidationErrors(errors);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("errors", this.errors, typeof(ValidationErrorCollection));
        }

        public ValidationErrorCollection Errors
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.errors;
            }
        }
    }
}

