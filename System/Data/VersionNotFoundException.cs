namespace System.Data
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class VersionNotFoundException : DataException
    {
        public VersionNotFoundException() : base(Res.GetString("DataSet_DefaultVersionNotFoundException"))
        {
            base.HResult = -2146232023;
        }

        public VersionNotFoundException(string s) : base(s)
        {
            base.HResult = -2146232023;
        }

        protected VersionNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public VersionNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
            base.HResult = -2146232023;
        }
    }
}

