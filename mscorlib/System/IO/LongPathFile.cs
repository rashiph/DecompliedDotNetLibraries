namespace System.IO
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [ComVisible(false)]
    internal static class LongPathFile
    {
        private const int ERROR_ACCESS_DENIED = 5;

        [SecurityCritical]
        internal static void Copy(string sourceFileName, string destFileName, bool overwrite)
        {
            string fullSourceFileName = LongPath.NormalizePath(sourceFileName);
            new FileIOPermission(FileIOPermissionAccess.Read, new string[] { fullSourceFileName }, false, false).Demand();
            string fullDestFileName = LongPath.NormalizePath(destFileName);
            new FileIOPermission(FileIOPermissionAccess.Write, new string[] { fullDestFileName }, false, false).Demand();
            InternalCopy(fullSourceFileName, fullDestFileName, sourceFileName, destFileName, overwrite);
        }

        [SecurityCritical]
        internal static void Delete(string path)
        {
            string str = LongPath.NormalizePath(path);
            new FileIOPermission(FileIOPermissionAccess.Write, new string[] { str }, false, false).Demand();
            if (!Win32Native.DeleteFile(Path.AddLongPathPrefix(str)))
            {
                int errorCode = Marshal.GetLastWin32Error();
                if (errorCode != 2)
                {
                    __Error.WinIOError(errorCode, str);
                }
            }
        }

        [SecurityCritical]
        internal static bool Exists(string path)
        {
            try
            {
                if (path == null)
                {
                    return false;
                }
                if (path.Length == 0)
                {
                    return false;
                }
                path = LongPath.NormalizePath(path);
                if ((path.Length > 0) && Path.IsDirectorySeparator(path[path.Length - 1]))
                {
                    return false;
                }
                new FileIOPermission(FileIOPermissionAccess.Read, new string[] { path }, false, false).Demand();
                return InternalExists(path);
            }
            catch (ArgumentException)
            {
            }
            catch (NotSupportedException)
            {
            }
            catch (SecurityException)
            {
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
            return false;
        }

        [SecurityCritical]
        internal static DateTimeOffset GetCreationTime(string path)
        {
            string str = LongPath.NormalizePath(path);
            new FileIOPermission(FileIOPermissionAccess.Read, new string[] { str }, false, false).Demand();
            string str2 = Path.AddLongPathPrefix(str);
            Win32Native.WIN32_FILE_ATTRIBUTE_DATA data = new Win32Native.WIN32_FILE_ATTRIBUTE_DATA();
            int errorCode = File.FillAttributeInfo(str2, ref data, false, false);
            if (errorCode != 0)
            {
                __Error.WinIOError(errorCode, str);
            }
            long fileTime = (data.ftCreationTimeHigh << 0x20) | data.ftCreationTimeLow;
            DateTimeOffset offset = new DateTimeOffset(DateTime.FromFileTimeUtc(fileTime).ToLocalTime());
            return offset.ToLocalTime();
        }

        [SecurityCritical]
        internal static DateTimeOffset GetLastAccessTime(string path)
        {
            string str = LongPath.NormalizePath(path);
            new FileIOPermission(FileIOPermissionAccess.Read, new string[] { str }, false, false).Demand();
            string str2 = Path.AddLongPathPrefix(str);
            Win32Native.WIN32_FILE_ATTRIBUTE_DATA data = new Win32Native.WIN32_FILE_ATTRIBUTE_DATA();
            int errorCode = File.FillAttributeInfo(str2, ref data, false, false);
            if (errorCode != 0)
            {
                __Error.WinIOError(errorCode, str);
            }
            long fileTime = (data.ftLastAccessTimeHigh << 0x20) | data.ftLastAccessTimeLow;
            DateTimeOffset offset = new DateTimeOffset(DateTime.FromFileTimeUtc(fileTime).ToLocalTime());
            return offset.ToLocalTime();
        }

        [SecurityCritical]
        internal static DateTimeOffset GetLastWriteTime(string path)
        {
            string str = LongPath.NormalizePath(path);
            new FileIOPermission(FileIOPermissionAccess.Read, new string[] { str }, false, false).Demand();
            string str2 = Path.AddLongPathPrefix(str);
            Win32Native.WIN32_FILE_ATTRIBUTE_DATA data = new Win32Native.WIN32_FILE_ATTRIBUTE_DATA();
            int errorCode = File.FillAttributeInfo(str2, ref data, false, false);
            if (errorCode != 0)
            {
                __Error.WinIOError(errorCode, str);
            }
            long fileTime = (data.ftLastWriteTimeHigh << 0x20) | data.ftLastWriteTimeLow;
            DateTimeOffset offset = new DateTimeOffset(DateTime.FromFileTimeUtc(fileTime).ToLocalTime());
            return offset.ToLocalTime();
        }

        [SecurityCritical]
        internal static long GetLength(string path)
        {
            string str = LongPath.NormalizePath(path);
            new FileIOPermission(FileIOPermissionAccess.Read, new string[] { str }, false, false).Demand();
            string str2 = Path.AddLongPathPrefix(str);
            Win32Native.WIN32_FILE_ATTRIBUTE_DATA data = new Win32Native.WIN32_FILE_ATTRIBUTE_DATA();
            int errorCode = File.FillAttributeInfo(str2, ref data, false, true);
            if (errorCode != 0)
            {
                __Error.WinIOError(errorCode, path);
            }
            if ((data.fileAttributes & 0x10) != 0)
            {
                __Error.WinIOError(2, path);
            }
            return ((data.fileSizeHigh << 0x20) | (data.fileSizeLow & ((long) 0xffffffffL)));
        }

        [SecurityCritical]
        private static string InternalCopy(string fullSourceFileName, string fullDestFileName, string sourceFileName, string destFileName, bool overwrite)
        {
            fullSourceFileName = Path.AddLongPathPrefix(fullSourceFileName);
            fullDestFileName = Path.AddLongPathPrefix(fullDestFileName);
            if (!Win32Native.CopyFile(fullSourceFileName, fullDestFileName, !overwrite))
            {
                int errorCode = Marshal.GetLastWin32Error();
                string maybeFullPath = destFileName;
                if (errorCode != 80)
                {
                    using (SafeFileHandle handle = Win32Native.UnsafeCreateFile(fullSourceFileName, -2147483648, FileShare.Read, null, FileMode.Open, 0, IntPtr.Zero))
                    {
                        if (handle.IsInvalid)
                        {
                            maybeFullPath = sourceFileName;
                        }
                    }
                    if ((errorCode == 5) && LongPathDirectory.InternalExists(fullDestFileName))
                    {
                        throw new IOException(Environment.GetResourceString("Arg_FileIsDirectory_Name", new object[] { destFileName }), 5, fullDestFileName);
                    }
                }
                __Error.WinIOError(errorCode, maybeFullPath);
            }
            return fullDestFileName;
        }

        [SecurityCritical]
        internal static bool InternalExists(string path)
        {
            return File.InternalExists(Path.AddLongPathPrefix(path));
        }

        [SecurityCritical]
        internal static void Move(string sourceFileName, string destFileName)
        {
            string path = LongPath.NormalizePath(sourceFileName);
            new FileIOPermission(FileIOPermissionAccess.Write | FileIOPermissionAccess.Read, new string[] { path }, false, false).Demand();
            string str2 = LongPath.NormalizePath(destFileName);
            new FileIOPermission(FileIOPermissionAccess.Write, new string[] { str2 }, false, false).Demand();
            if (!InternalExists(path))
            {
                __Error.WinIOError(2, path);
            }
            string src = Path.AddLongPathPrefix(path);
            string dst = Path.AddLongPathPrefix(str2);
            if (!Win32Native.MoveFile(src, dst))
            {
                __Error.WinIOError();
            }
        }
    }
}

