namespace System.Data.SqlTypes
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class SqlTruncateException : SqlTypeException
    {
        public SqlTruncateException() : this(SQLResource.TruncationMessage, null)
        {
        }

        public SqlTruncateException(string message) : this(message, null)
        {
        }

        private SqlTruncateException(SerializationInfo si, StreamingContext sc) : base(SqlTruncateExceptionSerialization(si, sc), sc)
        {
        }

        public SqlTruncateException(string message, Exception e) : base(message, e)
        {
            base.HResult = -2146232014;
        }

        private static SerializationInfo SqlTruncateExceptionSerialization(SerializationInfo si, StreamingContext sc)
        {
            if ((si != null) && (1 == si.MemberCount))
            {
                new SqlTruncateException(si.GetString("SqlTruncateExceptionMessage")).GetObjectData(si, sc);
            }
            return si;
        }
    }
}

