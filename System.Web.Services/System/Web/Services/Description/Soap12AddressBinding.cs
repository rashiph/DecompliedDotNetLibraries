namespace System.Web.Services.Description
{
    using System.Web.Services.Configuration;

    [XmlFormatExtension("address", "http://schemas.xmlsoap.org/wsdl/soap12/", typeof(Port))]
    public sealed class Soap12AddressBinding : SoapAddressBinding
    {
    }
}

