namespace System.Data
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class InRowChangingEventException : DataException
    {
        public InRowChangingEventException() : base(Res.GetString("DataSet_DefaultInRowChangingEventException"))
        {
            base.HResult = -2146232029;
        }

        public InRowChangingEventException(string s) : base(s)
        {
            base.HResult = -2146232029;
        }

        protected InRowChangingEventException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public InRowChangingEventException(string message, Exception innerException) : base(message, innerException)
        {
            base.HResult = -2146232029;
        }
    }
}

