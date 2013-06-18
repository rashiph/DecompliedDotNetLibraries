namespace System.Web.Services.Description
{
    using System.Web.Services.Configuration;

    [XmlFormatExtension("body", "http://schemas.xmlsoap.org/wsdl/soap12/", typeof(InputBinding), typeof(OutputBinding), typeof(MimePart))]
    public sealed class Soap12BodyBinding : SoapBodyBinding
    {
    }
}

