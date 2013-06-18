namespace System.Web
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Web.Util;

    internal sealed class DirMonCompletion : IDisposable
    {
        private static int _activeDirMonCompletions;
        private DirectoryMonitor _dirMon;
        private HandleRef _ndirMonCompletionHandle;
        private IntPtr _ndirMonCompletionPtr;
        private GCHandle _rootCallback;

        internal DirMonCompletion(DirectoryMonitor dirMon, string dir, bool watchSubtree, uint notifyFilter)
        {
            this._dirMon = dirMon;
            NativeFileChangeNotification notification = new NativeFileChangeNotification(this.OnFileChange);
            this._rootCallback = GCHandle.Alloc(notification);
            int hr = UnsafeNativeMethods.DirMonOpen(dir, HttpRuntime.AppDomainAppIdInternal, watchSubtree, notifyFilter, notification, out this._ndirMonCompletionPtr);
            if (hr != 0)
            {
                this._rootCallback.Free();
                throw FileChangesMonitor.CreateFileMonitoringException(hr, dir);
            }
            this._ndirMonCompletionHandle = new HandleRef(this, this._ndirMonCompletionPtr);
            Interlocked.Increment(ref _activeDirMonCompletions);
        }

        private void Dispose(bool disposing)
        {
            HandleRef dirMon = this._ndirMonCompletionHandle;
            if (dirMon.Handle != IntPtr.Zero)
            {
                this._ndirMonCompletionHandle = new HandleRef(this, IntPtr.Zero);
                UnsafeNativeMethods.DirMonClose(dirMon);
                Interlocked.Decrement(ref _activeDirMonCompletions);
            }
        }

        ~DirMonCompletion()
        {
            this.Dispose(false);
        }

        private void OnFileChange(FileAction action, string fileName, long ticks)
        {
            DateTime minValue;
            if (ticks == 0L)
            {
                minValue = DateTime.MinValue;
            }
            else
            {
                minValue = DateTimeUtil.FromFileTimeToUtc(ticks);
            }
            if (action == FileAction.Dispose)
            {
                if (this._rootCallback.IsAllocated)
                {
                    this._rootCallback.Free();
                }
            }
            else if (this._ndirMonCompletionHandle.Handle != IntPtr.Zero)
            {
                using (new ApplicationImpersonationContext())
                {
                    this._dirMon.OnFileChange(action, fileName, minValue);
                }
            }
        }

        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

