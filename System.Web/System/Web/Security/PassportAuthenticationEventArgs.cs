namespace System.Web.Security
{
    using System;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Web;

    [Obsolete("This type is obsolete. The Passport authentication product is no longer supported and has been superseded by Live ID.")]
    public sealed class PassportAuthenticationEventArgs : EventArgs
    {
        private HttpContext _Context;
        private PassportIdentity _Identity;
        private IPrincipal _User;

        public PassportAuthenticationEventArgs(PassportIdentity identity, HttpContext context)
        {
            this._Identity = identity;
            this._Context = context;
        }

        public HttpContext Context
        {
            get
            {
                return this._Context;
            }
        }

        public PassportIdentity Identity
        {
            get
            {
                return this._Identity;
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

