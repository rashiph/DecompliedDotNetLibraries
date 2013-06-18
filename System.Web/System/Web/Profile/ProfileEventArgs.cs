namespace System.Web.Profile
{
    using System;
    using System.Web;

    public sealed class ProfileEventArgs : EventArgs
    {
        private HttpContext _Context;
        private ProfileBase _Profile;

        public ProfileEventArgs(HttpContext context)
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

        public ProfileBase Profile
        {
            get
            {
                return this._Profile;
            }
            set
            {
                this._Profile = value;
            }
        }
    }
}

