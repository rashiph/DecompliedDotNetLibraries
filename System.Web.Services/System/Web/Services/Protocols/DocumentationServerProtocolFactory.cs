namespace System.Web.Services.Protocols
{
    using System.Web;

    internal class DocumentationServerProtocolFactory : ServerProtocolFactory
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
            return new DocumentationServerProtocol();
        }
    }
}

