namespace System.IdentityModel.Selectors
{
    using System;
    using System.Runtime;
    using System.Runtime.Serialization;

    [Serializable]
    public class ServiceNotStartedException : Exception
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ServiceNotStartedException()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ServiceNotStartedException(string message) : base(message)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ServiceNotStartedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public ServiceNotStartedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

