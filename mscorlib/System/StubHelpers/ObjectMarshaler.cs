namespace System.StubHelpers
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    internal static class ObjectMarshaler
    {
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern void ClearNative(IntPtr pVariant);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern object ConvertToManaged(IntPtr pSrcVariant);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern void ConvertToNative(object objSrc, IntPtr pDstVariant);
    }
}

