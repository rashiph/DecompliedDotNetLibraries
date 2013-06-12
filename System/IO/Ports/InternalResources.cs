namespace System.IO.Ports
{
    using Microsoft.Win32;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class InternalResources
    {
        internal static void EndOfFile()
        {
            throw new EndOfStreamException(SR.GetString("IO_EOF_ReadBeyondEOF"));
        }

        internal static void EndReadCalledTwice()
        {
            throw new ArgumentException(SR.GetString("InvalidOperation_EndReadCalledMultiple"));
        }

        internal static void EndWriteCalledTwice()
        {
            throw new ArgumentException(SR.GetString("InvalidOperation_EndWriteCalledMultiple"));
        }

        internal static void FileNotOpen()
        {
            throw new ObjectDisposedException(null, SR.GetString("Port_not_open"));
        }

        internal static string GetMessage(int errorCode)
        {
            StringBuilder lpBuffer = new StringBuilder(0x200);
            if (Microsoft.Win32.SafeNativeMethods.FormatMessage(0x3200, new HandleRef(null, IntPtr.Zero), errorCode, 0, lpBuffer, lpBuffer.Capacity, IntPtr.Zero) != 0)
            {
                return lpBuffer.ToString();
            }
            return SR.GetString("IO_UnknownError", new object[] { errorCode });
        }

        internal static int MakeHRFromErrorCode(int errorCode)
        {
            return (-2147024896 | errorCode);
        }

        internal static void WinIOError()
        {
            WinIOError(Marshal.GetLastWin32Error(), string.Empty);
        }

        internal static void WinIOError(string str)
        {
            WinIOError(Marshal.GetLastWin32Error(), str);
        }

        internal static void WinIOError(int errorCode, string str)
        {
            switch (errorCode)
            {
                case 2:
                case 3:
                    if (str.Length == 0)
                    {
                        throw new IOException(SR.GetString("IO_PortNotFound"));
                    }
                    throw new IOException(SR.GetString("IO_PortNotFoundFileName", new object[] { str }));

                case 5:
                    if (str.Length == 0)
                    {
                        throw new UnauthorizedAccessException(SR.GetString("UnauthorizedAccess_IODenied_NoPathName"));
                    }
                    throw new UnauthorizedAccessException(SR.GetString("UnauthorizedAccess_IODenied_Path", new object[] { str }));

                case 0x20:
                    if (str.Length == 0)
                    {
                        throw new IOException(SR.GetString("IO_SharingViolation_NoFileName"));
                    }
                    throw new IOException(SR.GetString("IO_SharingViolation_File", new object[] { str }));

                case 0xce:
                    throw new PathTooLongException(SR.GetString("IO_PathTooLong"));
            }
            throw new IOException(GetMessage(errorCode), MakeHRFromErrorCode(errorCode));
        }

        internal static void WrongAsyncResult()
        {
            throw new ArgumentException(SR.GetString("Arg_WrongAsyncResult"));
        }
    }
}

