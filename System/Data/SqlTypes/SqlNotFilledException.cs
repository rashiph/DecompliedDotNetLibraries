namespace System.Data.SqlTypes
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class SqlNotFilledException : SqlTypeException
    {
        public SqlNotFilledException() : this(SQLResource.NotFilledMessage, null)
        {
        }

        public SqlNotFilledException(string message) : this(message, null)
        {
        }

        private SqlNotFilledException(SerializationInfo si, StreamingContext sc) : base(si, sc)
        {
        }

        public SqlNotFilledException(string message, Exception e) : base(message, e)
        {
            base.HResult = -2146232015;
        }
    }
}

