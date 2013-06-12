namespace System.Data.SqlTypes
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    [SuppressUnmanagedCodeSecurity]
    internal static class UnsafeNativeMethods
    {
        internal const int ERROR_INVALID_HANDLE = 6;
        internal const int ERROR_MR_MID_NOT_FOUND = 0x13d;
        internal const ushort FILE_DEVICE_FILE_SYSTEM = 9;
        internal const int FILE_READ_ATTRIBUTES = 0x80;
        internal const int FILE_READ_DATA = 1;
        internal const int FILE_WRITE_DATA = 2;
        internal const uint SEM_FAILCRITICALERRORS = 1;
        internal const uint STATUS_INVALID_PARAMETER = 0xc000000d;
        internal const uint STATUS_OBJECT_NAME_NOT_FOUND = 0xc0000034;
        internal const uint STATUS_SHARING_VIOLATION = 0xc0000043;
        internal const int SYNCHRONIZE = 0x100000;

        internal static uint CTL_CODE(ushort deviceType, ushort function, byte method, byte access)
        {
            if (function > 0xfff)
            {
                throw ADP.ArgumentOutOfRange("function");
            }
            return (uint) ((((deviceType << 0x10) | (access << 14)) | (function << 2)) | method);
        }

        [DllImport("Kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        internal static extern bool DeviceIoControl(SafeFileHandle fileHandle, uint ioControlCode, IntPtr inBuffer, uint cbInBuffer, IntPtr outBuffer, uint cbOutBuffer, out uint cbBytesReturned, IntPtr overlapped);
        [DllImport("Kernel32.dll", SetLastError=true)]
        internal static extern FileType GetFileType(SafeFileHandle hFile);
        [DllImport("Kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        private static extern int GetFullPathName(string path, int numBufferChars, StringBuilder buffer, IntPtr lpFilePartOrNull);
        [DllImport("NtDll.dll", CharSet=CharSet.Unicode)]
        internal static extern uint NtCreateFile(out SafeFileHandle fileHandle, int desiredAccess, ref OBJECT_ATTRIBUTES objectAttributes, out IO_STATUS_BLOCK ioStatusBlock, ref long allocationSize, uint fileAttributes, FileShare shareAccess, uint createDisposition, uint createOptions, SafeHandle eaBuffer, uint eaLength);
        [DllImport("NtDll.dll")]
        internal static extern uint RtlNtStatusToDosError(uint status);
        internal static string SafeGetFullPathName(string path)
        {
            StringBuilder buffer = new StringBuilder(path.Length + 1);
            int num = GetFullPathName(path, buffer.Capacity, buffer, IntPtr.Zero);
            if (num > buffer.Capacity)
            {
                buffer.Capacity = num;
                num = GetFullPathName(path, buffer.Capacity, buffer, IntPtr.Zero);
            }
            if (num != 0)
            {
                return buffer.ToString();
            }
            int error = Marshal.GetLastWin32Error();
            if (error == 0)
            {
                throw ADP.Argument(Res.GetString("SqlFileStream_InvalidPath"), "path");
            }
            Win32Exception e = new Win32Exception(error);
            ADP.TraceExceptionAsReturnValue(e);
            throw e;
        }

        [DllImport("Kernel32.dll")]
        internal static extern uint SetErrorMode(uint mode);

        [StructLayout(LayoutKind.Sequential)]
        internal struct FILE_FULL_EA_INFORMATION
        {
            internal uint nextEntryOffset;
            internal byte flags;
            internal byte EaNameLength;
            internal ushort EaValueLength;
            internal byte EaName;
        }

        internal enum FileType : uint
        {
            Char = 2,
            Disk = 1,
            Pipe = 3,
            Remote = 0x8000,
            Unknown = 0
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct IO_STATUS_BLOCK
        {
            internal uint status;
            internal IntPtr information;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct OBJECT_ATTRIBUTES
        {
            internal int length;
            internal IntPtr rootDirectory;
            internal SafeHandle objectName;
            internal int attributes;
            internal IntPtr securityDescriptor;
            internal SafeHandle securityQualityOfService;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct SECURITY_QUALITY_OF_SERVICE
        {
            internal uint length;
            [MarshalAs(UnmanagedType.I4)]
            internal int impersonationLevel;
            internal byte contextDynamicTrackingMode;
            internal byte effectiveOnly;
        }

        internal enum SecurityImpersonationLevel
        {
            SecurityAnonymous,
            SecurityIdentification,
            SecurityImpersonation,
            SecurityDelegation
        }

        [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
        internal struct UNICODE_STRING
        {
            internal ushort length;
            internal ushort maximumLength;
            internal string buffer;
        }
    }
}

