namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [SuppressUnmanagedCodeSecurity]
    internal static class SafeNativeMethods
    {
        public const string KERNEL32 = "kernel32.dll";

        [DllImport("kernel32.dll")]
        private static extern uint GetSystemTimeAdjustment(out int adjustment, out uint increment, out uint adjustmentDisabled);
        [DllImport("kernel32.dll", SetLastError=true)]
        public static extern void GetSystemTimeAsFileTime(out long time);
        [SecuritySafeCritical]
        internal static long GetSystemTimeResolution()
        {
            int num;
            uint num2;
            uint num3;
            if (GetSystemTimeAdjustment(out num, out num2, out num3) != 0)
            {
                return (long) num2;
            }
            return 0x249f0L;
        }
    }
}

