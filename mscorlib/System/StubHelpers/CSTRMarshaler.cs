namespace System.StubHelpers
{
    using Microsoft.Win32;
    using System;
    using System.Runtime;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    internal static class CSTRMarshaler
    {
        [SecurityCritical, ForceTokenStabilization]
        internal static void ClearNative(IntPtr pNative)
        {
            Win32Native.CoTaskMemFree(pNative);
        }

        [SecurityCritical, ForceTokenStabilization]
        internal static unsafe string ConvertToManaged(IntPtr cstr)
        {
            if (IntPtr.Zero == cstr)
            {
                return null;
            }
            return new string((sbyte*) cstr);
        }

        [ForceTokenStabilization, SecurityCritical]
        internal static unsafe IntPtr ConvertToNative(int flags, string strManaged, IntPtr pNativeBuffer)
        {
            int num;
            if (strManaged == null)
            {
                return IntPtr.Zero;
            }
            System.StubHelpers.StubHelpers.CheckStringLength(strManaged.Length);
            byte[] src = AnsiCharMarshaler.DoAnsiConversion(strManaged, 0 != (flags & 0xff), 0 != (flags >> 8), out num);
            byte* pDest = (byte*) pNativeBuffer;
            if (pDest == null)
            {
                pDest = (byte*) Marshal.AllocCoTaskMem(num + 2);
            }
            Buffer.memcpy(src, 0, pDest, 0, num);
            pDest[num] = 0;
            pDest[num + 1] = 0;
            return (IntPtr) pDest;
        }
    }
}

