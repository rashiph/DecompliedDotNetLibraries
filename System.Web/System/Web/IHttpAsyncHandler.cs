namespace System.Web
{
    using System;

    public interface IHttpAsyncHandler : IHttpHandler
    {
        IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData);
        void EndProcessRequest(IAsyncResult result);
    }
}

