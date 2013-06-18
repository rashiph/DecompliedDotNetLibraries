namespace System.Data.SqlTypes
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    public sealed class SqlFileStream : Stream
    {
        private static int _objectTypeCount;
        internal const int DefaultBufferSize = 1;
        private static readonly char[] InvalidPathChars = Path.GetInvalidPathChars();
        private const ushort IoControlCodeFunctionCode = 0x958;
        private bool m_disposed;
        private FileStream m_fs;
        private string m_path;
        private byte[] m_txn;
        private static readonly int MaxWin32PathLength = 0x7ffe;
        internal readonly int ObjectID;

        public SqlFileStream(string path, byte[] transactionContext, FileAccess access) : this(path, transactionContext, access, FileOptions.None, 0L)
        {
        }

        public SqlFileStream(string path, byte[] transactionContext, FileAccess access, FileOptions options, long allocationSize)
        {
            IntPtr ptr;
            this.ObjectID = Interlocked.Increment(ref _objectTypeCount);
            Bid.ScopeEnter(out ptr, "<sc.SqlFileStream.ctor|API> %d# access=%d options=%d path='%ls' ", this.ObjectID, (int) access, (int) options, path);
            try
            {
                if (transactionContext == null)
                {
                    throw ADP.ArgumentNull("transactionContext");
                }
                if (path == null)
                {
                    throw ADP.ArgumentNull("path");
                }
                this.m_disposed = false;
                this.m_fs = null;
                this.OpenSqlFileStream(path, transactionContext, access, options, allocationSize);
                this.Name = path;
                this.TransactionContext = transactionContext;
            }
            finally
            {
                Bid.ScopeLeave(ref ptr);
            }
        }

        [Conditional("DEBUG")]
        private static void AssertPathFormat(string path)
        {
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            if (this.m_disposed)
            {
                throw ADP.ObjectDisposed(this);
            }
            return this.m_fs.BeginRead(buffer, offset, count, callback, state);
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            if (this.m_disposed)
            {
                throw ADP.ObjectDisposed(this);
            }
            IAsyncResult result = this.m_fs.BeginWrite(buffer, offset, count, callback, state);
            if (count == 1)
            {
                this.m_fs.Flush();
            }
            return result;
        }

        private static void DemandAccessPermission(string path, FileAccess access)
        {
            FileIOPermissionAccess read;
            switch (access)
            {
                case FileAccess.Read:
                    read = FileIOPermissionAccess.Read;
                    break;

                case FileAccess.Write:
                    read = FileIOPermissionAccess.Write;
                    break;

                default:
                    read = FileIOPermissionAccess.Write | FileIOPermissionAccess.Read;
                    break;
            }
            bool flag = false;
            try
            {
                FileIOPermission permission = new FileIOPermission(read, path);
                permission.Demand();
            }
            catch (PathTooLongException exception)
            {
                flag = true;
                ADP.TraceExceptionWithoutRethrow(exception);
            }
            if (flag)
            {
                new FileIOPermission(PermissionState.Unrestricted) { AllFiles = read }.Demand();
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!this.m_disposed)
                {
                    try
                    {
                        if (disposing && (this.m_fs != null))
                        {
                            this.m_fs.Close();
                            this.m_fs = null;
                        }
                    }
                    finally
                    {
                        this.m_disposed = true;
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            if (this.m_disposed)
            {
                throw ADP.ObjectDisposed(this);
            }
            return this.m_fs.EndRead(asyncResult);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            if (this.m_disposed)
            {
                throw ADP.ObjectDisposed(this);
            }
            this.m_fs.EndWrite(asyncResult);
        }

        ~SqlFileStream()
        {
            this.Dispose(false);
        }

        public override void Flush()
        {
            if (this.m_disposed)
            {
                throw ADP.ObjectDisposed(this);
            }
            this.m_fs.Flush();
        }

        private static string GetFullPathInternal(string path)
        {
            path = path.Trim();
            if (path.Length == 0)
            {
                throw ADP.Argument(Res.GetString("SqlFileStream_InvalidPath"), "path");
            }
            if (path.Length > MaxWin32PathLength)
            {
                throw ADP.Argument(Res.GetString("SqlFileStream_InvalidPath"), "path");
            }
            if (path.IndexOfAny(InvalidPathChars) >= 0)
            {
                throw ADP.Argument(Res.GetString("SqlFileStream_InvalidPath"), "path");
            }
            if (!path.StartsWith(@"\\", StringComparison.OrdinalIgnoreCase))
            {
                throw ADP.Argument(Res.GetString("SqlFileStream_InvalidPath"), "path");
            }
            path = System.Data.SqlTypes.UnsafeNativeMethods.SafeGetFullPathName(path);
            if (path.StartsWith(@"\\.\", StringComparison.Ordinal))
            {
                throw ADP.Argument(Res.GetString("SqlFileStream_PathNotValidDiskResource"), "path");
            }
            return path;
        }

        private static string InitializeNtPath(string path)
        {
            string format = @"\??\UNC\{0}\{1}";
            string str = Guid.NewGuid().ToString("N");
            return string.Format(CultureInfo.InvariantCulture, format, new object[] { path.Trim(new char[] { '\\' }), str });
        }

        private void OpenSqlFileStream(string path, byte[] transactionContext, FileAccess access, FileOptions options, long allocationSize)
        {
            if (((access != FileAccess.Read) && (access != FileAccess.Write)) && (access != FileAccess.ReadWrite))
            {
                throw ADP.ArgumentOutOfRange("access");
            }
            if ((options & ~(FileOptions.Asynchronous | FileOptions.RandomAccess | FileOptions.SequentialScan | FileOptions.WriteThrough)) != FileOptions.None)
            {
                throw ADP.ArgumentOutOfRange("options");
            }
            path = GetFullPathInternal(path);
            DemandAccessPermission(path, access);
            FileFullEaInformation eaBuffer = null;
            SecurityQualityOfService service = null;
            UnicodeString str = null;
            SafeFileHandle fileHandle = null;
            int num2 = 0x100080;
            uint num = 0;
            uint num4 = 0;
            FileShare none = FileShare.None;
            switch (access)
            {
                case FileAccess.Read:
                    num2 |= 1;
                    none = FileShare.Delete | FileShare.ReadWrite;
                    num4 = 1;
                    break;

                case FileAccess.Write:
                    num2 |= 2;
                    none = FileShare.Delete | FileShare.Read;
                    num4 = 4;
                    break;

                default:
                    num2 |= 3;
                    none = FileShare.Delete | FileShare.Read;
                    num4 = 4;
                    break;
            }
            if ((options & (FileOptions.None | FileOptions.WriteThrough)) != FileOptions.None)
            {
                num |= 2;
            }
            if ((options & FileOptions.Asynchronous) == FileOptions.None)
            {
                num |= 0x20;
            }
            if ((options & FileOptions.SequentialScan) != FileOptions.None)
            {
                num |= 4;
            }
            if ((options & FileOptions.RandomAccess) != FileOptions.None)
            {
                num |= 0x800;
            }
            try
            {
                System.Data.SqlTypes.UnsafeNativeMethods.OBJECT_ATTRIBUTES object_attributes;
                eaBuffer = new FileFullEaInformation(transactionContext);
                service = new SecurityQualityOfService(System.Data.SqlTypes.UnsafeNativeMethods.SecurityImpersonationLevel.SecurityAnonymous, false, false);
                str = new UnicodeString(InitializeNtPath(path));
                object_attributes.length = Marshal.SizeOf(typeof(System.Data.SqlTypes.UnsafeNativeMethods.OBJECT_ATTRIBUTES));
                object_attributes.rootDirectory = IntPtr.Zero;
                object_attributes.attributes = 0x40;
                object_attributes.securityDescriptor = IntPtr.Zero;
                object_attributes.securityQualityOfService = service;
                object_attributes.objectName = str;
                uint mode = System.Data.SqlTypes.UnsafeNativeMethods.SetErrorMode(1);
                uint status = 0;
                try
                {
                    System.Data.SqlTypes.UnsafeNativeMethods.IO_STATUS_BLOCK io_status_block;
                    Bid.Trace("<sc.SqlFileStream.OpenSqlFileStream|ADV> %d#, desiredAccess=0x%08x, allocationSize=%I64d, fileAttributes=0x%08x, shareAccess=0x%08x, dwCreateDisposition=0x%08x, createOptions=0x%08x\n", this.ObjectID, num2, allocationSize, 0, (int) none, num4, num);
                    status = System.Data.SqlTypes.UnsafeNativeMethods.NtCreateFile(out fileHandle, num2, ref object_attributes, out io_status_block, ref allocationSize, 0, none, num4, num, eaBuffer, (uint) eaBuffer.Length);
                }
                finally
                {
                    System.Data.SqlTypes.UnsafeNativeMethods.SetErrorMode(mode);
                }
                switch (status)
                {
                    case 0:
                        break;

                    case 0xc000000d:
                        throw ADP.Argument(Res.GetString("SqlFileStream_InvalidParameter"));

                    case 0xc0000034:
                    {
                        DirectoryNotFoundException e = new DirectoryNotFoundException();
                        ADP.TraceExceptionAsReturnValue(e);
                        throw e;
                    }
                    case 0xc0000043:
                        throw ADP.InvalidOperation(Res.GetString("SqlFileStream_FileAlreadyInTransaction"));

                    default:
                    {
                        uint num6 = System.Data.SqlTypes.UnsafeNativeMethods.RtlNtStatusToDosError(status);
                        if (num6 == 0x13d)
                        {
                            num6 = status;
                        }
                        Win32Exception exception3 = new Win32Exception((int) num6);
                        ADP.TraceExceptionAsReturnValue(exception3);
                        throw exception3;
                    }
                }
                if (fileHandle.IsInvalid)
                {
                    Win32Exception exception2 = new Win32Exception(6);
                    ADP.TraceExceptionAsReturnValue(exception2);
                    throw exception2;
                }
                if (System.Data.SqlTypes.UnsafeNativeMethods.GetFileType(fileHandle) != System.Data.SqlTypes.UnsafeNativeMethods.FileType.Disk)
                {
                    fileHandle.Dispose();
                    throw ADP.Argument(Res.GetString("SqlFileStream_PathNotValidDiskResource"));
                }
                if (access == FileAccess.ReadWrite)
                {
                    uint ioControlCode = System.Data.SqlTypes.UnsafeNativeMethods.CTL_CODE(9, 0x958, 0, 0);
                    uint cbBytesReturned = 0;
                    if (!System.Data.SqlTypes.UnsafeNativeMethods.DeviceIoControl(fileHandle, ioControlCode, IntPtr.Zero, 0, IntPtr.Zero, 0, out cbBytesReturned, IntPtr.Zero))
                    {
                        Win32Exception exception = new Win32Exception(Marshal.GetLastWin32Error());
                        ADP.TraceExceptionAsReturnValue(exception);
                        throw exception;
                    }
                }
                bool flag = false;
                try
                {
                    new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
                    flag = true;
                    this.m_fs = new FileStream(fileHandle, access, 1, (options & FileOptions.Asynchronous) != FileOptions.None);
                }
                finally
                {
                    if (flag)
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }
            }
            catch
            {
                if ((fileHandle != null) && !fileHandle.IsInvalid)
                {
                    fileHandle.Dispose();
                }
                throw;
            }
            finally
            {
                if (eaBuffer != null)
                {
                    eaBuffer.Dispose();
                    eaBuffer = null;
                }
                if (service != null)
                {
                    service.Dispose();
                    service = null;
                }
                if (str != null)
                {
                    str.Dispose();
                    str = null;
                }
            }
        }

        public override int Read([In, Out] byte[] buffer, int offset, int count)
        {
            if (this.m_disposed)
            {
                throw ADP.ObjectDisposed(this);
            }
            return this.m_fs.Read(buffer, offset, count);
        }

        public override int ReadByte()
        {
            if (this.m_disposed)
            {
                throw ADP.ObjectDisposed(this);
            }
            return this.m_fs.ReadByte();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (this.m_disposed)
            {
                throw ADP.ObjectDisposed(this);
            }
            return this.m_fs.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            if (this.m_disposed)
            {
                throw ADP.ObjectDisposed(this);
            }
            this.m_fs.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (this.m_disposed)
            {
                throw ADP.ObjectDisposed(this);
            }
            this.m_fs.Write(buffer, offset, count);
            if (count == 1)
            {
                this.m_fs.Flush();
            }
        }

        public override void WriteByte(byte value)
        {
            if (this.m_disposed)
            {
                throw ADP.ObjectDisposed(this);
            }
            this.m_fs.WriteByte(value);
            this.m_fs.Flush();
        }

        public override bool CanRead
        {
            get
            {
                if (this.m_disposed)
                {
                    throw ADP.ObjectDisposed(this);
                }
                return this.m_fs.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                if (this.m_disposed)
                {
                    throw ADP.ObjectDisposed(this);
                }
                return this.m_fs.CanSeek;
            }
        }

        [ComVisible(false)]
        public override bool CanTimeout
        {
            get
            {
                if (this.m_disposed)
                {
                    throw ADP.ObjectDisposed(this);
                }
                return this.m_fs.CanTimeout;
            }
        }

        public override bool CanWrite
        {
            get
            {
                if (this.m_disposed)
                {
                    throw ADP.ObjectDisposed(this);
                }
                return this.m_fs.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                if (this.m_disposed)
                {
                    throw ADP.ObjectDisposed(this);
                }
                return this.m_fs.Length;
            }
        }

        public string Name
        {
            get
            {
                return this.m_path;
            }
            private set
            {
                this.m_path = GetFullPathInternal(value);
            }
        }

        public override long Position
        {
            get
            {
                if (this.m_disposed)
                {
                    throw ADP.ObjectDisposed(this);
                }
                return this.m_fs.Position;
            }
            set
            {
                if (this.m_disposed)
                {
                    throw ADP.ObjectDisposed(this);
                }
                this.m_fs.Position = value;
            }
        }

        [ComVisible(false)]
        public override int ReadTimeout
        {
            get
            {
                if (this.m_disposed)
                {
                    throw ADP.ObjectDisposed(this);
                }
                return this.m_fs.ReadTimeout;
            }
            set
            {
                if (this.m_disposed)
                {
                    throw ADP.ObjectDisposed(this);
                }
                this.m_fs.ReadTimeout = value;
            }
        }

        public byte[] TransactionContext
        {
            get
            {
                if (this.m_txn == null)
                {
                    return null;
                }
                return (byte[]) this.m_txn.Clone();
            }
            private set
            {
                this.m_txn = (byte[]) value.Clone();
            }
        }

        [ComVisible(false)]
        public override int WriteTimeout
        {
            get
            {
                if (this.m_disposed)
                {
                    throw ADP.ObjectDisposed(this);
                }
                return this.m_fs.WriteTimeout;
            }
            set
            {
                if (this.m_disposed)
                {
                    throw ADP.ObjectDisposed(this);
                }
                this.m_fs.WriteTimeout = value;
            }
        }
    }
}

