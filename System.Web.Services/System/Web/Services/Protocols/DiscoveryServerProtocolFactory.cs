namespace System.Web.Services.Protocols
{
    using System;
    using System.Web;

    internal class DiscoveryServerProtocolFactory : ServerProtocolFactory
    {
        protected override ServerProtocol CreateIfRequestCompatible(HttpRequest request)
        {
            if (request.PathInfo.Length > 0)
            {
                return null;
            }
            if (request.HttpMethod != "GET")
            {
                return new UnsupportedRequestProtocol(0x195);
            }
            string strA = request.QueryString[null];
            if (strA == null)
            {
                strA = "";
            }
            if (((request.QueryString["schema"] == null) && (request.QueryString["wsdl"] == null)) && ((string.Compare(strA, "wsdl", StringComparison.OrdinalIgnoreCase) != 0) && (string.Compare(strA, "disco", StringComparison.OrdinalIgnoreCase) != 0)))
            {
                return null;
            }
            return new DiscoveryServerProtocol();
        }
    }
}

