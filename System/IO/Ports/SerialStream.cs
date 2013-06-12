namespace System.IO.Ports
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;

    internal sealed class SerialStream : Stream
    {
        internal SafeFileHandle _handle;
        private Microsoft.Win32.UnsafeNativeMethods.COMMPROP commProp;
        private Microsoft.Win32.UnsafeNativeMethods.COMMTIMEOUTS commTimeouts;
        private Microsoft.Win32.UnsafeNativeMethods.COMSTAT comStat;
        private Microsoft.Win32.UnsafeNativeMethods.DCB dcb;
        private const int errorEvents = 0x10f;
        internal EventLoopRunner eventRunner;
        private System.IO.Ports.Handshake handshake;
        private bool inBreak;
        private const int infiniteTimeoutConst = -2;
        private static readonly IOCompletionCallback IOCallback = new IOCompletionCallback(SerialStream.AsyncFSCallback);
        private bool isAsync = true;
        private const int maxDataBits = 8;
        private const int minDataBits = 5;
        private byte parityReplace = 0x3f;
        private const int pinChangedEvents = 0x178;
        private string portName;
        private const int receivedEvents = 3;
        private bool rtsEnable;
        private byte[] tempBuf;

        internal event SerialDataReceivedEventHandler DataReceived;

        internal event SerialErrorReceivedEventHandler ErrorReceived;

        internal event SerialPinChangedEventHandler PinChanged;

        internal SerialStream(string portName, int baudRate, System.IO.Ports.Parity parity, int dataBits, System.IO.Ports.StopBits stopBits, int readTimeout, int writeTimeout, System.IO.Ports.Handshake handshake, bool dtrEnable, bool rtsEnable, bool discardNull, byte parityReplace)
        {
            int dwFlagsAndAttributes = 0x40000000;
            if (Environment.OSVersion.Platform == PlatformID.Win32Windows)
            {
                dwFlagsAndAttributes = 0x80;
                this.isAsync = false;
            }
            if ((portName == null) || !portName.StartsWith("COM", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(SR.GetString("Arg_InvalidSerialPort"), "portName");
            }
            SafeFileHandle hFile = Microsoft.Win32.UnsafeNativeMethods.CreateFile(@"\\.\" + portName, -1073741824, 0, IntPtr.Zero, 3, dwFlagsAndAttributes, IntPtr.Zero);
            if (hFile.IsInvalid)
            {
                InternalResources.WinIOError(portName);
            }
            try
            {
                int fileType = Microsoft.Win32.UnsafeNativeMethods.GetFileType(hFile);
                if ((fileType != 2) && (fileType != 0))
                {
                    throw new ArgumentException(SR.GetString("Arg_InvalidSerialPort"), "portName");
                }
                this._handle = hFile;
                this.portName = portName;
                this.handshake = handshake;
                this.parityReplace = parityReplace;
                this.tempBuf = new byte[1];
                this.commProp = new Microsoft.Win32.UnsafeNativeMethods.COMMPROP();
                int lpModemStat = 0;
                if (!Microsoft.Win32.UnsafeNativeMethods.GetCommProperties(this._handle, ref this.commProp) || !Microsoft.Win32.UnsafeNativeMethods.GetCommModemStatus(this._handle, ref lpModemStat))
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    switch (errorCode)
                    {
                        case 0x57:
                        case 6:
                            throw new ArgumentException(SR.GetString("Arg_InvalidSerialPortExtended"), "portName");
                    }
                    InternalResources.WinIOError(errorCode, string.Empty);
                }
                if ((this.commProp.dwMaxBaud != 0) && (baudRate > this.commProp.dwMaxBaud))
                {
                    throw new ArgumentOutOfRangeException("baudRate", SR.GetString("Max_Baud", new object[] { this.commProp.dwMaxBaud }));
                }
                this.comStat = new Microsoft.Win32.UnsafeNativeMethods.COMSTAT();
                this.dcb = new Microsoft.Win32.UnsafeNativeMethods.DCB();
                this.InitializeDCB(baudRate, parity, dataBits, stopBits, discardNull);
                this.DtrEnable = dtrEnable;
                this.rtsEnable = this.GetDcbFlag(12) == 1;
                if ((handshake != System.IO.Ports.Handshake.RequestToSend) && (handshake != System.IO.Ports.Handshake.RequestToSendXOnXOff))
                {
                    this.RtsEnable = rtsEnable;
                }
                if (readTimeout == 0)
                {
                    this.commTimeouts.ReadTotalTimeoutConstant = 0;
                    this.commTimeouts.ReadTotalTimeoutMultiplier = 0;
                    this.commTimeouts.ReadIntervalTimeout = -1;
                }
                else if (readTimeout == -1)
                {
                    this.commTimeouts.ReadTotalTimeoutConstant = -2;
                    this.commTimeouts.ReadTotalTimeoutMultiplier = -1;
                    this.commTimeouts.ReadIntervalTimeout = -1;
                }
                else
                {
                    this.commTimeouts.ReadTotalTimeoutConstant = readTimeout;
                    this.commTimeouts.ReadTotalTimeoutMultiplier = -1;
                    this.commTimeouts.ReadIntervalTimeout = -1;
                }
                this.commTimeouts.WriteTotalTimeoutMultiplier = 0;
                this.commTimeouts.WriteTotalTimeoutConstant = (writeTimeout == -1) ? 0 : writeTimeout;
                if (!Microsoft.Win32.UnsafeNativeMethods.SetCommTimeouts(this._handle, ref this.commTimeouts))
                {
                    InternalResources.WinIOError();
                }
                if (this.isAsync && !ThreadPool.BindHandle(this._handle))
                {
                    throw new IOException(SR.GetString("IO_BindHandleFailed"));
                }
                Microsoft.Win32.UnsafeNativeMethods.SetCommMask(this._handle, 0x1fb);
                this.eventRunner = new EventLoopRunner(this);
                new Thread(new ThreadStart(this.eventRunner.WaitForCommEvent)) { IsBackground = true }.Start();
            }
            catch
            {
                hFile.Close();
                this._handle = null;
                throw;
            }
        }

        private static unsafe void AsyncFSCallback(uint errorCode, uint numBytes, NativeOverlapped* pOverlapped)
        {
            SerialStreamAsyncResult asyncResult = (SerialStreamAsyncResult) Overlapped.Unpack(pOverlapped).AsyncResult;
            asyncResult._numBytes = (int) numBytes;
            asyncResult._errorCode = (int) errorCode;
            asyncResult._completedSynchronously = false;
            asyncResult._isComplete = true;
            ManualResetEvent event2 = asyncResult._waitHandle;
            if ((event2 != null) && !event2.Set())
            {
                InternalResources.WinIOError();
            }
            AsyncCallback callback = asyncResult._userCallback;
            if (callback != null)
            {
                callback(asyncResult);
            }
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override IAsyncResult BeginRead(byte[] array, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
        {
            IAsyncResult result;
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", SR.GetString("ArgumentOutOfRange_NeedNonNegNumRequired"));
            }
            if (numBytes < 0)
            {
                throw new ArgumentOutOfRangeException("numBytes", SR.GetString("ArgumentOutOfRange_NeedNonNegNumRequired"));
            }
            if ((array.Length - offset) < numBytes)
            {
                throw new ArgumentException(SR.GetString("Argument_InvalidOffLen"));
            }
            if (this._handle == null)
            {
                InternalResources.FileNotOpen();
            }
            int readTimeout = this.ReadTimeout;
            this.ReadTimeout = -1;
            try
            {
                if (!this.isAsync)
                {
                    return base.BeginRead(array, offset, numBytes, userCallback, stateObject);
                }
                result = this.BeginReadCore(array, offset, numBytes, userCallback, stateObject);
            }
            finally
            {
                this.ReadTimeout = readTimeout;
            }
            return result;
        }

        private unsafe SerialStreamAsyncResult BeginReadCore(byte[] array, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
        {
            SerialStreamAsyncResult ar = new SerialStreamAsyncResult {
                _userCallback = userCallback,
                _userStateObject = stateObject,
                _isWrite = false
            };
            ManualResetEvent event2 = new ManualResetEvent(false);
            ar._waitHandle = event2;
            NativeOverlapped* overlapped = new Overlapped(0, 0, IntPtr.Zero, ar).Pack(IOCallback, array);
            ar._overlapped = overlapped;
            int hr = 0;
            if ((this.ReadFileNative(array, offset, numBytes, overlapped, out hr) == -1) && (hr != 0x3e5))
            {
                if (hr == 0x26)
                {
                    InternalResources.EndOfFile();
                    return ar;
                }
                InternalResources.WinIOError(hr, string.Empty);
            }
            return ar;
        }

        [HostProtection(SecurityAction.LinkDemand, ExternalThreading=true)]
        public override IAsyncResult BeginWrite(byte[] array, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
        {
            IAsyncResult result;
            if (this.inBreak)
            {
                throw new InvalidOperationException(SR.GetString("In_Break_State"));
            }
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", SR.GetString("ArgumentOutOfRange_NeedNonNegNumRequired"));
            }
            if (numBytes < 0)
            {
                throw new ArgumentOutOfRangeException("numBytes", SR.GetString("ArgumentOutOfRange_NeedNonNegNumRequired"));
            }
            if ((array.Length - offset) < numBytes)
            {
                throw new ArgumentException(SR.GetString("Argument_InvalidOffLen"));
            }
            if (this._handle == null)
            {
                InternalResources.FileNotOpen();
            }
            int writeTimeout = this.WriteTimeout;
            this.WriteTimeout = -1;
            try
            {
                if (!this.isAsync)
                {
                    return base.BeginWrite(array, offset, numBytes, userCallback, stateObject);
                }
                result = this.BeginWriteCore(array, offset, numBytes, userCallback, stateObject);
            }
            finally
            {
                this.WriteTimeout = writeTimeout;
            }
            return result;
        }

        private unsafe SerialStreamAsyncResult BeginWriteCore(byte[] array, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
        {
            SerialStreamAsyncResult ar = new SerialStreamAsyncResult {
                _userCallback = userCallback,
                _userStateObject = stateObject,
                _isWrite = true
            };
            ManualResetEvent event2 = new ManualResetEvent(false);
            ar._waitHandle = event2;
            NativeOverlapped* overlapped = new Overlapped(0, 0, IntPtr.Zero, ar).Pack(IOCallback, array);
            ar._overlapped = overlapped;
            int hr = 0;
            if ((this.WriteFileNative(array, offset, numBytes, overlapped, out hr) == -1) && (hr != 0x3e5))
            {
                if (hr == 0x26)
                {
                    InternalResources.EndOfFile();
                    return ar;
                }
                InternalResources.WinIOError(hr, string.Empty);
            }
            return ar;
        }

        internal void DiscardInBuffer()
        {
            if (!Microsoft.Win32.UnsafeNativeMethods.PurgeComm(this._handle, 10))
            {
                InternalResources.WinIOError();
            }
        }

        internal void DiscardOutBuffer()
        {
            if (!Microsoft.Win32.UnsafeNativeMethods.PurgeComm(this._handle, 5))
            {
                InternalResources.WinIOError();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if ((this._handle != null) && !this._handle.IsInvalid)
            {
                try
                {
                    this.eventRunner.endEventLoop = true;
                    Thread.MemoryBarrier();
                    bool flag = false;
                    Microsoft.Win32.UnsafeNativeMethods.SetCommMask(this._handle, 0);
                    if (!Microsoft.Win32.UnsafeNativeMethods.EscapeCommFunction(this._handle, 6))
                    {
                        if ((Marshal.GetLastWin32Error() == 5) && !disposing)
                        {
                            flag = true;
                        }
                        else
                        {
                            InternalResources.WinIOError();
                        }
                    }
                    if (!flag && !this._handle.IsClosed)
                    {
                        this.Flush();
                    }
                    this.eventRunner.waitCommEventWaitHandle.Set();
                    if (!flag)
                    {
                        this.DiscardInBuffer();
                        this.DiscardOutBuffer();
                    }
                    if (disposing && (this.eventRunner != null))
                    {
                        this.eventRunner.eventLoopEndedSignal.WaitOne();
                        this.eventRunner.eventLoopEndedSignal.Close();
                        this.eventRunner.waitCommEventWaitHandle.Close();
                    }
                }
                finally
                {
                    if (disposing)
                    {
                        lock (this)
                        {
                            this._handle.Close();
                            this._handle = null;
                            goto Label_0112;
                        }
                    }
                    this._handle.Close();
                    this._handle = null;
                Label_0112:
                    base.Dispose(disposing);
                }
            }
        }

        public override unsafe int EndRead(IAsyncResult asyncResult)
        {
            if (!this.isAsync)
            {
                return base.EndRead(asyncResult);
            }
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            SerialStreamAsyncResult result = asyncResult as SerialStreamAsyncResult;
            if ((result == null) || result._isWrite)
            {
                InternalResources.WrongAsyncResult();
            }
            if (1 == Interlocked.CompareExchange(ref result._EndXxxCalled, 1, 0))
            {
                InternalResources.EndReadCalledTwice();
            }
            bool flag = false;
            WaitHandle handle = result._waitHandle;
            if (handle != null)
            {
                try
                {
                    handle.WaitOne();
                    if (((result._numBytes == 0) && (this.ReadTimeout == -1)) && (result._errorCode == 0))
                    {
                        flag = true;
                    }
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
                InternalResources.WinIOError(result._errorCode, this.portName);
            }
            if (flag)
            {
                throw new IOException(SR.GetString("IO_OperationAborted"));
            }
            return result._numBytes;
        }

        public override unsafe void EndWrite(IAsyncResult asyncResult)
        {
            if (!this.isAsync)
            {
                base.EndWrite(asyncResult);
            }
            else
            {
                if (this.inBreak)
                {
                    throw new InvalidOperationException(SR.GetString("In_Break_State"));
                }
                if (asyncResult == null)
                {
                    throw new ArgumentNullException("asyncResult");
                }
                SerialStreamAsyncResult result = asyncResult as SerialStreamAsyncResult;
                if ((result == null) || !result._isWrite)
                {
                    InternalResources.WrongAsyncResult();
                }
                if (1 == Interlocked.CompareExchange(ref result._EndXxxCalled, 1, 0))
                {
                    InternalResources.EndWriteCalledTwice();
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
                    InternalResources.WinIOError(result._errorCode, this.portName);
                }
            }
        }

        ~SerialStream()
        {
            this.Dispose(false);
        }

        public override void Flush()
        {
            if (this._handle == null)
            {
                throw new ObjectDisposedException(SR.GetString("Port_not_open"));
            }
            Microsoft.Win32.UnsafeNativeMethods.FlushFileBuffers(this._handle);
        }

        internal int GetDcbFlag(int whichFlag)
        {
            uint num;
            if ((whichFlag == 4) || (whichFlag == 12))
            {
                num = 3;
            }
            else if (whichFlag == 15)
            {
                num = 0x1ffff;
            }
            else
            {
                num = 1;
            }
            uint num2 = this.dcb.Flags & (num << whichFlag);
            return (int) (num2 >> whichFlag);
        }

        private void InitializeDCB(int baudRate, System.IO.Ports.Parity parity, int dataBits, System.IO.Ports.StopBits stopBits, bool discardNull)
        {
            if (!Microsoft.Win32.UnsafeNativeMethods.GetCommState(this._handle, ref this.dcb))
            {
                InternalResources.WinIOError();
            }
            this.dcb.DCBlength = (uint) Marshal.SizeOf(this.dcb);
            this.dcb.BaudRate = (uint) baudRate;
            this.dcb.ByteSize = (byte) dataBits;
            switch (stopBits)
            {
                case System.IO.Ports.StopBits.One:
                    this.dcb.StopBits = 0;
                    break;

                case System.IO.Ports.StopBits.Two:
                    this.dcb.StopBits = 2;
                    break;

                case System.IO.Ports.StopBits.OnePointFive:
                    this.dcb.StopBits = 1;
                    break;
            }
            this.dcb.Parity = (byte) parity;
            this.SetDcbFlag(1, (parity == System.IO.Ports.Parity.None) ? 0 : 1);
            this.SetDcbFlag(0, 1);
            this.SetDcbFlag(2, ((this.handshake == System.IO.Ports.Handshake.RequestToSend) || (this.handshake == System.IO.Ports.Handshake.RequestToSendXOnXOff)) ? 1 : 0);
            this.SetDcbFlag(3, 0);
            this.SetDcbFlag(4, 0);
            this.SetDcbFlag(6, 0);
            this.SetDcbFlag(9, ((this.handshake == System.IO.Ports.Handshake.XOnXOff) || (this.handshake == System.IO.Ports.Handshake.RequestToSendXOnXOff)) ? 1 : 0);
            this.SetDcbFlag(8, ((this.handshake == System.IO.Ports.Handshake.XOnXOff) || (this.handshake == System.IO.Ports.Handshake.RequestToSendXOnXOff)) ? 1 : 0);
            if (parity != System.IO.Ports.Parity.None)
            {
                this.SetDcbFlag(10, (this.parityReplace != 0) ? 1 : 0);
                this.dcb.ErrorChar = this.parityReplace;
            }
            else
            {
                this.SetDcbFlag(10, 0);
                this.dcb.ErrorChar = 0;
            }
            this.SetDcbFlag(11, discardNull ? 1 : 0);
            if ((this.handshake == System.IO.Ports.Handshake.RequestToSend) || (this.handshake == System.IO.Ports.Handshake.RequestToSendXOnXOff))
            {
                this.SetDcbFlag(12, 2);
            }
            else if (this.GetDcbFlag(12) == 2)
            {
                this.SetDcbFlag(12, 0);
            }
            this.dcb.XonChar = 0x11;
            this.dcb.XoffChar = 0x13;
            this.dcb.XonLim = this.dcb.XoffLim = (ushort) (this.commProp.dwCurrentRxQueue / 4);
            this.dcb.EofChar = 0x1a;
            this.dcb.EvtChar = 0x1a;
            if (!Microsoft.Win32.UnsafeNativeMethods.SetCommState(this._handle, ref this.dcb))
            {
                InternalResources.WinIOError();
            }
        }

        public override int Read([In, Out] byte[] array, int offset, int count)
        {
            return this.Read(array, offset, count, this.ReadTimeout);
        }

        internal int Read([In, Out] byte[] array, int offset, int count, int timeout)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array", SR.GetString("ArgumentNull_Buffer"));
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", SR.GetString("ArgumentOutOfRange_NeedNonNegNumRequired"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", SR.GetString("ArgumentOutOfRange_NeedNonNegNumRequired"));
            }
            if ((array.Length - offset) < count)
            {
                throw new ArgumentException(SR.GetString("Argument_InvalidOffLen"));
            }
            if (count == 0)
            {
                return 0;
            }
            if (this._handle == null)
            {
                InternalResources.FileNotOpen();
            }
            int num = 0;
            if (this.isAsync)
            {
                IAsyncResult asyncResult = this.BeginReadCore(array, offset, count, null, null);
                num = this.EndRead(asyncResult);
            }
            else
            {
                int num2;
                num = this.ReadFileNative(array, offset, count, null, out num2);
                if (num == -1)
                {
                    InternalResources.WinIOError();
                }
            }
            if (num == 0)
            {
                throw new TimeoutException();
            }
            return num;
        }

        public override int ReadByte()
        {
            return this.ReadByte(this.ReadTimeout);
        }

        internal int ReadByte(int timeout)
        {
            if (this._handle == null)
            {
                InternalResources.FileNotOpen();
            }
            int num = 0;
            if (this.isAsync)
            {
                IAsyncResult asyncResult = this.BeginReadCore(this.tempBuf, 0, 1, null, null);
                num = this.EndRead(asyncResult);
            }
            else
            {
                int num2;
                num = this.ReadFileNative(this.tempBuf, 0, 1, null, out num2);
                if (num == -1)
                {
                    InternalResources.WinIOError();
                }
            }
            if (num == 0)
            {
                throw new TimeoutException();
            }
            return this.tempBuf[0];
        }

        private unsafe int ReadFileNative(byte[] bytes, int offset, int count, NativeOverlapped* overlapped, out int hr)
        {
            if ((bytes.Length - offset) < count)
            {
                throw new IndexOutOfRangeException(SR.GetString("IndexOutOfRange_IORaceCondition"));
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
                if (this.isAsync)
                {
                    num = Microsoft.Win32.UnsafeNativeMethods.ReadFile(this._handle, numRef + offset, count, IntPtr.Zero, overlapped);
                }
                else
                {
                    num = Microsoft.Win32.UnsafeNativeMethods.ReadFile(this._handle, numRef + offset, count, out numBytesRead, IntPtr.Zero);
                }
            }
            if (num == 0)
            {
                hr = Marshal.GetLastWin32Error();
                if (hr == 6)
                {
                    this._handle.SetHandleAsInvalid();
                }
                return -1;
            }
            hr = 0;
            return numBytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException(SR.GetString("NotSupported_UnseekableStream"));
        }

        internal void SetBufferSizes(int readBufferSize, int writeBufferSize)
        {
            if (this._handle == null)
            {
                InternalResources.FileNotOpen();
            }
            if (!Microsoft.Win32.UnsafeNativeMethods.SetupComm(this._handle, readBufferSize, writeBufferSize))
            {
                InternalResources.WinIOError();
            }
        }

        internal void SetDcbFlag(int whichFlag, int setting)
        {
            uint num;
            setting = setting << whichFlag;
            if ((whichFlag == 4) || (whichFlag == 12))
            {
                num = 3;
            }
            else if (whichFlag == 15)
            {
                num = 0x1ffff;
            }
            else
            {
                num = 1;
            }
            this.dcb.Flags &= ~(num << whichFlag);
            this.dcb.Flags |= (uint) setting;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException(SR.GetString("NotSupported_UnseekableStream"));
        }

        public override void Write(byte[] array, int offset, int count)
        {
            this.Write(array, offset, count, this.WriteTimeout);
        }

        internal void Write(byte[] array, int offset, int count, int timeout)
        {
            if (this.inBreak)
            {
                throw new InvalidOperationException(SR.GetString("In_Break_State"));
            }
            if (array == null)
            {
                throw new ArgumentNullException("buffer", SR.GetString("ArgumentNull_Array"));
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", SR.GetString("ArgumentOutOfRange_NeedPosNum"));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count", SR.GetString("ArgumentOutOfRange_NeedPosNum"));
            }
            if (count != 0)
            {
                int num;
                if ((array.Length - offset) < count)
                {
                    throw new ArgumentException("count", SR.GetString("ArgumentOutOfRange_OffsetOut"));
                }
                if (this._handle == null)
                {
                    InternalResources.FileNotOpen();
                }
                if (this.isAsync)
                {
                    IAsyncResult asyncResult = this.BeginWriteCore(array, offset, count, null, null);
                    this.EndWrite(asyncResult);
                    SerialStreamAsyncResult result2 = asyncResult as SerialStreamAsyncResult;
                    num = result2._numBytes;
                }
                else
                {
                    int num2;
                    num = this.WriteFileNative(array, offset, count, null, out num2);
                    if (num == -1)
                    {
                        if (num2 == 0x461)
                        {
                            throw new TimeoutException(SR.GetString("Write_timed_out"));
                        }
                        InternalResources.WinIOError();
                    }
                }
                if (num == 0)
                {
                    throw new TimeoutException(SR.GetString("Write_timed_out"));
                }
            }
        }

        public override void WriteByte(byte value)
        {
            this.WriteByte(value, this.WriteTimeout);
        }

        internal void WriteByte(byte value, int timeout)
        {
            int num;
            if (this.inBreak)
            {
                throw new InvalidOperationException(SR.GetString("In_Break_State"));
            }
            if (this._handle == null)
            {
                InternalResources.FileNotOpen();
            }
            this.tempBuf[0] = value;
            if (this.isAsync)
            {
                IAsyncResult asyncResult = this.BeginWriteCore(this.tempBuf, 0, 1, null, null);
                this.EndWrite(asyncResult);
                SerialStreamAsyncResult result2 = asyncResult as SerialStreamAsyncResult;
                num = result2._numBytes;
            }
            else
            {
                int num2;
                num = this.WriteFileNative(this.tempBuf, 0, 1, null, out num2);
                if (num == -1)
                {
                    if (Marshal.GetLastWin32Error() == 0x461)
                    {
                        throw new TimeoutException(SR.GetString("Write_timed_out"));
                    }
                    InternalResources.WinIOError();
                }
            }
            if (num == 0)
            {
                throw new TimeoutException(SR.GetString("Write_timed_out"));
            }
        }

        private unsafe int WriteFileNative(byte[] bytes, int offset, int count, NativeOverlapped* overlapped, out int hr)
        {
            if ((bytes.Length - offset) < count)
            {
                throw new IndexOutOfRangeException(SR.GetString("IndexOutOfRange_IORaceCondition"));
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
                if (this.isAsync)
                {
                    num2 = Microsoft.Win32.UnsafeNativeMethods.WriteFile(this._handle, numRef + offset, count, IntPtr.Zero, overlapped);
                }
                else
                {
                    num2 = Microsoft.Win32.UnsafeNativeMethods.WriteFile(this._handle, numRef + offset, count, out numBytesWritten, IntPtr.Zero);
                }
            }
            if (num2 == 0)
            {
                hr = Marshal.GetLastWin32Error();
                if (hr == 6)
                {
                    this._handle.SetHandleAsInvalid();
                }
                return -1;
            }
            hr = 0;
            return numBytesWritten;
        }

        internal int BaudRate
        {
            set
            {
                if ((value <= 0) || ((value > this.commProp.dwMaxBaud) && (this.commProp.dwMaxBaud > 0)))
                {
                    if (this.commProp.dwMaxBaud == 0)
                    {
                        throw new ArgumentOutOfRangeException("baudRate", SR.GetString("ArgumentOutOfRange_NeedPosNum"));
                    }
                    throw new ArgumentOutOfRangeException("baudRate", SR.GetString("ArgumentOutOfRange_Bounds_Lower_Upper", new object[] { 0, this.commProp.dwMaxBaud }));
                }
                if (value != this.dcb.BaudRate)
                {
                    int baudRate = (int) this.dcb.BaudRate;
                    this.dcb.BaudRate = (uint) value;
                    if (!Microsoft.Win32.UnsafeNativeMethods.SetCommState(this._handle, ref this.dcb))
                    {
                        this.dcb.BaudRate = (uint) baudRate;
                        InternalResources.WinIOError();
                    }
                }
            }
        }

        public bool BreakState
        {
            get
            {
                return this.inBreak;
            }
            set
            {
                if (value)
                {
                    if (!Microsoft.Win32.UnsafeNativeMethods.SetCommBreak(this._handle))
                    {
                        InternalResources.WinIOError();
                    }
                    this.inBreak = true;
                }
                else
                {
                    if (!Microsoft.Win32.UnsafeNativeMethods.ClearCommBreak(this._handle))
                    {
                        InternalResources.WinIOError();
                    }
                    this.inBreak = false;
                }
            }
        }

        internal int BytesToRead
        {
            get
            {
                int lpErrors = 0;
                if (!Microsoft.Win32.UnsafeNativeMethods.ClearCommError(this._handle, ref lpErrors, ref this.comStat))
                {
                    InternalResources.WinIOError();
                }
                return (int) this.comStat.cbInQue;
            }
        }

        internal int BytesToWrite
        {
            get
            {
                int lpErrors = 0;
                if (!Microsoft.Win32.UnsafeNativeMethods.ClearCommError(this._handle, ref lpErrors, ref this.comStat))
                {
                    InternalResources.WinIOError();
                }
                return (int) this.comStat.cbOutQue;
            }
        }

        public override bool CanRead
        {
            get
            {
                return (this._handle != null);
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanTimeout
        {
            get
            {
                return (this._handle != null);
            }
        }

        public override bool CanWrite
        {
            get
            {
                return (this._handle != null);
            }
        }

        internal bool CDHolding
        {
            get
            {
                int lpModemStat = 0;
                if (!Microsoft.Win32.UnsafeNativeMethods.GetCommModemStatus(this._handle, ref lpModemStat))
                {
                    InternalResources.WinIOError();
                }
                return ((0x80 & lpModemStat) != 0);
            }
        }

        internal bool CtsHolding
        {
            get
            {
                int lpModemStat = 0;
                if (!Microsoft.Win32.UnsafeNativeMethods.GetCommModemStatus(this._handle, ref lpModemStat))
                {
                    InternalResources.WinIOError();
                }
                return ((0x10 & lpModemStat) != 0);
            }
        }

        internal int DataBits
        {
            set
            {
                if (value != this.dcb.ByteSize)
                {
                    byte byteSize = this.dcb.ByteSize;
                    this.dcb.ByteSize = (byte) value;
                    if (!Microsoft.Win32.UnsafeNativeMethods.SetCommState(this._handle, ref this.dcb))
                    {
                        this.dcb.ByteSize = byteSize;
                        InternalResources.WinIOError();
                    }
                }
            }
        }

        internal bool DiscardNull
        {
            set
            {
                int dcbFlag = this.GetDcbFlag(11);
                if ((value && (dcbFlag == 0)) || (!value && (dcbFlag == 1)))
                {
                    int setting = dcbFlag;
                    this.SetDcbFlag(11, value ? 1 : 0);
                    if (!Microsoft.Win32.UnsafeNativeMethods.SetCommState(this._handle, ref this.dcb))
                    {
                        this.SetDcbFlag(11, setting);
                        InternalResources.WinIOError();
                    }
                }
            }
        }

        internal bool DsrHolding
        {
            get
            {
                int lpModemStat = 0;
                if (!Microsoft.Win32.UnsafeNativeMethods.GetCommModemStatus(this._handle, ref lpModemStat))
                {
                    InternalResources.WinIOError();
                }
                return ((0x20 & lpModemStat) != 0);
            }
        }

        internal bool DtrEnable
        {
            get
            {
                return (this.GetDcbFlag(4) == 1);
            }
            set
            {
                int dcbFlag = this.GetDcbFlag(4);
                this.SetDcbFlag(4, value ? 1 : 0);
                if (!Microsoft.Win32.UnsafeNativeMethods.SetCommState(this._handle, ref this.dcb))
                {
                    this.SetDcbFlag(4, dcbFlag);
                    InternalResources.WinIOError();
                }
                if (!Microsoft.Win32.UnsafeNativeMethods.EscapeCommFunction(this._handle, value ? 5 : 6))
                {
                    InternalResources.WinIOError();
                }
            }
        }

        internal System.IO.Ports.Handshake Handshake
        {
            set
            {
                if (value != this.handshake)
                {
                    System.IO.Ports.Handshake handshake = this.handshake;
                    int dcbFlag = this.GetDcbFlag(9);
                    int setting = this.GetDcbFlag(2);
                    int num3 = this.GetDcbFlag(12);
                    this.handshake = value;
                    int num4 = ((this.handshake == System.IO.Ports.Handshake.XOnXOff) || (this.handshake == System.IO.Ports.Handshake.RequestToSendXOnXOff)) ? 1 : 0;
                    this.SetDcbFlag(9, num4);
                    this.SetDcbFlag(8, num4);
                    this.SetDcbFlag(2, ((this.handshake == System.IO.Ports.Handshake.RequestToSend) || (this.handshake == System.IO.Ports.Handshake.RequestToSendXOnXOff)) ? 1 : 0);
                    if ((this.handshake == System.IO.Ports.Handshake.RequestToSend) || (this.handshake == System.IO.Ports.Handshake.RequestToSendXOnXOff))
                    {
                        this.SetDcbFlag(12, 2);
                    }
                    else if (this.rtsEnable)
                    {
                        this.SetDcbFlag(12, 1);
                    }
                    else
                    {
                        this.SetDcbFlag(12, 0);
                    }
                    if (!Microsoft.Win32.UnsafeNativeMethods.SetCommState(this._handle, ref this.dcb))
                    {
                        this.handshake = handshake;
                        this.SetDcbFlag(9, dcbFlag);
                        this.SetDcbFlag(8, dcbFlag);
                        this.SetDcbFlag(2, setting);
                        this.SetDcbFlag(12, num3);
                        InternalResources.WinIOError();
                    }
                }
            }
        }

        internal bool IsOpen
        {
            get
            {
                return ((this._handle != null) && !this.eventRunner.ShutdownLoop);
            }
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException(SR.GetString("NotSupported_UnseekableStream"));
            }
        }

        internal System.IO.Ports.Parity Parity
        {
            set
            {
                if (((byte) value) != this.dcb.Parity)
                {
                    byte parity = this.dcb.Parity;
                    int dcbFlag = this.GetDcbFlag(1);
                    byte errorChar = this.dcb.ErrorChar;
                    int setting = this.GetDcbFlag(10);
                    this.dcb.Parity = (byte) value;
                    int num5 = (this.dcb.Parity == 0) ? 0 : 1;
                    this.SetDcbFlag(1, num5);
                    if (num5 == 1)
                    {
                        this.SetDcbFlag(10, (this.parityReplace != 0) ? 1 : 0);
                        this.dcb.ErrorChar = this.parityReplace;
                    }
                    else
                    {
                        this.SetDcbFlag(10, 0);
                        this.dcb.ErrorChar = 0;
                    }
                    if (!Microsoft.Win32.UnsafeNativeMethods.SetCommState(this._handle, ref this.dcb))
                    {
                        this.dcb.Parity = parity;
                        this.SetDcbFlag(1, dcbFlag);
                        this.dcb.ErrorChar = errorChar;
                        this.SetDcbFlag(10, setting);
                        InternalResources.WinIOError();
                    }
                }
            }
        }

        internal byte ParityReplace
        {
            set
            {
                if (value != this.parityReplace)
                {
                    byte parityReplace = this.parityReplace;
                    byte errorChar = this.dcb.ErrorChar;
                    int dcbFlag = this.GetDcbFlag(10);
                    this.parityReplace = value;
                    if (this.GetDcbFlag(1) == 1)
                    {
                        this.SetDcbFlag(10, (this.parityReplace != 0) ? 1 : 0);
                        this.dcb.ErrorChar = this.parityReplace;
                    }
                    else
                    {
                        this.SetDcbFlag(10, 0);
                        this.dcb.ErrorChar = 0;
                    }
                    if (!Microsoft.Win32.UnsafeNativeMethods.SetCommState(this._handle, ref this.dcb))
                    {
                        this.parityReplace = parityReplace;
                        this.SetDcbFlag(10, dcbFlag);
                        this.dcb.ErrorChar = errorChar;
                        InternalResources.WinIOError();
                    }
                }
            }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException(SR.GetString("NotSupported_UnseekableStream"));
            }
            set
            {
                throw new NotSupportedException(SR.GetString("NotSupported_UnseekableStream"));
            }
        }

        public override int ReadTimeout
        {
            get
            {
                int readTotalTimeoutConstant = this.commTimeouts.ReadTotalTimeoutConstant;
                if (readTotalTimeoutConstant == -2)
                {
                    return -1;
                }
                return readTotalTimeoutConstant;
            }
            set
            {
                if ((value < 0) && (value != -1))
                {
                    throw new ArgumentOutOfRangeException("ReadTimeout", SR.GetString("ArgumentOutOfRange_Timeout"));
                }
                if (this._handle == null)
                {
                    InternalResources.FileNotOpen();
                }
                int readTotalTimeoutConstant = this.commTimeouts.ReadTotalTimeoutConstant;
                int readIntervalTimeout = this.commTimeouts.ReadIntervalTimeout;
                int readTotalTimeoutMultiplier = this.commTimeouts.ReadTotalTimeoutMultiplier;
                if (value == 0)
                {
                    this.commTimeouts.ReadTotalTimeoutConstant = 0;
                    this.commTimeouts.ReadTotalTimeoutMultiplier = 0;
                    this.commTimeouts.ReadIntervalTimeout = -1;
                }
                else if (value == -1)
                {
                    this.commTimeouts.ReadTotalTimeoutConstant = -2;
                    this.commTimeouts.ReadTotalTimeoutMultiplier = -1;
                    this.commTimeouts.ReadIntervalTimeout = -1;
                }
                else
                {
                    this.commTimeouts.ReadTotalTimeoutConstant = value;
                    this.commTimeouts.ReadTotalTimeoutMultiplier = -1;
                    this.commTimeouts.ReadIntervalTimeout = -1;
                }
                if (!Microsoft.Win32.UnsafeNativeMethods.SetCommTimeouts(this._handle, ref this.commTimeouts))
                {
                    this.commTimeouts.ReadTotalTimeoutConstant = readTotalTimeoutConstant;
                    this.commTimeouts.ReadTotalTimeoutMultiplier = readTotalTimeoutMultiplier;
                    this.commTimeouts.ReadIntervalTimeout = readIntervalTimeout;
                    InternalResources.WinIOError();
                }
            }
        }

        internal bool RtsEnable
        {
            get
            {
                int dcbFlag = this.GetDcbFlag(12);
                if (dcbFlag == 2)
                {
                    throw new InvalidOperationException(SR.GetString("CantSetRtsWithHandshaking"));
                }
                return (dcbFlag == 1);
            }
            set
            {
                if ((this.handshake == System.IO.Ports.Handshake.RequestToSend) || (this.handshake == System.IO.Ports.Handshake.RequestToSendXOnXOff))
                {
                    throw new InvalidOperationException(SR.GetString("CantSetRtsWithHandshaking"));
                }
                if (value != this.rtsEnable)
                {
                    int dcbFlag = this.GetDcbFlag(12);
                    this.rtsEnable = value;
                    if (value)
                    {
                        this.SetDcbFlag(12, 1);
                    }
                    else
                    {
                        this.SetDcbFlag(12, 0);
                    }
                    if (!Microsoft.Win32.UnsafeNativeMethods.SetCommState(this._handle, ref this.dcb))
                    {
                        this.SetDcbFlag(12, dcbFlag);
                        this.rtsEnable = !this.rtsEnable;
                        InternalResources.WinIOError();
                    }
                    if (!Microsoft.Win32.UnsafeNativeMethods.EscapeCommFunction(this._handle, value ? 3 : 4))
                    {
                        InternalResources.WinIOError();
                    }
                }
            }
        }

        internal System.IO.Ports.StopBits StopBits
        {
            set
            {
                byte num = 0;
                if (value == System.IO.Ports.StopBits.One)
                {
                    num = 0;
                }
                else if (value == System.IO.Ports.StopBits.OnePointFive)
                {
                    num = 1;
                }
                else
                {
                    num = 2;
                }
                if (num != this.dcb.StopBits)
                {
                    byte stopBits = this.dcb.StopBits;
                    this.dcb.StopBits = num;
                    if (!Microsoft.Win32.UnsafeNativeMethods.SetCommState(this._handle, ref this.dcb))
                    {
                        this.dcb.StopBits = stopBits;
                        InternalResources.WinIOError();
                    }
                }
            }
        }

        public override int WriteTimeout
        {
            get
            {
                int writeTotalTimeoutConstant = this.commTimeouts.WriteTotalTimeoutConstant;
                if (writeTotalTimeoutConstant != 0)
                {
                    return writeTotalTimeoutConstant;
                }
                return -1;
            }
            set
            {
                if ((value <= 0) && (value != -1))
                {
                    throw new ArgumentOutOfRangeException("WriteTimeout", SR.GetString("ArgumentOutOfRange_WriteTimeout"));
                }
                if (this._handle == null)
                {
                    InternalResources.FileNotOpen();
                }
                int writeTotalTimeoutConstant = this.commTimeouts.WriteTotalTimeoutConstant;
                this.commTimeouts.WriteTotalTimeoutConstant = (value == -1) ? 0 : value;
                if (!Microsoft.Win32.UnsafeNativeMethods.SetCommTimeouts(this._handle, ref this.commTimeouts))
                {
                    this.commTimeouts.WriteTotalTimeoutConstant = writeTotalTimeoutConstant;
                    InternalResources.WinIOError();
                }
            }
        }

        internal sealed class EventLoopRunner
        {
            private WaitCallback callErrorEvents;
            private WaitCallback callPinEvents;
            private WaitCallback callReceiveEvents;
            internal bool endEventLoop;
            internal ManualResetEvent eventLoopEndedSignal = new ManualResetEvent(false);
            private int eventsOccurred;
            private IOCompletionCallback freeNativeOverlappedCallback;
            private SafeFileHandle handle;
            private bool isAsync;
            private WeakReference streamWeakReference;
            internal ManualResetEvent waitCommEventWaitHandle = new ManualResetEvent(false);

            internal EventLoopRunner(SerialStream stream)
            {
                this.handle = stream._handle;
                this.streamWeakReference = new WeakReference(stream);
                this.callErrorEvents = new WaitCallback(this.CallErrorEvents);
                this.callReceiveEvents = new WaitCallback(this.CallReceiveEvents);
                this.callPinEvents = new WaitCallback(this.CallPinEvents);
                this.freeNativeOverlappedCallback = new IOCompletionCallback(this.FreeNativeOverlappedCallback);
                this.isAsync = stream.isAsync;
            }

            private void CallErrorEvents(object state)
            {
                int num = (int) state;
                SerialStream target = (SerialStream) this.streamWeakReference.Target;
                if (target != null)
                {
                    if (target.ErrorReceived != null)
                    {
                        if ((num & 0x100) != 0)
                        {
                            target.ErrorReceived(target, new SerialErrorReceivedEventArgs(SerialError.TXFull));
                        }
                        if ((num & 1) != 0)
                        {
                            target.ErrorReceived(target, new SerialErrorReceivedEventArgs(SerialError.RXOver));
                        }
                        if ((num & 2) != 0)
                        {
                            target.ErrorReceived(target, new SerialErrorReceivedEventArgs(SerialError.Overrun));
                        }
                        if ((num & 4) != 0)
                        {
                            target.ErrorReceived(target, new SerialErrorReceivedEventArgs(SerialError.RXParity));
                        }
                        if ((num & 8) != 0)
                        {
                            target.ErrorReceived(target, new SerialErrorReceivedEventArgs(SerialError.Frame));
                        }
                    }
                    target = null;
                }
            }

            private void CallEvents(int nativeEvents)
            {
                if ((nativeEvents & 0x81) != 0)
                {
                    int lpErrors = 0;
                    if (!Microsoft.Win32.UnsafeNativeMethods.ClearCommError(this.handle, ref lpErrors, IntPtr.Zero))
                    {
                        this.endEventLoop = true;
                        Thread.MemoryBarrier();
                        return;
                    }
                    lpErrors &= 0x10f;
                    if (lpErrors != 0)
                    {
                        ThreadPool.QueueUserWorkItem(this.callErrorEvents, lpErrors);
                    }
                }
                if ((nativeEvents & 0x178) != 0)
                {
                    ThreadPool.QueueUserWorkItem(this.callPinEvents, nativeEvents);
                }
                if ((nativeEvents & 3) != 0)
                {
                    ThreadPool.QueueUserWorkItem(this.callReceiveEvents, nativeEvents);
                }
            }

            private void CallPinEvents(object state)
            {
                int num = (int) state;
                SerialStream target = (SerialStream) this.streamWeakReference.Target;
                if (target != null)
                {
                    if (target.PinChanged != null)
                    {
                        if ((num & 8) != 0)
                        {
                            target.PinChanged(target, new SerialPinChangedEventArgs(SerialPinChange.CtsChanged));
                        }
                        if ((num & 0x10) != 0)
                        {
                            target.PinChanged(target, new SerialPinChangedEventArgs(SerialPinChange.DsrChanged));
                        }
                        if ((num & 0x20) != 0)
                        {
                            target.PinChanged(target, new SerialPinChangedEventArgs(SerialPinChange.CDChanged));
                        }
                        if ((num & 0x100) != 0)
                        {
                            target.PinChanged(target, new SerialPinChangedEventArgs(SerialPinChange.Ring));
                        }
                        if ((num & 0x40) != 0)
                        {
                            target.PinChanged(target, new SerialPinChangedEventArgs(SerialPinChange.Break));
                        }
                    }
                    target = null;
                }
            }

            private void CallReceiveEvents(object state)
            {
                int num = (int) state;
                SerialStream target = (SerialStream) this.streamWeakReference.Target;
                if (target != null)
                {
                    if (target.DataReceived != null)
                    {
                        if ((num & 1) != 0)
                        {
                            target.DataReceived(target, new SerialDataReceivedEventArgs(SerialData.Chars));
                        }
                        if ((num & 2) != 0)
                        {
                            target.DataReceived(target, new SerialDataReceivedEventArgs(SerialData.Eof));
                        }
                    }
                    target = null;
                }
            }

            private unsafe void FreeNativeOverlappedCallback(uint errorCode, uint numBytes, NativeOverlapped* pOverlapped)
            {
                SerialStream.SerialStreamAsyncResult asyncResult = (SerialStream.SerialStreamAsyncResult) Overlapped.Unpack(pOverlapped).AsyncResult;
                if (Interlocked.Decrement(ref asyncResult._numBytes) == 0)
                {
                    Overlapped.Free(pOverlapped);
                }
            }

            internal unsafe void WaitForCommEvent()
            {
                int lpNumberOfBytesTransferred = 0;
                bool flag = false;
                NativeOverlapped* lpOverlapped = null;
                while (!this.ShutdownLoop)
                {
                    SerialStream.SerialStreamAsyncResult ar = null;
                    if (this.isAsync)
                    {
                        ar = new SerialStream.SerialStreamAsyncResult {
                            _userCallback = null,
                            _userStateObject = null,
                            _isWrite = false,
                            _numBytes = 2,
                            _waitHandle = this.waitCommEventWaitHandle
                        };
                        this.waitCommEventWaitHandle.Reset();
                        lpOverlapped = new Overlapped(0, 0, this.waitCommEventWaitHandle.SafeWaitHandle.DangerousGetHandle(), ar).Pack(this.freeNativeOverlappedCallback, null);
                    }
                    fixed (int* numRef = &this.eventsOccurred)
                    {
                        if (!Microsoft.Win32.UnsafeNativeMethods.WaitCommEvent(this.handle, numRef, lpOverlapped))
                        {
                            int num2 = Marshal.GetLastWin32Error();
                            if (num2 == 5)
                            {
                                flag = true;
                                break;
                            }
                            if (num2 == 0x3e5)
                            {
                                int num3;
                                bool flag2 = this.waitCommEventWaitHandle.WaitOne();
                                do
                                {
                                    flag2 = Microsoft.Win32.UnsafeNativeMethods.GetOverlappedResult(this.handle, lpOverlapped, ref lpNumberOfBytesTransferred, false);
                                    num3 = Marshal.GetLastWin32Error();
                                }
                                while (((num3 == 0x3e4) && !this.ShutdownLoop) && !flag2);
                                if ((!flag2 && ((num3 == 0x3e4) || (num3 == 0x57))) && !this.ShutdownLoop)
                                {
                                }
                            }
                        }
                    }
                    if (!this.ShutdownLoop)
                    {
                        this.CallEvents(this.eventsOccurred);
                    }
                    if (this.isAsync && (Interlocked.Decrement(ref ar._numBytes) == 0))
                    {
                        Overlapped.Free(lpOverlapped);
                    }
                }
                if (flag)
                {
                    this.endEventLoop = true;
                    Overlapped.Free(lpOverlapped);
                }
                this.eventLoopEndedSignal.Set();
            }

            internal bool ShutdownLoop
            {
                get
                {
                    return this.endEventLoop;
                }
            }
        }

        internal sealed class SerialStreamAsyncResult : IAsyncResult
        {
            internal bool _completedSynchronously;
            internal int _EndXxxCalled;
            internal int _errorCode;
            internal bool _isComplete;
            internal bool _isWrite;
            internal int _numBytes;
            internal unsafe NativeOverlapped* _overlapped;
            internal AsyncCallback _userCallback;
            internal object _userStateObject;
            internal ManualResetEvent _waitHandle;

            public object AsyncState
            {
                get
                {
                    return this._userStateObject;
                }
            }

            public WaitHandle AsyncWaitHandle
            {
                get
                {
                    return this._waitHandle;
                }
            }

            public bool CompletedSynchronously
            {
                get
                {
                    return this._completedSynchronously;
                }
            }

            public bool IsCompleted
            {
                get
                {
                    return this._isComplete;
                }
            }
        }
    }
}

