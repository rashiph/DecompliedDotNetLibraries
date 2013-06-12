namespace System.StubHelpers
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    internal static class DateMarshaler
    {
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern long ConvertToManaged(double nativeDate);
        [MethodImpl(MethodImplOptions.InternalCall), ForceTokenStabilization]
        internal static extern double ConvertToNative(DateTime managedDate);
    }
}

