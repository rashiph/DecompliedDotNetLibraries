namespace System.Web.Configuration
{
    using System;
    using System.Web;

    internal class HandlerWithFactory
    {
        private IHttpHandlerFactory _factory;
        private IHttpHandler _handler;

        internal HandlerWithFactory(IHttpHandler handler, IHttpHandlerFactory factory)
        {
            this._handler = handler;
            this._factory = factory;
        }

        internal void Recycle()
        {
            this._factory.ReleaseHandler(this._handler);
        }
    }
}

