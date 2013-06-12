namespace System.StubHelpers
{
    using Microsoft.Win32;
    using System;
    using System.Runtime;
    using System.Runtime.ConstrainedExecution;
    using System.Security;

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    internal static class AnsiBSTRMarshaler
    {
        [SecurityCritical, ForceTokenStabilization]
        internal static void ClearNative(IntPtr pNative)
        {
            if (IntPtr.Zero != pNative)
            {
                Win32Native.SysFreeString(pNative);
            }
        }

        [SecurityCritical, ForceTokenStabilization]
        internal static unsafe string ConvertToManaged(IntPtr bstr)
        {
            if (IntPtr.Zero == bstr)
            {
                return null;
            }
            return new string((sbyte*) bstr);
        }

        [SecurityCritical, ForceTokenStabilization]
        internal static IntPtr ConvertToNative(int flags, string strManaged)
        {
            if (strManaged == null)
            {
                return IntPtr.Zero;
            }
            int length = strManaged.Length;
            System.StubHelpers.StubHelpers.CheckStringLength(length);
            byte[] str = null;
            int cbLength = 0;
            if (length > 0)
            {
                str = AnsiCharMarshaler.DoAnsiConversion(strManaged, 0 != (flags & 0xff), 0 != (flags >> 8), out cbLength);
            }
            return Win32Native.SysAllocStringByteLen(str, (uint) cbLength);
        }
    }
}

