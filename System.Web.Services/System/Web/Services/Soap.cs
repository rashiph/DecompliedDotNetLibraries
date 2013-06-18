namespace System.Web.Services
{
    using System;

    internal class Soap
    {
        internal const string Action = "SOAPAction";
        internal const string ArrayType = "Array";
        internal const string BasicProfile1_1 = "http://ws-i.org/profiles/basic/1.1";
        internal const string ClaimPrefix = "wsi";
        internal const string ConformanceClaim = "http://ws-i.org/schemas/conformanceClaim/";
        internal const string DimeContentType = "application/dime";
        internal const string Encoding = "http://schemas.xmlsoap.org/soap/encoding/";
        internal const string Namespace = "http://schemas.xmlsoap.org/soap/envelope/";
        internal const string Prefix = "soap";
        internal const string SoapContentType = "text/xml";
        internal const string XmlNamespace = "http://www.w3.org/XML/1998/namespace";

        private Soap()
        {
        }

        internal class Attribute
        {
            internal const string Actor = "actor";
            internal const string ConformsTo = "conformsTo";
            internal const string EncodingStyle = "encodingStyle";
            internal const string Lang = "lang";
            internal const string MustUnderstand = "mustUnderstand";

            private Attribute()
            {
            }
        }

        internal class Code
        {
            internal const string Client = "Client";
            internal const string MustUnderstand = "MustUnderstand";
            internal const string Server = "Server";
            internal const string VersionMismatch = "VersionMismatch";

            private Code()
            {
            }
        }

        internal class Element
        {
            internal const string Body = "Body";
            internal const string Claim = "Claim";
            internal const string Envelope = "Envelope";
            internal const string Fault = "Fault";
            internal const string FaultActor = "faultactor";
            internal const string FaultCode = "faultcode";
            internal const string FaultDetail = "detail";
            internal const string FaultString = "faultstring";
            internal const string Header = "Header";
            internal const string Message = "Message";
            internal const string StackTrace = "StackTrace";

            private Element()
            {
            }
        }
    }
}

