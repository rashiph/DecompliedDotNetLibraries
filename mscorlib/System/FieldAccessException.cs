namespace System
{
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class FieldAccessException : MemberAccessException
    {
        public FieldAccessException() : base(Environment.GetResourceString("Arg_FieldAccessException"))
        {
            base.SetErrorCode(-2146233081);
        }

        public FieldAccessException(string message) : base(message)
        {
            base.SetErrorCode(-2146233081);
        }

        [SecuritySafeCritical]
        protected FieldAccessException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public FieldAccessException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2146233081);
        }
    }
}

