namespace System.Runtime.Serialization
{
    using System;
    using System.Runtime;

    [Serializable]
    public class InvalidDataContractException : Exception
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InvalidDataContractException()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InvalidDataContractException(string message) : base(message)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected InvalidDataContractException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InvalidDataContractException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

