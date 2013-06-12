namespace System.Web.Configuration
{
    using System;
    using System.Web;

    internal class HandlerFactoryWrapper : IHttpHandlerFactory
    {
        private IHttpHandler _handler;
        private Type _handlerType;

        internal HandlerFactoryWrapper(IHttpHandler handler, Type handlerType)
        {
            this._handler = handler;
            this._handlerType = handlerType;
        }

        public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            if (this._handler == null)
            {
                this._handler = (IHttpHandler) HttpRuntime.CreateNonPublicInstance(this._handlerType);
            }
            return this._handler;
        }

        public void ReleaseHandler(IHttpHandler handler)
        {
            if (!this._handler.IsReusable)
            {
                this._handler = null;
            }
        }
    }
}

