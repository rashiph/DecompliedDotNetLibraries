namespace System.Web.Services.Protocols
{
    using System;
    using System.Xml;

    public sealed class Soap12FaultCodes
    {
        public static readonly XmlQualifiedName DataEncodingUnknownFaultCode = new XmlQualifiedName("DataEncodingUnknown", "http://www.w3.org/2003/05/soap-envelope");
        public static readonly XmlQualifiedName EncodingMissingIdFaultCode = new XmlQualifiedName("MissingID", "http://www.w3.org/2003/05/soap-encoding");
        public static readonly XmlQualifiedName EncodingUntypedValueFaultCode = new XmlQualifiedName("UntypedValue", "http://www.w3.org/2003/05/soap-encoding");
        internal static readonly XmlQualifiedName MethodNotAllowed = new XmlQualifiedName("MethodNotAllowed", "http://microsoft.com/soap/");
        public static readonly XmlQualifiedName MustUnderstandFaultCode = new XmlQualifiedName("MustUnderstand", "http://www.w3.org/2003/05/soap-envelope");
        public static readonly XmlQualifiedName ReceiverFaultCode = new XmlQualifiedName("Receiver", "http://www.w3.org/2003/05/soap-envelope");
        public static readonly XmlQualifiedName RpcBadArgumentsFaultCode = new XmlQualifiedName("BadArguments", "http://www.w3.org/2003/05/soap-rpc");
        public static readonly XmlQualifiedName RpcProcedureNotPresentFaultCode = new XmlQualifiedName("ProcedureNotPresent", "http://www.w3.org/2003/05/soap-rpc");
        public static readonly XmlQualifiedName SenderFaultCode = new XmlQualifiedName("Sender", "http://www.w3.org/2003/05/soap-envelope");
        internal static readonly XmlQualifiedName UnsupportedMediaTypeFaultCode = new XmlQualifiedName("UnsupportedMediaType", "http://microsoft.com/soap/");
        public static readonly XmlQualifiedName VersionMismatchFaultCode = new XmlQualifiedName("VersionMismatch", "http://www.w3.org/2003/05/soap-envelope");

        private Soap12FaultCodes()
        {
        }
    }
}

