namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    public sealed class SecurityCallContext
    {
        private ISecurityCallContext _ex;

        private SecurityCallContext()
        {
        }

        private SecurityCallContext(ISecurityCallContext ctx)
        {
            this._ex = ctx;
        }

        public bool IsCallerInRole(string role)
        {
            return this._ex.IsCallerInRole(role);
        }

        public bool IsUserInRole(string user, string role)
        {
            object pUser = user;
            return this._ex.IsUserInRole(ref pUser, role);
        }

        public SecurityCallers Callers
        {
            get
            {
                return new SecurityCallers((ISecurityCallersColl) this._ex.GetItem("Callers"));
            }
        }

        public static SecurityCallContext CurrentCall
        {
            get
            {
                SecurityCallContext context2;
                try
                {
                    ISecurityCallContext context;
                    Util.CoGetCallContext(Util.IID_ISecurityCallContext, out context);
                    context2 = new SecurityCallContext(context);
                }
                catch (InvalidCastException)
                {
                    throw new COMException(Resource.FormatString("Err_NoSecurityContext"), -2147467262);
                }
                return context2;
            }
        }

        public SecurityIdentity DirectCaller
        {
            get
            {
                return new SecurityIdentity((ISecurityIdentityColl) this._ex.GetItem("DirectCaller"));
            }
        }

        public bool IsSecurityEnabled
        {
            get
            {
                return this._ex.IsSecurityEnabled();
            }
        }

        public int MinAuthenticationLevel
        {
            get
            {
                return (int) this._ex.GetItem("MinAuthenticationLevel");
            }
        }

        public int NumCallers
        {
            get
            {
                return (int) this._ex.GetItem("NumCallers");
            }
        }

        public SecurityIdentity OriginalCaller
        {
            get
            {
                return new SecurityIdentity((ISecurityIdentityColl) this._ex.GetItem("OriginalCaller"));
            }
        }
    }
}

