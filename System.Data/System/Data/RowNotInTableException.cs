namespace System.Data
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class RowNotInTableException : DataException
    {
        public RowNotInTableException() : base(Res.GetString("DataSet_DefaultRowNotInTableException"))
        {
            base.HResult = -2146232024;
        }

        public RowNotInTableException(string s) : base(s)
        {
            base.HResult = -2146232024;
        }

        protected RowNotInTableException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public RowNotInTableException(string message, Exception innerException) : base(message, innerException)
        {
            base.HResult = -2146232024;
        }
    }
}

