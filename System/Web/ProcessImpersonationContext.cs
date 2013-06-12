namespace System.Web
{
    using System;
    using System.Runtime.InteropServices;

    internal sealed class ProcessImpersonationContext : ImpersonationContext
    {
        internal ProcessImpersonationContext()
        {
            base.ImpersonateToken(new HandleRef(this, IntPtr.Zero));
        }
    }
}

