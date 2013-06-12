namespace Microsoft.Win32
{
    using System;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class SessionEndedEventArgs : EventArgs
    {
        private readonly SessionEndReasons reason;

        public SessionEndedEventArgs(SessionEndReasons reason)
        {
            this.reason = reason;
        }

        public SessionEndReasons Reason
        {
            get
            {
                return this.reason;
            }
        }
    }
}

