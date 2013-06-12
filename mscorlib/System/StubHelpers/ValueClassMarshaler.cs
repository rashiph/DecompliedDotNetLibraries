namespace System.StubHelpers
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Security;

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    internal static class ValueClassMarshaler
    {
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern void ClearNative(IntPtr dst, IntPtr pMT);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern void ConvertToManaged(IntPtr dst, IntPtr src, IntPtr pMT);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical, ForceTokenStabilization]
        internal static extern void ConvertToNative(IntPtr dst, IntPtr src, IntPtr pMT, ref CleanupWorkList pCleanupWorkList);
    }
}

