namespace System.Runtime.InteropServices
{
    using System;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class SEHException : ExternalException
    {
        public SEHException()
        {
            base.SetErrorCode(-2147467259);
        }

        public SEHException(string message) : base(message)
        {
            base.SetErrorCode(-2147467259);
        }

        [SecuritySafeCritical]
        protected SEHException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public SEHException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2147467259);
        }

        public virtual bool CanResume()
        {
            return false;
        }
    }
}

