namespace System.Threading
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [ComVisible(true), HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public sealed class AutoResetEvent : EventWaitHandle
    {
        [SecuritySafeCritical]
        public AutoResetEvent(bool initialState) : base(initialState, EventResetMode.AutoReset)
        {
        }
    }
}

