namespace System.Data
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class DeletedRowInaccessibleException : DataException
    {
        public DeletedRowInaccessibleException() : base(Res.GetString("DataSet_DefaultDeletedRowInaccessibleException"))
        {
            base.HResult = -2146232031;
        }

        public DeletedRowInaccessibleException(string s) : base(s)
        {
            base.HResult = -2146232031;
        }

        protected DeletedRowInaccessibleException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public DeletedRowInaccessibleException(string message, Exception innerException) : base(message, innerException)
        {
            base.HResult = -2146232031;
        }
    }
}

