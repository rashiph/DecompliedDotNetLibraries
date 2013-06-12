namespace System.IO
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.AccessControl;
    using System.Security.Permissions;

    [ComVisible(false)]
    internal static class LongPathDirectory
    {
        [SecurityCritical]
        internal static void CreateDirectory(string path)
        {
            string fullPath = LongPath.NormalizePath(path);
            string demandDir = GetDemandDir(fullPath, true);
            new FileIOPermission(FileIOPermissionAccess.Read, new string[] { demandDir }, false, false).Demand();
            InternalCreateDirectory(fullPath, path, null);
        }

        [SecurityCritical]
        internal static void Delete(string path, bool recursive)
        {
            InternalDelete(LongPath.NormalizePath(path), path, recursive);
        }

        [SecurityCritical]
        private static void DeleteHelper(string fullPath, string userPath, bool recursive)
        {
            int num;
            Exception exception = null;
            if (recursive)
            {
                Win32Native.WIN32_FIND_DATA data = new Win32Native.WIN32_FIND_DATA();
                string fileName = null;
                if (fullPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                {
                    fileName = fullPath + "*";
                }
                else
                {
                    fileName = fullPath + Path.DirectorySeparatorChar + "*";
                }
                using (SafeFindHandle handle = Win32Native.FindFirstFile(fileName, data))
                {
                    if (handle.IsInvalid)
                    {
                        num = Marshal.GetLastWin32Error();
                        __Error.WinIOError(num, userPath);
                    }
                    do
                    {
                        if (0 != (data.dwFileAttributes & 0x10))
                        {
                            if (!data.cFileName.Equals(".") && !data.cFileName.Equals(".."))
                            {
                                if (0 == (data.dwFileAttributes & 0x400))
                                {
                                    string str2 = LongPath.InternalCombine(fullPath, data.cFileName);
                                    string str3 = LongPath.InternalCombine(userPath, data.cFileName);
                                    try
                                    {
                                        DeleteHelper(str2, str3, recursive);
                                    }
                                    catch (Exception exception2)
                                    {
                                        if (exception == null)
                                        {
                                            exception = exception2;
                                        }
                                    }
                                }
                                else
                                {
                                    if ((data.dwReserved0 == -1610612733) && !Win32Native.DeleteVolumeMountPoint(LongPath.InternalCombine(fullPath, data.cFileName + Path.DirectorySeparatorChar)))
                                    {
                                        num = Marshal.GetLastWin32Error();
                                        try
                                        {
                                            __Error.WinIOError(num, data.cFileName);
                                        }
                                        catch (Exception exception3)
                                        {
                                            if (exception == null)
                                            {
                                                exception = exception3;
                                            }
                                        }
                                    }
                                    if (!Win32Native.RemoveDirectory(LongPath.InternalCombine(fullPath, data.cFileName)))
                                    {
                                        num = Marshal.GetLastWin32Error();
                                        try
                                        {
                                            __Error.WinIOError(num, data.cFileName);
                                        }
                                        catch (Exception exception4)
                                        {
                                            if (exception == null)
                                            {
                                                exception = exception4;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (!Win32Native.DeleteFile(LongPath.InternalCombine(fullPath, data.cFileName)))
                        {
                            num = Marshal.GetLastWin32Error();
                            try
                            {
                                __Error.WinIOError(num, data.cFileName);
                            }
                            catch (Exception exception5)
                            {
                                if (exception == null)
                                {
                                    exception = exception5;
                                }
                            }
                        }
                    }
                    while (Win32Native.FindNextFile(handle, data));
                    num = Marshal.GetLastWin32Error();
                }
                if (exception != null)
                {
                    throw exception;
                }
                if ((num != 0) && (num != 0x12))
                {
                    __Error.WinIOError(num, userPath);
                }
            }
            if (!Win32Native.RemoveDirectory(fullPath))
            {
                num = Marshal.GetLastWin32Error();
                switch (num)
                {
                    case 2:
                        num = 3;
                        break;

                    case 5:
                        throw new IOException(Environment.GetResourceString("UnauthorizedAccess_IODenied_Path", new object[] { userPath }));
                }
                __Error.WinIOError(num, userPath);
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
                string fullPath = LongPath.NormalizePath(path);
                string demandDir = GetDemandDir(fullPath, true);
                new FileIOPermission(FileIOPermissionAccess.Read, new string[] { demandDir }, false, false).Demand();
                return InternalExists(fullPath);
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

        private static string GetDemandDir(string fullPath, bool thisDirOnly)
        {
            fullPath = Path.RemoveLongPathPrefix(fullPath);
            if (thisDirOnly)
            {
                if (fullPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) || fullPath.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                {
                    return (fullPath + '.');
                }
                return (fullPath + Path.DirectorySeparatorChar + '.');
            }
            if (!fullPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) && !fullPath.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                return (fullPath + Path.DirectorySeparatorChar);
            }
            return fullPath;
        }

        [SecurityCritical]
        private static unsafe void InternalCreateDirectory(string fullPath, string path, object dirSecurityObj)
        {
            DirectorySecurity security = (DirectorySecurity) dirSecurityObj;
            int length = fullPath.Length;
            if ((length >= 2) && Path.IsDirectorySeparator(fullPath[length - 1]))
            {
                length--;
            }
            int rootLength = LongPath.GetRootLength(fullPath);
            if ((length == 2) && Path.IsDirectorySeparator(fullPath[1]))
            {
                throw new IOException(Environment.GetResourceString("IO.IO_CannotCreateDirectory", new object[] { path }));
            }
            List<string> list = new List<string>();
            bool flag = false;
            if (length > rootLength)
            {
                for (int i = length - 1; (i >= rootLength) && !flag; i--)
                {
                    string str = fullPath.Substring(0, i + 1);
                    if (!InternalExists(str))
                    {
                        list.Add(str);
                    }
                    else
                    {
                        flag = true;
                    }
                    while (((i > rootLength) && (fullPath[i] != Path.DirectorySeparatorChar)) && (fullPath[i] != Path.AltDirectorySeparatorChar))
                    {
                        i--;
                    }
                }
            }
            int count = list.Count;
            if (list.Count != 0)
            {
                string[] array = new string[list.Count];
                list.CopyTo(array, 0);
                for (int j = 0; j < array.Length; j++)
                {
                    string[] strArray2;
                    IntPtr ptr;
                    (strArray2 = array)[(int) (ptr = (IntPtr) j)] = strArray2[(int) ptr] + @"\.";
                }
                AccessControlActions control = (security == null) ? AccessControlActions.None : AccessControlActions.Change;
                new FileIOPermission(FileIOPermissionAccess.Write, control, array, false, false).Demand();
            }
            Win32Native.SECURITY_ATTRIBUTES structure = null;
            if (security != null)
            {
                structure = new Win32Native.SECURITY_ATTRIBUTES {
                    nLength = Marshal.SizeOf(structure)
                };
                byte[] securityDescriptorBinaryForm = security.GetSecurityDescriptorBinaryForm();
                byte* pDest = stackalloc byte[(IntPtr) securityDescriptorBinaryForm.Length];
                Buffer.memcpy(securityDescriptorBinaryForm, 0, pDest, 0, securityDescriptorBinaryForm.Length);
                structure.pSecurityDescriptor = pDest;
            }
            bool flag2 = true;
            int errorCode = 0;
            string maybeFullPath = path;
            while (list.Count > 0)
            {
                string str3 = list[list.Count - 1];
                list.RemoveAt(list.Count - 1);
                if (str3.Length >= Path.MaxLongPath)
                {
                    throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
                }
                flag2 = Win32Native.CreateDirectory(Path.AddLongPathPrefix(str3), structure);
                if (!flag2 && (errorCode == 0))
                {
                    int lastError = Marshal.GetLastWin32Error();
                    if (lastError != 0xb7)
                    {
                        errorCode = lastError;
                    }
                    else if (LongPathFile.InternalExists(str3) || (!InternalExists(str3, out lastError) && (lastError == 5)))
                    {
                        errorCode = lastError;
                        try
                        {
                            new FileIOPermission(FileIOPermissionAccess.PathDiscovery, new string[] { GetDemandDir(str3, true) }, false, false).Demand();
                            maybeFullPath = str3;
                            continue;
                        }
                        catch (SecurityException)
                        {
                            continue;
                        }
                    }
                }
            }
            if ((count == 0) && !flag)
            {
                if (!InternalExists(InternalGetDirectoryRoot(fullPath)))
                {
                    __Error.WinIOError(3, InternalGetDirectoryRoot(path));
                }
            }
            else if (!flag2 && (errorCode != 0))
            {
                __Error.WinIOError(errorCode, maybeFullPath);
            }
        }

        [SecurityCritical]
        private static void InternalDelete(string fullPath, string userPath, bool recursive)
        {
            string demandDir = GetDemandDir(fullPath, !recursive);
            new FileIOPermission(FileIOPermissionAccess.Write, new string[] { demandDir }, false, false).Demand();
            string path = Path.AddLongPathPrefix(fullPath);
            Win32Native.WIN32_FILE_ATTRIBUTE_DATA data = new Win32Native.WIN32_FILE_ATTRIBUTE_DATA();
            int errorCode = File.FillAttributeInfo(path, ref data, false, true);
            switch (errorCode)
            {
                case 0:
                    goto Label_0051;

                case 2:
                    errorCode = 3;
                    break;
            }
            __Error.WinIOError(errorCode, fullPath);
        Label_0051:
            if ((data.fileAttributes & 0x400) != 0)
            {
                recursive = false;
            }
            DeleteHelper(path, userPath, recursive);
        }

        [SecurityCritical]
        internal static bool InternalExists(string path)
        {
            int lastError = 0;
            return InternalExists(path, out lastError);
        }

        [SecurityCritical]
        internal static bool InternalExists(string path, out int lastError)
        {
            return Directory.InternalExists(Path.AddLongPathPrefix(path), out lastError);
        }

        private static string InternalGetDirectoryRoot(string path)
        {
            if (path == null)
            {
                return null;
            }
            return path.Substring(0, LongPath.GetRootLength(path));
        }

        [SecurityCritical]
        internal static void Move(string sourceDirName, string destDirName)
        {
            string fullPath = LongPath.NormalizePath(sourceDirName);
            string demandDir = GetDemandDir(fullPath, false);
            if (demandDir.Length >= Path.MaxLongPath)
            {
                throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
            }
            string strB = GetDemandDir(LongPath.NormalizePath(destDirName), false);
            if (strB.Length >= Path.MaxLongPath)
            {
                throw new PathTooLongException(Environment.GetResourceString("IO.PathTooLong"));
            }
            new FileIOPermission(FileIOPermissionAccess.Write | FileIOPermissionAccess.Read, new string[] { demandDir }, false, false).Demand();
            new FileIOPermission(FileIOPermissionAccess.Write, new string[] { strB }, false, false).Demand();
            if (string.Compare(demandDir, strB, StringComparison.OrdinalIgnoreCase) == 0)
            {
                throw new IOException(Environment.GetResourceString("IO.IO_SourceDestMustBeDifferent"));
            }
            string pathRoot = LongPath.GetPathRoot(demandDir);
            string str6 = LongPath.GetPathRoot(strB);
            if (string.Compare(pathRoot, str6, StringComparison.OrdinalIgnoreCase) != 0)
            {
                throw new IOException(Environment.GetResourceString("IO.IO_SourceDestMustHaveSameRoot"));
            }
            string src = Path.AddLongPathPrefix(sourceDirName);
            string dst = Path.AddLongPathPrefix(destDirName);
            if (!Win32Native.MoveFile(src, dst))
            {
                int errorCode = Marshal.GetLastWin32Error();
                switch (errorCode)
                {
                    case 2:
                        errorCode = 3;
                        __Error.WinIOError(errorCode, fullPath);
                        break;

                    case 5:
                        throw new IOException(Environment.GetResourceString("UnauthorizedAccess_IODenied_Path", new object[] { sourceDirName }), Win32Native.MakeHRFromErrorCode(errorCode));
                }
                __Error.WinIOError(errorCode, string.Empty);
            }
        }
    }
}

