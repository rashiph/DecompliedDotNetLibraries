namespace System.Web.Security
{
    using System;
    using System.Web;

    public sealed class DefaultAuthenticationEventArgs : EventArgs
    {
        private HttpContext _Context;

        public DefaultAuthenticationEventArgs(HttpContext context)
        {
            this._Context = context;
        }

        public HttpContext Context
        {
            get
            {
                return this._Context;
            }
        }
    }
}

