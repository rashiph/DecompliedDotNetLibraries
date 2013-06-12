namespace System.StubHelpers
{
    using System;
    using System.Runtime.ConstrainedExecution;

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    internal static class WSTRBufferMarshaler
    {
        internal static void ClearNative(IntPtr pNative)
        {
        }

        internal static string ConvertToManaged(IntPtr bstr)
        {
            return null;
        }

        internal static IntPtr ConvertToNative(string strManaged)
        {
            return IntPtr.Zero;
        }
    }
}

