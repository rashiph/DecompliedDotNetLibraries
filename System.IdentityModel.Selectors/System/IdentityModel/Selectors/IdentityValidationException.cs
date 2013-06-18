namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;

    [Serializable]
    public class IdentityValidationException : Exception
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public IdentityValidationException()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public IdentityValidationException(string message) : base(message)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected IdentityValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public IdentityValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

