namespace System
{
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class OutOfMemoryException : SystemException
    {
        public OutOfMemoryException() : base(Exception.GetMessageFromNativeResources(Exception.ExceptionMessageKind.OutOfMemory))
        {
            base.SetErrorCode(-2147024882);
        }

        public OutOfMemoryException(string message) : base(message)
        {
            base.SetErrorCode(-2147024882);
        }

        [SecuritySafeCritical]
        protected OutOfMemoryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public OutOfMemoryException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCode(-2147024882);
        }
    }
}

