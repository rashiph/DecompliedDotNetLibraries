namespace System.ServiceModel
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class InvalidMessageContractException : SystemException
    {
        public InvalidMessageContractException()
        {
        }

        public InvalidMessageContractException(string message) : base(message)
        {
        }

        protected InvalidMessageContractException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InvalidMessageContractException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

