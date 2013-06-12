namespace System.StubHelpers
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    internal static class InterfaceMarshaler
    {
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern void ClearNative(IntPtr pUnk);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern object ConvertToManaged(IntPtr pUnk, IntPtr itfMT, IntPtr classMT, int flags);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern IntPtr ConvertToNative(object objSrc, IntPtr itfMT, IntPtr classMT, int flags);
    }
}

