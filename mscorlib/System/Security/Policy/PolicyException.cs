namespace System.Security.Policy
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class PolicyException : SystemException
    {
        public PolicyException() : base(Environment.GetResourceString("Policy_Default"))
        {
            base.HResult = -2146233322;
        }

        public PolicyException(string message) : base(message)
        {
            base.HResult = -2146233322;
        }

        [SecuritySafeCritical]
        protected PolicyException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public PolicyException(string message, Exception exception) : base(message, exception)
        {
            base.HResult = -2146233322;
        }

        internal PolicyException(string message, int hresult) : base(message)
        {
            base.HResult = hresult;
        }

        internal PolicyException(string message, int hresult, Exception exception) : base(message, exception)
        {
            base.HResult = hresult;
        }
    }
}

