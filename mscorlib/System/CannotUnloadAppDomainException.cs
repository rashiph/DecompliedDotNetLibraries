namespace System
{
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class CannotUnloadAppDomainException : SystemException
    {
        public CannotUnloadAppDomainException() : base(Environment.GetResourceString("Arg_CannotUnloadAppDomainException"))
        {
            base.SetErrorCode(-2146234347);
        }

        public CannotUnloadAppDomainException(string message) : base(message)
        {
            base.SetErrorCode(-2146234347);
        }

        [SecuritySafeCritical]
        protected CannotUnloadAppDomainException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public CannotUnloadAppDomainException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCode(-2146234347);
        }
    }
}

