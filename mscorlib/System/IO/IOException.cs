namespace System.IO
{
    using System;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class IOException : SystemException
    {
        [NonSerialized]
        private string _maybeFullPath;

        public IOException() : base(Environment.GetResourceString("Arg_IOException"))
        {
            base.SetErrorCode(-2146232800);
        }

        public IOException(string message) : base(message)
        {
            base.SetErrorCode(-2146232800);
        }

        [SecuritySafeCritical]
        protected IOException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public IOException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetErrorCode(-2146232800);
        }

        public IOException(string message, int hresult) : base(message)
        {
            base.SetErrorCode(hresult);
        }

        internal IOException(string message, int hresult, string maybeFullPath) : base(message)
        {
            base.SetErrorCode(hresult);
            this._maybeFullPath = maybeFullPath;
        }
    }
}

