namespace Microsoft.Win32
{
    using System;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class SessionSwitchEventArgs : EventArgs
    {
        private readonly SessionSwitchReason reason;

        public SessionSwitchEventArgs(SessionSwitchReason reason)
        {
            this.reason = reason;
        }

        public SessionSwitchReason Reason
        {
            get
            {
                return this.reason;
            }
        }
    }
}

