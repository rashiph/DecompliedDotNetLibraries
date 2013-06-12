namespace System.Web.Profile
{
    using System;
    using System.Web;

    public sealed class ProfileMigrateEventArgs : EventArgs
    {
        private string _AnonymousId;
        private HttpContext _Context;

        public ProfileMigrateEventArgs(HttpContext context, string anonymousId)
        {
            this._Context = context;
            this._AnonymousId = anonymousId;
        }

        public string AnonymousID
        {
            get
            {
                return this._AnonymousId;
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

