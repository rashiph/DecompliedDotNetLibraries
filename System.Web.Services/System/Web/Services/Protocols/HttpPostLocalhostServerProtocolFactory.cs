namespace System.Web.Services.Protocols
{
    using System.Web;

    internal class HttpPostLocalhostServerProtocolFactory : ServerProtocolFactory
    {
        protected override ServerProtocol CreateIfRequestCompatible(HttpRequest request)
        {
            if (request.PathInfo.Length < 2)
            {
                return null;
            }
            if (request.HttpMethod != "POST")
            {
                return new UnsupportedRequestProtocol(0x195);
            }
            if (!(request.Url.IsLoopback || request.IsLocal))
            {
                return null;
            }
            return new HttpPostServerProtocol();
        }
    }
}

