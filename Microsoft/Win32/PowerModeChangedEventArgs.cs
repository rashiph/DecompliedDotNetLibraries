namespace Microsoft.Win32
{
    using System;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class PowerModeChangedEventArgs : EventArgs
    {
        private readonly PowerModes mode;

        public PowerModeChangedEventArgs(PowerModes mode)
        {
            this.mode = mode;
        }

        public PowerModes Mode
        {
            get
            {
                return this.mode;
            }
        }
    }
}

