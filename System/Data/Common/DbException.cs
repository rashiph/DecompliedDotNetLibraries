namespace System.Data.Common
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [Serializable]
    public abstract class DbException : ExternalException
    {
        protected DbException()
        {
        }

        protected DbException(string message) : base(message)
        {
        }

        protected DbException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        protected DbException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DbException(string message, int errorCode) : base(message, errorCode)
        {
        }
    }
}

