namespace System.Data
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class ReadOnlyException : DataException
    {
        public ReadOnlyException() : base(Res.GetString("DataSet_DefaultReadOnlyException"))
        {
            base.HResult = -2146232025;
        }

        public ReadOnlyException(string s) : base(s)
        {
            base.HResult = -2146232025;
        }

        protected ReadOnlyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ReadOnlyException(string message, Exception innerException) : base(message, innerException)
        {
            base.HResult = -2146232025;
        }
    }
}

