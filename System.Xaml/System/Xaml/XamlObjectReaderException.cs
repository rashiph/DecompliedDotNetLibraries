namespace System.Xaml
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class XamlObjectReaderException : XamlException
    {
        public XamlObjectReaderException()
        {
        }

        public XamlObjectReaderException(string message) : base(message)
        {
        }

        protected XamlObjectReaderException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public XamlObjectReaderException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

