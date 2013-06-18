namespace System.Data
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class MissingPrimaryKeyException : DataException
    {
        public MissingPrimaryKeyException() : base(Res.GetString("DataSet_DefaultMissingPrimaryKeyException"))
        {
            base.HResult = -2146232027;
        }

        public MissingPrimaryKeyException(string s) : base(s)
        {
            base.HResult = -2146232027;
        }

        protected MissingPrimaryKeyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public MissingPrimaryKeyException(string message, Exception innerException) : base(message, innerException)
        {
            base.HResult = -2146232027;
        }
    }
}

