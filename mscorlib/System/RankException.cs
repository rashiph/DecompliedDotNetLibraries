namespace System
{
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class RankException : SystemException
    {
        public RankException() : base(Environment.GetResourceString("Arg_RankException"))
        {
            base.SetErrorCode(-2146233065);
        }

        public RankException(string message) : base(message)
        {
            base.SetErrorCode(-2146233065);
        }

        [SecuritySafeCritical]
        protected RankException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public RankException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCode(-2146233065);
        }
    }
}

