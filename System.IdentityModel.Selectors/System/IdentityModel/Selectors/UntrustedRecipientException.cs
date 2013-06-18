namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;

    [Serializable]
    public class UntrustedRecipientException : Exception
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public UntrustedRecipientException()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public UntrustedRecipientException(string message) : base(message)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected UntrustedRecipientException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public UntrustedRecipientException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

