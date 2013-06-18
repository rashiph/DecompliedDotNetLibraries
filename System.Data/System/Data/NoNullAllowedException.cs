namespace System.Data
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class NoNullAllowedException : DataException
    {
        public NoNullAllowedException() : base(Res.GetString("DataSet_DefaultNoNullAllowedException"))
        {
            base.HResult = -2146232026;
        }

        public NoNullAllowedException(string s) : base(s)
        {
            base.HResult = -2146232026;
        }

        protected NoNullAllowedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public NoNullAllowedException(string message, Exception innerException) : base(message, innerException)
        {
            base.HResult = -2146232026;
        }
    }
}

