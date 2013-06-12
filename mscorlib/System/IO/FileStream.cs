namespace System.IO
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.AccessControl;
    using System.Security.Permissions;
    using System.Threading;

    [ComVisible(true)]
    public class FileStream : Stream
    {
        private long _appendStart;
        private byte[] _buffer;
        private int _bufferSize;
        private bool _canRead;
        private bool _canSeek;
        private static readonly bool _canUseAsync = Environment.RunningOnWinNT;
        private bool _canWrite;
        private bool _exposedHandle;
        private string _fileName;
        [SecurityCritical]
        private Microsoft.Win32.SafeHandles.SafeFileHandle _handle;
        private bool _isAsync;
        private bool _isPipe;
        private long _pos;
        private int _readLen;
        private int _readPos;
        private int _writePos;
        internal const int DefaultBufferSize = 0x1000;
        private const int ERROR_BROKEN_PIPE = 0x6d;
        private const int ERROR_HANDLE_EOF = 0x26;
        private const int ERROR_INVALID_PARAMETER = 0x57;
        private const int ERROR_IO_PENDING = 0x3e5;
        private const int ERROR_NO_DATA = 0xe8;
        private const int FILE_ATTRIBUTE_ENCRYPTED = 0x4000;
        private const int FILE_ATTRIBUTE_NORMAL = 0x80;
        private const int FILE_BEGIN = 0;
        private const int FILE_CURRENT = 1;
        private const int FILE_END = 2;
        private const int FILE_FLAG_OVERLAPPED = 0x40000000;
        internal const int GENERIC_READ = -2147483648;
        private const int GENERIC_WRITE = 0x40000000;
        [SecurityCritical]
        private static readonly IOCompletionCallback IOCallback = new IOCompletionCallback(FileStream.AsyncFSCallback);

        internal FileStream()
        {
        }

        [SecuritySafeCritical]
        public FileStream(Microsoft.Win32.SafeHandles.SafeFileHandle handle, FileAccess access) : this(handle, access, 0x1000, false)
        {
        }

        [SecuritySafeCritical, Obsolete("This constructor has been deprecated.  Please use new FileStream(SafeFileHandle handle, FileAccess access) instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        public FileStream(IntPtr handle, FileAccess access) : this(handle, access, true, 0x1000, false)
        {
        }

        [SecuritySafeCritical]
        public FileStream(string path, FileMode mode) : this(path, mode, (mode == FileMode.Append) ? FileAccess.Write : FileAccess.ReadWrite, FileShare.Read, 0x1000, FileOptions.None, Path.GetFileName(path), false)
        {
        }

        [SecuritySafeCritical]
        public FileStream(Microsoft.Win32.SafeHandles.SafeFileHandle handle, FileAccess access, int bufferSize) : this(handle, access, bufferSize, false)
        {
        }

        [Obsolete("This constructor has been deprecated.  Please use new FileStream(SafeFileHandle handle, FileAccess access) instead, and optionally make a new SafeFileHandle with ownsHandle=false if needed.  http://go.microsoft.com/fwlink/?linkid=14202"), SecuritySafeCritical]
        public FileStream(IntPtr handle, FileAccess access, bool ownsHandle) : this(handle, access, ownsHandle, 0x1000, false)
        {
        }

        [SecuritySafeCritical]
        public FileStream(string path, FileMode mode, FileAccess access) : this(path, mode, access, FileShare.Read, 0x1000, FileOptions.None, Path.GetFileName(path), false)
        {
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public FileStream(Microsoft.Win32.SafeHandles.SafeFileHandle handle, FileAccess access, int bufferSize, bool isAsync)
        {
            if (handle.IsInvalid)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_InvalidHandle"), "handle");
            }
            this._handle = handle;
            this._exposedHandle = true;
            if ((access < FileAccess.Read) || (access > FileAccess.ReadWrite))
            {
                throw new ArgumentOutOfRangeException("access", Environment.GetResourceString("ArgumentOutOfRange_Enum"));
            }
            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
            }
            int fileType = Win32Native.GetFileType(this._handle);
            this._isAsync = isAsync && _canUseAsync;
            this._canRead = 0 != (access & FileAccess.Read);
            this._canWrite = 0 != (access & FileAccess.Write);
            this._canSeek = fileType == 1;
            this._bufferSize = bufferSize;
            this._readPos = 0;
            this._readLen = 0;
            this._writePos = 0;
            this._fileName = null;
            this._isPipe = fileType == 3;
            if (this._isAsync)
            {
                bool flag = false;
                try
                {
                    flag = ThreadPool.BindHandle(this._handle);
                }
                catch (ApplicationException)
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_HandleNotAsync"));
                }
                if (!flag)
                {
                    throw new IOException(Environment.GetResourceString("IO.IO_BindHandleFailed"));
                }
            }
            else if (fileType != 3)
            {
                this.VerifyHandleIsSync();
            }
            if (this._canSeek)
            {
                this.SeekCore(0L, SeekOrigin.Current);
            }
            else
            {
                this._pos = 0L;
            }
        }

        [Obsolete("This constructor has been deprecated.  Please use new FileStream(SafeFileHandle handle, FileAccess access, int bufferSize) instead, and optionally make a new SafeFileHandle with ownsHandle=false if needed.  http://go.microsoft.com/fwlink/?linkid=14202"), SecuritySafeCritical]
        public FileStream(IntPtr handle, FileAccess access, bool ownsHandle, int bufferSize) : this(handle, access, ownsHandle, bufferSize, false)
        {
        }

        [SecuritySafeCritical]
        public FileStream(string path, FileMode mode, FileAccess access, FileShare share) : this(path, mode, access, share, 0x1000, FileOptions.None, Path.GetFileName(path), false)
        {
        }

        [SecuritySafeCritical, Obsolete("This constructor has been deprecated.  Please use new FileStream(SafeFileHandle handle, FileAccess access, int bufferSize, bool isAsync) instead, and optionally make a new SafeFileHandle with ownsHandle=false if needed.  http://go.microsoft.com/fwlink/?linkid=14202"), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public FileStream(IntPtr handle, FileAccess access, bool ownsHandle, int bufferSize, bool isAsync) : this(new Microsoft.Win32.SafeHandles.SafeFileHandle(handle, ownsHandle), access, bufferSize, isAsync)
        {
        }

        [SecuritySafeCritical]
        public FileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize) : this(path, mode, access, share, bufferSize, FileOptions.None, Path.GetFileName(path), false)
        {
        }

        [SecuritySafeCritical]
        public FileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync) : this(path, mode, access, share, bufferSize, useAsync ? FileOptions.Asynchronous : FileOptions.None, Path.GetFileName(path), false)
        {
        }

        [SecuritySafeCritical]
        public FileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options) : this(path, mode, access, share, bufferSize, options, Path.GetFileName(path), false)
        {
        }

        [SecuritySafeCritical]
        public FileStream(string path, FileMode mode, FileSystemRights rights, FileShare share, int bufferSize, FileOptions options)
        {
            Win32Native.SECURITY_ATTRIBUTES secAttrs = GetSecAttrs(share);
            this.Init(path, mode, 0, (int) rights, true, share, bufferSize, options, secAttrs, Path.GetFileName(path), false, false);
        }

        [SecuritySafeCritical]
        public FileStream(string path, FileMode mode, FileSystemRights rights, FileShare share, int bufferSize, FileOptions options, FileSecurity fileSecurity)
        {
            object obj2;
            Win32Native.SECURITY_ATTRIBUTES secAttrs = GetSecAttrs(share, fileSecurity, out obj2);
            try
            {
                this.Init(path, mode, 0, (int) rights, true, share, bufferSize, options, secAttrs, Path.GetFileName(path), false, false);
            }
            finally
            {
                if (obj2 != null)
                {
                    ((GCHandle) obj2).Free();
                }
            }
        }

        [SecurityCritical]
        internal FileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options, string msgPath, bool bFromProxy)
        {
            Win32Native.SECURITY_ATTRIBUTES secAttrs = GetSecAttrs(share);
            this.Init(path, mode, access, 0, false, share, bufferSize, options, secAttrs, msgPath, bFromProxy, false);
        }

        [SecurityCritical]
        internal FileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options, string msgPath, bool bFromProxy, bool useLongPath)
        {
            Win32Native.SECURITY_ATTRIBUTES secAttrs = GetSecAttrs(share);
            this.Init(path, mode, access, 0, false, share, bufferSize, options, secAttrs, msgPath, bFromProxy, useLongPath);
        }

        [SecurityCritical]
        private static unsafe void AsyncFSCallback(uint errorCode, uint numBytes, NativeOverlapped* pOverlapped)
        {
            FileStreamAsyncResult asyncResult = (FileStreamAsyncResult) Overlapped.Unpack(pOverlapped).AsyncResult;
            asyncResult._numBytes = (int) numBytes;
            if ((errorCode == 0x6d) || (errorCode == 0xe8))
            {
                errorCode = 0;
            }
            asyncResult._errorCode = (int) errorCode;
            asyncResult._completedSynchronously = false;
            asyncResult._isComplete = true;
            Thread.MemoryBarrier();
            ManualResetEvent event2 = asyncResult._waitHandle;
            if ((event2 != null) && !event2.Set())
            {
                __Error.WinIOError();
            }
            AsyncCallback callback = asyncResult._userCallback;
            if (callback != null)
            {
                callback(asyncResult);
            }
        }

        [SecuritySafeCritical, HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override IAsyncResult BeginRead(byte[] array, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (numBytes < 0)
            {
                throw new ArgumentOutOfRangeException("numBytes", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((array.Length - offset) < numBytes)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            }
            if (this._handle.IsClosed)
            {
                __Error.FileNotOpen();
            }
            if (!this._isAsync)
            {
                return base.BeginRead(array, offset, numBytes, userCallback, stateObject);
            }
            if (!this.CanRead)
            {
                __Error.ReadNotSupported();
            }
            FileStreamAsyncResult result = null;
            if (this._isPipe)
            {
                if (this._readPos >= this._readLen)
                {
                    return this.BeginReadCore(array, offset, numBytes, userCallback, stateObject, 0);
                }
                int num = this._readLen - this._readPos;
                if (num > numBytes)
                {
                    num = numBytes;
                }
                Buffer.InternalBlockCopy(this._buffer, this._readPos, array, offset, num);
                this._readPos += num;
                result = FileStreamAsyncResult.CreateBufferedReadResult(num, userCallback, stateObject);
                result.CallUserCallback();
                return result;
            }
            if (this._writePos > 0)
            {
                this.FlushWrite(false);
            }
            if (this._readPos == this._readLen)
            {
                if (numBytes < this._bufferSize)
                {
                    if (this._buffer == null)
                    {
                        this._buffer = new byte[this._bufferSize];
                    }
                    IAsyncResult asyncResult = this.BeginReadCore(this._buffer, 0, this._bufferSize, null, null, 0);
                    this._readLen = this.EndRead(asyncResult);
                    int num2 = this._readLen;
                    if (num2 > numBytes)
                    {
                        num2 = numBytes;
                    }
                    Buffer.InternalBlockCopy(this._buffer, 0, array, offset, num2);
                    this._readPos = num2;
                    result = FileStreamAsyncResult.CreateBufferedReadResult(num2, userCallback, stateObject);
                    result.CallUserCallback();
                    return result;
                }
                this._readPos = 0;
                this._readLen = 0;
                return this.BeginReadCore(array, offset, numBytes, userCallback, stateObject, 0);
            }
            int byteCount = this._readLen - this._readPos;
            if (byteCount > numBytes)
            {
                byteCount = numBytes;
            }
            Buffer.InternalBlockCopy(this._buffer, this._readPos, array, offset, byteCount);
            this._readPos += byteCount;
            if ((byteCount >= numBytes) || this._isPipe)
            {
                result = FileStreamAsyncResult.CreateBufferedReadResult(byteCount, userCallback, stateObject);
                result.CallUserCallback();
                return result;
            }
            this._readPos = 0;
            this._readLen = 0;
            return this.BeginReadCore(array, offset + byteCount, numBytes - byteCount, userCallback, stateObject, byteCount);
        }

        [SecuritySafeCritical]
        private unsafe FileStreamAsyncResult BeginReadCore(byte[] bytes, int offset, int numBytes, AsyncCallback userCallback, object stateObject, int numBufferedBytesRead)
        {
            NativeOverlapped* overlappedPtr;
            FileStreamAsyncResult ar = new FileStreamAsyncResult {
                _handle = this._handle,
                _userCallback = userCallback,
                _userStateObject = stateObject,
                _isWrite = false,
                _numBufferedBytes = numBufferedBytesRead
            };
            ManualResetEvent event2 = new ManualResetEvent(false);
            ar._waitHandle = event2;
            Overlapped overlapped = new Overlapped(0, 0, IntPtr.Zero, ar);
            if (userCallback != null)
            {
                overlappedPtr = overlapped.Pack(IOCallback, bytes);
            }
            else
            {
                overlappedPtr = overlapped.UnsafePack(null, bytes);
            }
            ar._overlapped = overlappedPtr;
            if (this.CanSeek)
            {
                long length = this.Length;
                if (this._exposedHandle)
                {
                    this.VerifyOSHandlePosition();
                }
                if ((this._pos + numBytes) > length)
                {
                    if (this._pos <= length)
                    {
                        numBytes = (int) (length - this._pos);
                    }
                    else
                    {
                        numBytes = 0;
                    }
                }
                overlappedPtr->OffsetLow = (int) this._pos;
                overlappedPtr->OffsetHigh = (int) (this._pos >> 0x20);
                this.SeekCore((long) numBytes, SeekOrigin.Current);
            }
            int hr = 0;
            if ((this.ReadFileNative(this._handle, bytes, offset, numBytes, overlappedPtr, out hr) == -1) && (numBytes != -1))
            {
                if (hr == 0x6d)
                {
                    overlappedPtr->InternalLow = IntPtr.Zero;
                    ar.CallUserCallback();
                    return ar;
                }
                if (hr == 0x3e5)
                {
                    return ar;
                }
                if (!this._handle.IsClosed && this.CanSeek)
                {
                    this.SeekCore(0L, SeekOrigin.Current);
                }
                if (hr == 0x26)
                {
                    __Error.EndOfFile();
                    return ar;
                }
                __Error.WinIOError(hr, string.Empty);
            }
            return ar;
        }

        [SecuritySafeCritical, HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override IAsyncResult BeginWrite(byte[] array, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (numBytes < 0)
            {
                throw new ArgumentOutOfRangeException("numBytes", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((array.Length - offset) < numBytes)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            }
            if (this._handle.IsClosed)
            {
                __Error.FileNotOpen();
            }
            if (!this._isAsync)
            {
                return base.BeginWrite(array, offset, numBytes, userCallback, stateObject);
            }
            if (!this.CanWrite)
            {
                __Error.WriteNotSupported();
            }
            if (this._isPipe)
            {
                if (this._writePos > 0)
                {
                    this.FlushWrite(false);
                }
                return this.BeginWriteCore(array, offset, numBytes, userCallback, stateObject);
            }
            if (this._writePos == 0)
            {
                if (this._readPos < this._readLen)
                {
                    this.FlushRead();
                }
                this._readPos = 0;
                this._readLen = 0;
            }
            int num = this._bufferSize - this._writePos;
            if (numBytes <= num)
            {
                if (this._writePos == 0)
                {
                    this._buffer = new byte[this._bufferSize];
                }
                Buffer.InternalBlockCopy(array, offset, this._buffer, this._writePos, numBytes);
                this._writePos += numBytes;
                FileStreamAsyncResult result = new FileStreamAsyncResult {
                    _userCallback = userCallback,
                    _userStateObject = stateObject,
                    _waitHandle = null,
                    _isWrite = true,
                    _numBufferedBytes = numBytes
                };
                result.CallUserCallback();
                return result;
            }
            if (this._writePos > 0)
            {
                this.FlushWrite(false);
            }
            return this.BeginWriteCore(array, offset, numBytes, userCallback, stateObject);
        }

        [SecuritySafeCritical]
        private unsafe FileStreamAsyncResult BeginWriteCore(byte[] bytes, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
        {
            NativeOverlapped* overlappedPtr;
            FileStreamAsyncResult ar = new FileStreamAsyncResult {
                _handle = this._handle,
                _userCallback = userCallback,
                _userStateObject = stateObject,
                _isWrite = true
            };
            ManualResetEvent event2 = new ManualResetEvent(false);
            ar._waitHandle = event2;
            Overlapped overlapped = new Overlapped(0, 0, IntPtr.Zero, ar);
            if (userCallback != null)
            {
                overlappedPtr = overlapped.Pack(IOCallback, bytes);
            }
            else
            {
                overlappedPtr = overlapped.UnsafePack(null, bytes);
            }
            ar._overlapped = overlappedPtr;
            if (this.CanSeek)
            {
                long length = this.Length;
                if (this._exposedHandle)
                {
                    this.VerifyOSHandlePosition();
                }
                if ((this._pos + numBytes) > length)
                {
                    this.SetLengthCore(this._pos + numBytes);
                }
                overlappedPtr->OffsetLow = (int) this._pos;
                overlappedPtr->OffsetHigh = (int) (this._pos >> 0x20);
                this.SeekCore((long) numBytes, SeekOrigin.Current);
            }
            int hr = 0;
            if ((this.WriteFileNative(this._handle, bytes, offset, numBytes, overlappedPtr, out hr) == -1) && (numBytes != -1))
            {
                if (hr == 0xe8)
                {
                    ar.CallUserCallback();
                    return ar;
                }
                if (hr == 0x3e5)
                {
                    return ar;
                }
                if (!this._handle.IsClosed && this.CanSeek)
                {
                    this.SeekCore(0L, SeekOrigin.Current);
                }
                if (hr == 0x26)
                {
                    __Error.EndOfFile();
                    return ar;
                }
                __Error.WinIOError(hr, string.Empty);
            }
            return ar;
        }

        [SecuritySafeCritical]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (((this._handle != null) && !this._handle.IsClosed) && (this._writePos > 0))
                {
                    this.FlushWrite(!disposing);
                }
            }
            finally
            {
                if ((this._handle != null) && !this._handle.IsClosed)
                {
                    this._handle.Dispose();
                }
                this._canRead = false;
                this._canWrite = false;
                this._canSeek = false;
                base.Dispose(disposing);
            }
        }

        [SecuritySafeCritical]
        public override unsafe int EndRead(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            if (!this._isAsync)
            {
                return base.EndRead(asyncResult);
            }
            FileStreamAsyncResult result = asyncResult as FileStreamAsyncResult;
            if ((result == null) || result._isWrite)
            {
                __Error.WrongAsyncResult();
            }
            if (1 == Interlocked.CompareExchange(ref result._EndXxxCalled, 1, 0))
            {
                __Error.EndReadCalledTwice();
            }
            WaitHandle handle = result._waitHandle;
            if (handle != null)
            {
                try
                {
                    handle.WaitOne();
                }
                finally
                {
                    handle.Close();
                }
            }
            NativeOverlapped* nativeOverlappedPtr = result._overlapped;
            if (nativeOverlappedPtr != null)
            {
                Overlapped.Free(nativeOverlappedPtr);
            }
            if (result._errorCode != 0)
            {
                __Error.WinIOError(result._errorCode, string.Empty);
            }
            return (result._numBytes + result._numBufferedBytes);
        }

        [SecuritySafeCritical]
        public override unsafe void EndWrite(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            if (!this._isAsync)
            {
                base.EndWrite(asyncResult);
            }
            else
            {
                FileStreamAsyncResult result = asyncResult as FileStreamAsyncResult;
                if ((result == null) || !result._isWrite)
                {
                    __Error.WrongAsyncResult();
                }
                if (1 == Interlocked.CompareExchange(ref result._EndXxxCalled, 1, 0))
                {
                    __Error.EndWriteCalledTwice();
                }
                WaitHandle handle = result._waitHandle;
                if (handle != null)
                {
                    try
                    {
                        handle.WaitOne();
                    }
                    finally
                    {
                        handle.Close();
                    }
                }
                NativeOverlapped* nativeOverlappedPtr = result._overlapped;
                if (nativeOverlappedPtr != null)
                {
                    Overlapped.Free(nativeOverlappedPtr);
                }
                if (result._errorCode != 0)
                {
                    __Error.WinIOError(result._errorCode, string.Empty);
                }
            }
        }

        [SecuritySafeCritical]
        ~FileStream()
        {
            if (this._handle != null)
            {
                this.Dispose(false);
            }
        }

        [SecuritySafeCritical]
        public override void Flush()
        {
            this.Flush(false);
        }

        [SecuritySafeCritical]
        public virtual void Flush(bool flushToDisk)
        {
            if (this._handle.IsClosed)
            {
                __Error.FileNotOpen();
            }
            if (this._writePos > 0)
            {
                this.FlushWrite(false);
                if (flushToDisk && !Win32Native.FlushFileBuffers(this._handle))
                {
                    __Error.WinIOError();
                }
            }
            else if ((this._readPos < this._readLen) && this.CanSeek)
            {
                this.FlushRead();
            }
            this._readPos = 0;
            this._readLen = 0;
        }

        private void FlushRead()
        {
            if ((this._readPos - this._readLen) != 0)
            {
                this.SeekCore((long) (this._readPos - this._readLen), SeekOrigin.Current);
            }
            this._readPos = 0;
            this._readLen = 0;
        }

        private void FlushWrite(bool calledFromFinalizer)
        {
            if (this._isAsync)
            {
                IAsyncResult asyncResult = this.BeginWriteCore(this._buffer, 0, this._writePos, null, null);
                if (!calledFromFinalizer)
                {
                    this.EndWrite(asyncResult);
                }
            }
            else
            {
                this.WriteCore(this._buffer, 0, this._writePos);
            }
            this._writePos = 0;
        }

        [SecuritySafeCritical]
        public FileSecurity GetAccessControl()
        {
            if (this._handle.IsClosed)
            {
                __Error.FileNotOpen();
            }
            return new FileSecurity(this._handle, this._fileName, AccessControlSections.Group | AccessControlSections.Owner | AccessControlSections.Access);
        }

        [SecuritySafeCritical]
        private static Win32Native.SECURITY_ATTRIBUTES GetSecAttrs(FileShare share)
        {
            Win32Native.SECURITY_ATTRIBUTES structure = null;
            if ((share & FileShare.Inheritable) != FileShare.None)
            {
                structure = new Win32Native.SECURITY_ATTRIBUTES {
                    nLength = Marshal.SizeOf(structure),
                    bInheritHandle = 1
                };
            }
            return structure;
        }

        [SecuritySafeCritical]
        private static unsafe Win32Native.SECURITY_ATTRIBUTES GetSecAttrs(FileShare share, FileSecurity fileSecurity, out object pinningHandle)
        {
            pinningHandle = null;
            Win32Native.SECURITY_ATTRIBUTES structure = null;
            if (((share & FileShare.Inheritable) != FileShare.None) || (fileSecurity != null))
            {
                structure = new Win32Native.SECURITY_ATTRIBUTES {
                    nLength = Marshal.SizeOf(structure)
                };
                if ((share & FileShare.Inheritable) != FileShare.None)
                {
                    structure.bInheritHandle = 1;
                }
                if (fileSecurity == null)
                {
                    return structure;
                }
                byte[] securityDescriptorBinaryForm = fileSecurity.GetSecurityDescriptorBinaryForm();
                pinningHandle = GCHandle.Alloc(securityDescriptorBinaryForm, GCHandleType.Pinned);
                fixed (byte* numRef = securityDescriptorBinaryForm)
                {
                    structure.pSecurityDescriptor = numRef;
                }
            }
            return structure;
        }

        [SecuritySafeCritical]
        internal unsafe void Init(string path, FileMode mode, FileAccess access, int rights, bool useRights, FileShare share, int bufferSize, FileOptions options, Win32Native.SECURITY_ATTRIBUTES secAttrs, string msgPath, bool bFromProxy, bool useLongPath)
        {
            int num;
            if (path == null)
            {
                throw new ArgumentNullException("path", Environment.GetResourceString("ArgumentNull_Path"));
            }
            if (path.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_EmptyPath"));
            }
            FileSystemRights rights2 = (FileSystemRights) rights;
            this._fileName = msgPath;
            this._exposedHandle = false;
            FileShare share2 = share & ~FileShare.Inheritable;
            string paramName = null;
            if ((mode < FileMode.CreateNew) || (mode > FileMode.Append))
            {
                paramName = "mode";
            }
            else if (!useRights && ((access < FileAccess.Read) || (access > FileAccess.ReadWrite)))
            {
                paramName = "access";
            }
            else if (useRights && ((rights2 < FileSystemRights.ListDirectory) || (rights2 > FileSystemRights.FullControl)))
            {
                paramName = "rights";
            }
            else if ((share2 < FileShare.None) || (share2 > (FileShare.Delete | FileShare.ReadWrite)))
            {
                paramName = "share";
            }
            if (paramName != null)
            {
                throw new ArgumentOutOfRangeException(paramName, Environment.GetResourceString("ArgumentOutOfRange_Enum"));
            }
            if ((options != FileOptions.None) && ((options & 0x3ffbfff) != FileOptions.None))
            {
                throw new ArgumentOutOfRangeException("options", Environment.GetResourceString("ArgumentOutOfRange_Enum"));
            }
            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize", Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"));
            }
            if (((!useRights && ((access & FileAccess.Write) == 0)) || (useRights && ((rights2 & FileSystemRights.Write) == 0))) && (((mode == FileMode.Truncate) || (mode == FileMode.CreateNew)) || ((mode == FileMode.Create) || (mode == FileMode.Append))))
            {
                if (!useRights)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFileMode&AccessCombo", new object[] { mode, access }));
                }
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFileMode&RightsCombo", new object[] { mode, rights2 }));
            }
            if (useRights && (mode == FileMode.Truncate))
            {
                if (rights2 != FileSystemRights.Write)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFileModeTruncate&RightsCombo", new object[] { mode, rights2 }));
                }
                useRights = false;
                access = FileAccess.Write;
            }
            if (!useRights)
            {
                num = (access == FileAccess.Read) ? -2147483648 : ((access == FileAccess.Write) ? 0x40000000 : -1073741824);
            }
            else
            {
                num = rights;
            }
            int maxPathLength = useLongPath ? Path.MaxLongPath : Path.MaxPath;
            string str2 = Path.NormalizePath(path, true, maxPathLength);
            this._fileName = str2;
            if (str2.StartsWith(@"\\.\", StringComparison.Ordinal))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_DevicesNotSupported"));
            }
            FileIOPermissionAccess noAccess = FileIOPermissionAccess.NoAccess;
            if ((!useRights && ((access & FileAccess.Read) != 0)) || (useRights && ((rights2 & FileSystemRights.ReadAndExecute) != 0)))
            {
                if (mode == FileMode.Append)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidAppendMode"));
                }
                noAccess |= FileIOPermissionAccess.Read;
            }
            if (((!useRights && ((access & FileAccess.Write) != 0)) || (useRights && ((rights2 & (FileSystemRights.TakeOwnership | FileSystemRights.ChangePermissions | FileSystemRights.Delete | FileSystemRights.Write | FileSystemRights.DeleteSubdirectoriesAndFiles)) != 0))) || ((useRights && ((rights2 & FileSystemRights.Synchronize) != 0)) && (mode == FileMode.OpenOrCreate)))
            {
                if (mode == FileMode.Append)
                {
                    noAccess |= FileIOPermissionAccess.Append;
                }
                else
                {
                    noAccess |= FileIOPermissionAccess.Write;
                }
            }
            AccessControlActions control = ((secAttrs != null) && (secAttrs.pSecurityDescriptor != null)) ? AccessControlActions.Change : AccessControlActions.None;
            new FileIOPermission(noAccess, control, new string[] { str2 }, false, false).Demand();
            share &= ~FileShare.Inheritable;
            bool flag2 = mode == FileMode.Append;
            if (mode == FileMode.Append)
            {
                mode = FileMode.OpenOrCreate;
            }
            if (_canUseAsync && ((options & FileOptions.Asynchronous) != FileOptions.None))
            {
                this._isAsync = true;
            }
            else
            {
                options &= ~FileOptions.Asynchronous;
            }
            int dwFlagsAndAttributes = (int) options;
            dwFlagsAndAttributes |= 0x100000;
            int newMode = Win32Native.SetErrorMode(1);
            try
            {
                string str3 = str2;
                if (useLongPath)
                {
                    str3 = Path.AddLongPathPrefix(str3);
                }
                this._handle = Win32Native.SafeCreateFile(str3, num, share, secAttrs, mode, dwFlagsAndAttributes, Win32Native.NULL);
                if (this._handle.IsInvalid)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    if ((errorCode == 3) && str2.Equals(Directory.InternalGetDirectoryRoot(str2)))
                    {
                        errorCode = 5;
                    }
                    bool flag3 = false;
                    if (!bFromProxy)
                    {
                        try
                        {
                            new FileIOPermission(FileIOPermissionAccess.PathDiscovery, new string[] { this._fileName }, false, false).Demand();
                            flag3 = true;
                        }
                        catch (SecurityException)
                        {
                        }
                    }
                    if (flag3)
                    {
                        __Error.WinIOError(errorCode, this._fileName);
                    }
                    else
                    {
                        __Error.WinIOError(errorCode, msgPath);
                    }
                }
            }
            finally
            {
                Win32Native.SetErrorMode(newMode);
            }
            if (Win32Native.GetFileType(this._handle) != 1)
            {
                this._handle.Close();
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_FileStreamOnNonFiles"));
            }
            if (this._isAsync)
            {
                bool flag4 = false;
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
                try
                {
                    flag4 = ThreadPool.BindHandle(this._handle);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                    if (!flag4)
                    {
                        this._handle.Close();
                    }
                }
                if (!flag4)
                {
                    throw new IOException(Environment.GetResourceString("IO.IO_BindHandleFailed"));
                }
            }
            if (!useRights)
            {
                this._canRead = (access & FileAccess.Read) != 0;
                this._canWrite = (access & FileAccess.Write) != 0;
            }
            else
            {
                this._canRead = (rights2 & FileSystemRights.ListDirectory) != 0;
                this._canWrite = ((rights2 & FileSystemRights.CreateFiles) != 0) || ((rights2 & FileSystemRights.AppendData) != 0);
            }
            this._canSeek = true;
            this._isPipe = false;
            this._pos = 0L;
            this._bufferSize = bufferSize;
            this._readPos = 0;
            this._readLen = 0;
            this._writePos = 0;
            if (flag2)
            {
                this._appendStart = this.SeekCore(0L, SeekOrigin.End);
            }
            else
            {
                this._appendStart = -1L;
            }
        }

        [SecuritySafeCritical]
        public virtual void Lock(long position, long length)
        {
            if ((position < 0L) || (length < 0L))
            {
                throw new ArgumentOutOfRangeException((position < 0L) ? "position" : "length", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (this._handle.IsClosed)
            {
                __Error.FileNotOpen();
            }
            int offsetLow = (int) position;
            int offsetHigh = (int) (position >> 0x20);
            int countLow = (int) length;
            int countHigh = (int) (length >> 0x20);
            if (!Win32Native.LockFile(this._handle, offsetLow, offsetHigh, countLow, countHigh))
            {
                __Error.WinIOError();
            }
        }

        [SecuritySafeCritical]
        public override int Read([In, Out] byte[] array, int offset, int count)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array", Environment.GetResourceString("ArgumentNull_Buffer"));
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((array.Length - offset) < count)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            }
            if (this._handle.IsClosed)
            {
                __Error.FileNotOpen();
            }
            bool flag = false;
            int byteCount = this._readLen - this._readPos;
            if (byteCount == 0)
            {
                if (!this.CanRead)
                {
                    __Error.ReadNotSupported();
                }
                if (this._writePos > 0)
                {
                    this.FlushWrite(false);
                }
                if (!this.CanSeek || (count >= this._bufferSize))
                {
                    byteCount = this.ReadCore(array, offset, count);
                    this._readPos = 0;
                    this._readLen = 0;
                    return byteCount;
                }
                if (this._buffer == null)
                {
                    this._buffer = new byte[this._bufferSize];
                }
                byteCount = this.ReadCore(this._buffer, 0, this._bufferSize);
                if (byteCount == 0)
                {
                    return 0;
                }
                flag = byteCount < this._bufferSize;
                this._readPos = 0;
                this._readLen = byteCount;
            }
            if (byteCount > count)
            {
                byteCount = count;
            }
            Buffer.InternalBlockCopy(this._buffer, this._readPos, array, offset, byteCount);
            this._readPos += byteCount;
            if ((!this._isPipe && (byteCount < count)) && !flag)
            {
                int num2 = this.ReadCore(array, offset + byteCount, count - byteCount);
                byteCount += num2;
                this._readPos = 0;
                this._readLen = 0;
            }
            return byteCount;
        }

        [SecuritySafeCritical]
        public override int ReadByte()
        {
            if (this._handle.IsClosed)
            {
                __Error.FileNotOpen();
            }
            if ((this._readLen == 0) && !this.CanRead)
            {
                __Error.ReadNotSupported();
            }
            if (this._readPos == this._readLen)
            {
                if (this._writePos > 0)
                {
                    this.FlushWrite(false);
                }
                if (this._buffer == null)
                {
                    this._buffer = new byte[this._bufferSize];
                }
                this._readLen = this.ReadCore(this._buffer, 0, this._bufferSize);
                this._readPos = 0;
            }
            if (this._readPos == this._readLen)
            {
                return -1;
            }
            int num = this._buffer[this._readPos];
            this._readPos++;
            return num;
        }

        [SecuritySafeCritical]
        private int ReadCore(byte[] buffer, int offset, int count)
        {
            if (this._isAsync)
            {
                IAsyncResult asyncResult = this.BeginReadCore(buffer, offset, count, null, null, 0);
                return this.EndRead(asyncResult);
            }
            if (this._exposedHandle)
            {
                this.VerifyOSHandlePosition();
            }
            int hr = 0;
            int num2 = this.ReadFileNative(this._handle, buffer, offset, count, null, out hr);
            if (num2 == -1)
            {
                switch (hr)
                {
                    case 0x6d:
                        num2 = 0;
                        goto Label_006E;

                    case 0x57:
                        throw new ArgumentException(Environment.GetResourceString("Arg_HandleNotSync"));
                }
                __Error.WinIOError(hr, string.Empty);
            }
        Label_006E:
            this._pos += num2;
            return num2;
        }

        [SecurityCritical]
        private unsafe int ReadFileNative(Microsoft.Win32.SafeHandles.SafeFileHandle handle, byte[] bytes, int offset, int count, NativeOverlapped* overlapped, out int hr)
        {
            if ((bytes.Length - offset) < count)
            {
                throw new IndexOutOfRangeException(Environment.GetResourceString("IndexOutOfRange_IORaceCondition"));
            }
            if (bytes.Length == 0)
            {
                hr = 0;
                return 0;
            }
            int num = 0;
            int numBytesRead = 0;
            fixed (byte* numRef = bytes)
            {
                if (this._isAsync)
                {
                    num = Win32Native.ReadFile(handle, numRef + offset, count, IntPtr.Zero, overlapped);
                }
                else
                {
                    num = Win32Native.ReadFile(handle, numRef + offset, count, out numBytesRead, IntPtr.Zero);
                }
            }
            if (num == 0)
            {
                hr = Marshal.GetLastWin32Error();
                if (((hr != 0x6d) && (hr != 0xe9)) && (hr == 6))
                {
                    this._handle.SetHandleAsInvalid();
                }
                return -1;
            }
            hr = 0;
            return numBytesRead;
        }

        [SecuritySafeCritical]
        public override long Seek(long offset, SeekOrigin origin)
        {
            if ((origin < SeekOrigin.Begin) || (origin > SeekOrigin.End))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidSeekOrigin"));
            }
            if (this._handle.IsClosed)
            {
                __Error.FileNotOpen();
            }
            if (!this.CanSeek)
            {
                __Error.SeekNotSupported();
            }
            if (this._writePos > 0)
            {
                this.FlushWrite(false);
            }
            else if (origin == SeekOrigin.Current)
            {
                offset -= this._readLen - this._readPos;
            }
            if (this._exposedHandle)
            {
                this.VerifyOSHandlePosition();
            }
            long num = this._pos + (this._readPos - this._readLen);
            long num2 = this.SeekCore(offset, origin);
            if ((this._appendStart != -1L) && (num2 < this._appendStart))
            {
                this.SeekCore(num, SeekOrigin.Begin);
                throw new IOException(Environment.GetResourceString("IO.IO_SeekAppendOverwrite"));
            }
            if (this._readLen > 0)
            {
                if (num == num2)
                {
                    if (this._readPos > 0)
                    {
                        Buffer.InternalBlockCopy(this._buffer, this._readPos, this._buffer, 0, this._readLen - this._readPos);
                        this._readLen -= this._readPos;
                        this._readPos = 0;
                    }
                    if (this._readLen > 0)
                    {
                        this.SeekCore((long) this._readLen, SeekOrigin.Current);
                    }
                    return num2;
                }
                if (((num - this._readPos) < num2) && (num2 < ((num + this._readLen) - this._readPos)))
                {
                    int num3 = (int) (num2 - num);
                    Buffer.InternalBlockCopy(this._buffer, this._readPos + num3, this._buffer, 0, this._readLen - (this._readPos + num3));
                    this._readLen -= this._readPos + num3;
                    this._readPos = 0;
                    if (this._readLen > 0)
                    {
                        this.SeekCore((long) this._readLen, SeekOrigin.Current);
                    }
                    return num2;
                }
                this._readPos = 0;
                this._readLen = 0;
            }
            return num2;
        }

        [SecuritySafeCritical]
        private long SeekCore(long offset, SeekOrigin origin)
        {
            int hr = 0;
            long num2 = 0L;
            num2 = Win32Native.SetFilePointer(this._handle, offset, origin, out hr);
            if (num2 == -1L)
            {
                if (hr == 6)
                {
                    this._handle.SetHandleAsInvalid();
                }
                __Error.WinIOError(hr, string.Empty);
            }
            this._pos = num2;
            return num2;
        }

        [SecuritySafeCritical]
        public void SetAccessControl(FileSecurity fileSecurity)
        {
            if (fileSecurity == null)
            {
                throw new ArgumentNullException("fileSecurity");
            }
            if (this._handle.IsClosed)
            {
                __Error.FileNotOpen();
            }
            fileSecurity.Persist(this._handle, this._fileName);
        }

        [SecuritySafeCritical]
        public override void SetLength(long value)
        {
            if (value < 0L)
            {
                throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (this._handle.IsClosed)
            {
                __Error.FileNotOpen();
            }
            if (!this.CanSeek)
            {
                __Error.SeekNotSupported();
            }
            if (!this.CanWrite)
            {
                __Error.WriteNotSupported();
            }
            if (this._writePos > 0)
            {
                this.FlushWrite(false);
            }
            else if (this._readPos < this._readLen)
            {
                this.FlushRead();
            }
            this._readPos = 0;
            this._readLen = 0;
            if ((this._appendStart != -1L) && (value < this._appendStart))
            {
                throw new IOException(Environment.GetResourceString("IO.IO_SetLengthAppendTruncate"));
            }
            this.SetLengthCore(value);
        }

        [SecuritySafeCritical]
        private void SetLengthCore(long value)
        {
            long offset = this._pos;
            if (this._exposedHandle)
            {
                this.VerifyOSHandlePosition();
            }
            if (this._pos != value)
            {
                this.SeekCore(value, SeekOrigin.Begin);
            }
            if (!Win32Native.SetEndOfFile(this._handle))
            {
                int errorCode = Marshal.GetLastWin32Error();
                if (errorCode == 0x57)
                {
                    throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_FileLengthTooBig"));
                }
                __Error.WinIOError(errorCode, string.Empty);
            }
            if (offset != value)
            {
                if (offset < value)
                {
                    this.SeekCore(offset, SeekOrigin.Begin);
                }
                else
                {
                    this.SeekCore(0L, SeekOrigin.End);
                }
            }
        }

        [SecuritySafeCritical]
        public virtual void Unlock(long position, long length)
        {
            if ((position < 0L) || (length < 0L))
            {
                throw new ArgumentOutOfRangeException((position < 0L) ? "position" : "length", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (this._handle.IsClosed)
            {
                __Error.FileNotOpen();
            }
            int offsetLow = (int) position;
            int offsetHigh = (int) (position >> 0x20);
            int countLow = (int) length;
            int countHigh = (int) (length >> 0x20);
            if (!Win32Native.UnlockFile(this._handle, offsetLow, offsetHigh, countLow, countHigh))
            {
                __Error.WinIOError();
            }
        }

        [SecuritySafeCritical]
        private void VerifyHandleIsSync()
        {
            byte[] bytes = new byte[1];
            int hr = 0;
            if (this.CanRead)
            {
                this.ReadFileNative(this._handle, bytes, 0, 0, null, out hr);
            }
            else if (this.CanWrite)
            {
                this.WriteFileNative(this._handle, bytes, 0, 0, null, out hr);
            }
            if (hr == 0x57)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_HandleNotSync"));
            }
            if (hr == 6)
            {
                __Error.WinIOError(hr, "<OS handle>");
            }
        }

        private void VerifyOSHandlePosition()
        {
            if (this.CanSeek)
            {
                long num = this._pos;
                if (this.SeekCore(0L, SeekOrigin.Current) != num)
                {
                    this._readPos = 0;
                    this._readLen = 0;
                    if (this._writePos > 0)
                    {
                        this._writePos = 0;
                        throw new IOException(Environment.GetResourceString("IO.IO_FileStreamHandlePosition"));
                    }
                }
            }
        }

        [SecuritySafeCritical]
        public override void Write(byte[] array, int offset, int count)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array", Environment.GetResourceString("ArgumentNull_Buffer"));
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((array.Length - offset) < count)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOffLen"));
            }
            if (this._handle.IsClosed)
            {
                __Error.FileNotOpen();
            }
            if (this._writePos == 0)
            {
                if (!this.CanWrite)
                {
                    __Error.WriteNotSupported();
                }
                if (this._readPos < this._readLen)
                {
                    this.FlushRead();
                }
                this._readPos = 0;
                this._readLen = 0;
            }
            if (this._writePos > 0)
            {
                int byteCount = this._bufferSize - this._writePos;
                if (byteCount > 0)
                {
                    if (byteCount > count)
                    {
                        byteCount = count;
                    }
                    Buffer.InternalBlockCopy(array, offset, this._buffer, this._writePos, byteCount);
                    this._writePos += byteCount;
                    if (count == byteCount)
                    {
                        return;
                    }
                    offset += byteCount;
                    count -= byteCount;
                }
                if (this._isAsync)
                {
                    IAsyncResult asyncResult = this.BeginWriteCore(this._buffer, 0, this._writePos, null, null);
                    this.EndWrite(asyncResult);
                }
                else
                {
                    this.WriteCore(this._buffer, 0, this._writePos);
                }
                this._writePos = 0;
            }
            if (count >= this._bufferSize)
            {
                this.WriteCore(array, offset, count);
            }
            else if (count != 0)
            {
                if (this._buffer == null)
                {
                    this._buffer = new byte[this._bufferSize];
                }
                Buffer.InternalBlockCopy(array, offset, this._buffer, this._writePos, count);
                this._writePos = count;
            }
        }

        [SecuritySafeCritical]
        public override void WriteByte(byte value)
        {
            if (this._handle.IsClosed)
            {
                __Error.FileNotOpen();
            }
            if (this._writePos == 0)
            {
                if (!this.CanWrite)
                {
                    __Error.WriteNotSupported();
                }
                if (this._readPos < this._readLen)
                {
                    this.FlushRead();
                }
                this._readPos = 0;
                this._readLen = 0;
                if (this._buffer == null)
                {
                    this._buffer = new byte[this._bufferSize];
                }
            }
            if (this._writePos == this._bufferSize)
            {
                this.FlushWrite(false);
            }
            this._buffer[this._writePos] = value;
            this._writePos++;
        }

        [SecuritySafeCritical]
        private void WriteCore(byte[] buffer, int offset, int count)
        {
            if (this._isAsync)
            {
                IAsyncResult asyncResult = this.BeginWriteCore(buffer, offset, count, null, null);
                this.EndWrite(asyncResult);
                return;
            }
            if (this._exposedHandle)
            {
                this.VerifyOSHandlePosition();
            }
            int hr = 0;
            int num2 = this.WriteFileNative(this._handle, buffer, offset, count, null, out hr);
            if (num2 == -1)
            {
                switch (hr)
                {
                    case 0xe8:
                        num2 = 0;
                        goto Label_0070;

                    case 0x57:
                        throw new IOException(Environment.GetResourceString("IO.IO_FileTooLongOrHandleNotSync"));
                }
                __Error.WinIOError(hr, string.Empty);
            }
        Label_0070:
            this._pos += num2;
        }

        [SecurityCritical]
        private unsafe int WriteFileNative(Microsoft.Win32.SafeHandles.SafeFileHandle handle, byte[] bytes, int offset, int count, NativeOverlapped* overlapped, out int hr)
        {
            if ((bytes.Length - offset) < count)
            {
                throw new IndexOutOfRangeException(Environment.GetResourceString("IndexOutOfRange_IORaceCondition"));
            }
            if (bytes.Length == 0)
            {
                hr = 0;
                return 0;
            }
            int numBytesWritten = 0;
            int num2 = 0;
            fixed (byte* numRef = bytes)
            {
                if (this._isAsync)
                {
                    num2 = Win32Native.WriteFile(handle, numRef + offset, count, IntPtr.Zero, overlapped);
                }
                else
                {
                    num2 = Win32Native.WriteFile(handle, numRef + offset, count, out numBytesWritten, IntPtr.Zero);
                }
            }
            if (num2 == 0)
            {
                hr = Marshal.GetLastWin32Error();
                if ((hr != 0xe8) && (hr == 6))
                {
                    this._handle.SetHandleAsInvalid();
                }
                return -1;
            }
            hr = 0;
            return numBytesWritten;
        }

        public override bool CanRead
        {
            get
            {
                return this._canRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return this._canSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this._canWrite;
            }
        }

        [Obsolete("This property has been deprecated.  Please use FileStream's SafeFileHandle property instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        public virtual IntPtr Handle
        {
            [SecurityCritical, SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                this.Flush();
                this._readPos = 0;
                this._readLen = 0;
                this._writePos = 0;
                this._exposedHandle = true;
                return this._handle.DangerousGetHandle();
            }
        }

        public virtual bool IsAsync
        {
            get
            {
                return this._isAsync;
            }
        }

        public override long Length
        {
            [SecuritySafeCritical]
            get
            {
                if (this._handle.IsClosed)
                {
                    __Error.FileNotOpen();
                }
                if (!this.CanSeek)
                {
                    __Error.SeekNotSupported();
                }
                int highSize = 0;
                int fileSize = 0;
                fileSize = Win32Native.GetFileSize(this._handle, out highSize);
                if (fileSize == -1)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    if (errorCode != 0)
                    {
                        __Error.WinIOError(errorCode, string.Empty);
                    }
                }
                long num4 = (highSize << 0x20) | ((long) ((ulong) fileSize));
                if ((this._writePos > 0) && ((this._pos + this._writePos) > num4))
                {
                    num4 = this._writePos + this._pos;
                }
                return num4;
            }
        }

        public string Name
        {
            [SecuritySafeCritical]
            get
            {
                if (this._fileName == null)
                {
                    return Environment.GetResourceString("IO_UnknownFileName");
                }
                new FileIOPermission(FileIOPermissionAccess.PathDiscovery, new string[] { this._fileName }, false, false).Demand();
                return this._fileName;
            }
        }

        internal string NameInternal
        {
            get
            {
                if (this._fileName == null)
                {
                    return "<UnknownFileName>";
                }
                return this._fileName;
            }
        }

        public override long Position
        {
            [SecuritySafeCritical]
            get
            {
                if (this._handle.IsClosed)
                {
                    __Error.FileNotOpen();
                }
                if (!this.CanSeek)
                {
                    __Error.SeekNotSupported();
                }
                if (this._exposedHandle)
                {
                    this.VerifyOSHandlePosition();
                }
                return (this._pos + ((this._readPos - this._readLen) + this._writePos));
            }
            [SecuritySafeCritical]
            set
            {
                if (value < 0L)
                {
                    throw new ArgumentOutOfRangeException("value", Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
                }
                if (this._writePos > 0)
                {
                    this.FlushWrite(false);
                }
                this._readPos = 0;
                this._readLen = 0;
                this.Seek(value, SeekOrigin.Begin);
            }
        }

        public virtual Microsoft.Win32.SafeHandles.SafeFileHandle SafeFileHandle
        {
            [SecurityCritical, SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                this.Flush();
                this._readPos = 0;
                this._readLen = 0;
                this._writePos = 0;
                this._exposedHandle = true;
                return this._handle;
            }
        }
    }
}

