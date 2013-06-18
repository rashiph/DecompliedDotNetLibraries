namespace System.Configuration
{
    using Microsoft.Win32;
    using System;
    using System.Runtime.InteropServices;

    internal static class FileUtil
    {
        private const int HRESULT_WIN32_FILE_NOT_FOUND = -2147024894;
        private const int HRESULT_WIN32_PATH_NOT_FOUND = -2147024893;

        internal static bool FileExists(string filename, bool trueOnError)
        {
            Microsoft.Win32.UnsafeNativeMethods.WIN32_FILE_ATTRIBUTE_DATA win_file_attribute_data;
            if (Microsoft.Win32.UnsafeNativeMethods.GetFileAttributesEx(filename, 0, out win_file_attribute_data))
            {
                return ((win_file_attribute_data.fileAttributes & 0x10) != 0x10);
            }
            if (!trueOnError)
            {
                return false;
            }
            int num = Marshal.GetHRForLastWin32Error();
            return ((num != -2147024894) && (num != -2147024893));
        }
    }
}

