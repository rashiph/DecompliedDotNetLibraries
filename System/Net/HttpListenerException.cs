namespace System.Net
{
    using System;
    using System.ComponentModel;
    using System.Runtime.Serialization;

    [Serializable]
    public class HttpListenerException : Win32Exception
    {
        public HttpListenerException() : base(Marshal.GetLastWin32Error())
        {
        }

        public HttpListenerException(int errorCode) : base(errorCode)
        {
        }

        public HttpListenerException(int errorCode, string message) : base(errorCode, message)
        {
        }

        protected HttpListenerException(SerializationInfo serializationInfo, StreamingContext streamingContext) : base(serializationInfo, streamingContext)
        {
        }

        public override int ErrorCode
        {
            get
            {
                return base.NativeErrorCode;
            }
        }
    }
}

