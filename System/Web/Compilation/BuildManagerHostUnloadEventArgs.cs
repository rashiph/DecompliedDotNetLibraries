namespace System.Web.Compilation
{
    using System;
    using System.Security.Permissions;
    using System.Web;

    [PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true), PermissionSet(SecurityAction.LinkDemand, Unrestricted=true)]
    public class BuildManagerHostUnloadEventArgs : EventArgs
    {
        private ApplicationShutdownReason _reason;

        public BuildManagerHostUnloadEventArgs(ApplicationShutdownReason reason)
        {
            this._reason = reason;
        }

        public ApplicationShutdownReason Reason
        {
            get
            {
                return this._reason;
            }
        }
    }
}

