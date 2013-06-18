namespace System.Xaml
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class XamlXmlWriterException : XamlException
    {
        public XamlXmlWriterException()
        {
        }

        public XamlXmlWriterException(string message) : base(message)
        {
        }

        protected XamlXmlWriterException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public XamlXmlWriterException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

