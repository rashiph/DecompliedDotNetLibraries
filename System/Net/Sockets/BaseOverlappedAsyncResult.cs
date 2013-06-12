namespace System.Net.Sockets
{
    using System;
    using System.Net;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class BaseOverlappedAsyncResult : ContextAwareResult
    {
        private OverlappedCache m_Cache;
        private int m_CleanupCount;
        private bool m_DisableOverlapped;
        private GCHandle[] m_GCHandles;
        private AutoResetEvent m_OverlappedEvent;
        private SafeOverlappedFree m_UnmanagedBlob;
        private bool m_UseOverlappedIO;
        private static readonly IOCompletionCallback s_IOCallback = new IOCompletionCallback(BaseOverlappedAsyncResult.CompletionPortCallback);

        internal BaseOverlappedAsyncResult(Socket socket) : base(socket, null, null)
        {
            this.m_CleanupCount = 1;
            this.m_DisableOverlapped = true;
        }

        internal BaseOverlappedAsyncResult(Socket socket, object asyncState, AsyncCallback asyncCallback) : base(socket, asyncState, asyncCallback)
        {
            this.m_UseOverlappedIO = Socket.UseOverlappedIO || socket.UseOnlyOverlappedIO;
            if (this.m_UseOverlappedIO)
            {
                this.m_CleanupCount = 1;
            }
            else
            {
                this.m_CleanupCount = 2;
            }
        }

        internal SocketError CheckAsyncCallOverlappedResult(SocketError errorCode)
        {
            if (this.m_UseOverlappedIO)
            {
                switch (errorCode)
                {
                    case SocketError.Success:
                    case SocketError.IOPending:
                        ThreadPool.UnsafeRegisterWaitForSingleObject(this.m_OverlappedEvent, new WaitOrTimerCallback(this.OverlappedCallback), this, -1, true);
                        return SocketError.Success;
                }
                base.ErrorCode = (int) errorCode;
                base.Result = -1;
                this.ReleaseUnmanagedStructures();
                return errorCode;
            }
            this.ReleaseUnmanagedStructures();
            switch (errorCode)
            {
                case SocketError.Success:
                case SocketError.IOPending:
                    return SocketError.Success;
            }
            base.ErrorCode = (int) errorCode;
            base.Result = -1;
            if (this.m_Cache != null)
            {
                this.m_Cache.Overlapped.AsyncResult = null;
            }
            this.ReleaseUnmanagedStructures();
            return errorCode;
        }

        protected override void Cleanup()
        {
            base.Cleanup();
            if ((this.m_CleanupCount > 0) && (Interlocked.Exchange(ref this.m_CleanupCount, 0) > 0))
            {
                this.ForceReleaseUnmanagedStructures();
            }
        }

        private static unsafe void CompletionPortCallback(uint errorCode, uint numBytes, NativeOverlapped* nativeOverlapped)
        {
            Overlapped overlapped = Overlapped.Unpack(nativeOverlapped);
            BaseOverlappedAsyncResult asyncResult = (BaseOverlappedAsyncResult) overlapped.AsyncResult;
            overlapped.AsyncResult = null;
            object result = null;
            SocketError notSocket = (SocketError) errorCode;
            switch (notSocket)
            {
                case SocketError.Success:
                case SocketError.OperationAborted:
                    break;

                default:
                {
                    Socket asyncObject = asyncResult.AsyncObject as Socket;
                    if (asyncObject == null)
                    {
                        notSocket = SocketError.NotSocket;
                    }
                    else if (asyncObject.CleanedUp)
                    {
                        notSocket = SocketError.OperationAborted;
                    }
                    else
                    {
                        try
                        {
                            SocketFlags flags;
                            if (!UnsafeNclNativeMethods.OSSOCK.WSAGetOverlappedResult(asyncObject.SafeHandle, asyncResult.m_Cache.NativeOverlapped, out numBytes, false, out flags))
                            {
                                notSocket = (SocketError) Marshal.GetLastWin32Error();
                            }
                        }
                        catch (ObjectDisposedException)
                        {
                            notSocket = SocketError.OperationAborted;
                        }
                    }
                    break;
                }
            }
            asyncResult.ErrorCode = (int) notSocket;
            result = asyncResult.PostCompletion((int) numBytes);
            asyncResult.ReleaseUnmanagedStructures();
            asyncResult.InvokeCallback(result);
        }

        internal void ExtractCache(ref OverlappedCache overlappedCache)
        {
            if (!this.m_UseOverlappedIO && !this.m_DisableOverlapped)
            {
                OverlappedCache cache = (this.m_Cache == null) ? null : Interlocked.Exchange<OverlappedCache>(ref this.m_Cache, null);
                if (cache != null)
                {
                    if (overlappedCache == null)
                    {
                        overlappedCache = cache;
                    }
                    else
                    {
                        OverlappedCache cache2 = Interlocked.Exchange<OverlappedCache>(ref overlappedCache, cache);
                        if (cache2 != null)
                        {
                            cache2.Free();
                        }
                    }
                }
                this.ReleaseUnmanagedStructures();
            }
        }

        ~BaseOverlappedAsyncResult()
        {
            this.ReleaseGCHandles();
        }

        protected virtual void ForceReleaseUnmanagedStructures()
        {
            this.ReleaseGCHandles();
            GC.SuppressFinalize(this);
            if ((this.m_UnmanagedBlob != null) && !this.m_UnmanagedBlob.IsInvalid)
            {
                this.m_UnmanagedBlob.Close(true);
                this.m_UnmanagedBlob = null;
            }
            OverlappedCache.InterlockedFree(ref this.m_Cache);
            if (this.m_OverlappedEvent != null)
            {
                this.m_OverlappedEvent.Close();
                this.m_OverlappedEvent = null;
            }
        }

        private void OverlappedCallback(object stateObject, bool Signaled)
        {
            BaseOverlappedAsyncResult result = (BaseOverlappedAsyncResult) stateObject;
            uint num = (uint) Marshal.ReadInt32(IntPtrHelper.Add(result.m_UnmanagedBlob.DangerousGetHandle(), 0));
            uint num2 = (num != 0) ? uint.MaxValue : ((uint) Marshal.ReadInt32(IntPtrHelper.Add(result.m_UnmanagedBlob.DangerousGetHandle(), Win32.OverlappedInternalHighOffset)));
            result.ErrorCode = (int) num;
            object obj2 = result.PostCompletion((int) num2);
            result.ReleaseUnmanagedStructures();
            result.InvokeCallback(obj2);
        }

        protected void PinUnmanagedObjects(object objectsToPin)
        {
            if (this.m_Cache != null)
            {
                this.m_Cache.Free();
                this.m_Cache = null;
            }
            if (objectsToPin != null)
            {
                if (objectsToPin.GetType() == typeof(object[]))
                {
                    object[] objArray = (object[]) objectsToPin;
                    this.m_GCHandles = new GCHandle[objArray.Length];
                    for (int i = 0; i < objArray.Length; i++)
                    {
                        if (objArray[i] != null)
                        {
                            this.m_GCHandles[i] = GCHandle.Alloc(objArray[i], GCHandleType.Pinned);
                        }
                    }
                }
                else
                {
                    this.m_GCHandles = new GCHandle[] { GCHandle.Alloc(objectsToPin, GCHandleType.Pinned) };
                }
            }
        }

        internal virtual object PostCompletion(int numBytes)
        {
            return numBytes;
        }

        private void ReleaseGCHandles()
        {
            GCHandle[] gCHandles = this.m_GCHandles;
            if (gCHandles != null)
            {
                for (int i = 0; i < gCHandles.Length; i++)
                {
                    if (gCHandles[i].IsAllocated)
                    {
                        gCHandles[i].Free();
                    }
                }
            }
        }

        private void ReleaseUnmanagedStructures()
        {
            if (Interlocked.Decrement(ref this.m_CleanupCount) == 0)
            {
                this.ForceReleaseUnmanagedStructures();
            }
        }

        internal void SetUnmanagedStructures(object objectsToPin)
        {
            if (!this.m_DisableOverlapped)
            {
                object[] pinnedObjectsArray = null;
                bool alreadyTriedCast = false;
                bool flag2 = false;
                if (this.m_Cache != null)
                {
                    if ((objectsToPin == null) && (this.m_Cache.PinnedObjects == null))
                    {
                        flag2 = true;
                    }
                    else if (this.m_Cache.PinnedObjects != null)
                    {
                        if (this.m_Cache.PinnedObjectsArray == null)
                        {
                            if (objectsToPin == this.m_Cache.PinnedObjects)
                            {
                                flag2 = true;
                            }
                        }
                        else if (objectsToPin != null)
                        {
                            alreadyTriedCast = true;
                            pinnedObjectsArray = objectsToPin as object[];
                            if ((pinnedObjectsArray != null) && (pinnedObjectsArray.Length == 0))
                            {
                                pinnedObjectsArray = null;
                            }
                            if ((pinnedObjectsArray != null) && (pinnedObjectsArray.Length == this.m_Cache.PinnedObjectsArray.Length))
                            {
                                flag2 = true;
                                for (int i = 0; i < pinnedObjectsArray.Length; i++)
                                {
                                    if (pinnedObjectsArray[i] != this.m_Cache.PinnedObjectsArray[i])
                                    {
                                        flag2 = false;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                if (!flag2 && (this.m_Cache != null))
                {
                    this.m_Cache.Free();
                    this.m_Cache = null;
                }
                Socket asyncObject = (Socket) base.AsyncObject;
                if (this.m_UseOverlappedIO)
                {
                    this.m_UnmanagedBlob = SafeOverlappedFree.Alloc(asyncObject.SafeHandle);
                    this.PinUnmanagedObjects(objectsToPin);
                    this.m_OverlappedEvent = new AutoResetEvent(false);
                    Marshal.WriteIntPtr(this.m_UnmanagedBlob.DangerousGetHandle(), Win32.OverlappedhEventOffset, this.m_OverlappedEvent.SafeWaitHandle.DangerousGetHandle());
                }
                else
                {
                    asyncObject.BindToCompletionPort();
                    if (this.m_Cache == null)
                    {
                        if (pinnedObjectsArray != null)
                        {
                            this.m_Cache = new OverlappedCache(new Overlapped(), pinnedObjectsArray, s_IOCallback);
                        }
                        else
                        {
                            this.m_Cache = new OverlappedCache(new Overlapped(), objectsToPin, s_IOCallback, alreadyTriedCast);
                        }
                    }
                    this.m_Cache.Overlapped.AsyncResult = this;
                }
            }
        }

        protected void SetupCache(ref OverlappedCache overlappedCache)
        {
            if (!this.m_UseOverlappedIO && !this.m_DisableOverlapped)
            {
                this.m_Cache = (overlappedCache == null) ? null : Interlocked.Exchange<OverlappedCache>(ref overlappedCache, null);
                this.m_CleanupCount++;
            }
        }

        internal SafeHandle OverlappedHandle
        {
            get
            {
                if (this.m_UseOverlappedIO)
                {
                    if ((this.m_UnmanagedBlob != null) && !this.m_UnmanagedBlob.IsInvalid)
                    {
                        return this.m_UnmanagedBlob;
                    }
                    return SafeOverlappedFree.Zero;
                }
                if (this.m_Cache != null)
                {
                    return this.m_Cache.NativeOverlapped;
                }
                return SafeNativeOverlapped.Zero;
            }
        }
    }
}

