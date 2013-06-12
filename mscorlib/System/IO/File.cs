namespace System.IO
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.AccessControl;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    [ComVisible(true)]
    public static class File
    {
        private const int ERROR_ACCESS_DENIED = 5;
        private const int ERROR_INVALID_PARAMETER = 0x57;
        private const int GetFileExInfoStandard = 0;

        [SecuritySafeCritical]
        public static void AppendAllLines(string path, IEnumerable<string> contents)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (contents == null)
            {
                throw new ArgumentNullException("contents");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"));
            }
            InternalWriteAllLines(new StreamWriter(path, true, StreamWriter.UTF8NoBOM), contents);
        }

        [SecuritySafeCritical]
        public static void AppendAllLines(string path, IEnumerable<string> contents, Encoding encoding)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (contents == null)
            {
                throw new ArgumentNullException("contents");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"));
            }
            InternalWriteAllLines(new StreamWriter(path, true, encoding), contents);
        }

        [SecuritySafeCritical]
        public static void AppendAllText(string path, string contents)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"));
            }
            InternalAppendAllText(path, contents, StreamWriter.UTF8NoBOM);
        }

        [SecuritySafeCritical]
        public static void AppendAllText(string path, string contents, Encoding encoding)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"));
            }
            InternalAppendAllText(path, contents, encoding);
        }

        [SecuritySafeCritical]
        public static StreamWriter AppendText(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            return new StreamWriter(path, true);
        }

        public static void Copy(string sourceFileName, string destFileName)
        {
            if (sourceFileName == null)
            {
                throw new ArgumentNullException("sourceFileName", Environment.GetResourceString("ArgumentNull_FileName"));
            }
            if (destFileName == null)
            {
                throw new ArgumentNullException("destFileName", Environment.GetResourceString("ArgumentNull_FileName"));
            }
            if (sourceFileName.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyFileName"), "sourceFileName");
            }
            if (destFileName.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyFileName"), "destFileName");
            }
            InternalCopy(sourceFileName, destFileName, false);
        }

        public static void Copy(string sourceFileName, string destFileName, bool overwrite)
        {
            if (sourceFileName == null)
            {
                throw new ArgumentNullException("sourceFileName", Environment.GetResourceString("ArgumentNull_FileName"));
            }
            if (destFileName == null)
            {
                throw new ArgumentNullException("destFileName", Environment.GetResourceString("ArgumentNull_FileName"));
            }
            if (sourceFileName.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyFileName"), "sourceFileName");
            }
            if (destFileName.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyFileName"), "destFileName");
            }
            InternalCopy(sourceFileName, destFileName, overwrite);
        }

        [SecuritySafeCritical]
        public static FileStream Create(string path)
        {
            return Create(path, 0x1000, FileOptions.None);
        }

        [SecuritySafeCritical]
        public static FileStream Create(string path, int bufferSize)
        {
            return Create(path, bufferSize, FileOptions.None);
        }

        [SecuritySafeCritical]
        public static FileStream Create(string path, int bufferSize, FileOptions options)
        {
            return new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize, options);
        }

        public static FileStream Create(string path, int bufferSize, FileOptions options, FileSecurity fileSecurity)
        {
            return new FileStream(path, FileMode.Create, FileSystemRights.Read | FileSystemRights.Write, FileShare.None, bufferSize, options, fileSecurity);
        }

        [SecuritySafeCritical]
        public static StreamWriter CreateText(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            return new StreamWriter(path, false);
        }

        [SecuritySafeCritical]
        public static void Decrypt(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            string fullPathInternal = Path.GetFullPathInternal(path);
            new FileIOPermission(FileIOPermissionAccess.Write | FileIOPermissionAccess.Read, new string[] { fullPathInternal }, false, false).Demand();
            if (!Win32Native.DecryptFile(fullPathInternal, 0))
            {
                int errorCode = Marshal.GetLastWin32Error();
                if (errorCode == 5)
                {
                    DriveInfo info = new DriveInfo(Path.GetPathRoot(fullPathInternal));
                    if (!string.Equals("NTFS", info.DriveFormat))
                    {
                        throw new NotSupportedException(Environment.GetResourceString("NotSupported_EncryptionNeedsNTFS"));
                    }
                }
                __Error.WinIOError(errorCode, fullPathInternal);
            }
        }

        [SecuritySafeCritical]
        public static void Delete(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            string fullPathInternal = Path.GetFullPathInternal(path);
            new FileIOPermission(FileIOPermissionAccess.Write, new string[] { fullPathInternal }, false, false).Demand();
            if (!Win32Native.DeleteFile(fullPathInternal))
            {
                int errorCode = Marshal.GetLastWin32Error();
                if (errorCode != 2)
                {
                    __Error.WinIOError(errorCode, fullPathInternal);
                }
            }
        }

        [SecuritySafeCritical]
        public static void Encrypt(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            string fullPathInternal = Path.GetFullPathInternal(path);
            new FileIOPermission(FileIOPermissionAccess.Write | FileIOPermissionAccess.Read, new string[] { fullPathInternal }, false, false).Demand();
            if (!Win32Native.EncryptFile(fullPathInternal))
            {
                int errorCode = Marshal.GetLastWin32Error();
                if (errorCode == 5)
                {
                    DriveInfo info = new DriveInfo(Path.GetPathRoot(fullPathInternal));
                    if (!string.Equals("NTFS", info.DriveFormat))
                    {
                        throw new NotSupportedException(Environment.GetResourceString("NotSupported_EncryptionNeedsNTFS"));
                    }
                }
                __Error.WinIOError(errorCode, fullPathInternal);
            }
        }

        [SecuritySafeCritical]
        public static bool Exists(string path)
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
                path = Path.GetFullPathInternal(path);
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
        internal static int FillAttributeInfo(string path, ref Win32Native.WIN32_FILE_ATTRIBUTE_DATA data, bool tryagain, bool returnErrorOnNotFound)
        {
            int num = 0;
            if (tryagain)
            {
                Win32Native.WIN32_FIND_DATA win_find_data = new Win32Native.WIN32_FIND_DATA();
                string fileName = path.TrimEnd(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
                int num2 = Win32Native.SetErrorMode(1);
                try
                {
                    bool flag = false;
                    SafeFindHandle handle = Win32Native.FindFirstFile(fileName, win_find_data);
                    try
                    {
                        if (handle.IsInvalid)
                        {
                            flag = true;
                            num = Marshal.GetLastWin32Error();
                            if ((((num == 2) || (num == 3)) || (num == 0x15)) && !returnErrorOnNotFound)
                            {
                                num = 0;
                                data.fileAttributes = -1;
                            }
                            return num;
                        }
                    }
                    finally
                    {
                        try
                        {
                            handle.Close();
                        }
                        catch
                        {
                            if (!flag)
                            {
                                __Error.WinIOError();
                            }
                        }
                    }
                }
                finally
                {
                    Win32Native.SetErrorMode(num2);
                }
                data.PopulateFrom(win_find_data);
                return num;
            }
            bool flag2 = false;
            int newMode = Win32Native.SetErrorMode(1);
            try
            {
                flag2 = Win32Native.GetFileAttributesEx(path, 0, ref data);
            }
            finally
            {
                Win32Native.SetErrorMode(newMode);
            }
            if (!flag2)
            {
                num = Marshal.GetLastWin32Error();
                if (((num != 2) && (num != 3)) && (num != 0x15))
                {
                    return FillAttributeInfo(path, ref data, true, returnErrorOnNotFound);
                }
                if (!returnErrorOnNotFound)
                {
                    num = 0;
                    data.fileAttributes = -1;
                }
            }
            return num;
        }

        public static FileSecurity GetAccessControl(string path)
        {
            return GetAccessControl(path, AccessControlSections.Group | AccessControlSections.Owner | AccessControlSections.Access);
        }

        public static FileSecurity GetAccessControl(string path, AccessControlSections includeSections)
        {
            return new FileSecurity(path, includeSections);
        }

        [SecuritySafeCritical]
        public static FileAttributes GetAttributes(string path)
        {
            string fullPathInternal = Path.GetFullPathInternal(path);
            new FileIOPermission(FileIOPermissionAccess.Read, new string[] { fullPathInternal }, false, false).Demand();
            Win32Native.WIN32_FILE_ATTRIBUTE_DATA data = new Win32Native.WIN32_FILE_ATTRIBUTE_DATA();
            int errorCode = FillAttributeInfo(fullPathInternal, ref data, false, true);
            if (errorCode != 0)
            {
                __Error.WinIOError(errorCode, fullPathInternal);
            }
            return (FileAttributes) data.fileAttributes;
        }

        [SecuritySafeCritical]
        public static DateTime GetCreationTime(string path)
        {
            return GetCreationTimeUtc(path).ToLocalTime();
        }

        [SecuritySafeCritical]
        public static DateTime GetCreationTimeUtc(string path)
        {
            string fullPathInternal = Path.GetFullPathInternal(path);
            new FileIOPermission(FileIOPermissionAccess.Read, new string[] { fullPathInternal }, false, false).Demand();
            Win32Native.WIN32_FILE_ATTRIBUTE_DATA data = new Win32Native.WIN32_FILE_ATTRIBUTE_DATA();
            int errorCode = FillAttributeInfo(fullPathInternal, ref data, false, false);
            if (errorCode != 0)
            {
                __Error.WinIOError(errorCode, fullPathInternal);
            }
            long fileTime = (data.ftCreationTimeHigh << 0x20) | data.ftCreationTimeLow;
            return DateTime.FromFileTimeUtc(fileTime);
        }

        [SecuritySafeCritical]
        public static DateTime GetLastAccessTime(string path)
        {
            return GetLastAccessTimeUtc(path).ToLocalTime();
        }

        [SecuritySafeCritical]
        public static DateTime GetLastAccessTimeUtc(string path)
        {
            string fullPathInternal = Path.GetFullPathInternal(path);
            new FileIOPermission(FileIOPermissionAccess.Read, new string[] { fullPathInternal }, false, false).Demand();
            Win32Native.WIN32_FILE_ATTRIBUTE_DATA data = new Win32Native.WIN32_FILE_ATTRIBUTE_DATA();
            int errorCode = FillAttributeInfo(fullPathInternal, ref data, false, false);
            if (errorCode != 0)
            {
                __Error.WinIOError(errorCode, fullPathInternal);
            }
            long fileTime = (data.ftLastAccessTimeHigh << 0x20) | data.ftLastAccessTimeLow;
            return DateTime.FromFileTimeUtc(fileTime);
        }

        [SecuritySafeCritical]
        public static DateTime GetLastWriteTime(string path)
        {
            return GetLastWriteTimeUtc(path).ToLocalTime();
        }

        [SecuritySafeCritical]
        public static DateTime GetLastWriteTimeUtc(string path)
        {
            string fullPathInternal = Path.GetFullPathInternal(path);
            new FileIOPermission(FileIOPermissionAccess.Read, new string[] { fullPathInternal }, false, false).Demand();
            Win32Native.WIN32_FILE_ATTRIBUTE_DATA data = new Win32Native.WIN32_FILE_ATTRIBUTE_DATA();
            int errorCode = FillAttributeInfo(fullPathInternal, ref data, false, false);
            if (errorCode != 0)
            {
                __Error.WinIOError(errorCode, fullPathInternal);
            }
            long fileTime = (data.ftLastWriteTimeHigh << 0x20) | data.ftLastWriteTimeLow;
            return DateTime.FromFileTimeUtc(fileTime);
        }

        private static void InternalAppendAllText(string path, string contents, Encoding encoding)
        {
            using (StreamWriter writer = new StreamWriter(path, true, encoding))
            {
                writer.Write(contents);
            }
        }

        [SecuritySafeCritical]
        internal static string InternalCopy(string sourceFileName, string destFileName, bool overwrite)
        {
            string fullPathInternal = Path.GetFullPathInternal(sourceFileName);
            new FileIOPermission(FileIOPermissionAccess.Read, new string[] { fullPathInternal }, false, false).Demand();
            string dst = Path.GetFullPathInternal(destFileName);
            new FileIOPermission(FileIOPermissionAccess.Write, new string[] { dst }, false, false).Demand();
            if (!Win32Native.CopyFile(fullPathInternal, dst, !overwrite))
            {
                int errorCode = Marshal.GetLastWin32Error();
                string maybeFullPath = destFileName;
                if (errorCode != 80)
                {
                    using (SafeFileHandle handle = Win32Native.UnsafeCreateFile(fullPathInternal, -2147483648, FileShare.Read, null, FileMode.Open, 0, IntPtr.Zero))
                    {
                        if (handle.IsInvalid)
                        {
                            maybeFullPath = sourceFileName;
                        }
                    }
                    if ((errorCode == 5) && Directory.InternalExists(dst))
                    {
                        throw new IOException(Environment.GetResourceString("Arg_FileIsDirectory_Name", new object[] { destFileName }), 5, dst);
                    }
                }
                __Error.WinIOError(errorCode, maybeFullPath);
            }
            return dst;
        }

        [SecurityCritical]
        internal static bool InternalExists(string path)
        {
            Win32Native.WIN32_FILE_ATTRIBUTE_DATA data = new Win32Native.WIN32_FILE_ATTRIBUTE_DATA();
            return (((FillAttributeInfo(path, ref data, false, true) == 0) && (data.fileAttributes != -1)) && ((data.fileAttributes & 0x10) == 0));
        }

        private static string[] InternalReadAllLines(string path, Encoding encoding)
        {
            List<string> list = new List<string>();
            using (StreamReader reader = new StreamReader(path, encoding))
            {
                string str;
                while ((str = reader.ReadLine()) != null)
                {
                    list.Add(str);
                }
            }
            return list.ToArray();
        }

        private static string InternalReadAllText(string path, Encoding encoding)
        {
            using (StreamReader reader = new StreamReader(path, encoding))
            {
                return reader.ReadToEnd();
            }
        }

        private static IEnumerable<string> InternalReadLines(TextReader reader)
        {
            using (reader)
            {
                string iteratorVariable0 = null;
                while ((iteratorVariable0 = reader.ReadLine()) != null)
                {
                    yield return iteratorVariable0;
                }
            }
        }

        [SecuritySafeCritical]
        private static void InternalReplace(string sourceFileName, string destinationFileName, string destinationBackupFileName, bool ignoreMetadataErrors)
        {
            string fullPathInternal = Path.GetFullPathInternal(sourceFileName);
            string replacedFileName = Path.GetFullPathInternal(destinationFileName);
            string path = null;
            if (destinationBackupFileName != null)
            {
                path = Path.GetFullPathInternal(destinationBackupFileName);
            }
            FileIOPermission permission = new FileIOPermission(FileIOPermissionAccess.Write | FileIOPermissionAccess.Read, new string[] { fullPathInternal, replacedFileName });
            if (destinationBackupFileName != null)
            {
                permission.AddPathList(FileIOPermissionAccess.Write, path);
            }
            permission.Demand();
            int dwReplaceFlags = 1;
            if (ignoreMetadataErrors)
            {
                dwReplaceFlags |= 2;
            }
            if (!Win32Native.ReplaceFile(replacedFileName, fullPathInternal, path, dwReplaceFlags, IntPtr.Zero, IntPtr.Zero))
            {
                __Error.WinIOError();
            }
        }

        private static void InternalWriteAllLines(TextWriter writer, IEnumerable<string> contents)
        {
            using (writer)
            {
                foreach (string str in contents)
                {
                    writer.WriteLine(str);
                }
            }
        }

        private static void InternalWriteAllText(string path, string contents, Encoding encoding)
        {
            using (StreamWriter writer = new StreamWriter(path, false, encoding))
            {
                writer.Write(contents);
            }
        }

        [SecuritySafeCritical]
        public static void Move(string sourceFileName, string destFileName)
        {
            if (sourceFileName == null)
            {
                throw new ArgumentNullException("sourceFileName", Environment.GetResourceString("ArgumentNull_FileName"));
            }
            if (destFileName == null)
            {
                throw new ArgumentNullException("destFileName", Environment.GetResourceString("ArgumentNull_FileName"));
            }
            if (sourceFileName.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyFileName"), "sourceFileName");
            }
            if (destFileName.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyFileName"), "destFileName");
            }
            string fullPathInternal = Path.GetFullPathInternal(sourceFileName);
            new FileIOPermission(FileIOPermissionAccess.Write | FileIOPermissionAccess.Read, new string[] { fullPathInternal }, false, false).Demand();
            string dst = Path.GetFullPathInternal(destFileName);
            new FileIOPermission(FileIOPermissionAccess.Write, new string[] { dst }, false, false).Demand();
            if (!InternalExists(fullPathInternal))
            {
                __Error.WinIOError(2, fullPathInternal);
            }
            if (!Win32Native.MoveFile(fullPathInternal, dst))
            {
                __Error.WinIOError();
            }
        }

        [SecuritySafeCritical]
        public static FileStream Open(string path, FileMode mode)
        {
            return Open(path, mode, (mode == FileMode.Append) ? FileAccess.Write : FileAccess.ReadWrite, FileShare.None);
        }

        [SecuritySafeCritical]
        public static FileStream Open(string path, FileMode mode, FileAccess access)
        {
            return Open(path, mode, access, FileShare.None);
        }

        [SecuritySafeCritical]
        public static FileStream Open(string path, FileMode mode, FileAccess access, FileShare share)
        {
            return new FileStream(path, mode, access, share);
        }

        [SecurityCritical]
        private static FileStream OpenFile(string path, FileAccess access, out SafeFileHandle handle)
        {
            FileStream stream = new FileStream(path, FileMode.Open, access, FileShare.ReadWrite, 1);
            handle = stream.SafeFileHandle;
            if (handle.IsInvalid)
            {
                int errorCode = Marshal.GetLastWin32Error();
                string fullPathInternal = Path.GetFullPathInternal(path);
                if ((errorCode == 3) && fullPathInternal.Equals(Directory.GetDirectoryRoot(fullPathInternal)))
                {
                    errorCode = 5;
                }
                __Error.WinIOError(errorCode, path);
            }
            return stream;
        }

        [SecuritySafeCritical]
        public static FileStream OpenRead(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        [SecuritySafeCritical]
        public static StreamReader OpenText(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            return new StreamReader(path);
        }

        [SecuritySafeCritical]
        public static FileStream OpenWrite(string path)
        {
            return new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
        }

        [SecuritySafeCritical]
        public static byte[] ReadAllBytes(string path)
        {
            byte[] buffer;
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                int offset = 0;
                long length = stream.Length;
                if (length > 0x7fffffffL)
                {
                    throw new IOException(Environment.GetResourceString("IO.IO_FileTooLong2GB"));
                }
                int count = (int) length;
                buffer = new byte[count];
                while (count > 0)
                {
                    int num4 = stream.Read(buffer, offset, count);
                    if (num4 == 0)
                    {
                        __Error.EndOfFile();
                    }
                    offset += num4;
                    count -= num4;
                }
            }
            return buffer;
        }

        [SecuritySafeCritical]
        public static string[] ReadAllLines(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"));
            }
            return InternalReadAllLines(path, Encoding.UTF8);
        }

        [SecuritySafeCritical]
        public static string[] ReadAllLines(string path, Encoding encoding)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"));
            }
            return InternalReadAllLines(path, encoding);
        }

        [SecuritySafeCritical]
        public static string ReadAllText(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"));
            }
            return InternalReadAllText(path, Encoding.UTF8);
        }

        [SecuritySafeCritical]
        public static string ReadAllText(string path, Encoding encoding)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"));
            }
            return InternalReadAllText(path, encoding);
        }

        [SecuritySafeCritical]
        public static IEnumerable<string> ReadLines(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"));
            }
            return InternalReadLines(new StreamReader(path, Encoding.UTF8));
        }

        [SecuritySafeCritical]
        public static IEnumerable<string> ReadLines(string path, Encoding encoding)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"));
            }
            return InternalReadLines(new StreamReader(path, encoding));
        }

        public static void Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName)
        {
            if (sourceFileName == null)
            {
                throw new ArgumentNullException("sourceFileName");
            }
            if (destinationFileName == null)
            {
                throw new ArgumentNullException("destinationFileName");
            }
            InternalReplace(sourceFileName, destinationFileName, destinationBackupFileName, false);
        }

        [SecuritySafeCritical]
        public static void Replace(string sourceFileName, string destinationFileName, string destinationBackupFileName, bool ignoreMetadataErrors)
        {
            if (sourceFileName == null)
            {
                throw new ArgumentNullException("sourceFileName");
            }
            if (destinationFileName == null)
            {
                throw new ArgumentNullException("destinationFileName");
            }
            InternalReplace(sourceFileName, destinationFileName, destinationBackupFileName, ignoreMetadataErrors);
        }

        [SecuritySafeCritical]
        public static void SetAccessControl(string path, FileSecurity fileSecurity)
        {
            if (fileSecurity == null)
            {
                throw new ArgumentNullException("fileSecurity");
            }
            string fullPathInternal = Path.GetFullPathInternal(path);
            fileSecurity.Persist(fullPathInternal);
        }

        [SecuritySafeCritical]
        public static void SetAttributes(string path, FileAttributes fileAttributes)
        {
            string fullPathInternal = Path.GetFullPathInternal(path);
            new FileIOPermission(FileIOPermissionAccess.Write, new string[] { fullPathInternal }, false, false).Demand();
            if (!Win32Native.SetFileAttributes(fullPathInternal, (int) fileAttributes))
            {
                int errorCode = Marshal.GetLastWin32Error();
                if (errorCode == 0x57)
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_InvalidFileAttrs"));
                }
                __Error.WinIOError(errorCode, fullPathInternal);
            }
        }

        public static void SetCreationTime(string path, DateTime creationTime)
        {
            SetCreationTimeUtc(path, creationTime.ToUniversalTime());
        }

        [SecuritySafeCritical]
        public static unsafe void SetCreationTimeUtc(string path, DateTime creationTimeUtc)
        {
            SafeFileHandle handle;
            using (OpenFile(path, FileAccess.Write, out handle))
            {
                Win32Native.FILE_TIME creationTime = new Win32Native.FILE_TIME(creationTimeUtc.ToFileTimeUtc());
                if (!Win32Native.SetFileTime(handle, &creationTime, null, null))
                {
                    __Error.WinIOError(Marshal.GetLastWin32Error(), path);
                }
            }
        }

        public static void SetLastAccessTime(string path, DateTime lastAccessTime)
        {
            SetLastAccessTimeUtc(path, lastAccessTime.ToUniversalTime());
        }

        [SecuritySafeCritical]
        public static unsafe void SetLastAccessTimeUtc(string path, DateTime lastAccessTimeUtc)
        {
            SafeFileHandle handle;
            using (OpenFile(path, FileAccess.Write, out handle))
            {
                Win32Native.FILE_TIME lastAccessTime = new Win32Native.FILE_TIME(lastAccessTimeUtc.ToFileTimeUtc());
                if (!Win32Native.SetFileTime(handle, null, &lastAccessTime, null))
                {
                    __Error.WinIOError(Marshal.GetLastWin32Error(), path);
                }
            }
        }

        public static void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            SetLastWriteTimeUtc(path, lastWriteTime.ToUniversalTime());
        }

        [SecuritySafeCritical]
        public static unsafe void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            SafeFileHandle handle;
            using (OpenFile(path, FileAccess.Write, out handle))
            {
                Win32Native.FILE_TIME lastWriteTime = new Win32Native.FILE_TIME(lastWriteTimeUtc.ToFileTimeUtc());
                if (!Win32Native.SetFileTime(handle, null, null, &lastWriteTime))
                {
                    __Error.WinIOError(Marshal.GetLastWin32Error(), path);
                }
            }
        }

        [SecuritySafeCritical]
        public static void WriteAllBytes(string path, byte[] bytes)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path", Environment.GetResourceString("ArgumentNull_Path"));
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"));
            }
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        [SecuritySafeCritical]
        public static void WriteAllLines(string path, string[] contents)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (contents == null)
            {
                throw new ArgumentNullException("contents");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"));
            }
            InternalWriteAllLines(new StreamWriter(path, false, StreamWriter.UTF8NoBOM), contents);
        }

        [SecuritySafeCritical]
        public static void WriteAllLines(string path, IEnumerable<string> contents)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (contents == null)
            {
                throw new ArgumentNullException("contents");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"));
            }
            InternalWriteAllLines(new StreamWriter(path, false, StreamWriter.UTF8NoBOM), contents);
        }

        [SecuritySafeCritical]
        public static void WriteAllLines(string path, string[] contents, Encoding encoding)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (contents == null)
            {
                throw new ArgumentNullException("contents");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"));
            }
            InternalWriteAllLines(new StreamWriter(path, false, encoding), contents);
        }

        [SecuritySafeCritical]
        public static void WriteAllLines(string path, IEnumerable<string> contents, Encoding encoding)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (contents == null)
            {
                throw new ArgumentNullException("contents");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"));
            }
            InternalWriteAllLines(new StreamWriter(path, false, encoding), contents);
        }

        [SecuritySafeCritical]
        public static void WriteAllText(string path, string contents)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"));
            }
            InternalWriteAllText(path, contents, StreamWriter.UTF8NoBOM);
        }

        [SecuritySafeCritical]
        public static void WriteAllText(string path, string contents, Encoding encoding)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"));
            }
            InternalWriteAllText(path, contents, encoding);
        }

    }
}

