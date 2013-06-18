namespace System.ServiceModel.Activation
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.ServiceModel.Channels;
    using System.Threading;

    internal static class ServiceMemoryGates
    {
        [SecurityCritical]
        internal static void Check(int minFreeMemoryPercentage)
        {
            if ((minFreeMemoryPercentage >= 1) && (minFreeMemoryPercentage <= 0x63))
            {
                UnsafeNativeMethods.MEMORYSTATUSEX memoryStatus = new UnsafeNativeMethods.MEMORYSTATUSEX();
                GetGlobalMemoryStatus(ref memoryStatus);
                ulong num = (memoryStatus.ullTotalPageFile / ((ulong) 100L)) * minFreeMemoryPercentage;
                if (memoryStatus.ullAvailPageFile < num)
                {
                    Thread.Sleep(0);
                    uint sizeToAlloc = 0;
                    if (num < 0x7fffffffL)
                    {
                        sizeToAlloc = (((uint) num) / 2) * 3;
                    }
                    if (sizeToAlloc > 0)
                    {
                        ForcePageFileToGrow(sizeToAlloc);
                        GetGlobalMemoryStatus(ref memoryStatus);
                        num = (memoryStatus.ullTotalPageFile / ((ulong) 100L)) * minFreeMemoryPercentage;
                    }
                    if (memoryStatus.ullAvailPageFile < num)
                    {
                        throw FxTrace.Exception.AsError(new InsufficientMemoryException(System.ServiceModel.Activation.SR.Hosting_MemoryGatesCheckFailed(memoryStatus.ullAvailPageFile, minFreeMemoryPercentage)));
                    }
                }
            }
        }

        [SecurityCritical]
        private static void ForcePageFileToGrow(uint sizeToAlloc)
        {
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                IntPtr lpAddress = UnsafeNativeMethods.VirtualAlloc(IntPtr.Zero, (UIntPtr) sizeToAlloc, 0x1000, 4);
                if (lpAddress != IntPtr.Zero)
                {
                    UnsafeNativeMethods.VirtualFree(lpAddress, (UIntPtr) sizeToAlloc, 0x4000);
                }
            }
        }

        [SecurityCritical]
        private static void GetGlobalMemoryStatus(ref UnsafeNativeMethods.MEMORYSTATUSEX memoryStatus)
        {
            memoryStatus.dwLength = (uint) Marshal.SizeOf(typeof(UnsafeNativeMethods.MEMORYSTATUSEX));
            if (!UnsafeNativeMethods.GlobalMemoryStatusEx(ref memoryStatus))
            {
                int error = Marshal.GetLastWin32Error();
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activation.SR.Hosting_GetGlobalMemoryFailed, new Win32Exception(error)));
            }
        }
    }
}

