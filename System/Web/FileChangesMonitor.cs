namespace System.Web
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;
    using System.Web.Util;

    internal sealed class FileChangesMonitor
    {
        private int _activeCallbackCount;
        private Hashtable _aliases;
        private string _appPathInternal;
        private FileChangeEventHandler _callbackRenameOrCriticaldirChange;
        private DirectoryMonitor _dirMonAppPathInternal;
        private ArrayList _dirMonSpecialDirs;
        private DirectoryMonitor _dirMonSubdirs;
        private Hashtable _dirs;
        private bool _disposed;
        private int _FCNMode;
        private ReadWriteSpinLock _lockDispose;
        private Hashtable _subDirDirMons;
        internal const int MAX_PATH = 260;
        internal static string[] s_dirsToMonitor = new string[] { "bin", "App_GlobalResources", "App_Code", "App_WebReferences", "App_Browsers" };

        internal FileChangesMonitor()
        {
            System.Web.UnsafeNativeMethods.GetDirMonConfiguration(out this._FCNMode);
            if (!this.IsFCNDisabled)
            {
                this._aliases = Hashtable.Synchronized(new Hashtable(StringComparer.OrdinalIgnoreCase));
                this._dirs = new Hashtable(StringComparer.OrdinalIgnoreCase);
                this._subDirDirMons = new Hashtable(StringComparer.OrdinalIgnoreCase);
                if ((this._FCNMode == 2) && (HttpRuntime.AppDomainAppPathInternal != null))
                {
                    this._appPathInternal = GetFullPath(HttpRuntime.AppDomainAppPathInternal);
                    this._dirMonAppPathInternal = new DirectoryMonitor(this._appPathInternal);
                }
            }
        }

        internal static HttpException CreateFileMonitoringException(int hr, string path)
        {
            string str;
            bool flag = false;
            switch (hr)
            {
                case -2147024894:
                case -2147024893:
                    str = "Directory_does_not_exist_for_monitoring";
                    break;

                case -2147024891:
                    str = "Access_denied_for_monitoring";
                    flag = true;
                    break;

                case -2147024840:
                    str = "NetBios_command_limit_reached";
                    flag = true;
                    break;

                case -2147024809:
                    str = "Invalid_file_name_for_monitoring";
                    break;

                default:
                    str = "Failed_to_start_monitoring";
                    break;
            }
            if (flag)
            {
                System.Web.UnsafeNativeMethods.RaiseFileMonitoringEventlogEvent(System.Web.SR.GetString(str, new object[] { HttpRuntime.GetSafePath(path) }) + "\n\r" + System.Web.SR.GetString("App_Virtual_Path", new object[] { HttpRuntime.AppDomainAppVirtualPath }), path, HttpRuntime.AppDomainAppVirtualPath, hr);
            }
            return new HttpException(System.Web.SR.GetString(str, new object[] { HttpRuntime.GetSafePath(path) }), hr);
        }

        private DirectoryMonitor FindDirectoryMonitor(string dir, bool addIfNotFound, bool throwOnError)
        {
            FileAttributesData fad = null;
            DirectoryMonitor monitor = (DirectoryMonitor) this._dirs[dir];
            if (((monitor != null) && !monitor.IsMonitoring()) && ((FileAttributesData.GetFileAttributes(dir, out fad) != 0) || ((fad.FileAttributes & FileAttributes.Directory) == 0)))
            {
                monitor = null;
            }
            if ((monitor == null) && addIfNotFound)
            {
                lock (this._dirs.SyncRoot)
                {
                    int fileAttributes;
                    monitor = (DirectoryMonitor) this._dirs[dir];
                    if (monitor != null)
                    {
                        if (monitor.IsMonitoring())
                        {
                            return monitor;
                        }
                        fileAttributes = FileAttributesData.GetFileAttributes(dir, out fad);
                        if ((fileAttributes == 0) && ((fad.FileAttributes & FileAttributes.Directory) == 0))
                        {
                            fileAttributes = -2147024809;
                        }
                        if (fileAttributes == 0)
                        {
                            return monitor;
                        }
                        this._dirs.Remove(dir);
                        monitor.StopMonitoring();
                        if (addIfNotFound && throwOnError)
                        {
                            throw CreateFileMonitoringException(fileAttributes, dir);
                        }
                        return null;
                    }
                    if (!addIfNotFound)
                    {
                        return monitor;
                    }
                    fileAttributes = FileAttributesData.GetFileAttributes(dir, out fad);
                    if ((fileAttributes == 0) && ((fad.FileAttributes & FileAttributes.Directory) == 0))
                    {
                        fileAttributes = -2147024809;
                    }
                    if (fileAttributes == 0)
                    {
                        monitor = new DirectoryMonitor(dir, false, 0x15b);
                        this._dirs.Add(dir, monitor);
                        return monitor;
                    }
                    if (throwOnError)
                    {
                        throw CreateFileMonitoringException(fileAttributes, dir);
                    }
                }
            }
            return monitor;
        }

        internal FileAttributesData GetFileAttributes(string alias)
        {
            DirectoryMonitor directoryMonitor = null;
            string fullPath;
            string file = null;
            FileAttributesData fad = null;
            if (alias == null)
            {
                throw CreateFileMonitoringException(-2147024809, alias);
            }
            if (this.IsFCNDisabled)
            {
                if ((alias.Length == 0) || !UrlPath.IsAbsolutePhysicalPath(alias))
                {
                    throw CreateFileMonitoringException(-2147024809, alias);
                }
                fullPath = GetFullPath(alias);
                FindFileData data = null;
                if (FindFileData.FindFile(fullPath, out data) == 0)
                {
                    return data.FileAttributesData;
                }
                return null;
            }
            using (new ApplicationImpersonationContext())
            {
                this._lockDispose.AcquireReaderLock();
                try
                {
                    if (!this._disposed)
                    {
                        FileMonitor monitor = (FileMonitor) this._aliases[alias];
                        if ((monitor != null) && !monitor.IsDirectory)
                        {
                            directoryMonitor = monitor.DirectoryMonitor;
                            file = monitor.FileNameLong;
                        }
                        else
                        {
                            if ((alias.Length == 0) || !UrlPath.IsAbsolutePhysicalPath(alias))
                            {
                                throw CreateFileMonitoringException(-2147024809, alias);
                            }
                            fullPath = GetFullPath(alias);
                            string directoryOrRootName = UrlPath.GetDirectoryOrRootName(fullPath);
                            file = Path.GetFileName(fullPath);
                            if ((file != null) || (file.Length > 0))
                            {
                                directoryMonitor = this.FindDirectoryMonitor(directoryOrRootName, false, false);
                            }
                        }
                    }
                }
                finally
                {
                    this._lockDispose.ReleaseReaderLock();
                }
                if ((directoryMonitor == null) || !directoryMonitor.GetFileAttributes(file, out fad))
                {
                    FileAttributesData.GetFileAttributes(alias, out fad);
                }
                return fad;
            }
        }

        internal static string GetFullPath(string alias)
        {
            try
            {
                new FileIOPermission(FileIOPermissionAccess.PathDiscovery, alias).Assert();
            }
            catch
            {
                throw CreateFileMonitoringException(-2147024809, alias);
            }
            return FileUtil.RemoveTrailingDirectoryBackSlash(Path.GetFullPath(alias));
        }

        private bool IsBeneathAppPathInternal(string fullPathName)
        {
            return (((this._appPathInternal != null) && (fullPathName.Length > (this._appPathInternal.Length + 1))) && ((fullPathName.IndexOf(this._appPathInternal, StringComparison.OrdinalIgnoreCase) > -1) && (fullPathName[this._appPathInternal.Length] == Path.DirectorySeparatorChar)));
        }

        internal bool IsDirNameMonitored(string fullPath, string dirName)
        {
            if (this._dirs.ContainsKey(fullPath))
            {
                return true;
            }
            foreach (string str in s_dirsToMonitor)
            {
                if (StringUtil.StringStartsWithIgnoreCase(dirName, str))
                {
                    if (dirName.Length == str.Length)
                    {
                        return true;
                    }
                    if ((dirName.Length > str.Length) && (dirName[str.Length] == Path.DirectorySeparatorChar))
                    {
                        return true;
                    }
                }
            }
            return (dirName.IndexOf("App_LocalResources", StringComparison.OrdinalIgnoreCase) > -1);
        }

        private DirectoryMonitor ListenToSubdirectoryChanges(string dirRoot, string dirToListenTo)
        {
            string str;
            DirectoryMonitor monitor;
            if (StringUtil.StringEndsWith(dirRoot, '\\'))
            {
                str = dirRoot + dirToListenTo;
            }
            else
            {
                str = dirRoot + @"\" + dirToListenTo;
            }
            if (this.IsBeneathAppPathInternal(str))
            {
                monitor = this._dirMonAppPathInternal;
                dirToListenTo = str.Substring(this._appPathInternal.Length + 1);
                monitor.StartMonitoringFileWithAssert(dirToListenTo, new FileChangeEventHandler(this.OnCriticaldirChange), str);
                return monitor;
            }
            if (Directory.Exists(str))
            {
                monitor = new DirectoryMonitor(str, true, 0x159);
                try
                {
                    monitor.StartMonitoringFileWithAssert(null, new FileChangeEventHandler(this.OnCriticaldirChange), str);
                    return monitor;
                }
                catch
                {
                    ((IDisposable) monitor).Dispose();
                    monitor = null;
                    throw;
                }
            }
            monitor = (DirectoryMonitor) this._subDirDirMons[dirRoot];
            if (monitor == null)
            {
                monitor = new DirectoryMonitor(dirRoot, false, 0x15b);
                this._subDirDirMons[dirRoot] = monitor;
            }
            try
            {
                monitor.StartMonitoringFileWithAssert(dirToListenTo, new FileChangeEventHandler(this.OnCriticaldirChange), str);
            }
            catch
            {
                ((IDisposable) monitor).Dispose();
                monitor = null;
                throw;
            }
            return monitor;
        }

        private void OnCriticaldirChange(object sender, FileChangeEvent e)
        {
            try
            {
                Interlocked.Increment(ref this._activeCallbackCount);
                if (!this._disposed)
                {
                    HttpRuntime.SetShutdownMessage(System.Web.SR.GetString("Change_notification_critical_dir"));
                    FileChangeEventHandler handler = this._callbackRenameOrCriticaldirChange;
                    if (handler != null)
                    {
                        handler(this, e);
                    }
                }
            }
            finally
            {
                Interlocked.Decrement(ref this._activeCallbackCount);
            }
        }

        private void OnSubdirChange(object sender, FileChangeEvent e)
        {
            try
            {
                Interlocked.Increment(ref this._activeCallbackCount);
                if (!this._disposed)
                {
                    FileChangeEventHandler handler = this._callbackRenameOrCriticaldirChange;
                    if ((handler != null) && (((e.Action == FileAction.Error) || (e.Action == FileAction.Overwhelming)) || ((e.Action == FileAction.RenamedOldName) || (e.Action == FileAction.Removed))))
                    {
                        HttpRuntime.SetShutdownMessage(System.Web.SR.GetString("Directory_rename_notification", new object[] { e.FileName }));
                        handler(this, e);
                    }
                }
            }
            finally
            {
                Interlocked.Decrement(ref this._activeCallbackCount);
            }
        }

        internal void RemoveAliases(FileMonitor fileMon)
        {
            if (!this.IsFCNDisabled)
            {
                foreach (DictionaryEntry entry in fileMon.Aliases)
                {
                    if (this._aliases[entry.Key] == fileMon)
                    {
                        this._aliases.Remove(entry.Key);
                    }
                }
            }
        }

        internal void StartListeningToLocalResourcesDirectory(VirtualPath virtualDir)
        {
            if (!this.IsFCNDisabled && ((this._callbackRenameOrCriticaldirChange != null) && (this._dirMonSpecialDirs != null)))
            {
                using (new ApplicationImpersonationContext())
                {
                    this._lockDispose.AcquireReaderLock();
                    try
                    {
                        if (!this._disposed)
                        {
                            string path = FileUtil.RemoveTrailingDirectoryBackSlash(virtualDir.MapPath());
                            string fileName = Path.GetFileName(path);
                            path = Path.GetDirectoryName(path);
                            if (Directory.Exists(path))
                            {
                                this._dirMonSpecialDirs.Add(this.ListenToSubdirectoryChanges(path, fileName));
                            }
                        }
                    }
                    finally
                    {
                        this._lockDispose.ReleaseReaderLock();
                    }
                }
            }
        }

        internal void StartMonitoringDirectoryRenamesAndBinDirectory(string dir, FileChangeEventHandler callback)
        {
            if (string.IsNullOrEmpty(dir))
            {
                throw new HttpException(System.Web.SR.GetString("Invalid_file_name_for_monitoring", new object[] { string.Empty }));
            }
            if (!this.IsFCNDisabled)
            {
                using (new ApplicationImpersonationContext())
                {
                    this._lockDispose.AcquireReaderLock();
                    try
                    {
                        if (!this._disposed)
                        {
                            this._callbackRenameOrCriticaldirChange = callback;
                            string fullPath = GetFullPath(dir);
                            this._dirMonSubdirs = new DirectoryMonitor(fullPath, true, 2, true);
                            try
                            {
                                this._dirMonSubdirs.StartMonitoringFileWithAssert(null, new FileChangeEventHandler(this.OnSubdirChange), fullPath);
                            }
                            catch
                            {
                                ((IDisposable) this._dirMonSubdirs).Dispose();
                                this._dirMonSubdirs = null;
                                throw;
                            }
                            this._dirMonSpecialDirs = new ArrayList();
                            for (int i = 0; i < s_dirsToMonitor.Length; i++)
                            {
                                this._dirMonSpecialDirs.Add(this.ListenToSubdirectoryChanges(fullPath, s_dirsToMonitor[i]));
                            }
                        }
                    }
                    finally
                    {
                        this._lockDispose.ReleaseReaderLock();
                    }
                }
            }
        }

        internal DateTime StartMonitoringFile(string alias, FileChangeEventHandler callback)
        {
            string fullPath;
            bool flag = false;
            if (alias == null)
            {
                throw CreateFileMonitoringException(-2147024809, alias);
            }
            if (this.IsFCNDisabled)
            {
                fullPath = GetFullPath(alias);
                FindFileData data = null;
                if (FindFileData.FindFile(fullPath, out data) == 0)
                {
                    return data.FileAttributesData.UtcLastWriteTime;
                }
                return DateTime.MinValue;
            }
            using (new ApplicationImpersonationContext())
            {
                FileMonitor monitor;
                string fileNameLong;
                FileAttributesData data2;
                this._lockDispose.AcquireReaderLock();
                try
                {
                    DirectoryMonitor directoryMonitor;
                    if (this._disposed)
                    {
                        return DateTime.MinValue;
                    }
                    monitor = (FileMonitor) this._aliases[alias];
                    if (monitor != null)
                    {
                        directoryMonitor = monitor.DirectoryMonitor;
                        fileNameLong = monitor.FileNameLong;
                    }
                    else
                    {
                        flag = true;
                        if ((alias.Length == 0) || !UrlPath.IsAbsolutePhysicalPath(alias))
                        {
                            throw CreateFileMonitoringException(-2147024809, alias);
                        }
                        fullPath = GetFullPath(alias);
                        if (this.IsBeneathAppPathInternal(fullPath))
                        {
                            directoryMonitor = this._dirMonAppPathInternal;
                            fileNameLong = fullPath.Substring(this._appPathInternal.Length + 1);
                        }
                        else
                        {
                            string directoryOrRootName = UrlPath.GetDirectoryOrRootName(fullPath);
                            fileNameLong = Path.GetFileName(fullPath);
                            if (string.IsNullOrEmpty(fileNameLong))
                            {
                                throw CreateFileMonitoringException(-2147024809, alias);
                            }
                            directoryMonitor = this.FindDirectoryMonitor(directoryOrRootName, true, true);
                        }
                    }
                    monitor = directoryMonitor.StartMonitoringFileWithAssert(fileNameLong, callback, alias);
                    if (flag)
                    {
                        this._aliases[alias] = monitor;
                    }
                }
                finally
                {
                    this._lockDispose.ReleaseReaderLock();
                }
                monitor.DirectoryMonitor.GetFileAttributes(fileNameLong, out data2);
                if (data2 != null)
                {
                    return data2.UtcLastWriteTime;
                }
                return DateTime.MinValue;
            }
        }

        internal DateTime StartMonitoringPath(string alias, FileChangeEventHandler callback, out FileAttributesData fad)
        {
            FileMonitor monitor = null;
            DirectoryMonitor monitor2 = null;
            string fullPath;
            string file = null;
            bool flag = false;
            fad = null;
            if (alias == null)
            {
                throw new HttpException(System.Web.SR.GetString("Invalid_file_name_for_monitoring", new object[] { string.Empty }));
            }
            if (this.IsFCNDisabled)
            {
                fullPath = GetFullPath(alias);
                FindFileData data = null;
                if (FindFileData.FindFile(fullPath, out data) == 0)
                {
                    fad = data.FileAttributesData;
                    return data.FileAttributesData.UtcLastWriteTime;
                }
                return DateTime.MinValue;
            }
            using (new ApplicationImpersonationContext())
            {
                this._lockDispose.AcquireReaderLock();
                try
                {
                    if (this._disposed)
                    {
                        return DateTime.MinValue;
                    }
                    monitor = (FileMonitor) this._aliases[alias];
                    if (monitor != null)
                    {
                        file = monitor.FileNameLong;
                        monitor = monitor.DirectoryMonitor.StartMonitoringFileWithAssert(file, callback, alias);
                    }
                    else
                    {
                        flag = true;
                        if ((alias.Length == 0) || !UrlPath.IsAbsolutePhysicalPath(alias))
                        {
                            throw new HttpException(System.Web.SR.GetString("Invalid_file_name_for_monitoring", new object[] { HttpRuntime.GetSafePath(alias) }));
                        }
                        fullPath = GetFullPath(alias);
                        if (this.IsBeneathAppPathInternal(fullPath))
                        {
                            monitor2 = this._dirMonAppPathInternal;
                            file = fullPath.Substring(this._appPathInternal.Length + 1);
                            monitor = monitor2.StartMonitoringFileWithAssert(file, callback, alias);
                        }
                        else
                        {
                            monitor2 = this.FindDirectoryMonitor(fullPath, false, false);
                            if (monitor2 != null)
                            {
                                monitor = monitor2.StartMonitoringFileWithAssert(null, callback, alias);
                            }
                            else
                            {
                                string directoryOrRootName = UrlPath.GetDirectoryOrRootName(fullPath);
                                file = Path.GetFileName(fullPath);
                                if (!string.IsNullOrEmpty(file))
                                {
                                    monitor2 = this.FindDirectoryMonitor(directoryOrRootName, false, false);
                                    if (monitor2 != null)
                                    {
                                        try
                                        {
                                            monitor = monitor2.StartMonitoringFileWithAssert(file, callback, alias);
                                        }
                                        catch
                                        {
                                        }
                                        if (monitor != null)
                                        {
                                            goto Label_01C7;
                                        }
                                    }
                                }
                                monitor2 = this.FindDirectoryMonitor(fullPath, true, false);
                                if (monitor2 != null)
                                {
                                    file = null;
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(file))
                                    {
                                        throw CreateFileMonitoringException(-2147024809, alias);
                                    }
                                    monitor2 = this.FindDirectoryMonitor(directoryOrRootName, true, true);
                                }
                                monitor = monitor2.StartMonitoringFileWithAssert(file, callback, alias);
                            }
                        }
                    }
                Label_01C7:
                    if (!monitor.IsDirectory)
                    {
                        monitor.DirectoryMonitor.GetFileAttributes(file, out fad);
                    }
                    if (flag)
                    {
                        this._aliases[alias] = monitor;
                    }
                }
                finally
                {
                    this._lockDispose.ReleaseReaderLock();
                }
                if (fad != null)
                {
                    return fad.UtcLastWriteTime;
                }
                return DateTime.MinValue;
            }
        }

        internal void Stop()
        {
            if (!this.IsFCNDisabled)
            {
                using (new ApplicationImpersonationContext())
                {
                    this._lockDispose.AcquireWriterLock();
                    try
                    {
                        this._disposed = true;
                        goto Label_0039;
                    }
                    finally
                    {
                        this._lockDispose.ReleaseWriterLock();
                    }
                Label_002F:
                    Thread.Sleep(250);
                Label_0039:
                    if (this._activeCallbackCount != 0)
                    {
                        goto Label_002F;
                    }
                    if (this._dirMonSubdirs != null)
                    {
                        this._dirMonSubdirs.StopMonitoring();
                        this._dirMonSubdirs = null;
                    }
                    if (this._dirMonSpecialDirs != null)
                    {
                        foreach (DirectoryMonitor monitor in this._dirMonSpecialDirs)
                        {
                            if (monitor != null)
                            {
                                monitor.StopMonitoring();
                            }
                        }
                        this._dirMonSpecialDirs = null;
                    }
                    this._callbackRenameOrCriticaldirChange = null;
                    if (this._dirs != null)
                    {
                        IDictionaryEnumerator enumerator = this._dirs.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            ((DirectoryMonitor) enumerator.Value).StopMonitoring();
                        }
                    }
                    this._dirs.Clear();
                    this._aliases.Clear();
                }
            }
        }

        internal void StopMonitoringFile(string alias, object target)
        {
            if (!this.IsFCNDisabled)
            {
                DirectoryMonitor directoryMonitor = null;
                string fileNameLong = null;
                if (alias == null)
                {
                    throw new HttpException(System.Web.SR.GetString("Invalid_file_name_for_monitoring", new object[] { string.Empty }));
                }
                using (new ApplicationImpersonationContext())
                {
                    this._lockDispose.AcquireReaderLock();
                    try
                    {
                        if (!this._disposed)
                        {
                            FileMonitor monitor = (FileMonitor) this._aliases[alias];
                            if ((monitor != null) && !monitor.IsDirectory)
                            {
                                directoryMonitor = monitor.DirectoryMonitor;
                                fileNameLong = monitor.FileNameLong;
                            }
                            else
                            {
                                if ((alias.Length == 0) || !UrlPath.IsAbsolutePhysicalPath(alias))
                                {
                                    throw new HttpException(System.Web.SR.GetString("Invalid_file_name_for_monitoring", new object[] { HttpRuntime.GetSafePath(alias) }));
                                }
                                string fullPath = GetFullPath(alias);
                                string directoryOrRootName = UrlPath.GetDirectoryOrRootName(fullPath);
                                fileNameLong = Path.GetFileName(fullPath);
                                if (string.IsNullOrEmpty(fileNameLong))
                                {
                                    throw new HttpException(System.Web.SR.GetString("Invalid_file_name_for_monitoring", new object[] { HttpRuntime.GetSafePath(alias) }));
                                }
                                directoryMonitor = this.FindDirectoryMonitor(directoryOrRootName, false, false);
                            }
                            if (directoryMonitor != null)
                            {
                                directoryMonitor.StopMonitoringFile(fileNameLong, target);
                            }
                        }
                    }
                    finally
                    {
                        this._lockDispose.ReleaseReaderLock();
                    }
                }
            }
        }

        internal void StopMonitoringPath(string alias, object target)
        {
            if (!this.IsFCNDisabled)
            {
                DirectoryMonitor directoryMonitor = null;
                string fileNameLong = null;
                if (alias == null)
                {
                    throw new HttpException(System.Web.SR.GetString("Invalid_file_name_for_monitoring", new object[] { string.Empty }));
                }
                using (new ApplicationImpersonationContext())
                {
                    this._lockDispose.AcquireReaderLock();
                    try
                    {
                        if (!this._disposed)
                        {
                            FileMonitor monitor = (FileMonitor) this._aliases[alias];
                            if (monitor != null)
                            {
                                directoryMonitor = monitor.DirectoryMonitor;
                                fileNameLong = monitor.FileNameLong;
                            }
                            else
                            {
                                if ((alias.Length == 0) || !UrlPath.IsAbsolutePhysicalPath(alias))
                                {
                                    throw new HttpException(System.Web.SR.GetString("Invalid_file_name_for_monitoring", new object[] { HttpRuntime.GetSafePath(alias) }));
                                }
                                string fullPath = GetFullPath(alias);
                                directoryMonitor = this.FindDirectoryMonitor(fullPath, false, false);
                                if (directoryMonitor == null)
                                {
                                    string directoryOrRootName = UrlPath.GetDirectoryOrRootName(fullPath);
                                    fileNameLong = Path.GetFileName(fullPath);
                                    if (!string.IsNullOrEmpty(fileNameLong))
                                    {
                                        directoryMonitor = this.FindDirectoryMonitor(directoryOrRootName, false, false);
                                    }
                                }
                            }
                            if (directoryMonitor != null)
                            {
                                directoryMonitor.StopMonitoringFile(fileNameLong, target);
                            }
                        }
                    }
                    finally
                    {
                        this._lockDispose.ReleaseReaderLock();
                    }
                }
            }
        }

        private bool IsFCNDisabled
        {
            get
            {
                return (this._FCNMode == 1);
            }
        }
    }
}

