namespace System.IO
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class DirectoryNotFoundException : IOException
    {
        public DirectoryNotFoundException() : base(Environment.GetResourceString("Arg_DirectoryNotFoundException"))
        {
            base.SetErrorCode(-2147024893);
        }

        public DirectoryNotFoundException(string message) : base(message)
        {
            base.SetErrorCode(-2147024893);
        }

        [SecuritySafeCritical]
        protected DirectoryNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public DirectoryNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCode(-2147024893);
        }
    }
}

