namespace System.StubHelpers
{
    using Microsoft.Win32;
    using System;
    using System.Runtime;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    internal static class VBByValStrMarshaler
    {
        [ForceTokenStabilization, SecurityCritical]
        internal static void ClearNative(IntPtr pNative)
        {
            if (IntPtr.Zero != pNative)
            {
                Win32Native.CoTaskMemFree((IntPtr) (((long) pNative) - 4L));
            }
        }

        [ForceTokenStabilization, SecurityCritical]
        internal static unsafe string ConvertToManaged(IntPtr pNative, int cch)
        {
            if (IntPtr.Zero == pNative)
            {
                return null;
            }
            return new string((sbyte*) pNative, 0, cch);
        }

        [ForceTokenStabilization, SecurityCritical]
        internal static unsafe IntPtr ConvertToNative(string strManaged, bool fBestFit, bool fThrowOnUnmappableChar, ref int cch)
        {
            if (strManaged == null)
            {
                return IntPtr.Zero;
            }
            cch = strManaged.Length;
            System.StubHelpers.StubHelpers.CheckStringLength(cch);
            int cb = 4 + ((cch + 1) * Marshal.SystemMaxDBCSCharSize);
            byte* pDest = (byte*) Marshal.AllocCoTaskMem(cb);
            int* numPtr2 = (int*) pDest;
            pDest += 4;
            if (cch == 0)
            {
                pDest[0] = 0;
                numPtr2[0] = 0;
            }
            else
            {
                int num2;
                Buffer.memcpy(AnsiCharMarshaler.DoAnsiConversion(strManaged, fBestFit, fThrowOnUnmappableChar, out num2), 0, pDest, 0, num2);
                pDest[num2] = 0;
                numPtr2[0] = num2;
            }
            return new IntPtr((void*) pDest);
        }
    }
}

