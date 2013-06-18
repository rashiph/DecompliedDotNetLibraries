namespace System.Xaml
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class XamlObjectWriterException : XamlException
    {
        public XamlObjectWriterException()
        {
        }

        public XamlObjectWriterException(string message) : base(message)
        {
        }

        protected XamlObjectWriterException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public XamlObjectWriterException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

