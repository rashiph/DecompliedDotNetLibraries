namespace System.Data.SqlClient
{
    using System;
    using System.Data.Common;

    internal sealed class InOutOfProcHelper
    {
        private bool _inProc;
        private static readonly InOutOfProcHelper SingletonInstance = new InOutOfProcHelper();

        private InOutOfProcHelper()
        {
            IntPtr moduleHandle = SafeNativeMethods.GetModuleHandle(null);
            if (IntPtr.Zero != moduleHandle)
            {
                if (IntPtr.Zero != SafeNativeMethods.GetProcAddress(moduleHandle, "_______SQL______Process______Available@0"))
                {
                    this._inProc = true;
                }
                else if (IntPtr.Zero != SafeNativeMethods.GetProcAddress(moduleHandle, "______SQL______Process______Available"))
                {
                    this._inProc = true;
                }
            }
        }

        internal static bool InProc
        {
            get
            {
                return SingletonInstance._inProc;
            }
        }
    }
}

