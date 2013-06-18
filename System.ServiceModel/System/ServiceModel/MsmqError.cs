namespace System.ServiceModel
{
    using System;
    using System.Globalization;
    using System.ServiceModel.Channels;
    using System.Text;

    internal static class MsmqError
    {
        public static string GetErrorString(int error)
        {
            StringBuilder lpBuffer = new StringBuilder(0x200);
            bool flag = false;
            if ((error & 0xfff0000) == 0xe0000)
            {
                int dwFlags = 0x2a00;
                flag = 0 != UnsafeNativeMethods.FormatMessage(dwFlags, Msmq.ErrorStrings, error, CultureInfo.CurrentCulture.LCID, lpBuffer, lpBuffer.Capacity, IntPtr.Zero);
            }
            else
            {
                int num2 = 0x3200;
                flag = 0 != UnsafeNativeMethods.FormatMessage(num2, IntPtr.Zero, error, CultureInfo.CurrentCulture.LCID, lpBuffer, lpBuffer.Capacity, IntPtr.Zero);
            }
            if (flag)
            {
                lpBuffer = lpBuffer.Replace("\n", "").Replace("\r", "");
                return System.ServiceModel.SR.GetString("MsmqKnownWin32Error", new object[] { lpBuffer.ToString(), error.ToString(CultureInfo.InvariantCulture), Convert.ToString(error, 0x10) });
            }
            return System.ServiceModel.SR.GetString("MsmqUnknownWin32Error", new object[] { error.ToString(CultureInfo.InvariantCulture), Convert.ToString(error, 0x10) });
        }
    }
}

