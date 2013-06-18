namespace System.Web.Services.Protocols
{
    using System.Security.Permissions;
    using System.Web;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class SoapServerProtocolFactory : ServerProtocolFactory
    {
        protected override ServerProtocol CreateIfRequestCompatible(HttpRequest request)
        {
            if (request.PathInfo.Length > 0)
            {
                return null;
            }
            if (request.HttpMethod != "POST")
            {
                return new UnsupportedRequestProtocol(0x195);
            }
            return new SoapServerProtocol();
        }
    }
}

