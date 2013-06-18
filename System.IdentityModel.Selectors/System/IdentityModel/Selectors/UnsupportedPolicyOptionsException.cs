namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;

    [Serializable]
    public class UnsupportedPolicyOptionsException : Exception
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public UnsupportedPolicyOptionsException()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public UnsupportedPolicyOptionsException(string message) : base(message)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected UnsupportedPolicyOptionsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public UnsupportedPolicyOptionsException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

