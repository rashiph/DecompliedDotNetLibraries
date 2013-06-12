namespace System.Web
{
    using System;

    public class SiteMapResolveEventArgs : EventArgs
    {
        private HttpContext _context;
        private SiteMapProvider _provider;

        public SiteMapResolveEventArgs(HttpContext context, SiteMapProvider provider)
        {
            this._context = context;
            this._provider = provider;
        }

        public HttpContext Context
        {
            get
            {
                return this._context;
            }
        }

        public SiteMapProvider Provider
        {
            get
            {
                return this._provider;
            }
        }
    }
}

