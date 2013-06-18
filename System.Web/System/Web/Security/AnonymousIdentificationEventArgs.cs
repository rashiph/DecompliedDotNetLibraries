namespace System.Web.Security
{
    using System;
    using System.Web;

    public sealed class AnonymousIdentificationEventArgs : EventArgs
    {
        private string _AnonymousId;
        private HttpContext _Context;

        public AnonymousIdentificationEventArgs(HttpContext context)
        {
            this._Context = context;
        }

        public string AnonymousID
        {
            get
            {
                return this._AnonymousId;
            }
            set
            {
                this._AnonymousId = value;
            }
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

