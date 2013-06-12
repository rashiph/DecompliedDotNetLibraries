namespace System.IO.IsolatedStorage
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class IsolatedStorageException : Exception
    {
        public IsolatedStorageException() : base(Environment.GetResourceString("IsolatedStorage_Exception"))
        {
            base.SetErrorCode(-2146233264);
        }

        public IsolatedStorageException(string message) : base(message)
        {
            base.SetErrorCode(-2146233264);
        }

        [SecuritySafeCritical]
        protected IsolatedStorageException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public IsolatedStorageException(string message, Exception inner) : base(message, inner)
        {
            base.SetErrorCode(-2146233264);
        }
    }
}

