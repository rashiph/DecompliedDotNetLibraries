namespace System.Web.Util
{
    using System;
    using System.Web;

    internal static class SystemInfo
    {
        private static int _trueNumberOfProcessors;

        internal static int GetNumProcessCPUs()
        {
            if (_trueNumberOfProcessors == 0)
            {
                UnsafeNativeMethods.SYSTEM_INFO system_info;
                UnsafeNativeMethods.GetSystemInfo(out system_info);
                if (system_info.dwNumberOfProcessors == 1)
                {
                    _trueNumberOfProcessors = 1;
                }
                else
                {
                    IntPtr ptr2;
                    IntPtr ptr3;
                    if (UnsafeNativeMethods.GetProcessAffinityMask(UnsafeNativeMethods.INVALID_HANDLE_VALUE, out ptr2, out ptr3) == 0)
                    {
                        _trueNumberOfProcessors = 1;
                    }
                    else
                    {
                        int num2 = 0;
                        if (IntPtr.Size == 4)
                        {
                            for (uint i = (uint) ((int) ptr2); i != 0; i = i >> 1)
                            {
                                if ((i & 1) == 1)
                                {
                                    num2++;
                                }
                            }
                        }
                        else
                        {
                            for (ulong j = (ulong) ((long) ptr2); j != 0L; j = j >> 1)
                            {
                                if ((j & ((ulong) 1L)) == 1L)
                                {
                                    num2++;
                                }
                            }
                        }
                        _trueNumberOfProcessors = num2;
                    }
                }
            }
            return _trueNumberOfProcessors;
        }
    }
}

