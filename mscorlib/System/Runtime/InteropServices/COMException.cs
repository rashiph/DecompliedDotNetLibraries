namespace System.Runtime.InteropServices
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class COMException : ExternalException
    {
        public COMException() : base(Environment.GetResourceString("Arg_COMException"))
        {
            base.SetErrorCode(-2147467259);
        }

        public COMException(string message) : base(message)
        {
            base.SetErrorCode(-2147467259);
        }

        [SecuritySafeCritical]
        protected COMException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public COMException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2147467259);
        }

        public COMException(string message, int errorCode) : base(message)
        {
            base.SetErrorCode(errorCode);
        }

        [SecuritySafeCritical]
        public override string ToString()
        {
            string message = this.Message;
            string str2 = base.GetType().ToString() + " (0x" + base.HResult.ToString("X8", CultureInfo.InvariantCulture) + ")";
            if ((message != null) && (message.Length > 0))
            {
                str2 = str2 + ": " + message;
            }
            Exception innerException = base.InnerException;
            if (innerException != null)
            {
                str2 = str2 + " ---> " + innerException.ToString();
            }
            if (this.StackTrace != null)
            {
                str2 = str2 + Environment.NewLine + this.StackTrace;
            }
            return str2;
        }
    }
}

