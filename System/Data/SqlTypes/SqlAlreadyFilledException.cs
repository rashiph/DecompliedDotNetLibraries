namespace System.Data.SqlTypes
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class SqlAlreadyFilledException : SqlTypeException
    {
        public SqlAlreadyFilledException() : this(SQLResource.AlreadyFilledMessage, null)
        {
        }

        public SqlAlreadyFilledException(string message) : this(message, null)
        {
        }

        private SqlAlreadyFilledException(SerializationInfo si, StreamingContext sc) : base(si, sc)
        {
        }

        public SqlAlreadyFilledException(string message, Exception e) : base(message, e)
        {
            base.HResult = -2146232015;
        }
    }
}

