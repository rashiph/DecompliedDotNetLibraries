namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Diagnostics;
    using System.Threading;

    internal sealed class PipeConnection : IConnection
    {
        private bool aborted;
        private int asyncBytesRead;
        private byte[] asyncReadBuffer;
        private WaitCallback asyncReadCallback;
        private object asyncReadCallbackState;
        private Exception asyncReadException;
        private ManualResetEvent atEOFEvent;
        private bool autoBindToCompletionPort;
        private CloseState closeState;
        private TraceEventType exceptionEventType;
        private bool inReadingState;
        private bool inWritingState;
        private bool isAtEOF;
        private bool isBoundToCompletionPort;
        private bool isReadOutstanding;
        private bool isShutdownWritten;
        private bool isWriteOutstanding;
        private OverlappedIOCompleteCallback onAsyncReadComplete;
        private OverlappedIOCompleteCallback onAsyncWriteComplete;
        private static Action<object> onReadTimeout;
        private static Action<object> onWriteTimeout;
        private byte[] pendingWriteBuffer;
        private BufferManager pendingWriteBufferManager;
        private PipeHandle pipe;
        private int readBufferSize;
        private object readLock = new object();
        private OverlappedContext readOverlapped;
        private TimeSpan readTimeout;
        private IOThreadTimer readTimer;
        private int syncWriteSize;
        private string timeoutErrorString;
        private TransferOperation timeoutErrorTransferOperation;
        private WriteAsyncResult writeAsyncResult;
        private int writeBufferSize;
        private object writeLock = new object();
        private OverlappedContext writeOverlapped;
        private TimeSpan writeTimeout;
        private IOThreadTimer writeTimer;
        private static byte[] zeroBuffer;

        public PipeConnection(PipeHandle pipe, int connectionBufferSize, bool isBoundToCompletionPort, bool autoBindToCompletionPort)
        {
            if (pipe == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("pipe");
            }
            if (pipe.IsInvalid)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("pipe");
            }
            this.closeState = CloseState.Open;
            this.exceptionEventType = TraceEventType.Error;
            this.isBoundToCompletionPort = isBoundToCompletionPort;
            this.autoBindToCompletionPort = autoBindToCompletionPort;
            this.pipe = pipe;
            this.readBufferSize = connectionBufferSize;
            this.writeBufferSize = connectionBufferSize;
            this.readOverlapped = new OverlappedContext();
            this.writeOverlapped = new OverlappedContext();
            this.atEOFEvent = new ManualResetEvent(false);
            this.onAsyncReadComplete = new OverlappedIOCompleteCallback(this.OnAsyncReadComplete);
            this.onAsyncWriteComplete = new OverlappedIOCompleteCallback(this.OnAsyncWriteComplete);
        }

        public void Abort()
        {
            this.Abort(null, TransferOperation.Undefined);
        }

        private void Abort(string timeoutErrorString, TransferOperation transferOperation)
        {
            this.CloseHandle(true, timeoutErrorString, transferOperation);
        }

        public unsafe AsyncReadResult BeginRead(int offset, int size, TimeSpan timeout, WaitCallback callback, object state)
        {
            AsyncReadResult result;
            ConnectionUtilities.ValidateBufferBounds(this.AsyncReadBuffer, offset, size);
            lock (this.readLock)
            {
                try
                {
                    this.ValidateEnterReadingState(true);
                    if (this.isAtEOF)
                    {
                        this.asyncBytesRead = 0;
                        this.asyncReadException = null;
                        return AsyncReadResult.Completed;
                    }
                    if (this.autoBindToCompletionPort && !this.isBoundToCompletionPort)
                    {
                        lock (this.writeLock)
                        {
                            this.EnsureBoundToCompletionPort();
                        }
                    }
                    if (this.isReadOutstanding)
                    {
                        throw Fx.AssertAndThrow("Read I/O already pending when BeginRead called.");
                    }
                    try
                    {
                        this.readTimeout = timeout;
                        if (this.readTimeout != TimeSpan.MaxValue)
                        {
                            this.ReadTimer.Set(this.readTimeout);
                        }
                        this.asyncReadCallback = callback;
                        this.asyncReadCallbackState = state;
                        this.isReadOutstanding = true;
                        this.readOverlapped.StartAsyncOperation(this.AsyncReadBuffer, this.onAsyncReadComplete, this.isBoundToCompletionPort);
                        if (UnsafeNativeMethods.ReadFile(this.pipe.DangerousGetHandle(), this.readOverlapped.BufferPtr + offset, size, IntPtr.Zero, this.readOverlapped.NativeOverlapped) == 0)
                        {
                            int error = Marshal.GetLastWin32Error();
                            if ((error != 0x3e5) && (error != 0xea))
                            {
                                this.isReadOutstanding = false;
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Exceptions.CreateReadException(error));
                            }
                        }
                    }
                    finally
                    {
                        if (!this.isReadOutstanding)
                        {
                            this.readOverlapped.CancelAsyncOperation();
                            this.asyncReadCallback = null;
                            this.asyncReadCallbackState = null;
                            this.ReadTimer.Cancel();
                        }
                    }
                    if (!this.isReadOutstanding)
                    {
                        int num2;
                        Exception exception = Exceptions.GetOverlappedReadException(this.pipe, this.readOverlapped.NativeOverlapped, out num2);
                        if (exception != null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
                        }
                        this.asyncBytesRead = num2;
                        this.HandleReadComplete(this.asyncBytesRead);
                    }
                    else
                    {
                        this.EnterReadingState();
                    }
                    result = this.isReadOutstanding ? AsyncReadResult.Queued : AsyncReadResult.Completed;
                }
                catch (PipeException exception2)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(this.ConvertPipeException(exception2, TransferOperation.Read), this.ExceptionEventType);
                }
            }
            return result;
        }

        public unsafe IAsyncResult BeginWrite(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout, AsyncCallback callback, object state)
        {
            IAsyncResult result2;
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.FinishPendingWrite(timeout);
            ConnectionUtilities.ValidateBufferBounds(buffer, offset, size);
            if (this.autoBindToCompletionPort && !this.isBoundToCompletionPort)
            {
                lock (this.readLock)
                {
                    lock (this.writeLock)
                    {
                        this.ValidateEnterWritingState(true);
                        this.EnsureBoundToCompletionPort();
                    }
                }
            }
            lock (this.writeLock)
            {
                try
                {
                    this.ValidateEnterWritingState(true);
                    if (this.isWriteOutstanding)
                    {
                        throw Fx.AssertAndThrow("Write I/O already pending when BeginWrite called.");
                    }
                    WriteAsyncResult result = new WriteAsyncResult(callback, state, size);
                    try
                    {
                        this.writeTimeout = timeout;
                        this.WriteTimer.Set(helper.RemainingTime());
                        this.writeAsyncResult = result;
                        this.isWriteOutstanding = true;
                        this.writeOverlapped.StartAsyncOperation(buffer, this.onAsyncWriteComplete, this.isBoundToCompletionPort);
                        if (UnsafeNativeMethods.WriteFile(this.pipe.DangerousGetHandle(), this.writeOverlapped.BufferPtr + offset, size, IntPtr.Zero, this.writeOverlapped.NativeOverlapped) == 0)
                        {
                            int error = Marshal.GetLastWin32Error();
                            if (error != 0x3e5)
                            {
                                this.isWriteOutstanding = false;
                                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Exceptions.CreateWriteException(error));
                            }
                        }
                    }
                    finally
                    {
                        if (!this.isWriteOutstanding)
                        {
                            this.writeOverlapped.CancelAsyncOperation();
                            this.writeAsyncResult = null;
                            this.WriteTimer.Cancel();
                        }
                    }
                    if (!this.isWriteOutstanding)
                    {
                        int num2;
                        Exception exception = Exceptions.GetOverlappedWriteException(this.pipe, this.writeOverlapped.NativeOverlapped, out num2);
                        if ((exception == null) && (num2 != size))
                        {
                            exception = new PipeException(System.ServiceModel.SR.GetString("PipeWriteIncomplete"));
                        }
                        if (exception != null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
                        }
                        result.Complete(true);
                    }
                    else
                    {
                        this.EnterWritingState();
                    }
                    result2 = result;
                }
                catch (PipeException exception2)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(this.ConvertPipeException(exception2, TransferOperation.Write), this.ExceptionEventType);
                }
            }
            return result2;
        }

        public void Close(TimeSpan timeout, bool asyncAndLinger)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.FinishPendingWrite(timeout);
            bool flag = false;
            try
            {
                bool flag2 = false;
                bool flag3 = false;
                bool flag4 = false;
                lock (this.readLock)
                {
                    lock (this.writeLock)
                    {
                        if (!this.isShutdownWritten && this.inWritingState)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelper(new PipeException(System.ServiceModel.SR.GetString("PipeCantCloseWithPendingWrite")), this.ExceptionEventType);
                        }
                        if ((this.closeState == CloseState.Closing) || (this.closeState == CloseState.HandleClosed))
                        {
                            return;
                        }
                        this.closeState = CloseState.Closing;
                        flag = true;
                        if (!this.isAtEOF)
                        {
                            if (this.inReadingState)
                            {
                                flag2 = true;
                            }
                            else
                            {
                                flag3 = true;
                            }
                        }
                        if (!this.isShutdownWritten)
                        {
                            flag4 = true;
                            this.isShutdownWritten = true;
                        }
                    }
                }
                if (flag4)
                {
                    this.StartWriteZero(helper.RemainingTime());
                }
                if (flag3)
                {
                    this.StartReadZero();
                }
                try
                {
                    this.WaitForWriteZero(helper.RemainingTime(), true);
                }
                catch (TimeoutException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(new TimeoutException(System.ServiceModel.SR.GetString("PipeShutdownWriteError"), exception), this.ExceptionEventType);
                }
                if (flag3)
                {
                    try
                    {
                        this.WaitForReadZero(helper.RemainingTime(), true);
                        goto Label_018E;
                    }
                    catch (TimeoutException exception2)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelper(new TimeoutException(System.ServiceModel.SR.GetString("PipeShutdownReadError"), exception2), this.ExceptionEventType);
                    }
                }
                if (flag2 && !TimeoutHelper.WaitOne(this.atEOFEvent, helper.RemainingTime()))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(new TimeoutException(System.ServiceModel.SR.GetString("PipeShutdownReadError")), this.ExceptionEventType);
                }
            Label_018E:
                try
                {
                    this.StartWriteZero(helper.RemainingTime());
                    this.StartReadZero();
                    this.WaitForWriteZero(helper.RemainingTime(), false);
                    this.WaitForReadZero(helper.RemainingTime(), false);
                }
                catch (PipeException exception3)
                {
                    if (!this.IsBrokenPipeError(exception3.ErrorCode))
                    {
                        throw;
                    }
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                    }
                }
                catch (CommunicationException exception4)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception4, TraceEventType.Information);
                    }
                }
                catch (TimeoutException exception5)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception5, TraceEventType.Information);
                    }
                }
            }
            catch (TimeoutException exception6)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(new TimeoutException(System.ServiceModel.SR.GetString("PipeCloseFailed"), exception6), this.ExceptionEventType);
            }
            catch (PipeException exception7)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(this.ConvertPipeException(System.ServiceModel.SR.GetString("PipeCloseFailed"), exception7, TransferOperation.Undefined), this.ExceptionEventType);
            }
            finally
            {
                if (flag)
                {
                    this.CloseHandle(false, null, TransferOperation.Undefined);
                }
            }
        }

        private void CloseHandle(bool abort, string timeoutErrorString, TransferOperation transferOperation)
        {
            lock (this.readLock)
            {
                lock (this.writeLock)
                {
                    if (this.closeState == CloseState.HandleClosed)
                    {
                        return;
                    }
                    this.timeoutErrorString = timeoutErrorString;
                    this.timeoutErrorTransferOperation = transferOperation;
                    this.aborted = abort;
                    this.closeState = CloseState.HandleClosed;
                    this.pipe.Close();
                    this.readOverlapped.FreeOrDefer();
                    this.writeOverlapped.FreeOrDefer();
                    if (this.atEOFEvent != null)
                    {
                        this.atEOFEvent.Close();
                    }
                    try
                    {
                        this.FinishPendingWrite(TimeSpan.Zero);
                    }
                    catch (TimeoutException exception)
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                        }
                    }
                    catch (CommunicationException exception2)
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                        }
                    }
                }
            }
            if (abort)
            {
                TraceEventType warning = TraceEventType.Warning;
                if (this.ExceptionEventType == TraceEventType.Information)
                {
                    warning = this.ExceptionEventType;
                }
                if (DiagnosticUtility.ShouldTrace(warning))
                {
                    TraceUtility.TraceEvent(warning, 0x4001d, System.ServiceModel.SR.GetString("TraceCodePipeConnectionAbort"), this);
                }
            }
        }

        private Exception ConvertPipeException(PipeException pipeException, TransferOperation transferOperation)
        {
            return this.ConvertPipeException(pipeException.Message, pipeException, transferOperation);
        }

        private Exception ConvertPipeException(string exceptionMessage, PipeException pipeException, TransferOperation transferOperation)
        {
            if (this.timeoutErrorString != null)
            {
                if (transferOperation == this.timeoutErrorTransferOperation)
                {
                    return new TimeoutException(this.timeoutErrorString, pipeException);
                }
                return new CommunicationException(this.timeoutErrorString, pipeException);
            }
            if (this.aborted)
            {
                return new CommunicationObjectAbortedException(exceptionMessage, pipeException);
            }
            return new CommunicationException(exceptionMessage, pipeException);
        }

        private Exception CreatePipeClosedException(TransferOperation transferOperation)
        {
            return this.ConvertPipeException(new PipeException(System.ServiceModel.SR.GetString("PipeClosed")), transferOperation);
        }

        private CommunicationException CreatePipeDuplicationFailedException(int win32Error)
        {
            Exception innerException = new PipeException(System.ServiceModel.SR.GetString("PipeDuplicationFailed"), win32Error);
            return new CommunicationException(innerException.Message, innerException);
        }

        public object DuplicateAndClose(int targetProcessId)
        {
            object obj2;
            System.ServiceModel.Activation.SafeCloseHandle hTargetProcessHandle = ListenerUnsafeNativeMethods.OpenProcess(0x40, false, targetProcessId);
            if (hTargetProcessHandle.IsInvalid)
            {
                hTargetProcessHandle.SetHandleAsInvalid();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(this.CreatePipeDuplicationFailedException(Marshal.GetLastWin32Error()), this.ExceptionEventType);
            }
            try
            {
                IntPtr ptr2;
                IntPtr currentProcess = ListenerUnsafeNativeMethods.GetCurrentProcess();
                if (currentProcess == IntPtr.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(this.CreatePipeDuplicationFailedException(Marshal.GetLastWin32Error()), this.ExceptionEventType);
                }
                if (!UnsafeNativeMethods.DuplicateHandle(currentProcess, this.pipe, hTargetProcessHandle, out ptr2, 0, false, 2))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(this.CreatePipeDuplicationFailedException(Marshal.GetLastWin32Error()), this.ExceptionEventType);
                }
                this.Abort();
                obj2 = ptr2;
            }
            finally
            {
                hTargetProcessHandle.Close();
            }
            return obj2;
        }

        public int EndRead()
        {
            if (this.asyncReadException != null)
            {
                Exception asyncReadException = this.asyncReadException;
                this.asyncReadException = null;
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(asyncReadException, this.ExceptionEventType);
            }
            return this.asyncBytesRead;
        }

        public void EndWrite(IAsyncResult result)
        {
            WriteAsyncResult.End(result);
        }

        private void EnsureBoundToCompletionPort()
        {
            if (!this.isBoundToCompletionPort)
            {
                ThreadPool.BindHandle(this.pipe);
                this.isBoundToCompletionPort = true;
            }
        }

        private void EnterReadingState()
        {
            this.inReadingState = true;
        }

        private void EnterWritingState()
        {
            this.inWritingState = true;
        }

        private void ExitReadingState()
        {
            this.inReadingState = false;
        }

        private void ExitWritingState()
        {
            this.inWritingState = false;
        }

        private void FinishPendingWrite(TimeSpan timeout)
        {
            if (this.pendingWriteBuffer != null)
            {
                byte[] pendingWriteBuffer;
                BufferManager pendingWriteBufferManager;
                lock (this.writeLock)
                {
                    if (this.pendingWriteBuffer == null)
                    {
                        return;
                    }
                    pendingWriteBuffer = this.pendingWriteBuffer;
                    this.pendingWriteBuffer = null;
                    pendingWriteBufferManager = this.pendingWriteBufferManager;
                    this.pendingWriteBufferManager = null;
                }
                try
                {
                    bool flag = false;
                    try
                    {
                        this.WaitForSyncWrite(timeout, true);
                        flag = true;
                    }
                    finally
                    {
                        lock (this.writeLock)
                        {
                            try
                            {
                                if (flag)
                                {
                                    this.FinishSyncWrite(true);
                                }
                            }
                            finally
                            {
                                this.ExitWritingState();
                                if (!this.isWriteOutstanding)
                                {
                                    pendingWriteBufferManager.ReturnBuffer(pendingWriteBuffer);
                                    this.WriteIOCompleted();
                                }
                            }
                        }
                    }
                }
                catch (PipeException exception)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(this.ConvertPipeException(exception, TransferOperation.Write), this.ExceptionEventType);
                }
            }
        }

        private unsafe int FinishSyncRead(bool traceExceptionsAsErrors)
        {
            Exception exception;
            int bytesRead = -1;
            if (this.closeState == CloseState.HandleClosed)
            {
                exception = this.CreatePipeClosedException(TransferOperation.Read);
            }
            else
            {
                exception = Exceptions.GetOverlappedReadException(this.pipe, this.readOverlapped.NativeOverlapped, out bytesRead);
            }
            if (exception == null)
            {
                return bytesRead;
            }
            TraceEventType information = TraceEventType.Information;
            if (traceExceptionsAsErrors)
            {
                information = TraceEventType.Error;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelper(exception, information);
        }

        private unsafe void FinishSyncWrite(bool traceExceptionsAsErrors)
        {
            Exception exception;
            if (this.closeState == CloseState.HandleClosed)
            {
                exception = this.CreatePipeClosedException(TransferOperation.Write);
            }
            else
            {
                int num;
                exception = Exceptions.GetOverlappedWriteException(this.pipe, this.writeOverlapped.NativeOverlapped, out num);
                if ((exception == null) && (num != this.syncWriteSize))
                {
                    exception = new PipeException(System.ServiceModel.SR.GetString("PipeWriteIncomplete"));
                }
            }
            if (exception != null)
            {
                TraceEventType information = TraceEventType.Information;
                if (traceExceptionsAsErrors)
                {
                    information = TraceEventType.Error;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(exception, information);
            }
        }

        public object GetCoreTransport()
        {
            return this.pipe;
        }

        private void HandleReadComplete(int bytesRead)
        {
            if (bytesRead == 0)
            {
                this.isAtEOF = true;
                this.atEOFEvent.Set();
            }
        }

        private bool IsBrokenPipeError(int error)
        {
            if (error != 0xe8)
            {
                return (error == 0x6d);
            }
            return true;
        }

        private unsafe void OnAsyncReadComplete(bool haveResult, int error, int numBytes)
        {
            WaitCallback asyncReadCallback;
            object asyncReadCallbackState;
            lock (this.readLock)
            {
                try
                {
                    try
                    {
                        if ((this.readTimeout != TimeSpan.MaxValue) && !this.ReadTimer.Cancel())
                        {
                            this.Abort(System.ServiceModel.SR.GetString("PipeConnectionAbortedReadTimedOut", new object[] { this.readTimeout }), TransferOperation.Read);
                        }
                        if (this.closeState == CloseState.HandleClosed)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreatePipeClosedException(TransferOperation.Read));
                        }
                        if (!haveResult)
                        {
                            if (UnsafeNativeMethods.GetOverlappedResult(this.pipe.DangerousGetHandle(), this.readOverlapped.NativeOverlapped, out numBytes, 0) == 0)
                            {
                                error = Marshal.GetLastWin32Error();
                            }
                            else
                            {
                                error = 0;
                            }
                        }
                        if ((error != 0) && (error != 0xea))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Exceptions.CreateReadException(error));
                        }
                        this.asyncBytesRead = numBytes;
                        this.HandleReadComplete(this.asyncBytesRead);
                    }
                    catch (PipeException exception)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.ConvertPipeException(exception, TransferOperation.Read));
                    }
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    this.asyncReadException = exception2;
                }
                finally
                {
                    this.isReadOutstanding = false;
                    this.ReadIOCompleted();
                    this.ExitReadingState();
                    asyncReadCallback = this.asyncReadCallback;
                    this.asyncReadCallback = null;
                    asyncReadCallbackState = this.asyncReadCallbackState;
                    this.asyncReadCallbackState = null;
                }
            }
            asyncReadCallback(asyncReadCallbackState);
        }

        private unsafe void OnAsyncWriteComplete(bool haveResult, int error, int numBytes)
        {
            WriteAsyncResult writeAsyncResult = this.writeAsyncResult;
            this.writeAsyncResult = null;
            if (writeAsyncResult == null)
            {
                throw Fx.AssertAndThrow("Write completed with no WriteAsyncResult available.");
            }
            Exception e = null;
            this.WriteTimer.Cancel();
            lock (this.writeLock)
            {
                try
                {
                    try
                    {
                        if (this.closeState == CloseState.HandleClosed)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(this.CreatePipeClosedException(TransferOperation.Write));
                        }
                        if (!haveResult)
                        {
                            if (UnsafeNativeMethods.GetOverlappedResult(this.pipe.DangerousGetHandle(), this.writeOverlapped.NativeOverlapped, out numBytes, 0) == 0)
                            {
                                error = Marshal.GetLastWin32Error();
                            }
                            else
                            {
                                error = 0;
                            }
                        }
                        if (error != 0)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Exceptions.CreateWriteException(error));
                        }
                        if (numBytes != writeAsyncResult.WriteSize)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new PipeException(System.ServiceModel.SR.GetString("PipeWriteIncomplete")));
                        }
                    }
                    catch (PipeException exception2)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelper(this.ConvertPipeException(exception2, TransferOperation.Write), this.ExceptionEventType);
                    }
                }
                catch (Exception exception3)
                {
                    if (Fx.IsFatal(exception3))
                    {
                        throw;
                    }
                    e = exception3;
                }
                finally
                {
                    this.isWriteOutstanding = false;
                    this.WriteIOCompleted();
                    this.ExitWritingState();
                }
            }
            writeAsyncResult.Complete(false, e);
        }

        private static void OnReadTimeout(object state)
        {
            PipeConnection connection = (PipeConnection) state;
            connection.Abort(System.ServiceModel.SR.GetString("PipeConnectionAbortedReadTimedOut", new object[] { connection.readTimeout }), TransferOperation.Read);
        }

        private static void OnWriteTimeout(object state)
        {
            PipeConnection connection = (PipeConnection) state;
            connection.Abort(System.ServiceModel.SR.GetString("PipeConnectionAbortedWriteTimedOut", new object[] { connection.writeTimeout }), TransferOperation.Write);
        }

        public int Read(byte[] buffer, int offset, int size, TimeSpan timeout)
        {
            int num2;
            ConnectionUtilities.ValidateBufferBounds(buffer, offset, size);
            try
            {
                lock (this.readLock)
                {
                    this.ValidateEnterReadingState(true);
                    if (this.isAtEOF)
                    {
                        return 0;
                    }
                    this.StartSyncRead(buffer, offset, size);
                    this.EnterReadingState();
                }
                int bytesRead = -1;
                bool flag = false;
                try
                {
                    this.WaitForSyncRead(timeout, true);
                    flag = true;
                }
                finally
                {
                    lock (this.readLock)
                    {
                        try
                        {
                            if (flag)
                            {
                                bytesRead = this.FinishSyncRead(true);
                                this.HandleReadComplete(bytesRead);
                            }
                        }
                        finally
                        {
                            this.ExitReadingState();
                            if (!this.isReadOutstanding)
                            {
                                this.ReadIOCompleted();
                            }
                        }
                    }
                }
                num2 = bytesRead;
            }
            catch (PipeException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(this.ConvertPipeException(exception, TransferOperation.Read), this.ExceptionEventType);
            }
            return num2;
        }

        private void ReadIOCompleted()
        {
            this.readOverlapped.FreeIfDeferred();
        }

        public void Shutdown(TimeSpan timeout)
        {
            try
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                this.FinishPendingWrite(helper.RemainingTime());
                lock (this.writeLock)
                {
                    this.ValidateEnterWritingState(true);
                    this.StartWriteZero(helper.RemainingTime());
                    this.isShutdownWritten = true;
                }
            }
            catch (PipeException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(this.ConvertPipeException(exception, TransferOperation.Undefined), this.ExceptionEventType);
            }
        }

        private void StartReadZero()
        {
            lock (this.readLock)
            {
                this.ValidateEnterReadingState(false);
                this.StartSyncRead(ZeroBuffer, 0, 1);
                this.EnterReadingState();
            }
        }

        private void StartSyncRead(byte[] buffer, int offset, int size)
        {
            this.StartSyncRead(buffer, offset, size, ref this.readOverlapped.Holder[0]);
        }

        private unsafe void StartSyncRead(byte[] buffer, int offset, int size, ref object holder)
        {
            if (this.isReadOutstanding)
            {
                throw Fx.AssertAndThrow("StartSyncRead called when read I/O was already pending.");
            }
            try
            {
                this.isReadOutstanding = true;
                this.readOverlapped.StartSyncOperation(buffer, ref holder);
                if (UnsafeNativeMethods.ReadFile(this.pipe.DangerousGetHandle(), this.readOverlapped.BufferPtr + offset, size, IntPtr.Zero, this.readOverlapped.NativeOverlapped) == 0)
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error != 0x3e5)
                    {
                        this.isReadOutstanding = false;
                        if (error != 0xea)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Exceptions.CreateReadException(error));
                        }
                    }
                }
                else
                {
                    this.isReadOutstanding = false;
                }
            }
            finally
            {
                if (!this.isReadOutstanding)
                {
                    this.readOverlapped.CancelSyncOperation(ref holder);
                }
            }
        }

        private void StartSyncWrite(byte[] buffer, int offset, int size)
        {
            this.StartSyncWrite(buffer, offset, size, ref this.writeOverlapped.Holder[0]);
        }

        private unsafe void StartSyncWrite(byte[] buffer, int offset, int size, ref object holder)
        {
            if (this.isWriteOutstanding)
            {
                throw Fx.AssertAndThrow("StartSyncWrite called when write I/O was already pending.");
            }
            try
            {
                this.syncWriteSize = size;
                this.isWriteOutstanding = true;
                this.writeOverlapped.StartSyncOperation(buffer, ref holder);
                if (UnsafeNativeMethods.WriteFile(this.pipe.DangerousGetHandle(), this.writeOverlapped.BufferPtr + offset, size, IntPtr.Zero, this.writeOverlapped.NativeOverlapped) == 0)
                {
                    int error = Marshal.GetLastWin32Error();
                    if (error != 0x3e5)
                    {
                        this.isWriteOutstanding = false;
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Exceptions.CreateWriteException(error));
                    }
                }
                else
                {
                    this.isWriteOutstanding = false;
                }
            }
            finally
            {
                if (!this.isWriteOutstanding)
                {
                    this.writeOverlapped.CancelSyncOperation(ref holder);
                }
            }
        }

        private void StartWriteZero(TimeSpan timeout)
        {
            this.FinishPendingWrite(timeout);
            lock (this.writeLock)
            {
                this.ValidateEnterWritingState(false);
                this.StartSyncWrite(ZeroBuffer, 0, 0);
                this.EnterWritingState();
            }
        }

        public bool Validate(Uri uri)
        {
            return true;
        }

        private void ValidateEnterReadingState(bool checkEOF)
        {
            if (checkEOF && (this.closeState == CloseState.Closing))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(new PipeException(System.ServiceModel.SR.GetString("PipeAlreadyClosing")), this.ExceptionEventType);
            }
            if (this.inReadingState)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(new PipeException(System.ServiceModel.SR.GetString("PipeReadPending")), this.ExceptionEventType);
            }
            if (this.closeState == CloseState.HandleClosed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(new PipeException(System.ServiceModel.SR.GetString("PipeClosed")), this.ExceptionEventType);
            }
        }

        private void ValidateEnterWritingState(bool checkShutdown)
        {
            if (checkShutdown)
            {
                if (this.isShutdownWritten)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(new PipeException(System.ServiceModel.SR.GetString("PipeAlreadyShuttingDown")), this.ExceptionEventType);
                }
                if (this.closeState == CloseState.Closing)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(new PipeException(System.ServiceModel.SR.GetString("PipeAlreadyClosing")), this.ExceptionEventType);
                }
            }
            if (this.inWritingState)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(new PipeException(System.ServiceModel.SR.GetString("PipeWritePending")), this.ExceptionEventType);
            }
            if (this.closeState == CloseState.HandleClosed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(new PipeException(System.ServiceModel.SR.GetString("PipeClosed")), this.ExceptionEventType);
            }
        }

        private void WaitForReadZero(TimeSpan timeout, bool traceExceptionsAsErrors)
        {
            bool flag = false;
            try
            {
                this.WaitForSyncRead(timeout, traceExceptionsAsErrors);
                flag = true;
            }
            finally
            {
                lock (this.readLock)
                {
                    try
                    {
                        if (flag && (this.FinishSyncRead(traceExceptionsAsErrors) != 0))
                        {
                            Exception exception = this.ConvertPipeException(new PipeException(System.ServiceModel.SR.GetString("PipeSignalExpected")), TransferOperation.Read);
                            TraceEventType information = TraceEventType.Information;
                            if (traceExceptionsAsErrors)
                            {
                                information = TraceEventType.Error;
                            }
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelper(exception, information);
                        }
                    }
                    finally
                    {
                        this.ExitReadingState();
                        if (!this.isReadOutstanding)
                        {
                            this.ReadIOCompleted();
                        }
                    }
                }
            }
        }

        private void WaitForSyncRead(TimeSpan timeout, bool traceExceptionsAsErrors)
        {
            if (this.isReadOutstanding)
            {
                if (!this.readOverlapped.WaitForSyncOperation(timeout))
                {
                    this.Abort(System.ServiceModel.SR.GetString("PipeConnectionAbortedReadTimedOut", new object[] { this.readTimeout }), TransferOperation.Read);
                    Exception exception = new TimeoutException(System.ServiceModel.SR.GetString("PipeReadTimedOut", new object[] { timeout }));
                    TraceEventType information = TraceEventType.Information;
                    if (traceExceptionsAsErrors)
                    {
                        information = TraceEventType.Error;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(exception, information);
                }
                this.isReadOutstanding = false;
            }
        }

        private void WaitForSyncWrite(TimeSpan timeout, bool traceExceptionsAsErrors)
        {
            this.WaitForSyncWrite(timeout, traceExceptionsAsErrors, ref this.writeOverlapped.Holder[0]);
        }

        private void WaitForSyncWrite(TimeSpan timeout, bool traceExceptionsAsErrors, ref object holder)
        {
            if (this.isWriteOutstanding)
            {
                if (!this.writeOverlapped.WaitForSyncOperation(timeout, ref holder))
                {
                    this.Abort(System.ServiceModel.SR.GetString("PipeConnectionAbortedWriteTimedOut", new object[] { this.writeTimeout }), TransferOperation.Write);
                    Exception exception = new TimeoutException(System.ServiceModel.SR.GetString("PipeWriteTimedOut", new object[] { timeout }));
                    TraceEventType information = TraceEventType.Information;
                    if (traceExceptionsAsErrors)
                    {
                        information = TraceEventType.Error;
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelper(exception, information);
                }
                this.isWriteOutstanding = false;
            }
        }

        private void WaitForWriteZero(TimeSpan timeout, bool traceExceptionsAsErrors)
        {
            bool flag = false;
            try
            {
                this.WaitForSyncWrite(timeout, traceExceptionsAsErrors);
                flag = true;
            }
            finally
            {
                lock (this.writeLock)
                {
                    try
                    {
                        if (flag)
                        {
                            this.FinishSyncWrite(traceExceptionsAsErrors);
                        }
                    }
                    finally
                    {
                        this.ExitWritingState();
                        if (!this.isWriteOutstanding)
                        {
                            this.WriteIOCompleted();
                        }
                    }
                }
            }
        }

        public void Write(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout)
        {
            this.WriteHelper(buffer, offset, size, immediate, timeout, ref this.writeOverlapped.Holder[0]);
        }

        public void Write(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout, BufferManager bufferManager)
        {
            bool flag = true;
            try
            {
                if (size > this.writeBufferSize)
                {
                    this.WriteHelper(buffer, offset, size, immediate, timeout, ref this.writeOverlapped.Holder[0]);
                }
                else
                {
                    this.FinishPendingWrite(timeout);
                    ConnectionUtilities.ValidateBufferBounds(buffer, offset, size);
                    lock (this.writeLock)
                    {
                        this.ValidateEnterWritingState(true);
                        bool flag2 = false;
                        try
                        {
                            flag = false;
                            this.StartSyncWrite(buffer, offset, size);
                            flag2 = true;
                        }
                        finally
                        {
                            if (!this.isWriteOutstanding)
                            {
                                flag = true;
                            }
                            else if (flag2)
                            {
                                this.EnterWritingState();
                                this.pendingWriteBuffer = buffer;
                                this.pendingWriteBufferManager = bufferManager;
                            }
                        }
                    }
                }
            }
            catch (PipeException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(this.ConvertPipeException(exception, TransferOperation.Write), this.ExceptionEventType);
            }
            finally
            {
                if (flag)
                {
                    bufferManager.ReturnBuffer(buffer);
                }
            }
        }

        private void WriteHelper(byte[] buffer, int offset, int size, bool immediate, TimeSpan timeout, ref object holder)
        {
            try
            {
                this.FinishPendingWrite(timeout);
                ConnectionUtilities.ValidateBufferBounds(buffer, offset, size);
                int num = size;
                if (size > this.writeBufferSize)
                {
                    size = this.writeBufferSize;
                }
                while (num > 0)
                {
                    lock (this.writeLock)
                    {
                        this.ValidateEnterWritingState(true);
                        this.StartSyncWrite(buffer, offset, size, ref holder);
                        this.EnterWritingState();
                    }
                    bool flag = false;
                    try
                    {
                        this.WaitForSyncWrite(timeout, true, ref holder);
                        flag = true;
                    }
                    finally
                    {
                        lock (this.writeLock)
                        {
                            try
                            {
                                if (flag)
                                {
                                    this.FinishSyncWrite(true);
                                }
                            }
                            finally
                            {
                                this.ExitWritingState();
                                if (!this.isWriteOutstanding)
                                {
                                    this.WriteIOCompleted();
                                }
                            }
                        }
                    }
                    num -= size;
                    offset += size;
                    if (size > num)
                    {
                        size = num;
                    }
                }
            }
            catch (PipeException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelper(this.ConvertPipeException(exception, TransferOperation.Write), this.ExceptionEventType);
            }
        }

        private void WriteIOCompleted()
        {
            this.writeOverlapped.FreeIfDeferred();
        }

        public byte[] AsyncReadBuffer
        {
            get
            {
                if (this.asyncReadBuffer == null)
                {
                    Interlocked.CompareExchange<byte[]>(ref this.asyncReadBuffer, DiagnosticUtility.Utility.AllocateByteArray(this.AsyncReadBufferSize), null);
                }
                return this.asyncReadBuffer;
            }
        }

        public int AsyncReadBufferSize
        {
            get
            {
                return this.readBufferSize;
            }
        }

        public TraceEventType ExceptionEventType
        {
            get
            {
                return this.exceptionEventType;
            }
            set
            {
                this.exceptionEventType = value;
            }
        }

        private IOThreadTimer ReadTimer
        {
            get
            {
                if (this.readTimer == null)
                {
                    if (onReadTimeout == null)
                    {
                        onReadTimeout = new Action<object>(PipeConnection.OnReadTimeout);
                    }
                    this.readTimer = new IOThreadTimer(onReadTimeout, this, false);
                }
                return this.readTimer;
            }
        }

        public IPEndPoint RemoteIPEndPoint
        {
            get
            {
                return null;
            }
        }

        private IOThreadTimer WriteTimer
        {
            get
            {
                if (this.writeTimer == null)
                {
                    if (onWriteTimeout == null)
                    {
                        onWriteTimeout = new Action<object>(PipeConnection.OnWriteTimeout);
                    }
                    this.writeTimer = new IOThreadTimer(onWriteTimeout, this, false);
                }
                return this.writeTimer;
            }
        }

        private static byte[] ZeroBuffer
        {
            get
            {
                if (zeroBuffer == null)
                {
                    zeroBuffer = new byte[1];
                }
                return zeroBuffer;
            }
        }

        private enum CloseState
        {
            Open,
            Closing,
            HandleClosed
        }

        private static class Exceptions
        {
            private static PipeException CreateException(string resourceString, int error)
            {
                return new PipeException(System.ServiceModel.SR.GetString(resourceString, new object[] { PipeError.GetErrorString(error) }), error);
            }

            public static PipeException CreateReadException(int error)
            {
                return CreateException("PipeReadError", error);
            }

            public static PipeException CreateWriteException(int error)
            {
                return CreateException("PipeWriteError", error);
            }

            public static unsafe PipeException GetOverlappedReadException(PipeHandle pipe, NativeOverlapped* nativeOverlapped, out int bytesRead)
            {
                if (UnsafeNativeMethods.GetOverlappedResult(pipe.DangerousGetHandle(), nativeOverlapped, out bytesRead, 0) != 0)
                {
                    return null;
                }
                int error = Marshal.GetLastWin32Error();
                if (error == 0xea)
                {
                    return null;
                }
                return CreateReadException(error);
            }

            public static unsafe PipeException GetOverlappedWriteException(PipeHandle pipe, NativeOverlapped* nativeOverlapped, out int bytesWritten)
            {
                if (UnsafeNativeMethods.GetOverlappedResult(pipe.DangerousGetHandle(), nativeOverlapped, out bytesWritten, 0) == 0)
                {
                    return CreateWriteException(Marshal.GetLastWin32Error());
                }
                return null;
            }
        }

        private enum TransferOperation
        {
            Write,
            Read,
            Undefined
        }

        private class WriteAsyncResult : AsyncResult
        {
            private int writeSize;

            public WriteAsyncResult(AsyncCallback callback, object state, int writeSize) : base(callback, state)
            {
                this.writeSize = writeSize;
            }

            public void Complete(bool completedSynchronously)
            {
                base.Complete(completedSynchronously);
            }

            public void Complete(bool completedSynchronously, Exception e)
            {
                base.Complete(completedSynchronously, e);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<PipeConnection.WriteAsyncResult>(result);
            }

            public int WriteSize
            {
                get
                {
                    return this.writeSize;
                }
            }
        }
    }
}

