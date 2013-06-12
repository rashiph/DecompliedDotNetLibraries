namespace System.Web
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class HttpUnhandledException : HttpException
    {
        public HttpUnhandledException()
        {
        }

        public HttpUnhandledException(string message) : base(message)
        {
        }

        private HttpUnhandledException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public HttpUnhandledException(string message, Exception innerException) : base(message, innerException)
        {
            base.SetFormatter(new UnhandledErrorFormatter(innerException, message, null));
        }

        internal HttpUnhandledException(string message, string postMessage, Exception innerException) : base(message, innerException)
        {
            base.SetFormatter(new UnhandledErrorFormatter(innerException, message, postMessage));
        }
    }
}

