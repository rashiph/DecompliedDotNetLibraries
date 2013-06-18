namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Threading;

    internal class OverlappedContext
    {
        private object[] bufferHolder;
        private unsafe byte* bufferPtr;
        private static WaitOrTimerCallback cleanupCallback;
        private static IOCompletionCallback completeCallback;
        private ManualResetEvent completionEvent;
        private bool deferredFree;
        private static byte[] dummyBuffer = new byte[0];
        private static WaitOrTimerCallback eventCallback;
        private IntPtr eventHandle;
        private const int HandleOffsetFromOverlapped32 = -4;
        private const int HandleOffsetFromOverlapped64 = -3;
        private unsafe System.Threading.NativeOverlapped* nativeOverlapped;
        private Overlapped overlapped;
        private OverlappedIOCompleteCallback pendingCallback;
        private GCHandle pinnedHandle;
        private object pinnedTarget;
        private RegisteredWaitHandle registration;
        private RootedHolder rootedHolder;
        private bool syncOperationPending;

        public unsafe OverlappedContext()
        {
            if (completeCallback == null)
            {
                completeCallback = Fx.ThunkCallback(new IOCompletionCallback(OverlappedContext.CompleteCallback));
            }
            if (eventCallback == null)
            {
                eventCallback = Fx.ThunkCallback(new WaitOrTimerCallback(OverlappedContext.EventCallback));
            }
            if (cleanupCallback == null)
            {
                cleanupCallback = Fx.ThunkCallback(new WaitOrTimerCallback(OverlappedContext.CleanupCallback));
            }
            this.bufferHolder = new object[] { dummyBuffer };
            this.overlapped = new Overlapped();
            this.nativeOverlapped = this.overlapped.UnsafePack(completeCallback, this.bufferHolder);
            this.pinnedHandle = GCHandle.FromIntPtr(*((IntPtr*) (this.nativeOverlapped + (((IntPtr.Size == 4) ? -4 : -3) * sizeof(IntPtr)))));
            this.pinnedTarget = this.pinnedHandle.Target;
            this.rootedHolder = new RootedHolder();
            this.overlapped.AsyncResult = this.rootedHolder;
        }

        public unsafe void CancelAsyncOperation()
        {
            this.rootedHolder.ThisHolder = null;
            if (this.registration != null)
            {
                this.registration.Unregister(null);
                this.registration = null;
            }
            this.bufferPtr = null;
            this.bufferHolder[0] = dummyBuffer;
            this.pendingCallback = null;
        }

        public unsafe void CancelSyncOperation(ref object holder)
        {
            this.bufferPtr = null;
            holder = dummyBuffer;
            this.syncOperationPending = false;
            this.rootedHolder.EventHolder = null;
        }

        private static unsafe void CleanupCallback(object state, bool timedOut)
        {
            OverlappedContext context = state as OverlappedContext;
            if (!timedOut)
            {
                context.pinnedTarget = null;
                context.rootedHolder.EventHolder.Close();
                Overlapped.Free(context.nativeOverlapped);
            }
        }

        private static unsafe void CompleteCallback(uint error, uint numBytes, System.Threading.NativeOverlapped* nativeOverlapped)
        {
            OverlappedContext thisHolder = ((RootedHolder) Overlapped.Unpack(nativeOverlapped).AsyncResult).ThisHolder;
            thisHolder.rootedHolder.ThisHolder = null;
            thisHolder.bufferPtr = null;
            thisHolder.bufferHolder[0] = dummyBuffer;
            OverlappedIOCompleteCallback pendingCallback = thisHolder.pendingCallback;
            thisHolder.pendingCallback = null;
            pendingCallback(true, (int) error, (int) numBytes);
        }

        private static unsafe void EventCallback(object state, bool timedOut)
        {
            OverlappedContext context = state as OverlappedContext;
            if (timedOut)
            {
                if ((context == null) || (context.rootedHolder == null))
                {
                    DiagnosticUtility.FailFast("Can't prevent heap corruption.");
                }
                context.rootedHolder.ThisHolder = context;
            }
            else
            {
                context.registration = null;
                context.bufferPtr = null;
                context.bufferHolder[0] = dummyBuffer;
                OverlappedIOCompleteCallback pendingCallback = context.pendingCallback;
                context.pendingCallback = null;
                pendingCallback(false, 0, 0);
            }
        }

        ~OverlappedContext()
        {
            if (((this.nativeOverlapped != null) && !AppDomain.CurrentDomain.IsFinalizingForUnload()) && !Environment.HasShutdownStarted)
            {
                if (this.syncOperationPending)
                {
                    ThreadPool.UnsafeRegisterWaitForSingleObject(this.rootedHolder.EventHolder, cleanupCallback, this, -1, true);
                }
                else
                {
                    Overlapped.Free(this.nativeOverlapped);
                }
            }
        }

        public unsafe void Free()
        {
            if (this.pendingCallback != null)
            {
                throw Fx.AssertAndThrow("OverlappedContext.Free called while async operation is pending.");
            }
            if (this.syncOperationPending)
            {
                throw Fx.AssertAndThrow("OverlappedContext.Free called while sync operation is pending.");
            }
            if (this.nativeOverlapped == null)
            {
                throw Fx.AssertAndThrow("OverlappedContext.Free called multiple times.");
            }
            this.pinnedTarget = null;
            System.Threading.NativeOverlapped* nativeOverlapped = this.nativeOverlapped;
            this.nativeOverlapped = null;
            Overlapped.Free(nativeOverlapped);
            if (this.completionEvent != null)
            {
                this.completionEvent.Close();
            }
            GC.SuppressFinalize(this);
        }

        public bool FreeIfDeferred()
        {
            return (this.deferredFree && this.FreeOrDefer());
        }

        public bool FreeOrDefer()
        {
            if ((this.pendingCallback != null) || this.syncOperationPending)
            {
                this.deferredFree = true;
                return false;
            }
            this.Free();
            return true;
        }

        public unsafe void StartAsyncOperation(byte[] buffer, OverlappedIOCompleteCallback callback, bool bound)
        {
            if (callback == null)
            {
                throw Fx.AssertAndThrow("StartAsyncOperation called with null callback.");
            }
            if (this.pendingCallback != null)
            {
                throw Fx.AssertAndThrow("StartAsyncOperation called while another is in progress.");
            }
            if (this.syncOperationPending)
            {
                throw Fx.AssertAndThrow("StartAsyncOperation called while a sync operation was already pending.");
            }
            if (this.nativeOverlapped == null)
            {
                throw Fx.AssertAndThrow("StartAsyncOperation called on freed OverlappedContext.");
            }
            this.pendingCallback = callback;
            if (buffer != null)
            {
                this.bufferHolder[0] = buffer;
                this.pinnedHandle.Target = this.pinnedTarget;
                this.bufferPtr = (byte*) Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);
            }
            if (bound)
            {
                this.overlapped.EventHandleIntPtr = IntPtr.Zero;
                this.rootedHolder.ThisHolder = this;
            }
            else
            {
                if (this.completionEvent != null)
                {
                    this.completionEvent.Reset();
                }
                this.overlapped.EventHandleIntPtr = this.EventHandle;
                this.registration = ThreadPool.UnsafeRegisterWaitForSingleObject(this.completionEvent, eventCallback, this, -1, true);
            }
        }

        public unsafe void StartSyncOperation(byte[] buffer, ref object holder)
        {
            if (this.syncOperationPending)
            {
                throw Fx.AssertAndThrow("StartSyncOperation called while an operation was already pending.");
            }
            if (this.pendingCallback != null)
            {
                throw Fx.AssertAndThrow("StartSyncOperation called while an async operation was already pending.");
            }
            if (this.nativeOverlapped == null)
            {
                throw Fx.AssertAndThrow("StartSyncOperation called on freed OverlappedContext.");
            }
            this.overlapped.EventHandleIntPtr = this.EventHandle;
            this.rootedHolder.EventHolder = this.completionEvent;
            this.syncOperationPending = true;
            if (buffer != null)
            {
                holder = buffer;
                this.pinnedHandle.Target = this.pinnedTarget;
                this.bufferPtr = (byte*) Marshal.UnsafeAddrOfPinnedArrayElement(buffer, 0);
            }
        }

        public bool WaitForSyncOperation(TimeSpan timeout)
        {
            return this.WaitForSyncOperation(timeout, ref this.bufferHolder[0]);
        }

        public unsafe bool WaitForSyncOperation(TimeSpan timeout, ref object holder)
        {
            if (!this.syncOperationPending)
            {
                throw Fx.AssertAndThrow("WaitForSyncOperation called while no operation was pending.");
            }
            if (!UnsafeNativeMethods.HasOverlappedIoCompleted(this.nativeOverlapped) && !TimeoutHelper.WaitOne(this.completionEvent, timeout))
            {
                GC.SuppressFinalize(this);
                ThreadPool.UnsafeRegisterWaitForSingleObject(this.completionEvent, cleanupCallback, this, -1, true);
                return false;
            }
            this.CancelSyncOperation(ref holder);
            return true;
        }

        public byte* BufferPtr
        {
            get
            {
                byte* bufferPtr = this.bufferPtr;
                if (bufferPtr == null)
                {
                    throw Fx.AssertAndThrow("Pointer requested while no operation pending or no buffer provided.");
                }
                return bufferPtr;
            }
        }

        private IntPtr EventHandle
        {
            get
            {
                if (this.completionEvent == null)
                {
                    this.completionEvent = new ManualResetEvent(false);
                    this.eventHandle = (IntPtr) (1L | ((long) this.completionEvent.SafeWaitHandle.DangerousGetHandle()));
                }
                return this.eventHandle;
            }
        }

        public object[] Holder
        {
            get
            {
                return this.bufferHolder;
            }
        }

        public System.Threading.NativeOverlapped* NativeOverlapped
        {
            get
            {
                System.Threading.NativeOverlapped* nativeOverlapped = this.nativeOverlapped;
                if (nativeOverlapped == null)
                {
                    throw Fx.AssertAndThrow("NativeOverlapped pointer requested after it was freed.");
                }
                return nativeOverlapped;
            }
        }

        private class RootedHolder : IAsyncResult
        {
            private ManualResetEvent eventHolder;
            private OverlappedContext overlappedBuffer;

            public ManualResetEvent EventHolder
            {
                get
                {
                    return this.eventHolder;
                }
                set
                {
                    this.eventHolder = value;
                }
            }

            object IAsyncResult.AsyncState
            {
                get
                {
                    throw Fx.AssertAndThrow("RootedHolder.AsyncState called.");
                }
            }

            WaitHandle IAsyncResult.AsyncWaitHandle
            {
                get
                {
                    throw Fx.AssertAndThrow("RootedHolder.AsyncWaitHandle called.");
                }
            }

            bool IAsyncResult.CompletedSynchronously
            {
                get
                {
                    throw Fx.AssertAndThrow("RootedHolder.CompletedSynchronously called.");
                }
            }

            bool IAsyncResult.IsCompleted
            {
                get
                {
                    throw Fx.AssertAndThrow("RootedHolder.IsCompleted called.");
                }
            }

            public OverlappedContext ThisHolder
            {
                get
                {
                    return this.overlappedBuffer;
                }
                set
                {
                    this.overlappedBuffer = value;
                }
            }
        }
    }
}

