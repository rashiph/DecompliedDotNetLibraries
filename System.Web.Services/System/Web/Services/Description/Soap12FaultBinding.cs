namespace System.Web.Services.Description
{
    using System.Web.Services.Configuration;

    [XmlFormatExtension("fault", "http://schemas.xmlsoap.org/wsdl/soap12/", typeof(FaultBinding))]
    public sealed class Soap12FaultBinding : SoapFaultBinding
    {
    }
}

