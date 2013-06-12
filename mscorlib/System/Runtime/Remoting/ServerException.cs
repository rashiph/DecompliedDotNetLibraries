namespace System.Runtime.Remoting
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [Serializable, ComVisible(true)]
    public class ServerException : SystemException
    {
        private static string _nullMessage = Environment.GetResourceString("Remoting_Default");

        public ServerException() : base(_nullMessage)
        {
            base.SetErrorCode(-2146233074);
        }

        public ServerException(string message) : base(message)
        {
            base.SetErrorCode(-2146233074);
        }

        internal ServerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ServerException(string message, Exception InnerException) : base(message, InnerException)
        {
            base.SetErrorCode(-2146233074);
        }
    }
}

