namespace System.IO
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class PipeException : IOException
    {
        public PipeException()
        {
        }

        public PipeException(string message) : base(message)
        {
        }

        protected PipeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public PipeException(string message, Exception inner) : base(message, inner)
        {
        }

        public PipeException(string message, int errorCode) : base(message, errorCode)
        {
        }

        public virtual int ErrorCode
        {
            get
            {
                return base.HResult;
            }
        }
    }
}

