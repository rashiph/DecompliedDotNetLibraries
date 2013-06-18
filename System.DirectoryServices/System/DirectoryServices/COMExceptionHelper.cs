namespace System.DirectoryServices
{
    using System;
    using System.DirectoryServices.Interop;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class COMExceptionHelper
    {
        internal static Exception CreateFormattedComException(int hr)
        {
            string message = "";
            StringBuilder lpBuffer = new StringBuilder(0x100);
            int length = SafeNativeMethods.FormatMessageW(0x3200, 0, hr, 0, lpBuffer, lpBuffer.Capacity + 1, 0);
            if (length != 0)
            {
                message = lpBuffer.ToString(0, length);
            }
            else
            {
                message = Res.GetString("DSUnknown", new object[] { Convert.ToString(hr, 0x10) });
            }
            return CreateFormattedComException(new COMException(message, hr));
        }

        internal static Exception CreateFormattedComException(COMException e)
        {
            StringBuilder errorBuffer = new StringBuilder(0x100);
            StringBuilder nameBuffer = new StringBuilder();
            int error = 0;
            SafeNativeMethods.ADsGetLastError(out error, errorBuffer, 0x100, nameBuffer, 0);
            if (error != 0)
            {
                return new DirectoryServicesCOMException(errorBuffer.ToString(), error, e);
            }
            return e;
        }
    }
}

