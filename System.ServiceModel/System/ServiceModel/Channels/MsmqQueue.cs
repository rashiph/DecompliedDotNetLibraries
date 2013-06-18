namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Threading;
    using System.Transactions;

    internal class MsmqQueue : IDisposable
    {
        protected int accessMode;
        protected string formatName;
        private MsmqQueueHandle handle;
        private bool isAsyncEnabled;
        private bool isBoundToCompletionPort;
        protected int shareMode;

        public MsmqQueue(string formatName, int accessMode)
        {
            this.formatName = formatName;
            this.accessMode = accessMode;
            this.shareMode = 0;
        }

        public MsmqQueue(string formatName, int accessMode, int shareMode)
        {
            this.formatName = formatName;
            this.accessMode = accessMode;
            this.shareMode = shareMode;
        }

        public IAsyncResult BeginPeek(NativeMsmqMessage message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new TryReceiveAsyncResult(this, message, timeout, -2147483648, callback, state);
        }

        public IAsyncResult BeginTryReceive(NativeMsmqMessage message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new TryReceiveAsyncResult(this, message, timeout, 0, callback, state);
        }

        public virtual void CloseQueue()
        {
            if (this.handle != null)
            {
                this.CloseQueue(this.handle);
                this.handle = null;
                this.isBoundToCompletionPort = false;
                this.isAsyncEnabled = false;
                MsmqDiagnostics.QueueClosed(this.formatName);
            }
        }

        private void CloseQueue(MsmqQueueHandle handle)
        {
            handle.Dispose();
        }

        public void Dispose()
        {
            lock (this.ThisLock)
            {
                this.CloseQueue();
            }
        }

        public ReceiveResult EndPeek(IAsyncResult result)
        {
            return TryReceiveAsyncResult.End(result);
        }

        public ReceiveResult EndTryReceive(IAsyncResult result)
        {
            return TryReceiveAsyncResult.End(result);
        }

        internal void EnsureOpen()
        {
            this.GetHandle();
        }

        protected MsmqQueueHandle GetHandle()
        {
            lock (this.ThisLock)
            {
                if (this.handle == null)
                {
                    this.handle = this.OpenQueue();
                }
                return this.handle;
            }
        }

        private MsmqQueueHandle GetHandleForAsync(out bool useCompletionPort)
        {
            lock (this.ThisLock)
            {
                if (this.handle == null)
                {
                    this.handle = this.OpenQueue();
                }
                if (!this.isAsyncEnabled)
                {
                    if (IsCompletionPortSupported(this.handle))
                    {
                        ThreadPool.BindHandle(this.handle);
                        this.isBoundToCompletionPort = true;
                    }
                    this.isAsyncEnabled = true;
                }
                useCompletionPort = this.isBoundToCompletionPort;
                return this.handle;
            }
        }

        public static void GetMsmqInformation(ref Version version, ref bool activeDirectoryEnabled)
        {
            PrivateComputerProperties properties = new PrivateComputerProperties();
            using (properties)
            {
                IntPtr ptr = properties.Pin();
                try
                {
                    int error = UnsafeNativeMethods.MQGetPrivateComputerInformation(null, ptr);
                    if (error != 0)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(System.ServiceModel.SR.GetString("MsmqGetPrivateComputerInformationError", new object[] { MsmqError.GetErrorString(error) }), error));
                    }
                    int num2 = properties.Version.Value;
                    version = new Version(num2 >> 0x18, (num2 & 0xff0000) >> 0x10, num2 & 0xffff);
                    activeDirectoryEnabled = properties.ActiveDirectory.Value;
                }
                finally
                {
                    properties.Unpin();
                }
            }
        }

        protected IDtcTransaction GetNativeTransaction(MsmqTransactionMode transactionMode)
        {
            Transaction current = Transaction.Current;
            if (current != null)
            {
                return TransactionInterop.GetDtcTransaction(current);
            }
            if (transactionMode == MsmqTransactionMode.CurrentOrThrow)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqTransactionRequired")));
            }
            return null;
        }

        private int GetTransactionConstant(MsmqTransactionMode transactionMode)
        {
            switch (transactionMode)
            {
                case MsmqTransactionMode.None:
                case MsmqTransactionMode.CurrentOrNone:
                    return 0;

                case MsmqTransactionMode.Single:
                case MsmqTransactionMode.CurrentOrSingle:
                    return 3;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("transactionMode"));
        }

        protected void HandleIsStale(MsmqQueueHandle handle)
        {
            lock (this.ThisLock)
            {
                if (this.handle == handle)
                {
                    this.CloseQueue();
                }
            }
        }

        private static bool IsCompletionPortSupported(MsmqQueueHandle handle)
        {
            int num;
            return (UnsafeNativeMethods.GetHandleInformation(handle, out num) != 0);
        }

        protected static bool IsErrorDueToStaleHandle(int error)
        {
            switch (error)
            {
                case -1072824314:
                case -1072824313:
                case -1072824234:
                case -1072824230:
                    return true;
            }
            return false;
        }

        public static bool IsMoveable(string formatName)
        {
            MsmqException exception;
            return SupportsAccessMode(formatName, 4, out exception);
        }

        internal static bool IsQueueOpenable(string formatName, int accessMode, int shareMode, out int error)
        {
            MsmqQueueHandle handle;
            error = UnsafeNativeMethods.MQOpenQueue(formatName, accessMode, shareMode, out handle);
            if (error != 0)
            {
                Utility.CloseInvalidOutSafeHandle(handle);
                return false;
            }
            handle.Dispose();
            return true;
        }

        public static bool IsReadable(string formatName, out MsmqException ex)
        {
            return SupportsAccessMode(formatName, 1, out ex);
        }

        protected static bool IsReceiveErrorDueToInsufficientBuffer(int error)
        {
            switch (error)
            {
                case -1072824289:
                case -1072824286:
                case -1072824285:
                case -1072824280:
                case -1072824294:
                case -1072824226:
                case -1072824223:
                case -1072824222:
                case -1072824221:
                case 0x400e0009:
                case -1072824277:
                case -1072824250:
                    return true;
            }
            return false;
        }

        public static bool IsWriteable(string formatName)
        {
            MsmqException exception;
            return SupportsAccessMode(formatName, 2, out exception);
        }

        public void MarkMessageRejected(long lookupId)
        {
            MsmqQueueHandle handle = this.GetHandle();
            int error = 0;
            try
            {
                error = UnsafeNativeMethods.MQMarkMessageRejected(handle, lookupId);
            }
            catch (ObjectDisposedException exception)
            {
                MsmqDiagnostics.ExpectedException(exception);
            }
            if (error != 0)
            {
                if (IsErrorDueToStaleHandle(error))
                {
                    this.HandleIsStale(handle);
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(System.ServiceModel.SR.GetString("MsmqSendError", new object[] { MsmqError.GetErrorString(error) }), error));
            }
        }

        internal virtual MsmqQueueHandle OpenQueue()
        {
            MsmqQueueHandle handle;
            int error = UnsafeNativeMethods.MQOpenQueue(this.formatName, this.accessMode, this.shareMode, out handle);
            if (error != 0)
            {
                Utility.CloseInvalidOutSafeHandle(handle);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(System.ServiceModel.SR.GetString("MsmqOpenError", new object[] { MsmqError.GetErrorString(error) }), error));
            }
            MsmqDiagnostics.QueueOpened(this.formatName);
            return handle;
        }

        private int ReceiveByLookupIdCore(MsmqQueueHandle handle, long lookupId, NativeMsmqMessage message, MsmqTransactionMode transactionMode, int action)
        {
            int num;
            if (this.RequiresDtcTransaction(transactionMode))
            {
                return this.ReceiveByLookupIdCoreDtcTransacted(handle, lookupId, message, transactionMode, action);
            }
            IntPtr properties = message.Pin();
            try
            {
                num = UnsafeNativeMethods.MQReceiveMessageByLookupId(handle, lookupId, action, properties, null, IntPtr.Zero, (IntPtr) this.GetTransactionConstant(transactionMode));
            }
            finally
            {
                message.Unpin();
            }
            return num;
        }

        protected int ReceiveByLookupIdCoreDtcTransacted(MsmqQueueHandle handle, long lookupId, NativeMsmqMessage message, MsmqTransactionMode transactionMode, int action)
        {
            int num;
            IDtcTransaction nativeTransaction = this.GetNativeTransaction(transactionMode);
            IntPtr properties = message.Pin();
            try
            {
                if (nativeTransaction != null)
                {
                    try
                    {
                        return UnsafeNativeMethods.MQReceiveMessageByLookupId(handle, lookupId, action, properties, null, IntPtr.Zero, nativeTransaction);
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(nativeTransaction);
                    }
                }
                num = UnsafeNativeMethods.MQReceiveMessageByLookupId(handle, lookupId, action, properties, null, IntPtr.Zero, (IntPtr) this.GetTransactionConstant(transactionMode));
            }
            finally
            {
                message.Unpin();
            }
            return num;
        }

        private int ReceiveCore(MsmqQueueHandle handle, NativeMsmqMessage message, TimeSpan timeout, MsmqTransactionMode transactionMode, int action)
        {
            int num2;
            if (this.RequiresDtcTransaction(transactionMode))
            {
                return this.ReceiveCoreDtcTransacted(handle, message, timeout, transactionMode, action);
            }
            int num = TimeoutHelper.ToMilliseconds(timeout);
            IntPtr properties = message.Pin();
            try
            {
                num2 = UnsafeNativeMethods.MQReceiveMessage(handle.DangerousGetHandle(), num, action, properties, null, IntPtr.Zero, IntPtr.Zero, (IntPtr) this.GetTransactionConstant(transactionMode));
            }
            finally
            {
                message.Unpin();
            }
            return num2;
        }

        private unsafe int ReceiveCoreAsync(MsmqQueueHandle handle, IntPtr nativePropertiesPointer, TimeSpan timeout, int action, NativeOverlapped* nativeOverlapped, UnsafeNativeMethods.MQReceiveCallback receiveCallback)
        {
            int num = TimeoutHelper.ToMilliseconds(timeout);
            return UnsafeNativeMethods.MQReceiveMessage(handle, num, action, nativePropertiesPointer, nativeOverlapped, receiveCallback, IntPtr.Zero, IntPtr.Zero);
        }

        private int ReceiveCoreDtcTransacted(MsmqQueueHandle handle, NativeMsmqMessage message, TimeSpan timeout, MsmqTransactionMode transactionMode, int action)
        {
            int num2;
            IDtcTransaction nativeTransaction = this.GetNativeTransaction(transactionMode);
            int num = TimeoutHelper.ToMilliseconds(timeout);
            IntPtr properties = message.Pin();
            try
            {
                if (nativeTransaction != null)
                {
                    try
                    {
                        return UnsafeNativeMethods.MQReceiveMessage(handle.DangerousGetHandle(), num, action, properties, null, IntPtr.Zero, IntPtr.Zero, nativeTransaction);
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(nativeTransaction);
                    }
                }
                num2 = UnsafeNativeMethods.MQReceiveMessage(handle.DangerousGetHandle(), num, action, properties, null, IntPtr.Zero, IntPtr.Zero, (IntPtr) this.GetTransactionConstant(transactionMode));
            }
            finally
            {
                message.Unpin();
            }
            return num2;
        }

        private bool RequiresDtcTransaction(MsmqTransactionMode transactionMode)
        {
            switch (transactionMode)
            {
                case MsmqTransactionMode.None:
                case MsmqTransactionMode.Single:
                    return false;

                case MsmqTransactionMode.CurrentOrSingle:
                case MsmqTransactionMode.CurrentOrNone:
                case MsmqTransactionMode.CurrentOrThrow:
                    return true;
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("transactionMode"));
        }

        public void Send(NativeMsmqMessage message, MsmqTransactionMode transactionMode)
        {
            int error = 0;
            if (this.RequiresDtcTransaction(transactionMode))
            {
                error = this.SendDtcTransacted(message, transactionMode);
            }
            else
            {
                MsmqQueueHandle handle = this.GetHandle();
                IntPtr properties = message.Pin();
                try
                {
                    error = UnsafeNativeMethods.MQSendMessage(handle, properties, (IntPtr) this.GetTransactionConstant(transactionMode));
                }
                finally
                {
                    message.Unpin();
                }
            }
            if (error != 0)
            {
                if (IsErrorDueToStaleHandle(error))
                {
                    this.HandleIsStale(this.handle);
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(System.ServiceModel.SR.GetString("MsmqSendError", new object[] { MsmqError.GetErrorString(error) }), error));
            }
        }

        private int SendDtcTransacted(NativeMsmqMessage message, MsmqTransactionMode transactionMode)
        {
            int num;
            IDtcTransaction nativeTransaction = this.GetNativeTransaction(transactionMode);
            MsmqQueueHandle handle = this.GetHandle();
            IntPtr properties = message.Pin();
            try
            {
                if (nativeTransaction != null)
                {
                    try
                    {
                        return UnsafeNativeMethods.MQSendMessage(handle, properties, nativeTransaction);
                    }
                    finally
                    {
                        Marshal.ReleaseComObject(nativeTransaction);
                    }
                }
                num = UnsafeNativeMethods.MQSendMessage(handle, properties, (IntPtr) this.GetTransactionConstant(transactionMode));
            }
            finally
            {
                message.Unpin();
            }
            return num;
        }

        private static bool SupportsAccessMode(string formatName, int accessType, out MsmqException msmqException)
        {
            msmqException = null;
            try
            {
                using (MsmqQueue queue = new MsmqQueue(formatName, accessType))
                {
                    queue.GetHandle();
                }
            }
            catch (Exception exception)
            {
                msmqException = exception as MsmqException;
                if (msmqException == null)
                {
                    throw;
                }
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            return this.formatName;
        }

        public static bool TryGetIsTransactional(string formatName, out bool isTransactional)
        {
            bool flag;
            using (QueueTransactionProperties properties = new QueueTransactionProperties())
            {
                IntPtr ptr = properties.Pin();
                try
                {
                    if (UnsafeNativeMethods.MQGetQueueProperties(formatName, ptr) == 0)
                    {
                        isTransactional = properties.Transaction.Value != 0;
                        return true;
                    }
                    isTransactional = false;
                    MsmqDiagnostics.QueueTransactionalStatusUnknown(formatName);
                    flag = false;
                }
                finally
                {
                    properties.Unpin();
                }
            }
            return flag;
        }

        public MoveReceiveResult TryMoveMessage(long lookupId, MsmqQueue destinationQueue, MsmqTransactionMode transactionMode)
        {
            int num;
            MsmqQueueHandle sourceQueueHandle = this.GetHandle();
            MsmqQueueHandle handle = destinationQueue.GetHandle();
            try
            {
                if (this.RequiresDtcTransaction(transactionMode))
                {
                    num = this.TryMoveMessageDtcTransacted(lookupId, sourceQueueHandle, handle, transactionMode);
                }
                else
                {
                    num = UnsafeNativeMethods.MQMoveMessage(sourceQueueHandle, handle, lookupId, (IntPtr) this.GetTransactionConstant(transactionMode));
                }
            }
            catch (ObjectDisposedException exception)
            {
                MsmqDiagnostics.ExpectedException(exception);
                return MoveReceiveResult.Succeeded;
            }
            switch (num)
            {
                case 0:
                    return MoveReceiveResult.Succeeded;

                case -1072824184:
                    return MoveReceiveResult.MessageNotFound;

                case -1072824164:
                    return MoveReceiveResult.MessageLockedUnderTransaction;
            }
            if (IsErrorDueToStaleHandle(num))
            {
                this.HandleIsStale(sourceQueueHandle);
                destinationQueue.HandleIsStale(handle);
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(System.ServiceModel.SR.GetString("MsmqSendError", new object[] { MsmqError.GetErrorString(num) }), num));
        }

        private int TryMoveMessageDtcTransacted(long lookupId, MsmqQueueHandle sourceQueueHandle, MsmqQueueHandle destinationQueueHandle, MsmqTransactionMode transactionMode)
        {
            IDtcTransaction nativeTransaction = this.GetNativeTransaction(transactionMode);
            if (nativeTransaction != null)
            {
                try
                {
                    return UnsafeNativeMethods.MQMoveMessage(sourceQueueHandle, destinationQueueHandle, lookupId, nativeTransaction);
                }
                finally
                {
                    Marshal.ReleaseComObject(nativeTransaction);
                }
            }
            return UnsafeNativeMethods.MQMoveMessage(sourceQueueHandle, destinationQueueHandle, lookupId, (IntPtr) this.GetTransactionConstant(transactionMode));
        }

        public ReceiveResult TryPeek(NativeMsmqMessage message, TimeSpan timeout)
        {
            return this.TryReceiveInternal(message, timeout, MsmqTransactionMode.None, -2147483648);
        }

        public virtual ReceiveResult TryReceive(NativeMsmqMessage message, TimeSpan timeout, MsmqTransactionMode transactionMode)
        {
            return this.TryReceiveInternal(message, timeout, transactionMode, 0);
        }

        public MoveReceiveResult TryReceiveByLookupId(long lookupId, NativeMsmqMessage message, MsmqTransactionMode transactionMode)
        {
            return this.TryReceiveByLookupId(lookupId, message, transactionMode, 0x40000020);
        }

        public MoveReceiveResult TryReceiveByLookupId(long lookupId, NativeMsmqMessage message, MsmqTransactionMode transactionMode, int action)
        {
            MsmqQueueHandle handle = this.GetHandle();
            int error = 0;
            while (true)
            {
                try
                {
                    error = this.ReceiveByLookupIdCore(handle, lookupId, message, transactionMode, action);
                }
                catch (ObjectDisposedException exception)
                {
                    MsmqDiagnostics.ExpectedException(exception);
                    return MoveReceiveResult.Succeeded;
                }
                if (error == 0)
                {
                    return MoveReceiveResult.Succeeded;
                }
                if (!IsReceiveErrorDueToInsufficientBuffer(error))
                {
                    if (-1072824184 == error)
                    {
                        return MoveReceiveResult.MessageNotFound;
                    }
                    if (-1072824164 == error)
                    {
                        return MoveReceiveResult.MessageLockedUnderTransaction;
                    }
                    if (IsErrorDueToStaleHandle(error))
                    {
                        this.HandleIsStale(handle);
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(System.ServiceModel.SR.GetString("MsmqReceiveError", new object[] { MsmqError.GetErrorString(error) }), error));
                }
                message.GrowBuffers();
            }
        }

        private ReceiveResult TryReceiveInternal(NativeMsmqMessage message, TimeSpan timeout, MsmqTransactionMode transactionMode, int action)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            MsmqQueueHandle handle = this.GetHandle();
            while (true)
            {
                int error = this.ReceiveCore(handle, message, helper.RemainingTime(), transactionMode, action);
                if (error == 0)
                {
                    return ReceiveResult.MessageReceived;
                }
                if (!IsReceiveErrorDueToInsufficientBuffer(error))
                {
                    if (error == -1072824293)
                    {
                        return ReceiveResult.Timeout;
                    }
                    if (error == -1072824312)
                    {
                        return ReceiveResult.OperationCancelled;
                    }
                    if (error == -1072824313)
                    {
                        return ReceiveResult.OperationCancelled;
                    }
                    if (IsErrorDueToStaleHandle(error))
                    {
                        this.HandleIsStale(handle);
                    }
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(System.ServiceModel.SR.GetString("MsmqReceiveError", new object[] { MsmqError.GetErrorString(error) }), error));
                }
                message.GrowBuffers();
            }
        }

        public string FormatName
        {
            get
            {
                return this.formatName;
            }
        }

        protected object ThisLock
        {
            get
            {
                return this;
            }
        }

        public enum MoveReceiveResult
        {
            Unknown,
            Succeeded,
            MessageNotFound,
            MessageLockedUnderTransaction
        }

        private class PrivateComputerProperties : NativeMsmqMessage
        {
            private NativeMsmqMessage.BooleanProperty activeDirectory;
            private NativeMsmqMessage.IntProperty version;

            public PrivateComputerProperties() : base(2)
            {
                this.version = new NativeMsmqMessage.IntProperty(this, 0x16a9);
                this.activeDirectory = new NativeMsmqMessage.BooleanProperty(this, 0x16aa);
            }

            public NativeMsmqMessage.BooleanProperty ActiveDirectory
            {
                get
                {
                    return this.activeDirectory;
                }
            }

            public NativeMsmqMessage.IntProperty Version
            {
                get
                {
                    return this.version;
                }
            }
        }

        private class QueueTransactionProperties : NativeMsmqMessage
        {
            private NativeMsmqMessage.ByteProperty transaction;

            public QueueTransactionProperties() : base(1)
            {
                this.transaction = new NativeMsmqMessage.ByteProperty(this, 0x71);
            }

            public NativeMsmqMessage.ByteProperty Transaction
            {
                get
                {
                    return this.transaction;
                }
            }
        }

        internal enum ReceiveResult
        {
            Unknown,
            MessageReceived,
            Timeout,
            OperationCancelled
        }

        private class TryReceiveAsyncResult : AsyncResult
        {
            private int action;
            private MsmqQueueHandle handle;
            private NativeMsmqMessage message;
            private MsmqQueue msmqQueue;
            private unsafe NativeOverlapped* nativeOverlapped;
            private static UnsafeNativeMethods.MQReceiveCallback onNonPortedCompletion;
            private static IOCompletionCallback onPortedCompletion = Fx.ThunkCallback(new IOCompletionCallback(MsmqQueue.TryReceiveAsyncResult.OnPortedCompletion));
            private MsmqQueue.ReceiveResult receiveResult;
            private TimeoutHelper timeoutHelper;

            public unsafe TryReceiveAsyncResult(MsmqQueue msmqQueue, NativeMsmqMessage message, TimeSpan timeout, int action, AsyncCallback callback, object state) : base(callback, state)
            {
                this.nativeOverlapped = null;
                this.msmqQueue = msmqQueue;
                this.message = message;
                this.action = action;
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.StartReceive(true);
            }

            public static MsmqQueue.ReceiveResult End(IAsyncResult result)
            {
                return AsyncResult.End<MsmqQueue.TryReceiveAsyncResult>(result).receiveResult;
            }

            ~TryReceiveAsyncResult()
            {
                if (((null != this.nativeOverlapped) && !Environment.HasShutdownStarted) && !AppDomain.CurrentDomain.IsFinalizingForUnload())
                {
                    Overlapped.Free(this.nativeOverlapped);
                }
            }

            private void OnCompletion(int error, bool completedSynchronously)
            {
                Exception exception = null;
                this.receiveResult = MsmqQueue.ReceiveResult.MessageReceived;
                try
                {
                    if (error != 0)
                    {
                        if (error != -1072824293)
                        {
                            if (error != -1072824312)
                            {
                                if (!MsmqQueue.IsReceiveErrorDueToInsufficientBuffer(error))
                                {
                                    if (MsmqQueue.IsErrorDueToStaleHandle(error))
                                    {
                                        this.msmqQueue.HandleIsStale(this.handle);
                                    }
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MsmqException(System.ServiceModel.SR.GetString("MsmqReceiveError", new object[] { MsmqError.GetErrorString(error) }), error));
                                }
                                this.message.Unpin();
                                this.message.GrowBuffers();
                                this.StartReceive(completedSynchronously);
                                return;
                            }
                            this.receiveResult = MsmqQueue.ReceiveResult.OperationCancelled;
                        }
                        else
                        {
                            this.receiveResult = MsmqQueue.ReceiveResult.Timeout;
                        }
                    }
                }
                catch (Exception exception2)
                {
                    if ((exception2 is NullReferenceException) || (exception2 is SEHException))
                    {
                        throw;
                    }
                    exception = exception2;
                }
                this.message.Unpin();
                base.Complete(completedSynchronously, exception);
            }

            private static unsafe void OnNonPortedCompletion(int error, IntPtr handle, int timeout, int action, IntPtr props, NativeOverlapped* nativeOverlapped, IntPtr cursor)
            {
                ThreadPool.UnsafeQueueNativeOverlapped(nativeOverlapped);
            }

            private static unsafe void OnPortedCompletion(uint error, uint numBytes, NativeOverlapped* nativeOverlapped)
            {
                MsmqQueue.TryReceiveAsyncResult asyncResult = (MsmqQueue.TryReceiveAsyncResult) Overlapped.Unpack(nativeOverlapped).AsyncResult;
                if (error != 0)
                {
                    error = (uint) UnsafeNativeMethods.MQGetOverlappedResult(nativeOverlapped);
                }
                Overlapped.Free(nativeOverlapped);
                asyncResult.nativeOverlapped = null;
                GC.SuppressFinalize(asyncResult);
                asyncResult.OnCompletion((int) error, false);
            }

            private unsafe void StartReceive(bool synchronously)
            {
                bool flag;
                int num;
                try
                {
                    this.handle = this.msmqQueue.GetHandleForAsync(out flag);
                }
                catch (MsmqException exception)
                {
                    this.OnCompletion(exception.ErrorCode, synchronously);
                    return;
                }
                NativeOverlapped* nativeOverlapped = this.nativeOverlapped;
                IntPtr nativePropertiesPointer = this.message.Pin();
                this.nativeOverlapped = new Overlapped(0, 0, IntPtr.Zero, this).UnsafePack(onPortedCompletion, this.message.GetBuffersForAsync());
                try
                {
                    if (flag)
                    {
                        num = this.msmqQueue.ReceiveCoreAsync(this.handle, nativePropertiesPointer, this.timeoutHelper.RemainingTime(), this.action, this.nativeOverlapped, null);
                    }
                    else
                    {
                        if (onNonPortedCompletion == null)
                        {
                            onNonPortedCompletion = new UnsafeNativeMethods.MQReceiveCallback(MsmqQueue.TryReceiveAsyncResult.OnNonPortedCompletion);
                        }
                        num = this.msmqQueue.ReceiveCoreAsync(this.handle, nativePropertiesPointer, this.timeoutHelper.RemainingTime(), this.action, this.nativeOverlapped, onNonPortedCompletion);
                    }
                }
                catch (ObjectDisposedException exception2)
                {
                    MsmqDiagnostics.ExpectedException(exception2);
                    num = -1072824312;
                }
                if ((num != 0) && (num != 0x400e0006))
                {
                    Overlapped.Free(this.nativeOverlapped);
                    this.nativeOverlapped = null;
                    GC.SuppressFinalize(this);
                    this.OnCompletion(num, synchronously);
                }
            }
        }
    }
}

