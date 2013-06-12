namespace System
{
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class DllNotFoundException : TypeLoadException
    {
        public DllNotFoundException() : base(Environment.GetResourceString("Arg_DllNotFoundException"))
        {
            base.SetErrorCode(-2146233052);
        }

        public DllNotFoundException(string message) : base(message)
        {
            base.SetErrorCode(-2146233052);
        }

        [SecuritySafeCritical]
        protected DllNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public DllNotFoundException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2146233052);
        }
    }
}

