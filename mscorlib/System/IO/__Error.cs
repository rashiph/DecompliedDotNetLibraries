namespace System.IO
{
    using Microsoft.Win32;
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    internal static class __Error
    {
        internal const int ERROR_ACCESS_DENIED = 5;
        internal const int ERROR_FILE_NOT_FOUND = 2;
        internal const int ERROR_INVALID_PARAMETER = 0x57;
        internal const int ERROR_PATH_NOT_FOUND = 3;

        internal static void EndOfFile()
        {
            throw new EndOfStreamException(Environment.GetResourceString("IO.EOF_ReadBeyondEOF"));
        }

        internal static void EndReadCalledTwice()
        {
            throw new ArgumentException(Environment.GetResourceString("InvalidOperation_EndReadCalledMultiple"));
        }

        internal static void EndWriteCalledTwice()
        {
            throw new ArgumentException(Environment.GetResourceString("InvalidOperation_EndWriteCalledMultiple"));
        }

        internal static void FileNotOpen()
        {
            throw new ObjectDisposedException(null, Environment.GetResourceString("ObjectDisposed_FileClosed"));
        }

        [SecurityCritical]
        internal static string GetDisplayablePath(string path, bool isInvalidPath)
        {
            if (!string.IsNullOrEmpty(path))
            {
                bool flag = false;
                if (path.Length < 2)
                {
                    return path;
                }
                if (Path.IsDirectorySeparator(path[0]) && Path.IsDirectorySeparator(path[1]))
                {
                    flag = true;
                }
                else if (path[1] == Path.VolumeSeparatorChar)
                {
                    flag = true;
                }
                if (!flag && !isInvalidPath)
                {
                    return path;
                }
                bool flag2 = false;
                try
                {
                    if (!isInvalidPath)
                    {
                        new FileIOPermission(FileIOPermissionAccess.PathDiscovery, new string[] { path }, false, false).Demand();
                        flag2 = true;
                    }
                }
                catch (SecurityException)
                {
                }
                catch (ArgumentException)
                {
                }
                catch (NotSupportedException)
                {
                }
                if (flag2)
                {
                    return path;
                }
                if (Path.IsDirectorySeparator(path[path.Length - 1]))
                {
                    path = Environment.GetResourceString("IO.IO_NoPermissionToDirectoryName");
                    return path;
                }
                path = Path.GetFileName(path);
            }
            return path;
        }

        internal static void MemoryStreamNotExpandable()
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_MemStreamNotExpandable"));
        }

        internal static void ReaderClosed()
        {
            throw new ObjectDisposedException(null, Environment.GetResourceString("ObjectDisposed_ReaderClosed"));
        }

        internal static void ReadNotSupported()
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_UnreadableStream"));
        }

        internal static void SeekNotSupported()
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_UnseekableStream"));
        }

        internal static void StreamIsClosed()
        {
            throw new ObjectDisposedException(null, Environment.GetResourceString("ObjectDisposed_StreamClosed"));
        }

        [SecuritySafeCritical]
        internal static void WinIODriveError(string driveName)
        {
            int errorCode = Marshal.GetLastWin32Error();
            WinIODriveError(driveName, errorCode);
        }

        [SecurityCritical]
        internal static void WinIODriveError(string driveName, int errorCode)
        {
            switch (errorCode)
            {
                case 3:
                case 15:
                    throw new DriveNotFoundException(Environment.GetResourceString("IO.DriveNotFound_Drive", new object[] { driveName }));
            }
            WinIOError(errorCode, driveName);
        }

        [SecuritySafeCritical]
        internal static void WinIOError()
        {
            WinIOError(Marshal.GetLastWin32Error(), string.Empty);
        }

        [SecurityCritical]
        internal static void WinIOError(int errorCode, string maybeFullPath)
        {
            bool isInvalidPath = (errorCode == 0x7b) || (errorCode == 0xa1);
            string displayablePath = GetDisplayablePath(maybeFullPath, isInvalidPath);
            switch (errorCode)
            {
                case 0x20:
                    if (displayablePath.Length == 0)
                    {
                        throw new IOException(Environment.GetResourceString("IO.IO_SharingViolation_NoFileName"), Win32Native.MakeHRFromErrorCode(errorCode), maybeFullPath);
                    }
                    throw new IOException(Environment.GetResourceString("IO.IO_SharingViolation_File", new object[] { displayablePath }), Win32Native.MakeHRFromErrorCode(errorCode), maybeFullPath);

                case 80:
                    if (displayablePath.Length != 0)
                    {
                        throw new IOException(Environment.GetResourceString("IO.IO_FileExists_Name", new object[] { displayablePath }), Win32Native.MakeHRFromErrorCode(errorCode), maybeFullPath);
                    }
                    break;

                case 2:
                    if (displayablePath.Length == 0)
                    {
                        throw new FileNotFoundException(Environment.GetResourceString("IO.FileNotFound"));
                    }
                    throw new FileNotFoundException(Environment.GetResourceString("IO.FileNotFound_FileName", new object[] { displayablePath }), displayablePath);

                case 3:
                    if (displayablePath.Length == 0)
                    {
                        throw new DirectoryNotFoundException(Environment.GetResourceString("IO.PathNotFound_NoPathName"));
                    }
                    throw new DirectoryNotFoundException(Environment.GetResourceString("IO.PathNotFound_Path", new object[] { displayablePath }));

                case 5:
                    if (displayablePath.Length == 0)
                    {
                        throw new UnauthorizedAccessException(Environment.GetResourceString("UnauthorizedAccess_IODenied_NoPathName"));
                    }
                    throw new UnauthorizedAccessException(Environment.GetResourceString("UnauthorizedAccess_IODenied_Path", new object[] { displayablePath }));

                case 15:
                    throw new DriveNotFoundException(Environment.GetResourceString("IO.DriveNotFound_Drive", new object[] { displayablePath }));

                case 0x57:
                    throw new IOException(Win32Native.GetMessage(errorCode), Win32Native.MakeHRFromErrorCode(errorCode), maybeFullPath);

                case 0xb7:
                    if (displayablePath.Length != 0)
                    {
                        throw new IOException(Environment.GetResourceString("IO.IO_AlreadyExists_Name", new object[] { displayablePath }), Win32Native.MakeHRFromErrorCode(errorCode), maybeFullPath);
                    }
                    break;

                case 0xce:
                    throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));

                case 0x3e3:
                    throw new OperationCanceledException();
            }
            throw new IOException(Win32Native.GetMessage(errorCode), Win32Native.MakeHRFromErrorCode(errorCode), maybeFullPath);
        }

        internal static void WriteNotSupported()
        {
            throw new NotSupportedException(Environment.GetResourceString("NotSupported_UnwritableStream"));
        }

        internal static void WriterClosed()
        {
            throw new ObjectDisposedException(null, Environment.GetResourceString("ObjectDisposed_WriterClosed"));
        }

        internal static void WrongAsyncResult()
        {
            throw new ArgumentException(Environment.GetResourceString("Arg_WrongAsyncResult"));
        }
    }
}

