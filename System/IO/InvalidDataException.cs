namespace System.IO
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class InvalidDataException : SystemException
    {
        public InvalidDataException() : base(SR.GetString("GenericInvalidData"))
        {
        }

        public InvalidDataException(string message) : base(message)
        {
        }

        internal InvalidDataException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InvalidDataException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

