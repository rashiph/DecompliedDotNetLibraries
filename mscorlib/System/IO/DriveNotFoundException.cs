namespace System.IO
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class DriveNotFoundException : IOException
    {
        public DriveNotFoundException() : base(Environment.GetResourceString("Arg_DriveNotFoundException"))
        {
            base.SetErrorCode(-2147024893);
        }

        public DriveNotFoundException(string message) : base(message)
        {
            base.SetErrorCode(-2147024893);
        }

        [SecuritySafeCritical]
        protected DriveNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public DriveNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCode(-2147024893);
        }
    }
}

