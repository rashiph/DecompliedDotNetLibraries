namespace System.Web
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class HttpRequestValidationException : HttpException
    {
        public HttpRequestValidationException()
        {
        }

        public HttpRequestValidationException(string message) : base(message)
        {
            base.SetFormatter(new UnhandledErrorFormatter(this, System.Web.SR.GetString("Dangerous_input_detected_descr"), null));
        }

        private HttpRequestValidationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public HttpRequestValidationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

