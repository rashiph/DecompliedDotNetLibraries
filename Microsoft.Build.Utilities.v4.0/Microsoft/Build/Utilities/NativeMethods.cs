namespace Microsoft.Build.Utilities
{
    using System;
    using System.Runtime.InteropServices;

    internal static class NativeMethods
    {
        private const int MAX_PATH = 260;

        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool GetFileAttributesEx(string name, int fileInfoLevel, ref WIN32_FILE_ATTRIBUTE_DATA lpFileInformation);
        internal static DateTime GetLastWriteTimeUtc(string fullPath)
        {
            DateTime minValue = DateTime.MinValue;
            WIN32_FILE_ATTRIBUTE_DATA lpFileInformation = new WIN32_FILE_ATTRIBUTE_DATA();
            if (GetFileAttributesEx(fullPath, 0, ref lpFileInformation))
            {
                long fileTime = (lpFileInformation.ftLastWriteTimeHigh << 0x20) | lpFileInformation.ftLastWriteTimeLow;
                minValue = DateTime.FromFileTimeUtc(fileTime);
            }
            return minValue;
        }
    }
}

