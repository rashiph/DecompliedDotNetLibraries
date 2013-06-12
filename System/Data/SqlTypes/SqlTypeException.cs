namespace System.Data.SqlTypes
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class SqlTypeException : SystemException
    {
        public SqlTypeException() : this(Res.GetString("SqlMisc_SqlTypeMessage"), null)
        {
        }

        public SqlTypeException(string message) : this(message, null)
        {
        }

        protected SqlTypeException(SerializationInfo si, StreamingContext sc) : base(SqlTypeExceptionSerialization(si, sc), sc)
        {
        }

        public SqlTypeException(string message, Exception e) : base(message, e)
        {
            base.HResult = -2146232016;
        }

        private static SerializationInfo SqlTypeExceptionSerialization(SerializationInfo si, StreamingContext sc)
        {
            if ((si != null) && (1 == si.MemberCount))
            {
                new SqlTypeException(si.GetString("SqlTypeExceptionMessage")).GetObjectData(si, sc);
            }
            return si;
        }
    }
}

