namespace System.Data
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class DuplicateNameException : DataException
    {
        public DuplicateNameException() : base(Res.GetString("DataSet_DefaultDuplicateNameException"))
        {
            base.HResult = -2146232030;
        }

        public DuplicateNameException(string s) : base(s)
        {
            base.HResult = -2146232030;
        }

        protected DuplicateNameException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public DuplicateNameException(string message, Exception innerException) : base(message, innerException)
        {
            base.HResult = -2146232030;
        }
    }
}

