namespace System.Xaml
{
    using MS.Internal.Xaml.Parser;
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class XamlParseException : XamlException
    {
        public XamlParseException()
        {
        }

        public XamlParseException(string message) : base(message)
        {
        }

        internal XamlParseException(MeScanner meScanner, string message) : base(message, null, meScanner.LineNumber, meScanner.LinePosition)
        {
        }

        internal XamlParseException(XamlScanner xamlScanner, string message) : base(message, null, xamlScanner.LineNumber, xamlScanner.LinePosition)
        {
        }

        protected XamlParseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public XamlParseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        internal XamlParseException(int lineNumber, int linePosition, string message) : base(message, null, lineNumber, linePosition)
        {
        }
    }
}

