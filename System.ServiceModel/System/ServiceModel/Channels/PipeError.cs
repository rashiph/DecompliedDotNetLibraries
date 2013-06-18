namespace System.ServiceModel.Channels
{
    using System;
    using System.Globalization;
    using System.ServiceModel;
    using System.Text;

    internal static class PipeError
    {
        public static string GetErrorString(int error)
        {
            StringBuilder lpBuffer = new StringBuilder(0x200);
            if (UnsafeNativeMethods.FormatMessage(0x3200, IntPtr.Zero, error, CultureInfo.CurrentCulture.LCID, lpBuffer, lpBuffer.Capacity, IntPtr.Zero) != 0)
            {
                lpBuffer = lpBuffer.Replace("\n", "").Replace("\r", "");
                return System.ServiceModel.SR.GetString("PipeKnownWin32Error", new object[] { lpBuffer.ToString(), error.ToString(CultureInfo.InvariantCulture), Convert.ToString(error, 0x10) });
            }
            return System.ServiceModel.SR.GetString("PipeUnknownWin32Error", new object[] { error.ToString(CultureInfo.InvariantCulture), Convert.ToString(error, 0x10) });
        }
    }
}

