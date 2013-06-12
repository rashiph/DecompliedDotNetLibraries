namespace System.Runtime.Remoting
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class RemotingException : SystemException
    {
        private static string _nullMessage = Environment.GetResourceString("Remoting_Default");

        public RemotingException() : base(_nullMessage)
        {
            base.SetErrorCode(-2146233077);
        }

        public RemotingException(string message) : base(message)
        {
            base.SetErrorCode(-2146233077);
        }

        [SecuritySafeCritical]
        protected RemotingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public RemotingException(string message, Exception InnerException) : base(message, InnerException)
        {
            base.SetErrorCode(-2146233077);
        }
    }
}

