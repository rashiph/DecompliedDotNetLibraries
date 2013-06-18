namespace System.Web
{
    using System;

    public interface IHttpHandlerFactory
    {
        IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated);
        void ReleaseHandler(IHttpHandler handler);
    }
}

