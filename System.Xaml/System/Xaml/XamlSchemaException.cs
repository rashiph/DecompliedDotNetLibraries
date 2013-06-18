namespace System.Xaml
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class XamlSchemaException : XamlException
    {
        public XamlSchemaException()
        {
        }

        public XamlSchemaException(string message) : base(message, null)
        {
        }

        protected XamlSchemaException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public XamlSchemaException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}

