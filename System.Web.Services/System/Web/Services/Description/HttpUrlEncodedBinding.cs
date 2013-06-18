namespace System.Web.Services.Description
{
    using System.Web.Services.Configuration;

    [XmlFormatExtension("urlEncoded", "http://schemas.xmlsoap.org/wsdl/http/", typeof(InputBinding))]
    public sealed class HttpUrlEncodedBinding : ServiceDescriptionFormatExtension
    {
    }
}

