namespace System.Transactions
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal static class NativeMethods
    {
        [SuppressUnmanagedCodeSecurity, DllImport("Ole32")]
        internal static extern void CoGetContextToken(out IntPtr contextToken);
        [SuppressUnmanagedCodeSecurity, DllImport("Ole32")]
        internal static extern void CoGetDefaultContext(int aptType, ref Guid contextInterface, out SafeIUnknown safeUnknown);
    }
}

