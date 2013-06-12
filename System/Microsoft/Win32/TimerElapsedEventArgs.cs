namespace Microsoft.Win32
{
    using System;
    using System.Security.Permissions;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public class TimerElapsedEventArgs : EventArgs
    {
        private readonly IntPtr timerId;

        public TimerElapsedEventArgs(IntPtr timerId)
        {
            this.timerId = timerId;
        }

        public IntPtr TimerId
        {
            get
            {
                return this.timerId;
            }
        }
    }
}

