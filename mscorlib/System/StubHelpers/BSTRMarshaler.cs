namespace System.StubHelpers
{
    using Microsoft.Win32;
    using System;
    using System.Runtime;
    using System.Runtime.ConstrainedExecution;
    using System.Security;

    [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
    internal static class BSTRMarshaler
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
            uint length = Win32Native.SysStringByteLen(bstr);
            System.StubHelpers.StubHelpers.CheckStringLength(length);
            string str = new string((char*) bstr, 0, (int) (length / 2));
            if ((length & 1) == 1)
            {
                str.SetTrailByte(*((byte*) (bstr.ToPointer() + (length - 1))));
            }
            return str;
        }

        [ForceTokenStabilization, SecurityCritical]
        internal static unsafe IntPtr ConvertToNative(string strManaged, IntPtr pNativeBuffer)
        {
            byte num;
            byte* numPtr;
            if (strManaged == null)
            {
                return IntPtr.Zero;
            }
            System.StubHelpers.StubHelpers.CheckStringLength(strManaged.Length);
            bool flag = strManaged.TryGetTrailByte(out num);
            uint len = (uint) (strManaged.Length * 2);
            if (flag)
            {
                len++;
            }
            if (pNativeBuffer != IntPtr.Zero)
            {
                *((int*) pNativeBuffer.ToPointer()) = len;
                numPtr = (byte*) (pNativeBuffer.ToPointer() + 4);
            }
            else
            {
                numPtr = (byte*) Win32Native.SysAllocStringByteLen(null, len).ToPointer();
            }
            fixed (char* str = ((char*) strManaged))
            {
                char* chPtr = str;
                Buffer.memcpyimpl((byte*) chPtr, numPtr, (strManaged.Length + 1) * 2);
            }
            if (flag)
            {
                numPtr[len - 1] = num;
            }
            return (IntPtr) numPtr;
        }
    }
}

