namespace System.Data.Common
{
    using System;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Threading;

    [SuppressUnmanagedCodeSecurity]
    internal static class SafeNativeMethods
    {
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("ole32.dll")]
        internal static extern IntPtr CoTaskMemAlloc(IntPtr cb);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("ole32.dll")]
        internal static extern void CoTaskMemFree(IntPtr handle);
        [DllImport("kernel32.dll", EntryPoint="GetComputerNameExW", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern int GetComputerNameEx(int nameType, StringBuilder nameBuffer, ref int bufferSize);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        internal static extern int GetCurrentProcessId();
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        internal static extern IntPtr GetModuleHandle([In, MarshalAs(UnmanagedType.LPTStr)] string moduleName);
        [DllImport("kernel32.dll", CharSet=CharSet.Ansi, SetLastError=true)]
        internal static extern IntPtr GetProcAddress(IntPtr HModule, [In, MarshalAs(UnmanagedType.LPStr)] string funcName);
        [DllImport("kernel32.dll")]
        internal static extern void GetSystemTimeAsFileTime(out long lpSystemTimeAsFileTime);
        [DllImport("kernel32.dll", CharSet=CharSet.Unicode)]
        internal static extern int GetUserDefaultLCID();
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal static unsafe IntPtr InterlockedExchangePointer(IntPtr lpAddress, IntPtr lpValue)
        {
            IntPtr ptr2;
            IntPtr ptr = *((IntPtr*) lpAddress.ToPointer());
            do
            {
                ptr2 = ptr;
                ptr = Interlocked.CompareExchange(ref (IntPtr) ref lpAddress.ToPointer(), lpValue, ptr2);
            }
            while (ptr != ptr2);
            return ptr;
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("kernel32.dll", SetLastError=true)]
        internal static extern IntPtr LocalAlloc(int flags, IntPtr countOfBytes);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", SetLastError=true)]
        internal static extern IntPtr LocalFree(IntPtr handle);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("ole32.dll", PreserveSig=false)]
        internal static extern void PropVariantClear(IntPtr pObject);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("kernel32.dll", SetLastError=true)]
        internal static extern int ReleaseSemaphore(IntPtr handle, int releaseCount, IntPtr previousCount);
        [DllImport("oleaut32.dll", CharSet=CharSet.Unicode, PreserveSig=false)]
        private static extern void SetErrorInfo(int dwReserved, IntPtr pIErrorInfo);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("oleaut32.dll", CharSet=CharSet.Unicode)]
        internal static extern IntPtr SysAllocStringLen(string src, int len);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("oleaut32.dll")]
        internal static extern void SysFreeString(IntPtr bstr);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("oleaut32.dll", PreserveSig=false)]
        internal static extern void VariantClear(IntPtr pObject);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("kernel32.dll", SetLastError=true)]
        internal static extern int WaitForMultipleObjectsEx(uint nCount, IntPtr lpHandles, bool bWaitAll, uint dwMilliseconds, bool bAlertable);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail), DllImport("kernel32.dll")]
        internal static extern int WaitForSingleObjectEx(IntPtr lpHandles, uint dwMilliseconds, bool bAlertable);
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll")]
        internal static extern void ZeroMemory(IntPtr dest, IntPtr length);

        internal sealed class Wrapper
        {
            private Wrapper()
            {
            }

            internal static void ClearErrorInfo()
            {
                SafeNativeMethods.SetErrorInfo(0, ADP.PtrZero);
            }
        }
    }
}

