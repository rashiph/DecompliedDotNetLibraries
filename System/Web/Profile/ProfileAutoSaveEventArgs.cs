namespace System.Web.Profile
{
    using System;
    using System.Web;

    public sealed class ProfileAutoSaveEventArgs : EventArgs
    {
        private HttpContext _Context;
        private bool _ContinueSave = true;

        public ProfileAutoSaveEventArgs(HttpContext context)
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

        public bool ContinueWithProfileAutoSave
        {
            get
            {
                return this._ContinueSave;
            }
            set
            {
                this._ContinueSave = value;
            }
        }
    }
}

