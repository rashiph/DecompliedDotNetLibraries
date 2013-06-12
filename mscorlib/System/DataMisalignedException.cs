namespace System
{
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [Serializable, ComVisible(true)]
    public sealed class DataMisalignedException : SystemException
    {
        public DataMisalignedException() : base(Environment.GetResourceString("Arg_DataMisalignedException"))
        {
            base.SetErrorCode(-2146233023);
        }

        public DataMisalignedException(string message) : base(message)
        {
            base.SetErrorCode(-2146233023);
        }

        internal DataMisalignedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public DataMisalignedException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCode(-2146233023);
        }
    }
}

