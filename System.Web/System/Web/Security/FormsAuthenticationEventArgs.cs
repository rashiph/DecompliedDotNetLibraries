namespace System.Web.Security
{
    using System;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Web;

    public sealed class FormsAuthenticationEventArgs : EventArgs
    {
        private HttpContext _Context;
        private IPrincipal _User;

        public FormsAuthenticationEventArgs(HttpContext context)
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

        public IPrincipal User
        {
            get
            {
                return this._User;
            }
            [SecurityPermission(SecurityAction.Demand, ControlPrincipal=true)]
            set
            {
                this._User = value;
            }
        }
    }
}

