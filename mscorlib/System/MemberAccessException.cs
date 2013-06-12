namespace System
{
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class MemberAccessException : SystemException
    {
        public MemberAccessException() : base(Environment.GetResourceString("Arg_AccessException"))
        {
            base.SetErrorCode(-2146233062);
        }

        public MemberAccessException(string message) : base(message)
        {
            base.SetErrorCode(-2146233062);
        }

        [SecuritySafeCritical]
        protected MemberAccessException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public MemberAccessException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2146233062);
        }
    }
}

