namespace MS.Internal.Xaml.Parser
{
    using System;
    using System.Runtime.Serialization;
    using System.Xaml;

    [Serializable]
    internal class XamlUnexpectedParseException : XamlParseException
    {
        public XamlUnexpectedParseException()
        {
        }

        protected XamlUnexpectedParseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public XamlUnexpectedParseException(XamlScanner xamlScanner, ScannerNodeType nodetype, string parseRule) : base(xamlScanner, System.Xaml.SR.Get("UnexpectedNodeType", new object[] { nodetype.ToString(), parseRule }))
        {
        }
    }
}

