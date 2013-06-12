namespace System.Data
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class InvalidConstraintException : DataException
    {
        public InvalidConstraintException() : base(Res.GetString("DataSet_DefaultInvalidConstraintException"))
        {
            base.HResult = -2146232028;
        }

        public InvalidConstraintException(string s) : base(s)
        {
            base.HResult = -2146232028;
        }

        protected InvalidConstraintException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InvalidConstraintException(string message, Exception innerException) : base(message, innerException)
        {
            base.HResult = -2146232028;
        }
    }
}

