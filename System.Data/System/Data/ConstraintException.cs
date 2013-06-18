namespace System.Data
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class ConstraintException : DataException
    {
        public ConstraintException() : base(Res.GetString("DataSet_DefaultConstraintException"))
        {
            base.HResult = -2146232022;
        }

        public ConstraintException(string s) : base(s)
        {
            base.HResult = -2146232022;
        }

        protected ConstraintException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ConstraintException(string message, Exception innerException) : base(message, innerException)
        {
            base.HResult = -2146232022;
        }
    }
}

