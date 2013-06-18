namespace System.Data
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class DataException : SystemException
    {
        public DataException() : base(Res.GetString("DataSet_DefaultDataException"))
        {
            base.HResult = -2146232032;
        }

        public DataException(string s) : base(s)
        {
            base.HResult = -2146232032;
        }

        protected DataException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public DataException(string s, Exception innerException) : base(s, innerException)
        {
        }
    }
}

