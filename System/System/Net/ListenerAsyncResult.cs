namespace System.Net
{
    using System;
    using System.Threading;

    internal class ListenerAsyncResult : LazyAsyncResult
    {
        private AsyncRequestContext m_RequestContext;
        private static readonly IOCompletionCallback s_IOCallback = new IOCompletionCallback(ListenerAsyncResult.WaitCallback);

        internal ListenerAsyncResult(object asyncObject, object userState, AsyncCallback callback) : base(asyncObject, userState, callback)
        {
            this.m_RequestContext = new AsyncRequestContext(this);
        }

        protected override void Cleanup()
        {
            if (this.m_RequestContext != null)
            {
                this.m_RequestContext.ReleasePins();
                this.m_RequestContext.Close();
            }
            base.Cleanup();
        }

        internal unsafe uint QueueBeginGetContext()
        {
            uint num = 0;
        Label_0002:
            (base.AsyncObject as HttpListener).EnsureBoundHandle();
            uint pBytesReturned = 0;
            num = UnsafeNclNativeMethods.HttpApi.HttpReceiveHttpRequest((base.AsyncObject as HttpListener).RequestQueueHandle, this.m_RequestContext.RequestBlob.RequestId, 1, this.m_RequestContext.RequestBlob, this.m_RequestContext.Size, &pBytesReturned, this.m_RequestContext.NativeOverlapped);
            if ((num == 0x57) && (this.m_RequestContext.RequestBlob.RequestId != 0L))
            {
                this.m_RequestContext.RequestBlob.RequestId = 0L;
                goto Label_0002;
            }
            if (num == 0xea)
            {
                this.m_RequestContext.Reset(this.m_RequestContext.RequestBlob.RequestId, pBytesReturned);
                goto Label_0002;
            }
            return num;
        }

        private static unsafe void WaitCallback(uint errorCode, uint numBytes, NativeOverlapped* nativeOverlapped)
        {
            ListenerAsyncResult asyncResult = (ListenerAsyncResult) Overlapped.Unpack(nativeOverlapped).AsyncResult;
            object result = null;
            try
            {
                if ((errorCode != 0) && (errorCode != 0xea))
                {
                    asyncResult.ErrorCode = (int) errorCode;
                    result = new HttpListenerException((int) errorCode);
                }
                else
                {
                    HttpListener asyncObject = asyncResult.AsyncObject as HttpListener;
                    if (errorCode == 0)
                    {
                        bool stoleBlob = false;
                        try
                        {
                            result = asyncObject.HandleAuthentication(asyncResult.m_RequestContext, out stoleBlob);
                        }
                        finally
                        {
                            if (stoleBlob)
                            {
                                asyncResult.m_RequestContext = (result == null) ? new AsyncRequestContext(asyncResult) : null;
                            }
                            else
                            {
                                asyncResult.m_RequestContext.Reset(0L, 0);
                            }
                        }
                    }
                    else
                    {
                        asyncResult.m_RequestContext.Reset(asyncResult.m_RequestContext.RequestBlob.RequestId, numBytes);
                    }
                    if (result == null)
                    {
                        uint num = asyncResult.QueueBeginGetContext();
                        if ((num != 0) && (num != 0x3e5))
                        {
                            result = new HttpListenerException((int) num);
                        }
                    }
                    if (result == null)
                    {
                        return;
                    }
                }
            }
            catch (Exception exception)
            {
                if (NclUtilities.IsFatal(exception))
                {
                    throw;
                }
                result = exception;
            }
            asyncResult.InvokeCallback(result);
        }

        internal static IOCompletionCallback IOCallback
        {
            get
            {
                return s_IOCallback;
            }
        }
    }
}

