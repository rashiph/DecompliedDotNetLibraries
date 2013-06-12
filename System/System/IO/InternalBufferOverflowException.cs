namespace System.IO
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class InternalBufferOverflowException : SystemException
    {
        public InternalBufferOverflowException()
        {
            base.HResult = -2146232059;
        }

        public InternalBufferOverflowException(string message) : base(message)
        {
            base.HResult = -2146232059;
        }

        protected InternalBufferOverflowException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InternalBufferOverflowException(string message, Exception inner) : base(message, inner)
        {
            base.HResult = -2146232059;
        }
    }
}

