namespace System.EnterpriseServices
{
    using System;

    public sealed class SecurityIdentity
    {
        private ISecurityIdentityColl _ex;

        private SecurityIdentity()
        {
        }

        internal SecurityIdentity(ISecurityIdentityColl ifc)
        {
            this._ex = ifc;
        }

        public string AccountName
        {
            get
            {
                return (string) this._ex.GetItem("AccountName");
            }
        }

        public AuthenticationOption AuthenticationLevel
        {
            get
            {
                return (AuthenticationOption) this._ex.GetItem("AuthenticationLevel");
            }
        }

        public int AuthenticationService
        {
            get
            {
                return (int) this._ex.GetItem("AuthenticationService");
            }
        }

        public ImpersonationLevelOption ImpersonationLevel
        {
            get
            {
                return (ImpersonationLevelOption) this._ex.GetItem("ImpersonationLevel");
            }
        }
    }
}

