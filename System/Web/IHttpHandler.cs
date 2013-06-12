namespace System.Web
{
    using System;

    public interface IHttpHandler
    {
        void ProcessRequest(HttpContext context);

        bool IsReusable { get; }
    }
}

