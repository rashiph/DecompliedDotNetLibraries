namespace System
{
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class DuplicateWaitObjectException : ArgumentException
    {
        private static string _duplicateWaitObjectMessage;

        public DuplicateWaitObjectException() : base(DuplicateWaitObjectMessage)
        {
            base.SetErrorCode(-2146233047);
        }

        public DuplicateWaitObjectException(string parameterName) : base(DuplicateWaitObjectMessage, parameterName)
        {
            base.SetErrorCode(-2146233047);
        }

        [SecuritySafeCritical]
        protected DuplicateWaitObjectException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public DuplicateWaitObjectException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCode(-2146233047);
        }

        public DuplicateWaitObjectException(string parameterName, string message) : base(message, parameterName)
        {
            base.SetErrorCode(-2146233047);
        }

        private static string DuplicateWaitObjectMessage
        {
            get
            {
                if (_duplicateWaitObjectMessage == null)
                {
                    _duplicateWaitObjectMessage = Environment.GetResourceString("Arg_DuplicateWaitObjectException");
                }
                return _duplicateWaitObjectMessage;
            }
        }
    }
}

