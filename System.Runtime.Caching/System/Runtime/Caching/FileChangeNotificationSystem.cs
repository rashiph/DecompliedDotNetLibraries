namespace System.Runtime.Caching
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.Caching.Hosting;
    using System.Runtime.Caching.Resources;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    internal sealed class FileChangeNotificationSystem : IFileChangeNotificationSystem
    {
        private Hashtable _dirMonitors = Hashtable.Synchronized(new Hashtable(StringComparer.OrdinalIgnoreCase));
        private object _lock = new object();

        internal FileChangeNotificationSystem()
        {
        }

        [SecuritySafeCritical, PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        void IFileChangeNotificationSystem.StartMonitoring(string filePath, OnChangedCallback onChangedCallback, out object state, out DateTimeOffset lastWriteTime, out long fileSize)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException("filePath");
            }
            if (onChangedCallback == null)
            {
                throw new ArgumentNullException("onChangedCallback");
            }
            FileInfo info = new FileInfo(filePath);
            string directoryName = Path.GetDirectoryName(filePath);
            DirectoryMonitor monitor = this._dirMonitors[directoryName] as DirectoryMonitor;
            if (monitor == null)
            {
                lock (this._lock)
                {
                    monitor = this._dirMonitors[directoryName] as DirectoryMonitor;
                    if (monitor == null)
                    {
                        monitor = new DirectoryMonitor {
                            Fsw = new FileSystemWatcher(directoryName)
                        };
                        monitor.Fsw.NotifyFilter = NotifyFilters.Security | NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.DirectoryName | NotifyFilters.FileName;
                        monitor.Fsw.EnableRaisingEvents = true;
                    }
                    this._dirMonitors[directoryName] = monitor;
                }
            }
            FileChangeEventTarget target = new FileChangeEventTarget(info.Name, onChangedCallback);
            lock (monitor)
            {
                monitor.Fsw.Changed += target.ChangedHandler;
                monitor.Fsw.Created += target.ChangedHandler;
                monitor.Fsw.Deleted += target.ChangedHandler;
                monitor.Fsw.Error += target.ErrorHandler;
                monitor.Fsw.Renamed += target.RenamedHandler;
            }
            state = target;
            lastWriteTime = File.GetLastWriteTime(filePath);
            fileSize = info.Exists ? info.Length : -1L;
        }

        [SecuritySafeCritical, PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        void IFileChangeNotificationSystem.StopMonitoring(string filePath, object state)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException("filePath");
            }
            if (state == null)
            {
                throw new ArgumentNullException("state");
            }
            FileChangeEventTarget target = state as FileChangeEventTarget;
            if (target == null)
            {
                throw new ArgumentException(R.Invalid_state, "state");
            }
            string directoryName = Path.GetDirectoryName(filePath);
            DirectoryMonitor monitor = this._dirMonitors[directoryName] as DirectoryMonitor;
            if (monitor != null)
            {
                lock (monitor)
                {
                    monitor.Fsw.Changed -= target.ChangedHandler;
                    monitor.Fsw.Created -= target.ChangedHandler;
                    monitor.Fsw.Deleted -= target.ChangedHandler;
                    monitor.Fsw.Error -= target.ErrorHandler;
                    monitor.Fsw.Renamed -= target.RenamedHandler;
                }
            }
        }

        internal class DirectoryMonitor
        {
            internal FileSystemWatcher Fsw;
        }

        internal class FileChangeEventTarget
        {
            private FileSystemEventHandler _changedHandler;
            private ErrorEventHandler _errorHandler;
            private string _fileName;
            private OnChangedCallback _onChangedCallback;
            private RenamedEventHandler _renamedHandler;

            internal FileChangeEventTarget(string fileName, OnChangedCallback onChangedCallback)
            {
                this._fileName = fileName;
                this._onChangedCallback = onChangedCallback;
                this._changedHandler = new FileSystemEventHandler(this.OnChanged);
                this._errorHandler = new ErrorEventHandler(this.OnError);
                this._renamedHandler = new RenamedEventHandler(this.OnRenamed);
            }

            private static bool EqualsIgnoreCase(string s1, string s2)
            {
                if (string.IsNullOrEmpty(s1) && string.IsNullOrEmpty(s2))
                {
                    return true;
                }
                if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2))
                {
                    return false;
                }
                if (s2.Length != s1.Length)
                {
                    return false;
                }
                return (0 == string.Compare(s1, 0, s2, 0, s2.Length, StringComparison.OrdinalIgnoreCase));
            }

            private void OnChanged(object sender, FileSystemEventArgs e)
            {
                if (EqualsIgnoreCase(this._fileName, e.Name))
                {
                    this._onChangedCallback(null);
                }
            }

            private void OnError(object sender, ErrorEventArgs e)
            {
                this._onChangedCallback(null);
            }

            private void OnRenamed(object sender, RenamedEventArgs e)
            {
                if (EqualsIgnoreCase(this._fileName, e.Name) || EqualsIgnoreCase(this._fileName, e.OldName))
                {
                    this._onChangedCallback(null);
                }
            }

            internal FileSystemEventHandler ChangedHandler
            {
                get
                {
                    return this._changedHandler;
                }
            }

            internal ErrorEventHandler ErrorHandler
            {
                get
                {
                    return this._errorHandler;
                }
            }

            internal RenamedEventHandler RenamedHandler
            {
                get
                {
                    return this._renamedHandler;
                }
            }
        }
    }
}

