namespace System
{
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class AccessViolationException : SystemException
    {
        private int _accessType;
        private IntPtr _ip;
        private IntPtr _target;

        public AccessViolationException() : base(Environment.GetResourceString("Arg_AccessViolationException"))
        {
            base.SetErrorCode(-2147467261);
        }

        public AccessViolationException(string message) : base(message)
        {
            base.SetErrorCode(-2147467261);
        }

        [SecuritySafeCritical]
        protected AccessViolationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public AccessViolationException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCode(-2147467261);
        }
    }
}

