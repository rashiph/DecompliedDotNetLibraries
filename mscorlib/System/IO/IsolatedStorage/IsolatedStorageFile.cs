namespace System.IO.IsolatedStorage
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.ExceptionServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Text;
    using System.Threading;

    [ComVisible(true)]
    public sealed class IsolatedStorageFile : System.IO.IsolatedStorage.IsolatedStorage, IDisposable
    {
        private bool m_bDisposed;
        private bool m_closed;
        private FileIOPermission m_fiop;
        [SecurityCritical]
        private SafeIsolatedStorageFileHandle m_handle;
        private string m_InfoFile;
        private object m_internalLock = new object();
        private string m_RootDir;
        private IsolatedStorageScope m_StoreScope;
        private string m_SyncObjectName;
        private static string s_appDataDir;
        internal const string s_AppFiles = "AppFiles";
        internal const string s_AppInfoFile = "appinfo.dat";
        internal const string s_AssemFiles = "AssemFiles";
        private const int s_BlockSize = 0x400;
        private const int s_DirSize = 0x400;
        internal const string s_Files = "Files";
        internal const string s_IDFile = "identity.dat";
        internal const string s_InfoFile = "info.dat";
        private const string s_name = "file.store";
        private static IsolatedStorageFilePermission s_PermAdminUser;
        private static FileIOPermission s_PermMachine;
        private static FileIOPermission s_PermRoaming;
        private static FileIOPermission s_PermUser;
        private static string s_RootDirMachine;
        private static string s_RootDirRoaming;
        private static string s_RootDirUser;

        internal IsolatedStorageFile()
        {
        }

        [SecuritySafeCritical]
        public void Close()
        {
            if (!base.IsRoaming())
            {
                lock (this.m_internalLock)
                {
                    if (!this.m_closed)
                    {
                        this.m_closed = true;
                        if (this.m_handle != null)
                        {
                            this.m_handle.Dispose();
                        }
                        GC.SuppressFinalize(this);
                    }
                }
            }
        }

        private bool ContainsUnknownFiles(string rootDir)
        {
            string[] strArray;
            string[] strArray2;
            try
            {
                strArray2 = GetFileDirectoryNames(rootDir + "*", "*", true);
                strArray = GetFileDirectoryNames(rootDir + "*", "*", false);
            }
            catch
            {
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_DeleteDirectories"));
            }
            if ((strArray != null) && (strArray.Length > 0))
            {
                if (strArray.Length > 1)
                {
                    return true;
                }
                if (base.IsApp())
                {
                    if (NotAppFilesDir(strArray[0]))
                    {
                        return true;
                    }
                }
                else if (base.IsDomain())
                {
                    if (NotFilesDir(strArray[0]))
                    {
                        return true;
                    }
                }
                else if (NotAssemFilesDir(strArray[0]))
                {
                    return true;
                }
            }
            if ((strArray2 == null) || (strArray2.Length == 0))
            {
                return false;
            }
            if (base.IsRoaming())
            {
                if ((strArray2.Length <= 1) && !NotIDFile(strArray2[0]))
                {
                    return false;
                }
                return true;
            }
            if (((strArray2.Length <= 2) && (!NotIDFile(strArray2[0]) || !NotInfoFile(strArray2[0]))) && (((strArray2.Length != 2) || !NotIDFile(strArray2[1])) || !NotInfoFile(strArray2[1])))
            {
                return false;
            }
            return true;
        }

        [ComVisible(false)]
        public void CopyFile(string sourceFileName, string destinationFileName)
        {
            if (sourceFileName == null)
            {
                throw new ArgumentNullException("sourceFileName");
            }
            if (destinationFileName == null)
            {
                throw new ArgumentNullException("destinationFileName");
            }
            if (sourceFileName.Trim().Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"), "sourceFileName");
            }
            if (destinationFileName.Trim().Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"), "destinationFileName");
            }
            this.CopyFile(sourceFileName, destinationFileName, false);
        }

        [SecuritySafeCritical, ComVisible(false)]
        public void CopyFile(string sourceFileName, string destinationFileName, bool overwrite)
        {
            if (sourceFileName == null)
            {
                throw new ArgumentNullException("sourceFileName");
            }
            if (destinationFileName == null)
            {
                throw new ArgumentNullException("destinationFileName");
            }
            if (sourceFileName.Trim().Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"), "sourceFileName");
            }
            if (destinationFileName.Trim().Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"), "destinationFileName");
            }
            if (this.m_bDisposed)
            {
                throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
            }
            if (this.m_closed)
            {
                throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
            }
            this.m_fiop.Assert();
            this.m_fiop.PermitOnly();
            string path = LongPath.NormalizePath(this.GetFullPath(sourceFileName));
            string str2 = LongPath.NormalizePath(this.GetFullPath(destinationFileName));
            try
            {
                Demand(new FileIOPermission(FileIOPermissionAccess.Write | FileIOPermissionAccess.Read, new string[] { path }, false, false));
                Demand(new FileIOPermission(FileIOPermissionAccess.Write, new string[] { str2 }, false, false));
            }
            catch
            {
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Operation"));
            }
            bool locked = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this.Lock(ref locked);
                long length = 0L;
                try
                {
                    length = LongPathFile.GetLength(path);
                }
                catch (FileNotFoundException)
                {
                    throw new FileNotFoundException(Environment.GetResourceString("IO.PathNotFound_Path", new object[] { sourceFileName }));
                }
                catch (DirectoryNotFoundException)
                {
                    throw new DirectoryNotFoundException(Environment.GetResourceString("IO.PathNotFound_Path", new object[] { sourceFileName }));
                }
                catch
                {
                    throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Operation"));
                }
                long num2 = 0L;
                if (LongPathFile.Exists(str2))
                {
                    try
                    {
                        num2 = LongPathFile.GetLength(str2);
                    }
                    catch
                    {
                        throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Operation"));
                    }
                }
                if (num2 < length)
                {
                    this.Reserve(RoundToBlockSize((ulong) (length - num2)));
                }
                try
                {
                    LongPathFile.Copy(path, str2, overwrite);
                }
                catch (FileNotFoundException)
                {
                    if (num2 < length)
                    {
                        this.Unreserve(RoundToBlockSize((ulong) (length - num2)));
                    }
                    throw new FileNotFoundException(Environment.GetResourceString("IO.PathNotFound_Path", new object[] { sourceFileName }));
                }
                catch
                {
                    if (num2 < length)
                    {
                        this.Unreserve(RoundToBlockSize((ulong) (length - num2)));
                    }
                    throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Operation"));
                }
                if ((num2 > length) && overwrite)
                {
                    this.Unreserve(RoundToBlockSizeFloor((ulong) (num2 - length)));
                }
            }
            finally
            {
                if (locked)
                {
                    this.Unlock();
                }
            }
        }

        [SecuritySafeCritical]
        public void CreateDirectory(string dir)
        {
            if (dir == null)
            {
                throw new ArgumentNullException("dir");
            }
            this.m_fiop.Assert();
            this.m_fiop.PermitOnly();
            string fullPath = this.GetFullPath(dir);
            string str2 = LongPath.NormalizePath(fullPath);
            try
            {
                Demand(new FileIOPermission(FileIOPermissionAccess.Read, new string[] { str2 }, false, false));
            }
            catch
            {
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_CreateDirectory"));
            }
            string[] strArray = this.DirectoriesToCreate(str2);
            if ((strArray == null) || (strArray.Length == 0))
            {
                if (!LongPathDirectory.Exists(fullPath))
                {
                    throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_CreateDirectory"));
                }
            }
            else
            {
                this.Reserve((ulong) (0x400L * strArray.Length));
                try
                {
                    LongPathDirectory.CreateDirectory(strArray[strArray.Length - 1]);
                }
                catch
                {
                    this.Unreserve((ulong) (0x400L * strArray.Length));
                    try
                    {
                        LongPathDirectory.Delete(strArray[0], true);
                    }
                    catch
                    {
                    }
                    throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_CreateDirectory"));
                }
                CodeAccessPermission.RevertAll();
            }
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern void CreateDirectoryWithDacl(string path);
        [ComVisible(false)]
        public IsolatedStorageFileStream CreateFile(string path)
        {
            return new IsolatedStorageFileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None, this);
        }

        [SecuritySafeCritical]
        internal void CreateIDFile(string path, IsolatedStorageScope scope)
        {
            try
            {
                using (FileStream stream = new FileStream(path + "identity.dat", FileMode.OpenOrCreate))
                {
                    MemoryStream identityStream = base.GetIdentityStream(scope);
                    byte[] buffer = identityStream.GetBuffer();
                    stream.Write(buffer, 0, (int) identityStream.Length);
                    identityStream.Close();
                }
            }
            catch
            {
            }
        }

        [SecurityCritical]
        internal static Mutex CreateMutexNotOwned(string pathName)
        {
            return new Mutex(false, @"Global\" + GetStrongHashSuitableForObjectName(pathName));
        }

        [HandleProcessCorruptedStateExceptions, SecuritySafeCritical]
        internal static string CreateRandomDirectory(string rootDir)
        {
            string str;
            string str2;
            do
            {
                str = Path.GetRandomFileName() + @"\" + Path.GetRandomFileName();
                str2 = rootDir + str;
            }
            while (LongPathDirectory.Exists(str2));
            try
            {
                LongPathDirectory.CreateDirectory(str2);
            }
            catch
            {
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Init"));
            }
            return str;
        }

        [SecuritySafeCritical]
        public void DeleteDirectory(string dir)
        {
            if (dir == null)
            {
                throw new ArgumentNullException("dir");
            }
            this.m_fiop.Assert();
            this.m_fiop.PermitOnly();
            bool locked = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this.Lock(ref locked);
                try
                {
                    string path = LongPath.NormalizePath(this.GetFullPath(dir));
                    if (path.Equals(LongPath.NormalizePath(this.GetFullPath(".")), StringComparison.Ordinal))
                    {
                        throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_DeleteDirectory"));
                    }
                    LongPathDirectory.Delete(path, false);
                }
                catch
                {
                    throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_DeleteDirectory"));
                }
                this.Unreserve(0x400L);
            }
            finally
            {
                if (locked)
                {
                    this.Unlock();
                }
            }
            CodeAccessPermission.RevertAll();
        }

        [SecuritySafeCritical]
        public void DeleteFile(string file)
        {
            if (file == null)
            {
                throw new ArgumentNullException("file");
            }
            this.m_fiop.Assert();
            this.m_fiop.PermitOnly();
            long length = 0L;
            bool locked = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this.Lock(ref locked);
                try
                {
                    string fullPath = this.GetFullPath(file);
                    length = LongPathFile.GetLength(fullPath);
                    LongPathFile.Delete(fullPath);
                }
                catch
                {
                    throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_DeleteFile"));
                }
                this.Unreserve(RoundToBlockSize((ulong) length));
            }
            finally
            {
                if (locked)
                {
                    this.Unlock();
                }
            }
            CodeAccessPermission.RevertAll();
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecurityCritical]
        private static void Demand(CodeAccessPermission permission)
        {
            permission.Demand();
        }

        [SecurityCritical]
        private static void DemandAdminPermission()
        {
            if (s_PermAdminUser == null)
            {
                s_PermAdminUser = new IsolatedStorageFilePermission(IsolatedStorageContainment.AdministerIsolatedStorageByUser, 0L, false);
            }
            s_PermAdminUser.Demand();
        }

        [SecurityCritical]
        private string[] DirectoriesToCreate(string fullPath)
        {
            List<string> list = new List<string>();
            int length = fullPath.Length;
            if ((length >= 2) && (fullPath[length - 1] == this.SeparatorExternal))
            {
                length--;
            }
            int rootLength = LongPath.GetRootLength(fullPath);
            while (rootLength < length)
            {
                rootLength++;
                while ((rootLength < length) && (fullPath[rootLength] != this.SeparatorExternal))
                {
                    rootLength++;
                }
                string path = fullPath.Substring(0, rootLength);
                if (!LongPathDirectory.InternalExists(path))
                {
                    list.Add(path);
                }
            }
            if (list.Count != 0)
            {
                return list.ToArray();
            }
            return null;
        }

        [SecuritySafeCritical, ComVisible(false)]
        public bool DirectoryExists(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (this.m_bDisposed)
            {
                throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
            }
            if (this.m_closed)
            {
                throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
            }
            this.m_fiop.Assert();
            this.m_fiop.PermitOnly();
            string fullPath = this.GetFullPath(path);
            string str2 = LongPath.NormalizePath(fullPath);
            if (fullPath.EndsWith(Path.DirectorySeparatorChar + ".", StringComparison.Ordinal))
            {
                if (str2.EndsWith(Path.DirectorySeparatorChar))
                {
                    str2 = str2 + ".";
                }
                else
                {
                    str2 = str2 + Path.DirectorySeparatorChar + ".";
                }
            }
            try
            {
                Demand(new FileIOPermission(FileIOPermissionAccess.Read, new string[] { str2 }, false, false));
            }
            catch
            {
                return false;
            }
            bool flag = LongPathDirectory.Exists(str2);
            CodeAccessPermission.RevertAll();
            return flag;
        }

        public void Dispose()
        {
            this.Close();
            this.m_bDisposed = true;
        }

        [SecuritySafeCritical]
        internal void EnsureStoreIsValid()
        {
            if (this.m_bDisposed)
            {
                throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
            }
            if (this.m_closed)
            {
                throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
            }
        }

        [SecuritySafeCritical, ComVisible(false)]
        public bool FileExists(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (this.m_bDisposed)
            {
                throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
            }
            if (this.m_closed)
            {
                throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
            }
            this.m_fiop.Assert();
            this.m_fiop.PermitOnly();
            string str2 = LongPath.NormalizePath(this.GetFullPath(path));
            if (path.EndsWith(Path.DirectorySeparatorChar + ".", StringComparison.Ordinal))
            {
                if (str2.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
                {
                    str2 = str2 + ".";
                }
                else
                {
                    str2 = str2 + Path.DirectorySeparatorChar + ".";
                }
            }
            try
            {
                Demand(new FileIOPermission(FileIOPermissionAccess.Read, new string[] { str2 }, false, false));
            }
            catch
            {
                return false;
            }
            bool flag = LongPathFile.Exists(str2);
            CodeAccessPermission.RevertAll();
            return flag;
        }

        ~IsolatedStorageFile()
        {
            this.Dispose();
        }

        [ComVisible(false), SecuritySafeCritical]
        public DateTimeOffset GetCreationTime(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path.Trim().Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"), "path");
            }
            if (this.m_bDisposed)
            {
                throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
            }
            if (this.m_closed)
            {
                throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
            }
            this.m_fiop.Assert();
            this.m_fiop.PermitOnly();
            string str2 = LongPath.NormalizePath(this.GetFullPath(path));
            try
            {
                Demand(new FileIOPermission(FileIOPermissionAccess.Read, new string[] { str2 }, false, false));
            }
            catch
            {
                DateTimeOffset offset3 = new DateTimeOffset(0x641, 1, 1, 0, 0, 0, TimeSpan.Zero);
                return offset3.ToLocalTime();
            }
            DateTimeOffset creationTime = LongPathFile.GetCreationTime(str2);
            CodeAccessPermission.RevertAll();
            return creationTime;
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
        private static string GetDataDirectoryFromActivationContext()
        {
            if (s_appDataDir == null)
            {
                ActivationContext activationContext = AppDomain.CurrentDomain.ActivationContext;
                if (activationContext == null)
                {
                    throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_ApplicationMissingIdentity"));
                }
                string dataDirectory = activationContext.DataDirectory;
                if ((dataDirectory != null) && (dataDirectory[dataDirectory.Length - 1] != '\\'))
                {
                    dataDirectory = dataDirectory + @"\";
                }
                s_appDataDir = dataDirectory;
            }
            return s_appDataDir;
        }

        [ComVisible(false), SecuritySafeCritical]
        public string[] GetDirectoryNames()
        {
            return this.GetDirectoryNames("*");
        }

        [SecuritySafeCritical]
        public string[] GetDirectoryNames(string searchPattern)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException("searchPattern");
            }
            if (this.m_bDisposed)
            {
                throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
            }
            if (this.m_closed)
            {
                throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
            }
            this.m_fiop.Assert();
            this.m_fiop.PermitOnly();
            string[] strArray = GetFileDirectoryNames(this.GetFullPath(searchPattern), searchPattern, false);
            CodeAccessPermission.RevertAll();
            return strArray;
        }

        [SecuritySafeCritical]
        public static IEnumerator GetEnumerator(IsolatedStorageScope scope)
        {
            VerifyGlobalScope(scope);
            DemandAdminPermission();
            return new IsolatedStorageFileEnumerator(scope);
        }

        [SecuritySafeCritical]
        internal static string[] GetFileDirectoryNames(string path, string userSearchPattern, bool file)
        {
            int num;
            if (path == null)
            {
                throw new ArgumentNullException("path", Environment.GetResourceString("ArgumentNull_Path"));
            }
            userSearchPattern = NormalizeSearchPattern(userSearchPattern);
            if (userSearchPattern.Length == 0)
            {
                return new string[0];
            }
            bool flag = false;
            char ch = path[path.Length - 1];
            if (((ch == Path.DirectorySeparatorChar) || (ch == Path.AltDirectorySeparatorChar)) || (ch == '.'))
            {
                flag = true;
            }
            string str = LongPath.NormalizePath(path);
            if (flag && (str[str.Length - 1] != ch))
            {
                str = str + @"\*";
            }
            string directoryName = LongPath.GetDirectoryName(str);
            if (directoryName != null)
            {
                directoryName = directoryName + @"\";
            }
            try
            {
                string[] pathList = new string[] { (directoryName == null) ? str : directoryName };
                new FileIOPermission(FileIOPermissionAccess.Read, pathList, false, false).Demand();
            }
            catch
            {
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Operation"));
            }
            string[] sourceArray = new string[10];
            int length = 0;
            Win32Native.WIN32_FIND_DATA data = new Win32Native.WIN32_FIND_DATA();
            SafeFindHandle hndFindFile = Win32Native.FindFirstFile(Path.AddLongPathPrefix(str), data);
            if (hndFindFile.IsInvalid)
            {
                num = Marshal.GetLastWin32Error();
                if (num == 2)
                {
                    return new string[0];
                }
                __Error.WinIOError(num, userSearchPattern);
            }
            int num3 = 0;
            do
            {
                bool flag2;
                if (file)
                {
                    flag2 = 0 == (data.dwFileAttributes & 0x10);
                }
                else
                {
                    flag2 = 0 != (data.dwFileAttributes & 0x10);
                    if (flag2 && (data.cFileName.Equals(".") || data.cFileName.Equals("..")))
                    {
                        flag2 = false;
                    }
                }
                if (flag2)
                {
                    num3++;
                    if (length == sourceArray.Length)
                    {
                        string[] strArray3 = new string[sourceArray.Length * 2];
                        Array.Copy(sourceArray, 0, strArray3, 0, length);
                        sourceArray = strArray3;
                    }
                    sourceArray[length++] = data.cFileName;
                }
            }
            while (Win32Native.FindNextFile(hndFindFile, data));
            num = Marshal.GetLastWin32Error();
            hndFindFile.Close();
            if ((num != 0) && (num != 0x12))
            {
                __Error.WinIOError(num, userSearchPattern);
            }
            if ((!file && (num3 == 1)) && ((data.dwFileAttributes & 0x10) != 0))
            {
                return new string[] { data.cFileName };
            }
            if (length == sourceArray.Length)
            {
                return sourceArray;
            }
            string[] destinationArray = new string[length];
            Array.Copy(sourceArray, 0, destinationArray, 0, length);
            return destinationArray;
        }

        [ComVisible(false)]
        public string[] GetFileNames()
        {
            return this.GetFileNames("*");
        }

        [SecuritySafeCritical]
        public string[] GetFileNames(string searchPattern)
        {
            if (searchPattern == null)
            {
                throw new ArgumentNullException("searchPattern");
            }
            if (this.m_bDisposed)
            {
                throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
            }
            if (this.m_closed)
            {
                throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
            }
            this.m_fiop.Assert();
            this.m_fiop.PermitOnly();
            string[] strArray = GetFileDirectoryNames(this.GetFullPath(searchPattern), searchPattern, true);
            CodeAccessPermission.RevertAll();
            return strArray;
        }

        internal string GetFullPath(string path)
        {
            if (path == string.Empty)
            {
                return this.RootDirectory;
            }
            StringBuilder builder = new StringBuilder();
            builder.Append(this.RootDirectory);
            if (path[0] == this.SeparatorExternal)
            {
                builder.Append(path.Substring(1));
            }
            else
            {
                builder.Append(path);
            }
            return builder.ToString();
        }

        [SecurityCritical]
        internal static FileIOPermission GetGlobalFileIOPerm(IsolatedStorageScope scope)
        {
            if (System.IO.IsolatedStorage.IsolatedStorage.IsRoaming(scope))
            {
                if (s_PermRoaming == null)
                {
                    s_PermRoaming = new FileIOPermission(FileIOPermissionAccess.AllAccess, GetRootDir(scope));
                }
                return s_PermRoaming;
            }
            if (System.IO.IsolatedStorage.IsolatedStorage.IsMachine(scope))
            {
                if (s_PermMachine == null)
                {
                    s_PermMachine = new FileIOPermission(FileIOPermissionAccess.AllAccess, GetRootDir(scope));
                }
                return s_PermMachine;
            }
            if (s_PermUser == null)
            {
                s_PermUser = new FileIOPermission(FileIOPermissionAccess.AllAccess, GetRootDir(scope));
            }
            return s_PermUser;
        }

        [SecuritySafeCritical, ComVisible(false)]
        public DateTimeOffset GetLastAccessTime(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path.Trim().Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"), "path");
            }
            if (this.m_bDisposed)
            {
                throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
            }
            if (this.m_closed)
            {
                throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
            }
            this.m_fiop.Assert();
            this.m_fiop.PermitOnly();
            string str2 = LongPath.NormalizePath(this.GetFullPath(path));
            try
            {
                Demand(new FileIOPermission(FileIOPermissionAccess.Read, new string[] { str2 }, false, false));
            }
            catch
            {
                DateTimeOffset offset3 = new DateTimeOffset(0x641, 1, 1, 0, 0, 0, TimeSpan.Zero);
                return offset3.ToLocalTime();
            }
            DateTimeOffset lastAccessTime = LongPathFile.GetLastAccessTime(str2);
            CodeAccessPermission.RevertAll();
            return lastAccessTime;
        }

        [SecuritySafeCritical, ComVisible(false)]
        public DateTimeOffset GetLastWriteTime(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (path.Trim().Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"), "path");
            }
            if (this.m_bDisposed)
            {
                throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
            }
            if (this.m_closed)
            {
                throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
            }
            this.m_fiop.Assert();
            this.m_fiop.PermitOnly();
            string str2 = LongPath.NormalizePath(this.GetFullPath(path));
            try
            {
                Demand(new FileIOPermission(FileIOPermissionAccess.Read, new string[] { str2 }, false, false));
            }
            catch
            {
                DateTimeOffset offset3 = new DateTimeOffset(0x641, 1, 1, 0, 0, 0, TimeSpan.Zero);
                return offset3.ToLocalTime();
            }
            DateTimeOffset lastWriteTime = LongPathFile.GetLastWriteTime(str2);
            CodeAccessPermission.RevertAll();
            return lastWriteTime;
        }

        internal static string GetMachineRandomDirectory(string rootDir)
        {
            string[] strArray = GetFileDirectoryNames(rootDir + "*", "*", false);
            for (int i = 0; i < strArray.Length; i++)
            {
                if (strArray[i].Length == 12)
                {
                    string[] strArray2 = GetFileDirectoryNames(rootDir + strArray[i] + @"\*", "*", false);
                    for (int j = 0; j < strArray2.Length; j++)
                    {
                        if (strArray2[j].Length == 12)
                        {
                            return (strArray[i] + @"\" + strArray2[j]);
                        }
                    }
                }
            }
            return null;
        }

        [SecuritySafeCritical]
        public static IsolatedStorageFile GetMachineStoreForApplication()
        {
            return GetStore(IsolatedStorageScope.Application | IsolatedStorageScope.Machine, (Type) null);
        }

        [SecuritySafeCritical]
        public static IsolatedStorageFile GetMachineStoreForAssembly()
        {
            return GetStore(IsolatedStorageScope.Machine | IsolatedStorageScope.Assembly, (Type) null, (Type) null);
        }

        [SecuritySafeCritical]
        public static IsolatedStorageFile GetMachineStoreForDomain()
        {
            return GetStore(IsolatedStorageScope.Machine | IsolatedStorageScope.Assembly | IsolatedStorageScope.Domain, (Type) null, (Type) null);
        }

        protected override IsolatedStoragePermission GetPermission(PermissionSet ps)
        {
            if (ps == null)
            {
                return null;
            }
            if (ps.IsUnrestricted())
            {
                return new IsolatedStorageFilePermission(PermissionState.Unrestricted);
            }
            return (IsolatedStoragePermission) ps.GetPermission(typeof(IsolatedStorageFilePermission));
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern bool GetQuota(SafeIsolatedStorageFileHandle scope, out long quota);
        internal static string GetRandomDirectory(string rootDir, out bool bMigrateNeeded, out string sOldStoreLocation)
        {
            bMigrateNeeded = false;
            sOldStoreLocation = null;
            string[] strArray = GetFileDirectoryNames(rootDir + "*", "*", false);
            for (int i = 0; i < strArray.Length; i++)
            {
                if (strArray[i].Length == 12)
                {
                    string[] strArray2 = GetFileDirectoryNames(rootDir + strArray[i] + @"\*", "*", false);
                    for (int k = 0; k < strArray2.Length; k++)
                    {
                        if (strArray2[k].Length == 12)
                        {
                            return (strArray[i] + @"\" + strArray2[k]);
                        }
                    }
                }
            }
            for (int j = 0; j < strArray.Length; j++)
            {
                if (strArray[j].Length == 0x18)
                {
                    bMigrateNeeded = true;
                    sOldStoreLocation = strArray[j];
                    return null;
                }
            }
            return null;
        }

        [SecurityCritical]
        internal static string GetRootDir(IsolatedStorageScope scope)
        {
            if (System.IO.IsolatedStorage.IsolatedStorage.IsRoaming(scope))
            {
                if (s_RootDirRoaming == null)
                {
                    string s = null;
                    GetRootDir(scope, JitHelpers.GetStringHandleOnStack(ref s));
                    s_RootDirRoaming = s;
                }
                return s_RootDirRoaming;
            }
            if (System.IO.IsolatedStorage.IsolatedStorage.IsMachine(scope))
            {
                if (s_RootDirMachine == null)
                {
                    InitGlobalsMachine(scope);
                }
                return s_RootDirMachine;
            }
            if (s_RootDirUser == null)
            {
                InitGlobalsNonRoamingUser(scope);
            }
            return s_RootDirUser;
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern void GetRootDir(IsolatedStorageScope scope, StringHandleOnStack retRootDir);
        [SecuritySafeCritical]
        public static IsolatedStorageFile GetStore(IsolatedStorageScope scope, object applicationIdentity)
        {
            if (applicationIdentity == null)
            {
                throw new ArgumentNullException("applicationIdentity");
            }
            DemandAdminPermission();
            IsolatedStorageFile file = new IsolatedStorageFile();
            file.InitStore(scope, null, null, applicationIdentity);
            file.Init(scope);
            return file;
        }

        [SecuritySafeCritical]
        public static IsolatedStorageFile GetStore(IsolatedStorageScope scope, Type applicationEvidenceType)
        {
            if (applicationEvidenceType != null)
            {
                DemandAdminPermission();
            }
            IsolatedStorageFile file = new IsolatedStorageFile();
            file.InitStore(scope, applicationEvidenceType);
            file.Init(scope);
            return file;
        }

        [SecuritySafeCritical]
        public static IsolatedStorageFile GetStore(IsolatedStorageScope scope, object domainIdentity, object assemblyIdentity)
        {
            if (assemblyIdentity == null)
            {
                throw new ArgumentNullException("assemblyIdentity");
            }
            if (System.IO.IsolatedStorage.IsolatedStorage.IsDomain(scope) && (domainIdentity == null))
            {
                throw new ArgumentNullException("domainIdentity");
            }
            DemandAdminPermission();
            IsolatedStorageFile file = new IsolatedStorageFile();
            file.InitStore(scope, domainIdentity, assemblyIdentity, null);
            file.Init(scope);
            return file;
        }

        [SecuritySafeCritical]
        public static IsolatedStorageFile GetStore(IsolatedStorageScope scope, Type domainEvidenceType, Type assemblyEvidenceType)
        {
            if (domainEvidenceType != null)
            {
                DemandAdminPermission();
            }
            IsolatedStorageFile file = new IsolatedStorageFile();
            file.InitStore(scope, domainEvidenceType, assemblyEvidenceType);
            file.Init(scope);
            return file;
        }

        [SecuritySafeCritical]
        public static IsolatedStorageFile GetStore(IsolatedStorageScope scope, Evidence domainEvidence, Type domainEvidenceType, Evidence assemblyEvidence, Type assemblyEvidenceType)
        {
            if (assemblyEvidence == null)
            {
                throw new ArgumentNullException("assemblyEvidence");
            }
            if (System.IO.IsolatedStorage.IsolatedStorage.IsDomain(scope) && (domainEvidence == null))
            {
                throw new ArgumentNullException("domainEvidence");
            }
            DemandAdminPermission();
            IsolatedStorageFile file = new IsolatedStorageFile();
            file.InitStore(scope, domainEvidence, domainEvidenceType, assemblyEvidence, assemblyEvidenceType, null, null);
            file.Init(scope);
            return file;
        }

        internal static string GetStrongHashSuitableForObjectName(string name)
        {
            MemoryStream output = new MemoryStream();
            new BinaryWriter(output).Write(name.ToUpper(CultureInfo.InvariantCulture));
            output.Position = 0L;
            return Path.ToBase32StringSuitableForDirName(new SHA1CryptoServiceProvider().ComputeHash(output));
        }

        private string GetSyncObjectName()
        {
            if (this.m_SyncObjectName == null)
            {
                this.m_SyncObjectName = GetStrongHashSuitableForObjectName(this.m_InfoFile);
            }
            return this.m_SyncObjectName;
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern ulong GetUsage(SafeIsolatedStorageFileHandle handle);
        [SecuritySafeCritical]
        public static IsolatedStorageFile GetUserStoreForApplication()
        {
            return GetStore(IsolatedStorageScope.Application | IsolatedStorageScope.User, (Type) null);
        }

        [SecuritySafeCritical]
        public static IsolatedStorageFile GetUserStoreForAssembly()
        {
            return GetStore(IsolatedStorageScope.Assembly | IsolatedStorageScope.User, (Type) null, (Type) null);
        }

        [SecuritySafeCritical]
        public static IsolatedStorageFile GetUserStoreForDomain()
        {
            return GetStore(IsolatedStorageScope.Assembly | IsolatedStorageScope.Domain | IsolatedStorageScope.User, (Type) null, (Type) null);
        }

        [SecuritySafeCritical, ComVisible(false)]
        public static IsolatedStorageFile GetUserStoreForSite()
        {
            throw new NotSupportedException(Environment.GetResourceString("IsolatedStorage_NotValidOnDesktop"));
        }

        [SecuritySafeCritical, ComVisible(false)]
        public override bool IncreaseQuotaTo(long newQuotaSize)
        {
            if (newQuotaSize <= this.Quota)
            {
                throw new ArgumentException(Environment.GetResourceString("IsolatedStorage_OldQuotaLarger"));
            }
            if (this.m_StoreScope != (IsolatedStorageScope.Application | IsolatedStorageScope.User))
            {
                throw new NotSupportedException(Environment.GetResourceString("IsolatedStorage_OnlyIncreaseUserApplicationStore"));
            }
            IsolatedStorageSecurityState state = IsolatedStorageSecurityState.CreateStateToIncreaseQuotaForApplication(newQuotaSize, this.Quota - this.AvailableFreeSpace);
            try
            {
                state.EnsureState();
            }
            catch (IsolatedStorageException)
            {
                return false;
            }
            this.Quota = newQuotaSize;
            return true;
        }

        [SecuritySafeCritical]
        internal void Init(IsolatedStorageScope scope)
        {
            GetGlobalFileIOPerm(scope).Assert();
            this.m_StoreScope = scope;
            StringBuilder builder = new StringBuilder();
            if (System.IO.IsolatedStorage.IsolatedStorage.IsApp(scope))
            {
                builder.Append(GetRootDir(scope));
                if (s_appDataDir == null)
                {
                    builder.Append(base.AppName);
                    builder.Append(this.SeparatorExternal);
                }
                try
                {
                    LongPathDirectory.CreateDirectory(builder.ToString());
                }
                catch
                {
                    throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Init"));
                }
                this.CreateIDFile(builder.ToString(), IsolatedStorageScope.Application);
                this.m_InfoFile = builder.ToString() + "appinfo.dat";
                builder.Append("AppFiles");
            }
            else
            {
                builder.Append(GetRootDir(scope));
                if (System.IO.IsolatedStorage.IsolatedStorage.IsDomain(scope))
                {
                    builder.Append(base.DomainName);
                    builder.Append(this.SeparatorExternal);
                    try
                    {
                        LongPathDirectory.CreateDirectory(builder.ToString());
                        this.CreateIDFile(builder.ToString(), IsolatedStorageScope.Domain);
                    }
                    catch
                    {
                        throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Init"));
                    }
                    this.m_InfoFile = builder.ToString() + "info.dat";
                }
                builder.Append(base.AssemName);
                builder.Append(this.SeparatorExternal);
                try
                {
                    LongPathDirectory.CreateDirectory(builder.ToString());
                    this.CreateIDFile(builder.ToString(), IsolatedStorageScope.Assembly);
                }
                catch
                {
                    throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Init"));
                }
                if (System.IO.IsolatedStorage.IsolatedStorage.IsDomain(scope))
                {
                    builder.Append("Files");
                }
                else
                {
                    this.m_InfoFile = builder.ToString() + "info.dat";
                    builder.Append("AssemFiles");
                }
            }
            builder.Append(this.SeparatorExternal);
            string path = builder.ToString();
            try
            {
                LongPathDirectory.CreateDirectory(path);
            }
            catch
            {
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Init"));
            }
            this.m_RootDir = path;
            this.m_fiop = new FileIOPermission(FileIOPermissionAccess.AllAccess, path);
            if (scope == (IsolatedStorageScope.Application | IsolatedStorageScope.User))
            {
                this.UpdateQuotaFromInfoFile();
            }
        }

        [SecuritySafeCritical]
        internal bool InitExistingStore(IsolatedStorageScope scope)
        {
            StringBuilder builder = new StringBuilder();
            this.m_StoreScope = scope;
            builder.Append(GetRootDir(scope));
            if (System.IO.IsolatedStorage.IsolatedStorage.IsApp(scope))
            {
                builder.Append(base.AppName);
                builder.Append(this.SeparatorExternal);
                this.m_InfoFile = builder.ToString() + "appinfo.dat";
                builder.Append("AppFiles");
            }
            else
            {
                if (System.IO.IsolatedStorage.IsolatedStorage.IsDomain(scope))
                {
                    builder.Append(base.DomainName);
                    builder.Append(this.SeparatorExternal);
                    this.m_InfoFile = builder.ToString() + "info.dat";
                }
                builder.Append(base.AssemName);
                builder.Append(this.SeparatorExternal);
                if (System.IO.IsolatedStorage.IsolatedStorage.IsDomain(scope))
                {
                    builder.Append("Files");
                }
                else
                {
                    this.m_InfoFile = builder.ToString() + "info.dat";
                    builder.Append("AssemFiles");
                }
            }
            builder.Append(this.SeparatorExternal);
            FileIOPermission permission = new FileIOPermission(FileIOPermissionAccess.AllAccess, builder.ToString());
            permission.Assert();
            if (!LongPathDirectory.Exists(builder.ToString()))
            {
                return false;
            }
            this.m_RootDir = builder.ToString();
            this.m_fiop = permission;
            if (scope == (IsolatedStorageScope.Application | IsolatedStorageScope.User))
            {
                this.UpdateQuotaFromInfoFile();
            }
            return true;
        }

        [SecuritySafeCritical, HandleProcessCorruptedStateExceptions]
        private static void InitGlobalsMachine(IsolatedStorageScope scope)
        {
            string s = null;
            GetRootDir(scope, JitHelpers.GetStringHandleOnStack(ref s));
            new FileIOPermission(FileIOPermissionAccess.AllAccess, s).Assert();
            string machineRandomDirectory = GetMachineRandomDirectory(s);
            if (machineRandomDirectory == null)
            {
                Mutex mutex = CreateMutexNotOwned(s);
                if (!mutex.WaitOne())
                {
                    throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Init"));
                }
                try
                {
                    machineRandomDirectory = GetMachineRandomDirectory(s);
                    if (machineRandomDirectory == null)
                    {
                        string randomFileName = Path.GetRandomFileName();
                        string str4 = Path.GetRandomFileName();
                        try
                        {
                            CreateDirectoryWithDacl(s + randomFileName);
                            CreateDirectoryWithDacl(s + randomFileName + @"\" + str4);
                        }
                        catch
                        {
                            throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Init"));
                        }
                        machineRandomDirectory = randomFileName + @"\" + str4;
                    }
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            s_RootDirMachine = s + machineRandomDirectory + @"\";
        }

        [SecuritySafeCritical]
        private static void InitGlobalsNonRoamingUser(IsolatedStorageScope scope)
        {
            string s = null;
            if (scope == (IsolatedStorageScope.Application | IsolatedStorageScope.User))
            {
                s = GetDataDirectoryFromActivationContext();
                if (s != null)
                {
                    s_RootDirUser = s;
                    return;
                }
            }
            GetRootDir(scope, JitHelpers.GetStringHandleOnStack(ref s));
            new FileIOPermission(FileIOPermissionAccess.AllAccess, s).Assert();
            bool bMigrateNeeded = false;
            string sOldStoreLocation = null;
            string str3 = GetRandomDirectory(s, out bMigrateNeeded, out sOldStoreLocation);
            if (str3 == null)
            {
                Mutex mutex = CreateMutexNotOwned(s);
                if (!mutex.WaitOne())
                {
                    throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Init"));
                }
                try
                {
                    str3 = GetRandomDirectory(s, out bMigrateNeeded, out sOldStoreLocation);
                    if (str3 == null)
                    {
                        if (bMigrateNeeded)
                        {
                            str3 = MigrateOldIsoStoreDirectory(s, sOldStoreLocation);
                        }
                        else
                        {
                            str3 = CreateRandomDirectory(s);
                        }
                    }
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
            s_RootDirUser = s + str3 + @"\";
        }

        [SecuritySafeCritical]
        internal void Lock(ref bool locked)
        {
            locked = false;
            if (!base.IsRoaming())
            {
                lock (this.m_internalLock)
                {
                    if (this.m_bDisposed)
                    {
                        throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
                    }
                    if (this.m_closed)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
                    }
                    if (this.InvalidFileHandle)
                    {
                        this.m_handle = Open(this.m_InfoFile, this.GetSyncObjectName());
                    }
                    locked = Lock(this.m_handle, true);
                }
            }
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern bool Lock(SafeIsolatedStorageFileHandle handle, [MarshalAs(UnmanagedType.Bool)] bool fLock);
        [SecuritySafeCritical, HandleProcessCorruptedStateExceptions]
        internal static string MigrateOldIsoStoreDirectory(string rootDir, string oldRandomDirectory)
        {
            string randomFileName = Path.GetRandomFileName();
            string str2 = Path.GetRandomFileName();
            string path = rootDir + randomFileName;
            string destDirName = path + @"\" + str2;
            try
            {
                LongPathDirectory.CreateDirectory(path);
                LongPathDirectory.Move(rootDir + oldRandomDirectory, destDirName);
            }
            catch
            {
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Init"));
            }
            return (randomFileName + @"\" + str2);
        }

        [SecuritySafeCritical, ComVisible(false)]
        public void MoveDirectory(string sourceDirectoryName, string destinationDirectoryName)
        {
            if (sourceDirectoryName == null)
            {
                throw new ArgumentNullException("sourceDirectoryName");
            }
            if (destinationDirectoryName == null)
            {
                throw new ArgumentNullException("destinationDirectoryName");
            }
            if (sourceDirectoryName.Trim().Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"), "sourceDirectoryName");
            }
            if (destinationDirectoryName.Trim().Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"), "destinationDirectoryName");
            }
            if (this.m_bDisposed)
            {
                throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
            }
            if (this.m_closed)
            {
                throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
            }
            this.m_fiop.Assert();
            this.m_fiop.PermitOnly();
            string sourceDirName = LongPath.NormalizePath(this.GetFullPath(sourceDirectoryName));
            string destDirName = LongPath.NormalizePath(this.GetFullPath(destinationDirectoryName));
            try
            {
                Demand(new FileIOPermission(FileIOPermissionAccess.Write | FileIOPermissionAccess.Read, new string[] { sourceDirName }, false, false));
                Demand(new FileIOPermission(FileIOPermissionAccess.Write, new string[] { destDirName }, false, false));
            }
            catch
            {
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Operation"));
            }
            try
            {
                LongPathDirectory.Move(sourceDirName, destDirName);
            }
            catch (DirectoryNotFoundException)
            {
                throw new DirectoryNotFoundException(Environment.GetResourceString("IO.PathNotFound_Path", new object[] { sourceDirectoryName }));
            }
            catch
            {
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Operation"));
            }
            CodeAccessPermission.RevertAll();
        }

        [ComVisible(false), SecuritySafeCritical]
        public void MoveFile(string sourceFileName, string destinationFileName)
        {
            if (sourceFileName == null)
            {
                throw new ArgumentNullException("sourceFileName");
            }
            if (destinationFileName == null)
            {
                throw new ArgumentNullException("destinationFileName");
            }
            if (sourceFileName.Trim().Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"), "sourceFileName");
            }
            if (destinationFileName.Trim().Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"), "destinationFileName");
            }
            if (this.m_bDisposed)
            {
                throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
            }
            if (this.m_closed)
            {
                throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
            }
            this.m_fiop.Assert();
            this.m_fiop.PermitOnly();
            string str = LongPath.NormalizePath(this.GetFullPath(sourceFileName));
            string destFileName = LongPath.NormalizePath(this.GetFullPath(destinationFileName));
            try
            {
                Demand(new FileIOPermission(FileIOPermissionAccess.Write | FileIOPermissionAccess.Read, new string[] { str }, false, false));
                Demand(new FileIOPermission(FileIOPermissionAccess.Write, new string[] { destFileName }, false, false));
            }
            catch
            {
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Operation"));
            }
            try
            {
                LongPathFile.Move(str, destFileName);
            }
            catch (FileNotFoundException)
            {
                throw new FileNotFoundException(Environment.GetResourceString("IO.PathNotFound_Path", new object[] { sourceFileName }));
            }
            catch
            {
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Operation"));
            }
            CodeAccessPermission.RevertAll();
        }

        private static string NormalizeSearchPattern(string searchPattern)
        {
            string str = searchPattern.TrimEnd(Path.TrimEndChars);
            Path.CheckSearchPattern(str);
            return str;
        }

        internal static bool NotAppFilesDir(string dir)
        {
            return (string.Compare(dir, "AppFiles", StringComparison.Ordinal) != 0);
        }

        internal static bool NotAssemFilesDir(string dir)
        {
            return (string.Compare(dir, "AssemFiles", StringComparison.Ordinal) != 0);
        }

        private static bool NotFilesDir(string dir)
        {
            return (string.Compare(dir, "Files", StringComparison.Ordinal) != 0);
        }

        private static bool NotIDFile(string file)
        {
            return (string.Compare(file, "identity.dat", StringComparison.Ordinal) != 0);
        }

        private static bool NotInfoFile(string file)
        {
            return ((string.Compare(file, "info.dat", StringComparison.Ordinal) != 0) && (string.Compare(file, "appinfo.dat", StringComparison.Ordinal) != 0));
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern SafeIsolatedStorageFileHandle Open(string infoFile, string syncName);
        [ComVisible(false)]
        public IsolatedStorageFileStream OpenFile(string path, FileMode mode)
        {
            return new IsolatedStorageFileStream(path, mode, this);
        }

        [ComVisible(false)]
        public IsolatedStorageFileStream OpenFile(string path, FileMode mode, FileAccess access)
        {
            return new IsolatedStorageFileStream(path, mode, access, this);
        }

        [ComVisible(false)]
        public IsolatedStorageFileStream OpenFile(string path, FileMode mode, FileAccess access, FileShare share)
        {
            return new IsolatedStorageFileStream(path, mode, access, share, this);
        }

        [SecuritySafeCritical]
        public override void Remove()
        {
            string path = null;
            this.RemoveLogicalDir();
            this.Close();
            StringBuilder builder = new StringBuilder();
            builder.Append(GetRootDir(base.Scope));
            if (base.IsApp())
            {
                builder.Append(base.AppName);
                builder.Append(this.SeparatorExternal);
            }
            else
            {
                if (base.IsDomain())
                {
                    builder.Append(base.DomainName);
                    builder.Append(this.SeparatorExternal);
                    path = builder.ToString();
                }
                builder.Append(base.AssemName);
                builder.Append(this.SeparatorExternal);
            }
            string str = builder.ToString();
            new FileIOPermission(FileIOPermissionAccess.AllAccess, str).Assert();
            if (!this.ContainsUnknownFiles(str))
            {
                try
                {
                    LongPathDirectory.Delete(str, true);
                }
                catch
                {
                    return;
                }
                if (base.IsDomain())
                {
                    CodeAccessPermission.RevertAssert();
                    new FileIOPermission(FileIOPermissionAccess.AllAccess, path).Assert();
                    if (!this.ContainsUnknownFiles(path))
                    {
                        try
                        {
                            LongPathDirectory.Delete(path, true);
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        [SecuritySafeCritical]
        public static void Remove(IsolatedStorageScope scope)
        {
            VerifyGlobalScope(scope);
            DemandAdminPermission();
            string rootDir = GetRootDir(scope);
            new FileIOPermission(FileIOPermissionAccess.Write, rootDir).Assert();
            try
            {
                LongPathDirectory.Delete(rootDir, true);
                LongPathDirectory.CreateDirectory(rootDir);
            }
            catch
            {
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_DeleteDirectories"));
            }
        }

        [SecuritySafeCritical]
        private void RemoveLogicalDir()
        {
            this.m_fiop.Assert();
            bool locked = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this.Lock(ref locked);
                if (Directory.Exists(this.RootDirectory))
                {
                    ulong lFree = base.IsRoaming() ? ((ulong) 0L) : ((ulong) (this.Quota - this.AvailableFreeSpace));
                    ulong quota = (ulong) this.Quota;
                    try
                    {
                        LongPathDirectory.Delete(this.RootDirectory, true);
                    }
                    catch
                    {
                        throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_DeleteDirectories"));
                    }
                    this.Unreserve(lFree, quota);
                }
            }
            finally
            {
                if (locked)
                {
                    this.Unlock();
                }
            }
        }

        [SecuritySafeCritical]
        internal void Reserve(ulong lReserve)
        {
            if (!base.IsRoaming())
            {
                ulong quota = (ulong) this.Quota;
                ulong plReserve = lReserve;
                lock (this.m_internalLock)
                {
                    if (this.m_bDisposed)
                    {
                        throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
                    }
                    if (this.m_closed)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
                    }
                    if (this.InvalidFileHandle)
                    {
                        this.m_handle = Open(this.m_InfoFile, this.GetSyncObjectName());
                    }
                    Reserve(this.m_handle, quota, plReserve, false);
                }
            }
        }

        internal void Reserve(ulong oldLen, ulong newLen)
        {
            oldLen = RoundToBlockSize(oldLen);
            if (newLen > oldLen)
            {
                this.Reserve(RoundToBlockSize(newLen - oldLen));
            }
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern void Reserve(SafeIsolatedStorageFileHandle handle, ulong plQuota, ulong plReserve, [MarshalAs(UnmanagedType.Bool)] bool fFree);
        internal void ReserveOneBlock()
        {
            this.Reserve(0x400L);
        }

        internal static ulong RoundToBlockSize(ulong num)
        {
            if (num < 0x400L)
            {
                return 0x400L;
            }
            ulong num2 = num % ((ulong) 0x400L);
            if (num2 != 0L)
            {
                num += ((ulong) 0x400L) - num2;
            }
            return num;
        }

        internal static ulong RoundToBlockSizeFloor(ulong num)
        {
            if (num < 0x400L)
            {
                return 0L;
            }
            ulong num2 = num % ((ulong) 0x400L);
            num -= num2;
            return num;
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern void SetQuota(SafeIsolatedStorageFileHandle scope, long quota);
        internal void UndoReserveOperation(ulong oldLen, ulong newLen)
        {
            oldLen = RoundToBlockSize(oldLen);
            if (newLen > oldLen)
            {
                this.Unreserve(RoundToBlockSize(newLen - oldLen));
            }
        }

        [SecuritySafeCritical]
        internal void Unlock()
        {
            if (!base.IsRoaming())
            {
                lock (this.m_internalLock)
                {
                    if (this.m_bDisposed)
                    {
                        throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
                    }
                    if (this.m_closed)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
                    }
                    if (this.InvalidFileHandle)
                    {
                        this.m_handle = Open(this.m_InfoFile, this.GetSyncObjectName());
                    }
                    Lock(this.m_handle, false);
                }
            }
        }

        [SecuritySafeCritical]
        internal void Unreserve(ulong lFree)
        {
            if (!base.IsRoaming())
            {
                ulong quota = (ulong) this.Quota;
                this.Unreserve(lFree, quota);
            }
        }

        [SecuritySafeCritical]
        internal void Unreserve(ulong lFree, ulong quota)
        {
            if (!base.IsRoaming())
            {
                ulong plReserve = lFree;
                lock (this.m_internalLock)
                {
                    if (this.m_bDisposed)
                    {
                        throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
                    }
                    if (this.m_closed)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
                    }
                    if (this.InvalidFileHandle)
                    {
                        this.m_handle = Open(this.m_InfoFile, this.GetSyncObjectName());
                    }
                    Reserve(this.m_handle, quota, plReserve, true);
                }
            }
        }

        internal void UnreserveOneBlock()
        {
            this.Unreserve(0x400L);
        }

        [SecurityCritical]
        private void UpdateQuotaFromInfoFile()
        {
            bool locked = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                this.Lock(ref locked);
                lock (this.m_internalLock)
                {
                    if (this.InvalidFileHandle)
                    {
                        this.m_handle = Open(this.m_InfoFile, this.GetSyncObjectName());
                    }
                    long quota = 0L;
                    if (GetQuota(this.m_handle, out quota))
                    {
                        base.Quota = quota;
                    }
                }
            }
            finally
            {
                if (locked)
                {
                    this.Unlock();
                }
            }
        }

        internal static void VerifyGlobalScope(IsolatedStorageScope scope)
        {
            if (((scope != IsolatedStorageScope.User) && (scope != (IsolatedStorageScope.Roaming | IsolatedStorageScope.User))) && (scope != IsolatedStorageScope.Machine))
            {
                throw new ArgumentException(Environment.GetResourceString("IsolatedStorage_Scope_U_R_M"));
            }
        }

        [ComVisible(false)]
        public override long AvailableFreeSpace
        {
            [SecuritySafeCritical]
            get
            {
                long usage;
                if (base.IsRoaming())
                {
                    return 0x7fffffffffffffffL;
                }
                lock (this.m_internalLock)
                {
                    if (this.m_bDisposed)
                    {
                        throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
                    }
                    if (this.m_closed)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
                    }
                    if (this.InvalidFileHandle)
                    {
                        this.m_handle = Open(this.m_InfoFile, this.GetSyncObjectName());
                    }
                    usage = (long) GetUsage(this.m_handle);
                }
                return (this.Quota - usage);
            }
        }

        [Obsolete("IsolatedStorageFile.CurrentSize has been deprecated because it is not CLS Compliant.  To get the current size use IsolatedStorageFile.UsedSize"), CLSCompliant(false)]
        public override ulong CurrentSize
        {
            [SecuritySafeCritical]
            get
            {
                if (base.IsRoaming())
                {
                    throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_CurrentSizeUndefined"));
                }
                lock (this.m_internalLock)
                {
                    if (this.m_bDisposed)
                    {
                        throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
                    }
                    if (this.m_closed)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
                    }
                    if (this.InvalidFileHandle)
                    {
                        this.m_handle = Open(this.m_InfoFile, this.GetSyncObjectName());
                    }
                    return GetUsage(this.m_handle);
                }
            }
        }

        internal bool Disposed
        {
            get
            {
                return this.m_bDisposed;
            }
        }

        private bool InvalidFileHandle
        {
            [SecuritySafeCritical]
            get
            {
                if ((this.m_handle != null) && !this.m_handle.IsClosed)
                {
                    return this.m_handle.IsInvalid;
                }
                return true;
            }
        }

        [ComVisible(false)]
        public static bool IsEnabled
        {
            get
            {
                return true;
            }
        }

        [CLSCompliant(false), Obsolete("IsolatedStorageFile.MaximumSize has been deprecated because it is not CLS Compliant.  To get the maximum size use IsolatedStorageFile.Quota")]
        public override ulong MaximumSize
        {
            [SecuritySafeCritical]
            get
            {
                if (base.IsRoaming())
                {
                    return 0x7fffffffffffffffL;
                }
                return base.MaximumSize;
            }
        }

        [ComVisible(false)]
        public override long Quota
        {
            [SecuritySafeCritical]
            get
            {
                if (base.IsRoaming())
                {
                    return 0x7fffffffffffffffL;
                }
                return base.Quota;
            }
            [SecuritySafeCritical]
            internal set
            {
                bool locked = false;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                    this.Lock(ref locked);
                    lock (this.m_internalLock)
                    {
                        if (this.InvalidFileHandle)
                        {
                            this.m_handle = Open(this.m_InfoFile, this.GetSyncObjectName());
                        }
                        SetQuota(this.m_handle, value);
                    }
                }
                finally
                {
                    if (locked)
                    {
                        this.Unlock();
                    }
                }
                base.Quota = value;
            }
        }

        internal string RootDirectory
        {
            get
            {
                return this.m_RootDir;
            }
        }

        public override long UsedSize
        {
            [SecuritySafeCritical]
            get
            {
                if (base.IsRoaming())
                {
                    throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_CurrentSizeUndefined"));
                }
                lock (this.m_internalLock)
                {
                    if (this.m_bDisposed)
                    {
                        throw new ObjectDisposedException(null, Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
                    }
                    if (this.m_closed)
                    {
                        throw new InvalidOperationException(Environment.GetResourceString("IsolatedStorage_StoreNotOpen"));
                    }
                    if (this.InvalidFileHandle)
                    {
                        this.m_handle = Open(this.m_InfoFile, this.GetSyncObjectName());
                    }
                    return (long) GetUsage(this.m_handle);
                }
            }
        }
    }
}

