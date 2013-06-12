namespace System
{
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [Serializable, ComVisible(true)]
    public sealed class IndexOutOfRangeException : SystemException
    {
        public IndexOutOfRangeException() : base(Environment.GetResourceString("Arg_IndexOutOfRangeException"))
        {
            base.SetErrorCode(-2146233080);
        }

        public IndexOutOfRangeException(string message) : base(message)
        {
            base.SetErrorCode(-2146233080);
        }

        internal IndexOutOfRangeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public IndexOutOfRangeException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCode(-2146233080);
        }
    }
}

