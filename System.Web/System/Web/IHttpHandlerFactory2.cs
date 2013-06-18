namespace System.Web
{
    using System;

    internal interface IHttpHandlerFactory2 : IHttpHandlerFactory
    {
        IHttpHandler GetHandler(HttpContext context, string requestType, VirtualPath virtualPath, string physicalPath);
    }
}

