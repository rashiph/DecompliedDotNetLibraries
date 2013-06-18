namespace System.Web.Hosting
{
    using System;
    using System.Security.Principal;

    internal sealed class IIS7UserPrincipal : IPrincipal
    {
        private IIdentity _identity;
        private IIS7WorkerRequest _wr;

        internal IIS7UserPrincipal(IIS7WorkerRequest wr, IIdentity identity)
        {
            this._wr = wr;
            this._identity = identity;
        }

        public bool IsInRole(string role)
        {
            return this._wr.IsUserInRole(role);
        }

        public IIdentity Identity
        {
            get
            {
                return this._identity;
            }
        }
    }
}

