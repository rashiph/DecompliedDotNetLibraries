namespace System.IO
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    [DefaultEvent("Changed"), IODescription("FileSystemWatcherDesc"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class FileSystemWatcher : Component, ISupportInitialize
    {
        private WaitForChangedResult changedResult;
        private int currentSession;
        private const NotifyFilters defaultNotifyFilters = (NotifyFilters.LastWrite | NotifyFilters.DirectoryName | NotifyFilters.FileName);
        private string directory;
        private SafeFileHandle directoryHandle;
        private bool disposed;
        private bool enabled;
        private string filter;
        private bool includeSubdirectories;
        private bool initializing;
        private int internalBufferSize;
        private bool isChanged;
        private NotifyFilters notifyFilters;
        private static int notifyFiltersValidMask = 0;
        private bool readGranted;
        private bool runOnce;
        private bool stopListening;
        private ISynchronizeInvoke synchronizingObject;
        private static readonly char[] wildcards = new char[] { '?', '*' };

        [IODescription("FSW_Changed")]
        public event FileSystemEventHandler Changed;

        [IODescription("FSW_Created")]
        public event FileSystemEventHandler Created;

        [IODescription("FSW_Deleted")]
        public event FileSystemEventHandler Deleted;

        [Browsable(false)]
        public event ErrorEventHandler Error;

        [IODescription("FSW_Renamed")]
        public event RenamedEventHandler Renamed;

        static FileSystemWatcher()
        {
            foreach (int num in Enum.GetValues(typeof(NotifyFilters)))
            {
                notifyFiltersValidMask |= num;
            }
        }

        public FileSystemWatcher()
        {
            this.notifyFilters = NotifyFilters.LastWrite | NotifyFilters.DirectoryName | NotifyFilters.FileName;
            this.internalBufferSize = 0x2000;
            this.directory = string.Empty;
            this.filter = "*.*";
        }

        public FileSystemWatcher(string path) : this(path, "*.*")
        {
        }

        public FileSystemWatcher(string path, string filter)
        {
            this.notifyFilters = NotifyFilters.LastWrite | NotifyFilters.DirectoryName | NotifyFilters.FileName;
            this.internalBufferSize = 0x2000;
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (filter == null)
            {
                throw new ArgumentNullException("filter");
            }
            if ((path.Length == 0) || !Directory.Exists(path))
            {
                throw new ArgumentException(SR.GetString("InvalidDirName", new object[] { path }));
            }
            this.directory = path;
            this.filter = filter;
        }

        public void BeginInit()
        {
            bool enabled = this.enabled;
            this.StopRaisingEvents();
            this.enabled = enabled;
            this.initializing = true;
        }

        private unsafe void CompletionStatusChanged(uint errorCode, uint numBytes, NativeOverlapped* overlappedPointer)
        {
            FSWAsyncResult asyncResult = (FSWAsyncResult) Overlapped.Unpack(overlappedPointer).AsyncResult;
            try
            {
                if (!this.stopListening)
                {
                    lock (this)
                    {
                        if (errorCode != 0)
                        {
                            if (errorCode != 0x3e3)
                            {
                                this.OnError(new ErrorEventArgs(new Win32Exception((int) errorCode)));
                                this.EnableRaisingEvents = false;
                            }
                        }
                        else if (asyncResult.session == this.currentSession)
                        {
                            if (numBytes == 0)
                            {
                                this.NotifyInternalBufferOverflowEvent();
                            }
                            else
                            {
                                int num2;
                                int index = 0;
                                string oldName = null;
                                string name = null;
                                do
                                {
                                    int num3;
                                    try
                                    {
                                        fixed (byte* numRef = asyncResult.buffer)
                                        {
                                            num2 = numRef[index];
                                            num3 = (numRef + index)[4];
                                            int num4 = (numRef + index)[8];
                                            name = new string((char*) ((numRef + index) + 12), 0, num4 / 2);
                                        }
                                    }
                                    finally
                                    {
                                        numRef = null;
                                    }
                                    switch (num3)
                                    {
                                        case 4:
                                            oldName = name;
                                            break;

                                        case 5:
                                            if (oldName != null)
                                            {
                                                this.NotifyRenameEventArgs(WatcherChangeTypes.Renamed, name, oldName);
                                                oldName = null;
                                            }
                                            else
                                            {
                                                this.NotifyRenameEventArgs(WatcherChangeTypes.Renamed, name, oldName);
                                                oldName = null;
                                            }
                                            break;

                                        default:
                                            if (oldName != null)
                                            {
                                                this.NotifyRenameEventArgs(WatcherChangeTypes.Renamed, null, oldName);
                                                oldName = null;
                                            }
                                            this.NotifyFileSystemEventArgs(num3, name);
                                            break;
                                    }
                                    index += num2;
                                }
                                while (num2 != 0);
                                if (oldName != null)
                                {
                                    this.NotifyRenameEventArgs(WatcherChangeTypes.Renamed, null, oldName);
                                    oldName = null;
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                Overlapped.Free(overlappedPointer);
                if (!this.stopListening && !this.runOnce)
                {
                    this.Monitor(asyncResult.buffer);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    this.StopRaisingEvents();
                    this.onChangedHandler = null;
                    this.onCreatedHandler = null;
                    this.onDeletedHandler = null;
                    this.onRenamedHandler = null;
                    this.onErrorHandler = null;
                    this.readGranted = false;
                }
                else
                {
                    this.stopListening = true;
                    if (!this.IsHandleInvalid)
                    {
                        this.directoryHandle.Close();
                    }
                }
            }
            finally
            {
                this.disposed = true;
                base.Dispose(disposing);
            }
        }

        public void EndInit()
        {
            this.initializing = false;
            if ((this.directory.Length != 0) && this.enabled)
            {
                this.StartRaisingEvents();
            }
        }

        private bool IsSuspended()
        {
            if (!this.initializing)
            {
                return base.DesignMode;
            }
            return true;
        }

        private bool MatchPattern(string relativePath)
        {
            string fileName = System.IO.Path.GetFileName(relativePath);
            return ((fileName != null) && PatternMatcher.StrictMatchPattern(this.filter.ToUpper(CultureInfo.InvariantCulture), fileName.ToUpper(CultureInfo.InvariantCulture)));
        }

        private unsafe void Monitor(byte[] buffer)
        {
            if (this.enabled && !this.IsHandleInvalid)
            {
                Overlapped overlapped = new Overlapped();
                if (buffer == null)
                {
                    try
                    {
                        buffer = new byte[this.internalBufferSize];
                    }
                    catch (OutOfMemoryException)
                    {
                        throw new OutOfMemoryException(SR.GetString("BufferSizeTooLarge", new object[] { this.internalBufferSize.ToString(CultureInfo.CurrentCulture) }));
                    }
                }
                FSWAsyncResult result = new FSWAsyncResult {
                    session = this.currentSession,
                    buffer = buffer
                };
                overlapped.AsyncResult = result;
                NativeOverlapped* overlappedPointer = overlapped.Pack(new IOCompletionCallback(this.CompletionStatusChanged), buffer);
                bool flag = false;
                try
                {
                    if (!this.IsHandleInvalid)
                    {
                        try
                        {
                            byte[] buffer2;
                            if (((buffer2 = buffer) == null) || (buffer2.Length == 0))
                            {
                                numRef = null;
                                goto Label_00B3;
                            }
                            fixed (byte* numRef = buffer2)
                            {
                                int num;
                            Label_00B3:
                                flag = Microsoft.Win32.UnsafeNativeMethods.ReadDirectoryChangesW(this.directoryHandle, new HandleRef(this, (IntPtr) numRef), this.internalBufferSize, this.includeSubdirectories ? 1 : 0, (int) this.notifyFilters, out num, overlappedPointer, Microsoft.Win32.NativeMethods.NullHandleRef);
                            }
                        }
                        finally
                        {
                            numRef = null;
                        }
                    }
                }
                catch (ObjectDisposedException)
                {
                }
                catch (ArgumentNullException)
                {
                }
                finally
                {
                    if (!flag)
                    {
                        Overlapped.Free(overlappedPointer);
                        if (!this.IsHandleInvalid)
                        {
                            this.OnError(new ErrorEventArgs(new Win32Exception()));
                        }
                    }
                }
            }
        }

        private void NotifyFileSystemEventArgs(int action, string name)
        {
            if (this.MatchPattern(name))
            {
                switch (action)
                {
                    case 1:
                        this.OnCreated(new FileSystemEventArgs(WatcherChangeTypes.Created, this.directory, name));
                        return;

                    case 2:
                        this.OnDeleted(new FileSystemEventArgs(WatcherChangeTypes.Deleted, this.directory, name));
                        return;

                    case 3:
                        this.OnChanged(new FileSystemEventArgs(WatcherChangeTypes.Changed, this.directory, name));
                        return;
                }
            }
        }

        private void NotifyInternalBufferOverflowEvent()
        {
            InternalBufferOverflowException exception = new InternalBufferOverflowException(SR.GetString("FSW_BufferOverflow", new object[] { this.directory }));
            ErrorEventArgs e = new ErrorEventArgs(exception);
            this.OnError(e);
        }

        private void NotifyRenameEventArgs(WatcherChangeTypes action, string name, string oldName)
        {
            if (this.MatchPattern(name) || this.MatchPattern(oldName))
            {
                RenamedEventArgs e = new RenamedEventArgs(action, this.directory, name, oldName);
                this.OnRenamed(e);
            }
        }

        protected void OnChanged(FileSystemEventArgs e)
        {
            FileSystemEventHandler onChangedHandler = this.onChangedHandler;
            if (onChangedHandler != null)
            {
                if ((this.SynchronizingObject != null) && this.SynchronizingObject.InvokeRequired)
                {
                    this.SynchronizingObject.BeginInvoke(onChangedHandler, new object[] { this, e });
                }
                else
                {
                    onChangedHandler(this, e);
                }
            }
        }

        protected void OnCreated(FileSystemEventArgs e)
        {
            FileSystemEventHandler onCreatedHandler = this.onCreatedHandler;
            if (onCreatedHandler != null)
            {
                if ((this.SynchronizingObject != null) && this.SynchronizingObject.InvokeRequired)
                {
                    this.SynchronizingObject.BeginInvoke(onCreatedHandler, new object[] { this, e });
                }
                else
                {
                    onCreatedHandler(this, e);
                }
            }
        }

        protected void OnDeleted(FileSystemEventArgs e)
        {
            FileSystemEventHandler onDeletedHandler = this.onDeletedHandler;
            if (onDeletedHandler != null)
            {
                if ((this.SynchronizingObject != null) && this.SynchronizingObject.InvokeRequired)
                {
                    this.SynchronizingObject.BeginInvoke(onDeletedHandler, new object[] { this, e });
                }
                else
                {
                    onDeletedHandler(this, e);
                }
            }
        }

        protected void OnError(ErrorEventArgs e)
        {
            ErrorEventHandler onErrorHandler = this.onErrorHandler;
            if (onErrorHandler != null)
            {
                if ((this.SynchronizingObject != null) && this.SynchronizingObject.InvokeRequired)
                {
                    this.SynchronizingObject.BeginInvoke(onErrorHandler, new object[] { this, e });
                }
                else
                {
                    onErrorHandler(this, e);
                }
            }
        }

        private void OnInternalFileSystemEventArgs(object sender, FileSystemEventArgs e)
        {
            lock (this)
            {
                if (!this.isChanged)
                {
                    this.changedResult = new WaitForChangedResult(e.ChangeType, e.Name, false);
                    this.isChanged = true;
                    System.Threading.Monitor.Pulse(this);
                }
            }
        }

        private void OnInternalRenameEventArgs(object sender, RenamedEventArgs e)
        {
            lock (this)
            {
                if (!this.isChanged)
                {
                    this.changedResult = new WaitForChangedResult(e.ChangeType, e.Name, e.OldName, false);
                    this.isChanged = true;
                    System.Threading.Monitor.Pulse(this);
                }
            }
        }

        protected void OnRenamed(RenamedEventArgs e)
        {
            RenamedEventHandler onRenamedHandler = this.onRenamedHandler;
            if (onRenamedHandler != null)
            {
                if ((this.SynchronizingObject != null) && this.SynchronizingObject.InvokeRequired)
                {
                    this.SynchronizingObject.BeginInvoke(onRenamedHandler, new object[] { this, e });
                }
                else
                {
                    onRenamedHandler(this, e);
                }
            }
        }

        private void Restart()
        {
            if (!this.IsSuspended() && this.enabled)
            {
                this.StopRaisingEvents();
                this.StartRaisingEvents();
            }
        }

        private void StartRaisingEvents()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            try
            {
                new EnvironmentPermission(PermissionState.Unrestricted).Assert();
                if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                {
                    throw new PlatformNotSupportedException(SR.GetString("WinNTRequired"));
                }
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            if (this.IsSuspended())
            {
                this.enabled = true;
            }
            else
            {
                if (!this.readGranted)
                {
                    string fullPath = System.IO.Path.GetFullPath(this.directory);
                    new FileIOPermission(FileIOPermissionAccess.Read, fullPath).Demand();
                    this.readGranted = true;
                }
                if (this.IsHandleInvalid)
                {
                    this.directoryHandle = Microsoft.Win32.NativeMethods.CreateFile(this.directory, 1, 7, null, 3, 0x42000000, new SafeFileHandle(IntPtr.Zero, false));
                    if (this.IsHandleInvalid)
                    {
                        throw new FileNotFoundException(SR.GetString("FSW_IOError", new object[] { this.directory }));
                    }
                    this.stopListening = false;
                    Interlocked.Increment(ref this.currentSession);
                    new SecurityPermission(PermissionState.Unrestricted).Assert();
                    try
                    {
                        ThreadPool.BindHandle(this.directoryHandle);
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                    this.enabled = true;
                    this.Monitor(null);
                }
            }
        }

        private void StopRaisingEvents()
        {
            if (this.IsSuspended())
            {
                this.enabled = false;
            }
            else if (!this.IsHandleInvalid)
            {
                this.stopListening = true;
                this.directoryHandle.Close();
                this.directoryHandle = null;
                Interlocked.Increment(ref this.currentSession);
                this.enabled = false;
            }
        }

        public WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType)
        {
            return this.WaitForChanged(changeType, -1);
        }

        public WaitForChangedResult WaitForChanged(WatcherChangeTypes changeType, int timeout)
        {
            FileSystemEventHandler handler = new FileSystemEventHandler(this.OnInternalFileSystemEventArgs);
            RenamedEventHandler handler2 = new RenamedEventHandler(this.OnInternalRenameEventArgs);
            this.isChanged = false;
            this.changedResult = WaitForChangedResult.TimedOutResult;
            if ((changeType & WatcherChangeTypes.Created) != 0)
            {
                this.Created += handler;
            }
            if ((changeType & WatcherChangeTypes.Deleted) != 0)
            {
                this.Deleted += handler;
            }
            if ((changeType & WatcherChangeTypes.Changed) != 0)
            {
                this.Changed += handler;
            }
            if ((changeType & WatcherChangeTypes.Renamed) != 0)
            {
                this.Renamed += handler2;
            }
            bool enableRaisingEvents = this.EnableRaisingEvents;
            if (!enableRaisingEvents)
            {
                this.runOnce = true;
                this.EnableRaisingEvents = true;
            }
            WaitForChangedResult timedOutResult = WaitForChangedResult.TimedOutResult;
            lock (this)
            {
                if (timeout == -1)
                {
                    while (!this.isChanged)
                    {
                        System.Threading.Monitor.Wait(this);
                    }
                }
                else
                {
                    System.Threading.Monitor.Wait(this, timeout, true);
                }
                timedOutResult = this.changedResult;
            }
            this.EnableRaisingEvents = enableRaisingEvents;
            this.runOnce = false;
            if ((changeType & WatcherChangeTypes.Created) != 0)
            {
                this.Created -= handler;
            }
            if ((changeType & WatcherChangeTypes.Deleted) != 0)
            {
                this.Deleted -= handler;
            }
            if ((changeType & WatcherChangeTypes.Changed) != 0)
            {
                this.Changed -= handler;
            }
            if ((changeType & WatcherChangeTypes.Renamed) != 0)
            {
                this.Renamed -= handler2;
            }
            return timedOutResult;
        }

        [IODescription("FSW_Enabled"), DefaultValue(false)]
        public bool EnableRaisingEvents
        {
            get
            {
                return this.enabled;
            }
            set
            {
                if (this.enabled != value)
                {
                    this.enabled = value;
                    if (!this.IsSuspended())
                    {
                        if (this.enabled)
                        {
                            this.StartRaisingEvents();
                        }
                        else
                        {
                            this.StopRaisingEvents();
                        }
                    }
                }
            }
        }

        [SettingsBindable(true), DefaultValue("*.*"), IODescription("FSW_Filter"), TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string Filter
        {
            get
            {
                return this.filter;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = "*.*";
                }
                if (string.Compare(this.filter, value, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    this.filter = value;
                }
            }
        }

        [DefaultValue(false), IODescription("FSW_IncludeSubdirectories")]
        public bool IncludeSubdirectories
        {
            get
            {
                return this.includeSubdirectories;
            }
            set
            {
                if (this.includeSubdirectories != value)
                {
                    this.includeSubdirectories = value;
                    this.Restart();
                }
            }
        }

        [DefaultValue(0x2000), Browsable(false)]
        public int InternalBufferSize
        {
            get
            {
                return this.internalBufferSize;
            }
            set
            {
                if (this.internalBufferSize != value)
                {
                    if (value < 0x1000)
                    {
                        value = 0x1000;
                    }
                    this.internalBufferSize = value;
                    this.Restart();
                }
            }
        }

        private bool IsHandleInvalid
        {
            get
            {
                if (this.directoryHandle != null)
                {
                    return this.directoryHandle.IsInvalid;
                }
                return true;
            }
        }

        [DefaultValue(0x13), IODescription("FSW_ChangedFilter")]
        public NotifyFilters NotifyFilter
        {
            get
            {
                return this.notifyFilters;
            }
            set
            {
                if ((value & ~notifyFiltersValidMask) != 0)
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(NotifyFilters));
                }
                if (this.notifyFilters != value)
                {
                    this.notifyFilters = value;
                    this.Restart();
                }
            }
        }

        [Editor("System.Diagnostics.Design.FSWPathEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), IODescription("FSW_Path"), DefaultValue(""), TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), SettingsBindable(true)]
        public string Path
        {
            get
            {
                return this.directory;
            }
            set
            {
                value = (value == null) ? string.Empty : value;
                if (string.Compare(this.directory, value, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    if (base.DesignMode)
                    {
                        if ((value.IndexOfAny(wildcards) != -1) || (value.IndexOfAny(System.IO.Path.GetInvalidPathChars()) != -1))
                        {
                            throw new ArgumentException(SR.GetString("InvalidDirName", new object[] { value }));
                        }
                    }
                    else if (!Directory.Exists(value))
                    {
                        throw new ArgumentException(SR.GetString("InvalidDirName", new object[] { value }));
                    }
                    this.directory = value;
                    this.readGranted = false;
                    this.Restart();
                }
            }
        }

        [Browsable(false)]
        public override ISite Site
        {
            get
            {
                return base.Site;
            }
            set
            {
                base.Site = value;
                if ((this.Site != null) && this.Site.DesignMode)
                {
                    this.EnableRaisingEvents = true;
                }
            }
        }

        [DefaultValue((string) null), Browsable(false), IODescription("FSW_SynchronizingObject")]
        public ISynchronizeInvoke SynchronizingObject
        {
            get
            {
                if ((this.synchronizingObject == null) && base.DesignMode)
                {
                    IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                    if (service != null)
                    {
                        object rootComponent = service.RootComponent;
                        if ((rootComponent != null) && (rootComponent is ISynchronizeInvoke))
                        {
                            this.synchronizingObject = (ISynchronizeInvoke) rootComponent;
                        }
                    }
                }
                return this.synchronizingObject;
            }
            set
            {
                this.synchronizingObject = value;
            }
        }

        private sealed class FSWAsyncResult : IAsyncResult
        {
            internal byte[] buffer;
            internal int session;

            public object AsyncState
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public WaitHandle AsyncWaitHandle
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public bool CompletedSynchronously
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public bool IsCompleted
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}

