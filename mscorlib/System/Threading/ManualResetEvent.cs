namespace System.Threading
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [ComVisible(true), HostProtection(SecurityAction.LinkDemand, Synchronization=true, ExternalThreading=true)]
    public sealed class ManualResetEvent : EventWaitHandle
    {
        [SecuritySafeCritical]
        public ManualResetEvent(bool initialState) : base(initialState, EventResetMode.ManualReset)
        {
        }
    }
}

