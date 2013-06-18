namespace System.Web.Services.Description
{
    using System;
    using System.Web.Services.Configuration;

    [XmlFormatExtension("binding", "http://schemas.xmlsoap.org/wsdl/soap12/", typeof(Binding)), XmlFormatExtensionPrefix("soap12", "http://schemas.xmlsoap.org/wsdl/soap12/")]
    public sealed class Soap12Binding : SoapBinding
    {
        public const string HttpTransport = "http://schemas.xmlsoap.org/soap/http";
        public const string Namespace = "http://schemas.xmlsoap.org/wsdl/soap12/";
    }
}

