namespace System.Web.Services.Description
{
    using System.Web.Services.Configuration;

    [XmlFormatExtension("urlReplacement", "http://schemas.xmlsoap.org/wsdl/http/", typeof(InputBinding))]
    public sealed class HttpUrlReplacementBinding : ServiceDescriptionFormatExtension
    {
    }
}

