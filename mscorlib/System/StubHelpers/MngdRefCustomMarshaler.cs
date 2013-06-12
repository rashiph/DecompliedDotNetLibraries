namespace System.StubHelpers
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    internal static class MngdRefCustomMarshaler
    {
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern void ClearManaged(IntPtr pMarshalState, ref object pManagedHome, IntPtr pNativeHome);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern void ClearNative(IntPtr pMarshalState, ref object pManagedHome, IntPtr pNativeHome);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern void ConvertContentsToManaged(IntPtr pMarshalState, ref object pManagedHome, IntPtr pNativeHome);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern void ConvertContentsToNative(IntPtr pMarshalState, ref object pManagedHome, IntPtr pNativeHome);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern void CreateMarshaler(IntPtr pMarshalState, IntPtr pCMHelper);
    }
}

