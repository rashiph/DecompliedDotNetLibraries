namespace System.Web.Security
{
    using System;
    using System.Web;

    public sealed class RoleManagerEventArgs : EventArgs
    {
        private HttpContext _Context;
        private bool _RolesPopulated;

        public RoleManagerEventArgs(HttpContext context)
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

        public bool RolesPopulated
        {
            get
            {
                return this._RolesPopulated;
            }
            set
            {
                this._RolesPopulated = value;
            }
        }
    }
}

