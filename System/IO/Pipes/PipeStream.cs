namespace System.IO.Pipes
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.AccessControl;
    using System.Security.Permissions;
    using System.Threading;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public abstract class PipeStream : Stream
    {
        private static readonly bool _canUseAsync = (Environment.OSVersion.Platform == PlatformID.Win32NT);
        private static readonly IOCompletionCallback IOCallback = new IOCompletionCallback(PipeStream.AsyncPSCallback);
        private bool m_canRead;
        private bool m_canWrite;
        private Microsoft.Win32.SafeHandles.SafePipeHandle m_handle;
        private bool m_isAsync;
        private bool m_isFromExistingHandle;
        private bool m_isHandleExposed;
        private bool m_isMessageComplete;
        private int m_outBufferSize;
        private PipeDirection m_pipeDirection;
        private PipeTransmissionMode m_readMode;
        private PipeState m_state;
        private PipeTransmissionMode m_transmissionMode;

        protected PipeStream(PipeDirection direction, int bufferSize)
        {
            if ((direction < PipeDirection.In) || (direction > PipeDirection.InOut))
            {
                throw new ArgumentOutOfRangeException("direction", System.SR.GetString("ArgumentOutOfRange_DirectionModeInOutOrInOut"));
            }
            if (bufferSize < 0)
            {
                throw new ArgumentOutOfRangeException("bufferSize", System.SR.GetString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            this.Init(direction, PipeTransmissionMode.Byte, bufferSize);
        }

        protected PipeStream(PipeDirection direction, PipeTransmissionMode transmissionMode, int outBufferSize)
        {
            if ((direction < PipeDirection.In) || (direction > PipeDirection.InOut))
            {
                throw new ArgumentOutOfRangeException("direction", System.SR.GetString("ArgumentOutOfRange_DirectionModeInOutOrInOut"));
            }
            if ((transmissionMode < PipeTransmissionMode.Byte) || (transmissionMode > PipeTransmissionMode.Message))
            {
                throw new ArgumentOutOfRangeException("transmissionMode", System.SR.GetString("ArgumentOutOfRange_TransmissionModeByteOrMsg"));
            }
            if (outBufferSize < 0)
            {
                throw new ArgumentOutOfRangeException("outBufferSize", System.SR.GetString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            this.Init(direction, transmissionMode, outBufferSize);
        }

        [SecurityCritical]
        private static unsafe void AsyncPSCallback(uint errorCode, uint numBytes, NativeOverlapped* pOverlapped)
        {
            PipeStreamAsyncResult asyncResult = (PipeStreamAsyncResult) Overlapped.Unpack(pOverlapped).AsyncResult;
            asyncResult._numBytes = (int) numBytes;
            if (!asyncResult._isWrite && (((errorCode == 0x6d) || (errorCode == 0xe9)) || (errorCode == 0xe8)))
            {
                errorCode = 0;
                numBytes = 0;
            }
            if (errorCode == 0xea)
            {
                errorCode = 0;
                asyncResult._isMessageComplete = false;
            }
            else
            {
                asyncResult._isMessageComplete = true;
            }
            asyncResult._errorCode = (int) errorCode;
            asyncResult._completedSynchronously = false;
            asyncResult._isComplete = true;
            ManualResetEvent event2 = asyncResult._waitHandle;
            if ((event2 != null) && !event2.Set())
            {
                System.IO.__Error.WinIOError();
            }
            AsyncCallback callback = asyncResult._userCallback;
            if (callback != null)
            {
                callback(asyncResult);
            }
        }

        [SecurityCritical, HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer", System.SR.GetString("ArgumentNull_Buffer"));
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", System.SR.GetString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", System.SR.GetString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((buffer.Length - offset) < count)
            {
                throw new ArgumentException(System.SR.GetString("Argument_InvalidOffLen"));
            }
            if (!this.CanRead)
            {
                System.IO.__Error.ReadNotSupported();
            }
            this.CheckReadOperations();
            if (this.m_isAsync)
            {
                return this.BeginReadCore(buffer, offset, count, callback, state);
            }
            if (this.m_state == PipeState.Broken)
            {
                PipeStreamAsyncResult result = new PipeStreamAsyncResult {
                    _handle = this.m_handle,
                    _userCallback = callback,
                    _userStateObject = state,
                    _isWrite = false
                };
                result.CallUserCallback();
                return result;
            }
            return base.BeginRead(buffer, offset, count, callback, state);
        }

        [SecurityCritical]
        private unsafe PipeStreamAsyncResult BeginReadCore(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            PipeStreamAsyncResult ar = new PipeStreamAsyncResult {
                _handle = this.m_handle,
                _userCallback = callback,
                _userStateObject = state,
                _isWrite = false
            };
            if (buffer.Length == 0)
            {
                ar.CallUserCallback();
                return ar;
            }
            ManualResetEvent event2 = new ManualResetEvent(false);
            ar._waitHandle = event2;
            NativeOverlapped* overlapped = new Overlapped(0, 0, IntPtr.Zero, ar).Pack(IOCallback, buffer);
            ar._overlapped = overlapped;
            int hr = 0;
            if (this.ReadFileNative(this.m_handle, buffer, offset, count, overlapped, out hr) == -1)
            {
                if ((hr == 0x6d) || (hr == 0xe9))
                {
                    this.State = PipeState.Broken;
                    overlapped->InternalLow = IntPtr.Zero;
                    ar.CallUserCallback();
                    return ar;
                }
                if (hr != 0x3e5)
                {
                    System.IO.__Error.WinIOError(hr, string.Empty);
                }
            }
            return ar;
        }

        [SecurityCritical, HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer", System.SR.GetString("ArgumentNull_Buffer"));
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", System.SR.GetString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", System.SR.GetString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((buffer.Length - offset) < count)
            {
                throw new ArgumentException(System.SR.GetString("Argument_InvalidOffLen"));
            }
            if (!this.CanWrite)
            {
                System.IO.__Error.WriteNotSupported();
            }
            this.CheckWriteOperations();
            if (!this.m_isAsync)
            {
                return base.BeginWrite(buffer, offset, count, callback, state);
            }
            return this.BeginWriteCore(buffer, offset, count, callback, state);
        }

        [SecurityCritical]
        private unsafe PipeStreamAsyncResult BeginWriteCore(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            PipeStreamAsyncResult ar = new PipeStreamAsyncResult {
                _userCallback = callback,
                _userStateObject = state,
                _isWrite = true,
                _handle = this.m_handle
            };
            if (buffer.Length == 0)
            {
                ar.CallUserCallback();
                return ar;
            }
            ManualResetEvent event2 = new ManualResetEvent(false);
            ar._waitHandle = event2;
            NativeOverlapped* overlapped = new Overlapped(0, 0, IntPtr.Zero, ar).Pack(IOCallback, buffer);
            ar._overlapped = overlapped;
            int hr = 0;
            if ((this.WriteFileNative(this.m_handle, buffer, offset, count, overlapped, out hr) == -1) && (hr != 0x3e5))
            {
                if (overlapped != null)
                {
                    Overlapped.Free(overlapped);
                }
                this.WinIOError(hr);
            }
            return ar;
        }

        [SecurityCritical]
        protected internal virtual void CheckPipePropertyOperations()
        {
            if (this.m_handle == null)
            {
                throw new InvalidOperationException(System.SR.GetString("InvalidOperation_PipeHandleNotSet"));
            }
            if (this.m_state == PipeState.Closed)
            {
                System.IO.__Error.PipeNotOpen();
            }
            if (this.m_handle.IsClosed)
            {
                System.IO.__Error.PipeNotOpen();
            }
        }

        [SecurityCritical]
        protected internal void CheckReadOperations()
        {
            if (this.m_state == PipeState.WaitingToConnect)
            {
                throw new InvalidOperationException(System.SR.GetString("InvalidOperation_PipeNotYetConnected"));
            }
            if (this.m_state == PipeState.Disconnected)
            {
                throw new InvalidOperationException(System.SR.GetString("InvalidOperation_PipeDisconnected"));
            }
            if (this.m_handle == null)
            {
                throw new InvalidOperationException(System.SR.GetString("InvalidOperation_PipeHandleNotSet"));
            }
            if (this.m_state == PipeState.Closed)
            {
                System.IO.__Error.PipeNotOpen();
            }
            if (this.m_handle.IsClosed)
            {
                System.IO.__Error.PipeNotOpen();
            }
        }

        [SecurityCritical]
        protected internal void CheckWriteOperations()
        {
            if (this.m_state == PipeState.WaitingToConnect)
            {
                throw new InvalidOperationException(System.SR.GetString("InvalidOperation_PipeNotYetConnected"));
            }
            if (this.m_state == PipeState.Disconnected)
            {
                throw new InvalidOperationException(System.SR.GetString("InvalidOperation_PipeDisconnected"));
            }
            if (this.m_handle == null)
            {
                throw new InvalidOperationException(System.SR.GetString("InvalidOperation_PipeHandleNotSet"));
            }
            if (this.m_state == PipeState.Broken)
            {
                throw new IOException(System.SR.GetString("IO_IO_PipeBroken"));
            }
            if (this.m_state == PipeState.Closed)
            {
                System.IO.__Error.PipeNotOpen();
            }
            if (this.m_handle.IsClosed)
            {
                System.IO.__Error.PipeNotOpen();
            }
        }

        [SecurityCritical]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if ((this.m_handle != null) && !this.m_handle.IsClosed)
                {
                    this.m_handle.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
            this.m_state = PipeState.Closed;
        }

        [SecurityCritical]
        public override unsafe int EndRead(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            if (!this.m_isAsync)
            {
                return base.EndRead(asyncResult);
            }
            PipeStreamAsyncResult result = asyncResult as PipeStreamAsyncResult;
            if ((result == null) || result._isWrite)
            {
                System.IO.__Error.WrongAsyncResult();
            }
            if (1 == Interlocked.CompareExchange(ref result._EndXxxCalled, 1, 0))
            {
                System.IO.__Error.EndReadCalledTwice();
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
                this.WinIOError(result._errorCode);
            }
            this.m_isMessageComplete = (this.m_state == PipeState.Broken) || result._isMessageComplete;
            return result._numBytes;
        }

        [SecurityCritical]
        public override unsafe void EndWrite(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            if (!this.m_isAsync)
            {
                base.EndWrite(asyncResult);
            }
            else
            {
                PipeStreamAsyncResult result = asyncResult as PipeStreamAsyncResult;
                if ((result == null) || !result._isWrite)
                {
                    System.IO.__Error.WrongAsyncResult();
                }
                if (1 == Interlocked.CompareExchange(ref result._EndXxxCalled, 1, 0))
                {
                    System.IO.__Error.EndWriteCalledTwice();
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
                    this.WinIOError(result._errorCode);
                }
            }
        }

        [SecurityCritical]
        public override void Flush()
        {
            this.CheckWriteOperations();
            if (!this.CanWrite)
            {
                System.IO.__Error.WriteNotSupported();
            }
        }

        [SecurityCritical]
        public PipeSecurity GetAccessControl()
        {
            if (this.m_state == PipeState.Closed)
            {
                System.IO.__Error.PipeNotOpen();
            }
            if (this.m_handle == null)
            {
                throw new InvalidOperationException(System.SR.GetString("InvalidOperation_PipeHandleNotSet"));
            }
            if (this.m_handle.IsClosed)
            {
                System.IO.__Error.PipeNotOpen();
            }
            return new PipeSecurity(this.m_handle, AccessControlSections.Group | AccessControlSections.Owner | AccessControlSections.Access);
        }

        [SecurityCritical]
        internal static Microsoft.Win32.UnsafeNativeMethods.SECURITY_ATTRIBUTES GetSecAttrs(HandleInheritability inheritability)
        {
            Microsoft.Win32.UnsafeNativeMethods.SECURITY_ATTRIBUTES structure = null;
            if ((inheritability & HandleInheritability.Inheritable) != HandleInheritability.None)
            {
                structure = new Microsoft.Win32.UnsafeNativeMethods.SECURITY_ATTRIBUTES {
                    nLength = Marshal.SizeOf(structure),
                    bInheritHandle = 1
                };
            }
            return structure;
        }

        [SecurityCritical]
        internal static unsafe Microsoft.Win32.UnsafeNativeMethods.SECURITY_ATTRIBUTES GetSecAttrs(HandleInheritability inheritability, PipeSecurity pipeSecurity, out object pinningHandle)
        {
            pinningHandle = null;
            Microsoft.Win32.UnsafeNativeMethods.SECURITY_ATTRIBUTES structure = null;
            if (((inheritability & HandleInheritability.Inheritable) != HandleInheritability.None) || (pipeSecurity != null))
            {
                structure = new Microsoft.Win32.UnsafeNativeMethods.SECURITY_ATTRIBUTES {
                    nLength = Marshal.SizeOf(structure)
                };
                if ((inheritability & HandleInheritability.Inheritable) != HandleInheritability.None)
                {
                    structure.bInheritHandle = 1;
                }
                if (pipeSecurity == null)
                {
                    return structure;
                }
                byte[] securityDescriptorBinaryForm = pipeSecurity.GetSecurityDescriptorBinaryForm();
                pinningHandle = GCHandle.Alloc(securityDescriptorBinaryForm, GCHandleType.Pinned);
                fixed (byte* numRef = securityDescriptorBinaryForm)
                {
                    structure.pSecurityDescriptor = numRef;
                }
            }
            return structure;
        }

        private void Init(PipeDirection direction, PipeTransmissionMode transmissionMode, int outBufferSize)
        {
            this.m_readMode = transmissionMode;
            this.m_transmissionMode = transmissionMode;
            this.m_pipeDirection = direction;
            if ((this.m_pipeDirection & PipeDirection.In) != ((PipeDirection) 0))
            {
                this.m_canRead = true;
            }
            if ((this.m_pipeDirection & PipeDirection.Out) != ((PipeDirection) 0))
            {
                this.m_canWrite = true;
            }
            this.m_outBufferSize = outBufferSize;
            this.m_isMessageComplete = true;
            this.m_state = PipeState.WaitingToConnect;
        }

        [SecurityCritical]
        protected void InitializeHandle(Microsoft.Win32.SafeHandles.SafePipeHandle handle, bool isExposed, bool isAsync)
        {
            isAsync &= _canUseAsync;
            if (isAsync)
            {
                bool flag = false;
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
                try
                {
                    flag = ThreadPool.BindHandle(handle);
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                if (!flag)
                {
                    throw new IOException(System.SR.GetString("IO_IO_BindHandleFailed"));
                }
            }
            this.m_handle = handle;
            this.m_isAsync = isAsync;
            this.m_isHandleExposed = isExposed;
            this.m_isFromExistingHandle = isExposed;
        }

        [SecurityCritical]
        public override int Read([In, Out] byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer", System.SR.GetString("ArgumentNull_Buffer"));
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", System.SR.GetString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", System.SR.GetString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((buffer.Length - offset) < count)
            {
                throw new ArgumentException(System.SR.GetString("Argument_InvalidOffLen"));
            }
            if (!this.CanRead)
            {
                System.IO.__Error.ReadNotSupported();
            }
            this.CheckReadOperations();
            return this.ReadCore(buffer, offset, count);
        }

        [SecurityCritical]
        public override int ReadByte()
        {
            this.CheckReadOperations();
            if (!this.CanRead)
            {
                System.IO.__Error.ReadNotSupported();
            }
            byte[] buffer = new byte[1];
            if (this.ReadCore(buffer, 0, 1) == 0)
            {
                return -1;
            }
            return buffer[0];
        }

        [SecurityCritical]
        private int ReadCore(byte[] buffer, int offset, int count)
        {
            if (this.m_isAsync)
            {
                IAsyncResult asyncResult = this.BeginReadCore(buffer, offset, count, null, null);
                return this.EndRead(asyncResult);
            }
            int hr = 0;
            int num2 = this.ReadFileNative(this.m_handle, buffer, offset, count, null, out hr);
            if (num2 == -1)
            {
                switch (hr)
                {
                    case 0x6d:
                    case 0xe9:
                        this.State = PipeState.Broken;
                        num2 = 0;
                        goto Label_0059;
                }
                System.IO.__Error.WinIOError(hr, string.Empty);
            }
        Label_0059:
            this.m_isMessageComplete = hr != 0xea;
            return num2;
        }

        [SecurityCritical]
        private unsafe int ReadFileNative(Microsoft.Win32.SafeHandles.SafePipeHandle handle, byte[] buffer, int offset, int count, NativeOverlapped* overlapped, out int hr)
        {
            if (buffer.Length == 0)
            {
                hr = 0;
                return 0;
            }
            int num = 0;
            int numBytesRead = 0;
            fixed (byte* numRef = buffer)
            {
                if (this.m_isAsync)
                {
                    num = Microsoft.Win32.UnsafeNativeMethods.ReadFile(handle, numRef + offset, count, IntPtr.Zero, overlapped);
                }
                else
                {
                    num = Microsoft.Win32.UnsafeNativeMethods.ReadFile(handle, numRef + offset, count, out numBytesRead, IntPtr.Zero);
                }
            }
            if (num == 0)
            {
                hr = Marshal.GetLastWin32Error();
                if (hr == 0xea)
                {
                    return numBytesRead;
                }
                return -1;
            }
            hr = 0;
            return numBytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            System.IO.__Error.SeekNotSupported();
            return 0L;
        }

        [SecurityCritical]
        public void SetAccessControl(PipeSecurity pipeSecurity)
        {
            if (pipeSecurity == null)
            {
                throw new ArgumentNullException("pipeSecurity");
            }
            this.CheckPipePropertyOperations();
            pipeSecurity.Persist(this.m_handle);
        }

        public override void SetLength(long value)
        {
            System.IO.__Error.SeekNotSupported();
        }

        [SecurityCritical]
        private void UpdateReadMode()
        {
            int num;
            if (!Microsoft.Win32.UnsafeNativeMethods.GetNamedPipeHandleState(this.SafePipeHandle, out num, Microsoft.Win32.UnsafeNativeMethods.NULL, Microsoft.Win32.UnsafeNativeMethods.NULL, Microsoft.Win32.UnsafeNativeMethods.NULL, Microsoft.Win32.UnsafeNativeMethods.NULL, 0))
            {
                this.WinIOError(Marshal.GetLastWin32Error());
            }
            if ((num & 2) != 0)
            {
                this.m_readMode = PipeTransmissionMode.Message;
            }
            else
            {
                this.m_readMode = PipeTransmissionMode.Byte;
            }
        }

        [SecurityCritical]
        public void WaitForPipeDrain()
        {
            this.CheckWriteOperations();
            if (!this.CanWrite)
            {
                System.IO.__Error.WriteNotSupported();
            }
            if (!Microsoft.Win32.UnsafeNativeMethods.FlushFileBuffers(this.m_handle))
            {
                this.WinIOError(Marshal.GetLastWin32Error());
            }
        }

        [SecurityCritical]
        internal void WinIOError(int errorCode)
        {
            if (((errorCode == 0x6d) || (errorCode == 0xe9)) || (errorCode == 0xe8))
            {
                this.m_state = PipeState.Broken;
                throw new IOException(System.SR.GetString("IO_IO_PipeBroken"), Microsoft.Win32.UnsafeNativeMethods.MakeHRFromErrorCode(errorCode));
            }
            if (errorCode == 0x26)
            {
                System.IO.__Error.EndOfFile();
            }
            else
            {
                if (errorCode == 6)
                {
                    this.m_handle.SetHandleAsInvalid();
                    this.m_state = PipeState.Broken;
                }
                System.IO.__Error.WinIOError(errorCode, string.Empty);
            }
        }

        [SecurityCritical]
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer", System.SR.GetString("ArgumentNull_Buffer"));
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", System.SR.GetString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", System.SR.GetString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((buffer.Length - offset) < count)
            {
                throw new ArgumentException(System.SR.GetString("Argument_InvalidOffLen"));
            }
            if (!this.CanWrite)
            {
                System.IO.__Error.WriteNotSupported();
            }
            this.CheckWriteOperations();
            this.WriteCore(buffer, offset, count);
        }

        [SecurityCritical]
        public override void WriteByte(byte value)
        {
            this.CheckWriteOperations();
            if (!this.CanWrite)
            {
                System.IO.__Error.WriteNotSupported();
            }
            byte[] buffer = new byte[] { value };
            this.WriteCore(buffer, 0, 1);
        }

        [SecurityCritical]
        private void WriteCore(byte[] buffer, int offset, int count)
        {
            if (this.m_isAsync)
            {
                IAsyncResult asyncResult = this.BeginWriteCore(buffer, offset, count, null, null);
                this.EndWrite(asyncResult);
            }
            else
            {
                int hr = 0;
                if (this.WriteFileNative(this.m_handle, buffer, offset, count, null, out hr) == -1)
                {
                    this.WinIOError(hr);
                }
            }
        }

        [SecurityCritical]
        private unsafe int WriteFileNative(Microsoft.Win32.SafeHandles.SafePipeHandle handle, byte[] buffer, int offset, int count, NativeOverlapped* overlapped, out int hr)
        {
            if (buffer.Length == 0)
            {
                hr = 0;
                return 0;
            }
            int numBytesWritten = 0;
            int num2 = 0;
            fixed (byte* numRef = buffer)
            {
                if (this.m_isAsync)
                {
                    num2 = Microsoft.Win32.UnsafeNativeMethods.WriteFile(handle, numRef + offset, count, IntPtr.Zero, overlapped);
                }
                else
                {
                    num2 = Microsoft.Win32.UnsafeNativeMethods.WriteFile(handle, numRef + offset, count, out numBytesWritten, IntPtr.Zero);
                }
            }
            if (num2 == 0)
            {
                hr = Marshal.GetLastWin32Error();
                return -1;
            }
            hr = 0;
            return numBytesWritten;
        }

        public override bool CanRead
        {
            get
            {
                return this.m_canRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this.m_canWrite;
            }
        }

        public virtual int InBufferSize
        {
            [SecurityCritical]
            get
            {
                int num;
                this.CheckPipePropertyOperations();
                if (!this.CanRead)
                {
                    throw new NotSupportedException(System.SR.GetString("NotSupported_UnreadableStream"));
                }
                if (!Microsoft.Win32.UnsafeNativeMethods.GetNamedPipeInfo(this.m_handle, Microsoft.Win32.UnsafeNativeMethods.NULL, Microsoft.Win32.UnsafeNativeMethods.NULL, out num, Microsoft.Win32.UnsafeNativeMethods.NULL))
                {
                    this.WinIOError(Marshal.GetLastWin32Error());
                }
                return num;
            }
        }

        internal Microsoft.Win32.SafeHandles.SafePipeHandle InternalHandle
        {
            [SecurityCritical]
            get
            {
                return this.m_handle;
            }
        }

        public bool IsAsync
        {
            get
            {
                return this.m_isAsync;
            }
        }

        public bool IsConnected
        {
            get
            {
                return (this.State == PipeState.Connected);
            }
            protected set
            {
                this.m_state = value ? PipeState.Connected : PipeState.Disconnected;
            }
        }

        protected bool IsHandleExposed
        {
            get
            {
                return this.m_isHandleExposed;
            }
        }

        public bool IsMessageComplete
        {
            [SecurityCritical]
            get
            {
                if (this.m_state == PipeState.WaitingToConnect)
                {
                    throw new InvalidOperationException(System.SR.GetString("InvalidOperation_PipeNotYetConnected"));
                }
                if (this.m_state == PipeState.Disconnected)
                {
                    throw new InvalidOperationException(System.SR.GetString("InvalidOperation_PipeDisconnected"));
                }
                if (this.m_handle == null)
                {
                    throw new InvalidOperationException(System.SR.GetString("InvalidOperation_PipeHandleNotSet"));
                }
                if (this.m_state == PipeState.Closed)
                {
                    System.IO.__Error.PipeNotOpen();
                }
                if (this.m_handle.IsClosed)
                {
                    System.IO.__Error.PipeNotOpen();
                }
                if (this.m_readMode != PipeTransmissionMode.Message)
                {
                    throw new InvalidOperationException(System.SR.GetString("InvalidOperation_PipeReadModeNotMessage"));
                }
                return this.m_isMessageComplete;
            }
        }

        public override long Length
        {
            get
            {
                System.IO.__Error.SeekNotSupported();
                return 0L;
            }
        }

        public virtual int OutBufferSize
        {
            [SecurityCritical]
            get
            {
                int num;
                this.CheckPipePropertyOperations();
                if (!this.CanWrite)
                {
                    throw new NotSupportedException(System.SR.GetString("NotSupported_UnwritableStream"));
                }
                if (this.m_pipeDirection == PipeDirection.Out)
                {
                    return this.m_outBufferSize;
                }
                if (!Microsoft.Win32.UnsafeNativeMethods.GetNamedPipeInfo(this.m_handle, Microsoft.Win32.UnsafeNativeMethods.NULL, out num, Microsoft.Win32.UnsafeNativeMethods.NULL, Microsoft.Win32.UnsafeNativeMethods.NULL))
                {
                    this.WinIOError(Marshal.GetLastWin32Error());
                }
                return num;
            }
        }

        public override long Position
        {
            get
            {
                System.IO.__Error.SeekNotSupported();
                return 0L;
            }
            set
            {
                System.IO.__Error.SeekNotSupported();
            }
        }

        public virtual PipeTransmissionMode ReadMode
        {
            [SecurityCritical]
            get
            {
                this.CheckPipePropertyOperations();
                if (this.m_isFromExistingHandle || this.IsHandleExposed)
                {
                    this.UpdateReadMode();
                }
                return this.m_readMode;
            }
            [SecurityCritical]
            set
            {
                this.CheckPipePropertyOperations();
                if ((value < PipeTransmissionMode.Byte) || (value > PipeTransmissionMode.Message))
                {
                    throw new ArgumentOutOfRangeException("value", System.SR.GetString("ArgumentOutOfRange_TransmissionModeByteOrMsg"));
                }
                int lpMode = ((int) value) << 1;
                if (!Microsoft.Win32.UnsafeNativeMethods.SetNamedPipeHandleState(this.m_handle, &lpMode, Microsoft.Win32.UnsafeNativeMethods.NULL, Microsoft.Win32.UnsafeNativeMethods.NULL))
                {
                    this.WinIOError(Marshal.GetLastWin32Error());
                }
                else
                {
                    this.m_readMode = value;
                }
            }
        }

        public Microsoft.Win32.SafeHandles.SafePipeHandle SafePipeHandle
        {
            [SecurityCritical]
            get
            {
                if (this.m_handle == null)
                {
                    throw new InvalidOperationException(System.SR.GetString("InvalidOperation_PipeHandleNotSet"));
                }
                if (this.m_handle.IsClosed)
                {
                    System.IO.__Error.PipeNotOpen();
                }
                this.m_isHandleExposed = true;
                return this.m_handle;
            }
        }

        internal PipeState State
        {
            get
            {
                return this.m_state;
            }
            set
            {
                this.m_state = value;
            }
        }

        public virtual PipeTransmissionMode TransmissionMode
        {
            [SecurityCritical]
            get
            {
                int num;
                this.CheckPipePropertyOperations();
                if (!this.m_isFromExistingHandle)
                {
                    return this.m_transmissionMode;
                }
                if (!Microsoft.Win32.UnsafeNativeMethods.GetNamedPipeInfo(this.m_handle, out num, Microsoft.Win32.UnsafeNativeMethods.NULL, Microsoft.Win32.UnsafeNativeMethods.NULL, Microsoft.Win32.UnsafeNativeMethods.NULL))
                {
                    this.WinIOError(Marshal.GetLastWin32Error());
                }
                if ((num & 4) != 0)
                {
                    return PipeTransmissionMode.Message;
                }
                return PipeTransmissionMode.Byte;
            }
        }
    }
}

