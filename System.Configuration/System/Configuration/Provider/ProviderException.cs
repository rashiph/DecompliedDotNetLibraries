namespace System.Configuration.Provider
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;

    [Serializable]
    public class ProviderException : Exception
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ProviderException()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ProviderException(string message) : base(message)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ProviderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ProviderException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

