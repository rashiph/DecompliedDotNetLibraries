namespace System.Runtime.InteropServices
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class ExternalException : SystemException
    {
        public ExternalException() : base(Environment.GetResourceString("Arg_ExternalException"))
        {
            base.SetErrorCode(-2147467259);
        }

        public ExternalException(string message) : base(message)
        {
            base.SetErrorCode(-2147467259);
        }

        [SecuritySafeCritical]
        protected ExternalException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ExternalException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2147467259);
        }

        public ExternalException(string message, int errorCode) : base(message)
        {
            base.SetErrorCode(errorCode);
        }

        public override string ToString()
        {
            string message = this.Message;
            string str2 = base.GetType().ToString() + " (0x" + base.HResult.ToString("X8", CultureInfo.InvariantCulture) + ")";
            if (!string.IsNullOrEmpty(message))
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

        public virtual int ErrorCode
        {
            get
            {
                return base.HResult;
            }
        }
    }
}

