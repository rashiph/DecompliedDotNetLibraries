namespace System.Net
{
    using System;
    using System.Runtime.Serialization;

    internal class InternalException : SystemException
    {
        internal InternalException()
        {
        }

        internal InternalException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }
    }
}

