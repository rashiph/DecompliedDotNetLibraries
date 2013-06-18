namespace Microsoft.Win32
{
    using System;
    using System.Configuration;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    [SuppressUnmanagedCodeSecurity]
    internal static class UnsafeNativeMethods
    {
        internal const int GetFileExInfoStandard = 0;
        internal const int MOVEFILE_REPLACE_EXISTING = 1;

        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern int CryptAcquireContext(out SafeCryptContextHandle phProv, string pszContainer, string pszProvider, uint dwProvType, uint dwFlags);
        [DllImport("crypt32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool CryptProtectData(ref DATA_BLOB inputData, string description, ref DATA_BLOB entropy, IntPtr pReserved, ref CRYPTPROTECT_PROMPTSTRUCT promptStruct, uint flags, ref DATA_BLOB outputData);
        [DllImport("advapi32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern int CryptReleaseContext(SafeCryptContextHandle hProv, uint dwFlags);
        [DllImport("crypt32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool CryptUnprotectData(ref DATA_BLOB inputData, IntPtr description, ref DATA_BLOB entropy, IntPtr pReserved, ref CRYPTPROTECT_PROMPTSTRUCT promptStruct, uint flags, ref DATA_BLOB outputData);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto, SetLastError=true)]
        internal static extern bool GetFileAttributesEx(string name, int fileInfoLevel, out WIN32_FILE_ATTRIBUTE_DATA data);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        internal static extern int GetModuleFileName(HandleRef hModule, StringBuilder buffer, int length);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        internal static extern IntPtr LocalFree(IntPtr buf);
        [DllImport("kernel32.dll", CharSet=CharSet.Auto)]
        internal static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, int dwFlags);

        [StructLayout(LayoutKind.Sequential)]
        internal struct WIN32_FILE_ATTRIBUTE_DATA
        {
            internal int fileAttributes;
            internal uint ftCreationTimeLow;
            internal uint ftCreationTimeHigh;
            internal uint ftLastAccessTimeLow;
            internal uint ftLastAccessTimeHigh;
            internal uint ftLastWriteTimeLow;
            internal uint ftLastWriteTimeHigh;
            internal uint fileSizeHigh;
            internal uint fileSizeLow;
        }
    }
}

