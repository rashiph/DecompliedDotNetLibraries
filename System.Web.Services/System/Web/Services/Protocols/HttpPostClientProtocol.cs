namespace System.Web.Services.Protocols
{
    using System;
    using System.Net;

    public class HttpPostClientProtocol : HttpSimpleClientProtocol
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest webRequest = base.GetWebRequest(uri);
            webRequest.Method = "POST";
            return webRequest;
        }
    }
}

