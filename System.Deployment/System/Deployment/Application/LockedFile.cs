namespace System.Deployment.Application
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal static class LockedFile
    {
        [ThreadStatic]
        private static Hashtable _threadReaderLocks;
        [ThreadStatic]
        private static Hashtable _threadWriterLocks;

        public static IDisposable AcquireLock(string path, TimeSpan timeout, bool writer)
        {
            FileAccess write;
            System.Deployment.Application.NativeMethods.GenericAccess access2;
            System.Deployment.Application.NativeMethods.ShareMode mode;
            LockedFileHandle handle = LockHeldByThread(path, writer);
            if (handle != null)
            {
                return handle;
            }
            DateTime time = DateTime.UtcNow + timeout;
            if (writer)
            {
                write = FileAccess.Write;
                access2 = System.Deployment.Application.NativeMethods.GenericAccess.GENERIC_WRITE;
                mode = System.Deployment.Application.NativeMethods.ShareMode.FILE_SHARE_NONE;
            }
            else
            {
                write = FileAccess.Read;
                access2 = -2147483648;
                mode = PlatformSpecific.OnWin9x ? (System.Deployment.Application.NativeMethods.ShareMode.FILE_SHARE_NONE | System.Deployment.Application.NativeMethods.ShareMode.FILE_SHARE_READ) : (System.Deployment.Application.NativeMethods.ShareMode.FILE_SHARE_DELETE | System.Deployment.Application.NativeMethods.ShareMode.FILE_SHARE_READ);
            }
            while (true)
            {
                SafeFileHandle handle2 = System.Deployment.Application.NativeMethods.CreateFile(path, (uint) access2, (uint) mode, IntPtr.Zero, 4, 0x4000000, IntPtr.Zero);
                int num = Marshal.GetLastWin32Error();
                if (!handle2.IsInvalid)
                {
                    return new LockedFileHandle(handle2, path, write);
                }
                if ((num != 0x20) && (num != 5))
                {
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                }
                if (DateTime.UtcNow > time)
                {
                    throw new DeploymentException(ExceptionTypes.LockTimeout, Resources.GetString("Ex_LockTimeoutException"));
                }
                Thread.Sleep(1);
            }
        }

        private static LockedFileHandle LockHeldByThread(string path, bool writer)
        {
            if (((LockedFileHandle) ThreadWriterLocks[path]) == null)
            {
                if (((LockedFileHandle) ThreadReaderLocks[path]) == null)
                {
                    return null;
                }
                if (writer)
                {
                    throw new NotImplementedException();
                }
            }
            return new LockedFileHandle();
        }

        private static Hashtable ThreadReaderLocks
        {
            get
            {
                if (_threadReaderLocks == null)
                {
                    _threadReaderLocks = new Hashtable();
                }
                return _threadReaderLocks;
            }
        }

        private static Hashtable ThreadWriterLocks
        {
            get
            {
                if (_threadWriterLocks == null)
                {
                    _threadWriterLocks = new Hashtable();
                }
                return _threadWriterLocks;
            }
        }

        private class LockedFileHandle : IDisposable
        {
            private FileAccess _access;
            private bool _disposed;
            private SafeFileHandle _handle;
            private string _path;

            public LockedFileHandle()
            {
            }

            public LockedFileHandle(SafeFileHandle handle, string path, FileAccess access)
            {
                if (handle == null)
                {
                    throw new ArgumentNullException("handle");
                }
                this._handle = handle;
                this._path = path;
                this._access = access;
                ((this._access == FileAccess.Read) ? LockedFile.ThreadReaderLocks : LockedFile.ThreadWriterLocks).Add(this._path, this);
            }

            public void Dispose()
            {
                if (!this._disposed)
                {
                    if (this._handle != null)
                    {
                        ((this._access == FileAccess.Read) ? LockedFile.ThreadReaderLocks : LockedFile.ThreadWriterLocks).Remove(this._path);
                        this._handle.Dispose();
                    }
                    GC.SuppressFinalize(this);
                    this._disposed = true;
                }
            }
        }
    }
}

