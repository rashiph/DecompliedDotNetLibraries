namespace System.Web
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;
    using System.Web.Util;

    internal sealed class DirectoryMonitor : IDisposable
    {
        private FileMonitor _anyFileMon;
        private int _cShortNames;
        private DirMonCompletion _dirMonCompletion;
        private Hashtable _fileMons;
        private bool _ignoreSubdirChange;
        private bool _isDirMonAppPathInternal;
        private uint _notifyFilter;
        private bool _watchSubtree;
        internal readonly string Directory;
        private static int s_inNotificationThread;
        private static int s_notificationBufferSizeIncreased = 0;
        private static WorkItemCallback s_notificationCallback = new WorkItemCallback(DirectoryMonitor.FireNotifications);
        private static Queue s_notificationQueue = new Queue();

        internal DirectoryMonitor(string appPathInternal) : this(appPathInternal, true, 0x15b)
        {
            this._isDirMonAppPathInternal = true;
        }

        internal DirectoryMonitor(string dir, bool watchSubtree, uint notifyFilter) : this(dir, watchSubtree, notifyFilter, false)
        {
        }

        internal DirectoryMonitor(string dir, bool watchSubtree, uint notifyFilter, bool ignoreSubdirChange)
        {
            this.Directory = dir;
            this._fileMons = new Hashtable(StringComparer.OrdinalIgnoreCase);
            this._watchSubtree = watchSubtree;
            this._notifyFilter = notifyFilter;
            this._ignoreSubdirChange = ignoreSubdirChange;
        }

        private FileMonitor AddFileMonitor(string file)
        {
            FileMonitor monitor;
            FindFileData data = null;
            int num;
            if (string.IsNullOrEmpty(file))
            {
                monitor = new FileMonitor(this, null, null, true, null, null);
                this._anyFileMon = monitor;
                return monitor;
            }
            string fullPath = Path.Combine(this.Directory, file);
            if (this._isDirMonAppPathInternal)
            {
                num = FindFileData.FindFile(fullPath, this.Directory, out data);
            }
            else
            {
                num = FindFileData.FindFile(fullPath, out data);
            }
            if (num == 0)
            {
                if (!this._isDirMonAppPathInternal && ((data.FileAttributesData.FileAttributes & FileAttributes.Directory) != 0))
                {
                    throw FileChangesMonitor.CreateFileMonitoringException(-2147024809, fullPath);
                }
                byte[] dacl = FileSecurity.GetDacl(fullPath);
                monitor = new FileMonitor(this, data.FileNameLong, data.FileNameShort, true, data.FileAttributesData, dacl);
                this._fileMons.Add(data.FileNameLong, monitor);
                this.UpdateFileNameShort(monitor, null, data.FileNameShort);
                return monitor;
            }
            if ((num != -2147024893) && (num != -2147024894))
            {
                throw FileChangesMonitor.CreateFileMonitoringException(num, fullPath);
            }
            if (file.IndexOf('~') != -1)
            {
                throw FileChangesMonitor.CreateFileMonitoringException(-2147024809, fullPath);
            }
            monitor = new FileMonitor(this, file, null, false, null, null);
            this._fileMons.Add(file, monitor);
            return monitor;
        }

        private FileMonitor FindFileMonitor(string file)
        {
            if (file == null)
            {
                return this._anyFileMon;
            }
            return (FileMonitor) this._fileMons[file];
        }

        private static void FireNotifications()
        {
            try
            {
                NotificationQueueItem item;
            Label_0000:
                item = null;
                lock (s_notificationQueue.SyncRoot)
                {
                    if (s_notificationQueue.Count > 0)
                    {
                        item = (NotificationQueueItem) s_notificationQueue.Dequeue();
                    }
                }
                if (item != null)
                {
                    try
                    {
                        item.Callback(null, new FileChangeEvent(item.Action, item.Filename));
                        goto Label_0000;
                    }
                    catch (Exception)
                    {
                        goto Label_0000;
                    }
                }
                Interlocked.Exchange(ref s_inNotificationThread, 0);
                if ((s_notificationQueue.Count != 0) && (Interlocked.Exchange(ref s_inNotificationThread, 1) == 0))
                {
                    goto Label_0000;
                }
            }
            catch
            {
                Interlocked.Exchange(ref s_inNotificationThread, 0);
            }
        }

        internal bool GetFileAttributes(string file, out FileAttributesData fad)
        {
            FileMonitor monitor = null;
            fad = null;
            lock (this)
            {
                monitor = this.FindFileMonitor(file);
                if (monitor != null)
                {
                    fad = monitor.Attributes;
                    return true;
                }
            }
            return false;
        }

        private bool GetFileMonitorForSpecialDirectory(string fileName, ref FileMonitor fileMon)
        {
            for (int i = 0; i < FileChangesMonitor.s_dirsToMonitor.Length; i++)
            {
                if (StringUtil.StringStartsWithIgnoreCase(fileName, FileChangesMonitor.s_dirsToMonitor[i]))
                {
                    fileMon = (FileMonitor) this._fileMons[FileChangesMonitor.s_dirsToMonitor[i]];
                    return (fileMon != null);
                }
            }
            int index = fileName.IndexOf("App_LocalResources", StringComparison.OrdinalIgnoreCase);
            if (index > -1)
            {
                int length = index + "App_LocalResources".Length;
                if ((fileName.Length == length) || (fileName[length] == Path.DirectorySeparatorChar))
                {
                    string str = fileName.Substring(0, length);
                    fileMon = (FileMonitor) this._fileMons[str];
                    return (fileMon != null);
                }
            }
            return false;
        }

        private int GetFileMonitorsCount()
        {
            int num = this._fileMons.Count - this._cShortNames;
            if (this._anyFileMon != null)
            {
                num++;
            }
            return num;
        }

        private bool IsChangeAfterStartMonitoring(FileAttributesData fad, FileMonitorTarget target, DateTime utcCompletion)
        {
            return ((fad.UtcLastAccessTime.AddSeconds(60.0) < target.UtcStartMonitoring) || ((utcCompletion > target.UtcStartMonitoring) || ((fad.UtcLastAccessTime < fad.UtcLastWriteTime) || ((fad.UtcLastAccessTime.TimeOfDay == TimeSpan.Zero) || (fad.UtcLastAccessTime >= target.UtcStartMonitoring)))));
        }

        internal bool IsMonitoring()
        {
            return (this.GetFileMonitorsCount() > 0);
        }

        internal void OnFileChange(FileAction action, string fileName, DateTime utcCompletion)
        {
            try
            {
                FileMonitor fileMon = null;
                ArrayList list = null;
                FileAttributesData attributes = null;
                FileAttributesData fad = null;
                byte[] dacl = null;
                byte[] buffer2 = null;
                FileAction error = FileAction.Error;
                DateTime minValue = DateTime.MinValue;
                bool fileMonitorForSpecialDirectory = false;
                if (this._dirMonCompletion != null)
                {
                    lock (this)
                    {
                        ICollection targets;
                        if (this._fileMons.Count > 0)
                        {
                            if ((action == FileAction.Error) || (action == FileAction.Overwhelming))
                            {
                                if (action == FileAction.Overwhelming)
                                {
                                    HttpRuntime.SetShutdownMessage("Overwhelming Change Notification in " + this.Directory);
                                    if (Interlocked.Increment(ref s_notificationBufferSizeIncreased) == 1)
                                    {
                                        System.Web.UnsafeNativeMethods.GrowFileNotificationBuffer(HttpRuntime.AppDomainAppIdInternal, this._watchSubtree);
                                    }
                                }
                                else if (action == FileAction.Error)
                                {
                                    HttpRuntime.SetShutdownMessage("File Change Notification Error in " + this.Directory);
                                }
                                list = new ArrayList();
                                foreach (DictionaryEntry entry in this._fileMons)
                                {
                                    string key = (string) entry.Key;
                                    fileMon = (FileMonitor) entry.Value;
                                    if ((fileMon.FileNameLong == key) && fileMon.Exists)
                                    {
                                        fileMon.ResetCachedAttributes();
                                        fileMon.LastAction = action;
                                        fileMon.UtcLastCompletion = utcCompletion;
                                        targets = fileMon.Targets;
                                        list.AddRange(targets);
                                    }
                                }
                                fileMon = null;
                            }
                            else
                            {
                                fileMon = (FileMonitor) this._fileMons[fileName];
                                if (this._isDirMonAppPathInternal && (fileMon == null))
                                {
                                    fileMonitorForSpecialDirectory = this.GetFileMonitorForSpecialDirectory(fileName, ref fileMon);
                                }
                                if (fileMon != null)
                                {
                                    list = new ArrayList(fileMon.Targets);
                                    attributes = fileMon.Attributes;
                                    dacl = fileMon.Dacl;
                                    error = fileMon.LastAction;
                                    minValue = fileMon.UtcLastCompletion;
                                    fileMon.LastAction = action;
                                    fileMon.UtcLastCompletion = utcCompletion;
                                    if ((action == FileAction.Removed) || (action == FileAction.RenamedOldName))
                                    {
                                        fileMon.MakeExtinct();
                                    }
                                    else if (fileMon.Exists)
                                    {
                                        if (minValue != utcCompletion)
                                        {
                                            fileMon.UpdateCachedAttributes();
                                        }
                                    }
                                    else
                                    {
                                        int num3;
                                        FindFileData data = null;
                                        string fullPath = Path.Combine(this.Directory, fileMon.FileNameLong);
                                        if (this._isDirMonAppPathInternal)
                                        {
                                            num3 = FindFileData.FindFile(fullPath, this.Directory, out data);
                                        }
                                        else
                                        {
                                            num3 = FindFileData.FindFile(fullPath, out data);
                                        }
                                        if (num3 == 0)
                                        {
                                            string fileNameShort = fileMon.FileNameShort;
                                            byte[] buffer3 = FileSecurity.GetDacl(fullPath);
                                            fileMon.MakeExist(data, buffer3);
                                            this.UpdateFileNameShort(fileMon, fileNameShort, data.FileNameShort);
                                        }
                                    }
                                    fad = fileMon.Attributes;
                                    buffer2 = fileMon.Dacl;
                                }
                            }
                        }
                        if (this._anyFileMon != null)
                        {
                            targets = this._anyFileMon.Targets;
                            if (list != null)
                            {
                                list.AddRange(targets);
                            }
                            else
                            {
                                list = new ArrayList(targets);
                            }
                        }
                        if ((action == FileAction.Error) || (action == FileAction.Overwhelming))
                        {
                            ((IDisposable) this).Dispose();
                        }
                    }
                    bool flag2 = false;
                    if ((!fileMonitorForSpecialDirectory && (fileName != null)) && (action == FileAction.Modified))
                    {
                        FileAttributesData data4 = fad;
                        if (data4 == null)
                        {
                            FileAttributesData.GetFileAttributes(Path.Combine(this.Directory, fileName), out data4);
                        }
                        if ((data4 != null) && ((data4.FileAttributes & FileAttributes.Directory) != 0))
                        {
                            flag2 = true;
                        }
                    }
                    if ((this._ignoreSubdirChange && ((action == FileAction.Removed) || (action == FileAction.RenamedOldName))) && (fileName != null))
                    {
                        string str5 = Path.Combine(this.Directory, fileName);
                        if (!HttpRuntime.FileChangesMonitor.IsDirNameMonitored(str5, fileName))
                        {
                            flag2 = true;
                        }
                    }
                    if ((list != null) && !flag2)
                    {
                        lock (s_notificationQueue.SyncRoot)
                        {
                            int num = 0;
                            int count = list.Count;
                            while (num < count)
                            {
                                bool flag3;
                                FileMonitorTarget target = (FileMonitorTarget) list[num];
                                if (((action != FileAction.Added) && (action != FileAction.Modified)) || (fad == null))
                                {
                                    flag3 = true;
                                }
                                else if (action == FileAction.Added)
                                {
                                    flag3 = this.IsChangeAfterStartMonitoring(fad, target, utcCompletion);
                                }
                                else if (utcCompletion == minValue)
                                {
                                    flag3 = error != FileAction.Modified;
                                }
                                else if (attributes == null)
                                {
                                    flag3 = true;
                                }
                                else if ((dacl == null) || (dacl != buffer2))
                                {
                                    flag3 = true;
                                }
                                else
                                {
                                    flag3 = this.IsChangeAfterStartMonitoring(fad, target, utcCompletion);
                                }
                                if (flag3)
                                {
                                    s_notificationQueue.Enqueue(new NotificationQueueItem(target.Callback, action, target.Alias));
                                }
                                num++;
                            }
                        }
                        if (((s_notificationQueue.Count > 0) && (s_inNotificationThread == 0)) && (Interlocked.Exchange(ref s_inNotificationThread, 1) == 0))
                        {
                            WorkItem.PostInternal(s_notificationCallback);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void RemoveFileMonitor(FileMonitor fileMon)
        {
            if (fileMon == this._anyFileMon)
            {
                this._anyFileMon = null;
            }
            else
            {
                this._fileMons.Remove(fileMon.FileNameLong);
                if (fileMon.FileNameShort != null)
                {
                    this._fileMons.Remove(fileMon.FileNameShort);
                    this._cShortNames--;
                }
            }
            HttpRuntime.FileChangesMonitor.RemoveAliases(fileMon);
        }

        private void StartMonitoring()
        {
            if (this._dirMonCompletion == null)
            {
                this._dirMonCompletion = new DirMonCompletion(this, this.Directory, this._watchSubtree, this._notifyFilter);
            }
        }

        [FileIOPermission(SecurityAction.Assert, Unrestricted=true)]
        internal FileMonitor StartMonitoringFileWithAssert(string file, FileChangeEventHandler callback, string alias)
        {
            FileMonitor monitor = null;
            bool flag = false;
            lock (this)
            {
                monitor = this.FindFileMonitor(file);
                if (monitor == null)
                {
                    monitor = this.AddFileMonitor(file);
                    if (this.GetFileMonitorsCount() == 1)
                    {
                        flag = true;
                    }
                }
                monitor.AddTarget(callback, alias, true);
                if (flag)
                {
                    this.StartMonitoring();
                }
            }
            return monitor;
        }

        internal void StopMonitoring()
        {
            lock (this)
            {
                ((IDisposable) this).Dispose();
            }
        }

        internal void StopMonitoringFile(string file, object target)
        {
            lock (this)
            {
                FileMonitor fileMon = this.FindFileMonitor(file);
                if ((fileMon != null) && (fileMon.RemoveTarget(target) == 0))
                {
                    this.RemoveFileMonitor(fileMon);
                    if (this.GetFileMonitorsCount() == 0)
                    {
                        ((IDisposable) this).Dispose();
                    }
                }
            }
        }

        void IDisposable.Dispose()
        {
            if (this._dirMonCompletion != null)
            {
                ((IDisposable) this._dirMonCompletion).Dispose();
                this._dirMonCompletion = null;
            }
            if (this._anyFileMon != null)
            {
                HttpRuntime.FileChangesMonitor.RemoveAliases(this._anyFileMon);
                this._anyFileMon = null;
            }
            foreach (DictionaryEntry entry in this._fileMons)
            {
                string key = (string) entry.Key;
                FileMonitor fileMon = (FileMonitor) entry.Value;
                if (fileMon.FileNameLong == key)
                {
                    HttpRuntime.FileChangesMonitor.RemoveAliases(fileMon);
                }
            }
            this._fileMons.Clear();
            this._cShortNames = 0;
        }

        private void UpdateFileNameShort(FileMonitor fileMon, string oldFileNameShort, string newFileNameShort)
        {
            if (oldFileNameShort != null)
            {
                FileMonitor monitor = (FileMonitor) this._fileMons[oldFileNameShort];
                if (monitor != null)
                {
                    if (monitor != fileMon)
                    {
                        monitor.RemoveFileNameShort();
                    }
                    this._fileMons.Remove(oldFileNameShort);
                    this._cShortNames--;
                }
            }
            if (newFileNameShort != null)
            {
                this._fileMons.Add(newFileNameShort, fileMon);
                this._cShortNames++;
            }
        }
    }
}

