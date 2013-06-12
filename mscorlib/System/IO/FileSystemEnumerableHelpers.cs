namespace System.IO
{
    using Microsoft.Win32;
    using System;
    using System.Security;

    internal static class FileSystemEnumerableHelpers
    {
        [SecurityCritical]
        internal static bool IsDir(Win32Native.WIN32_FIND_DATA data)
        {
            return ((((data.dwFileAttributes & 0x10) != 0) && !data.cFileName.Equals(".")) && !data.cFileName.Equals(".."));
        }

        [SecurityCritical]
        internal static bool IsFile(Win32Native.WIN32_FIND_DATA data)
        {
            return (0 == (data.dwFileAttributes & 0x10));
        }
    }
}

