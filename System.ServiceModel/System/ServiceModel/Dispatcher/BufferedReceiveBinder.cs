namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;

    internal class BufferedReceiveBinder : IChannelBinder
    {
        private IChannelBinder channelBinder;
        private InputQueue<RequestContextWrapper> inputQueue;
        private int pendingOperationSemaphore;
        private static Action<object> tryReceive = new Action<object>(BufferedReceiveBinder.TryReceive);
        private static AsyncCallback tryReceiveCallback = Fx.ThunkCallback(new AsyncCallback(BufferedReceiveBinder.TryReceiveCallback));

        public BufferedReceiveBinder(IChannelBinder channelBinder)
        {
            this.channelBinder = channelBinder;
            this.inputQueue = new InputQueue<RequestContextWrapper>();
        }

        public void Abort()
        {
            this.inputQueue.Close();
            this.channelBinder.Abort();
        }

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.channelBinder.BeginRequest(message, timeout, callback, state);
        }

        public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.channelBinder.BeginSend(message, timeout, callback, state);
        }

        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (Interlocked.CompareExchange(ref this.pendingOperationSemaphore, 1, 0) == 0)
            {
                IAsyncResult result = this.channelBinder.BeginTryReceive(timeout, tryReceiveCallback, this);
                if (result.CompletedSynchronously)
                {
                    HandleEndTryReceive(result);
                }
            }
            return this.inputQueue.BeginDequeue(timeout, callback, state);
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.channelBinder.BeginWaitForMessage(timeout, callback, state);
        }

        public void CloseAfterFault(TimeSpan timeout)
        {
            this.inputQueue.Close();
            this.channelBinder.CloseAfterFault(timeout);
        }

        public Message EndRequest(IAsyncResult result)
        {
            return this.channelBinder.EndRequest(result);
        }

        public void EndSend(IAsyncResult result)
        {
            this.channelBinder.EndSend(result);
        }

        public bool EndTryReceive(IAsyncResult result, out RequestContext requestContext)
        {
            RequestContextWrapper wrapper;
            bool flag = this.inputQueue.EndDequeue(result, out wrapper);
            if (flag && (wrapper != null))
            {
                requestContext = wrapper.RequestContext;
                return flag;
            }
            requestContext = null;
            return flag;
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            return this.channelBinder.EndWaitForMessage(result);
        }

        private static void HandleEndTryReceive(IAsyncResult result)
        {
            BufferedReceiveBinder asyncState = (BufferedReceiveBinder) result.AsyncState;
            bool flag = false;
            try
            {
                RequestContext context;
                if (asyncState.channelBinder.EndTryReceive(result, out context))
                {
                    flag = asyncState.inputQueue.EnqueueWithoutDispatch(new RequestContextWrapper(context), null);
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                flag = asyncState.inputQueue.EnqueueWithoutDispatch(exception, null);
            }
            finally
            {
                Interlocked.Exchange(ref asyncState.pendingOperationSemaphore, 0);
                if (flag)
                {
                    asyncState.inputQueue.Dispatch();
                }
            }
        }

        internal void InjectRequest(RequestContext requestContext)
        {
            this.inputQueue.EnqueueAndDispatch(new RequestContextWrapper(requestContext));
        }

        public Message Request(Message message, TimeSpan timeout)
        {
            return this.channelBinder.Request(message, timeout);
        }

        public void Send(Message message, TimeSpan timeout)
        {
            this.channelBinder.Send(message, timeout);
        }

        private static void TryReceive(object state)
        {
            BufferedReceiveBinder binder = (BufferedReceiveBinder) state;
            bool flag = false;
            try
            {
                RequestContext context;
                if (binder.channelBinder.TryReceive(TimeSpan.MaxValue, out context))
                {
                    flag = binder.inputQueue.EnqueueWithoutDispatch(new RequestContextWrapper(context), null);
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                flag = binder.inputQueue.EnqueueWithoutDispatch(exception, null);
            }
            finally
            {
                Interlocked.Exchange(ref binder.pendingOperationSemaphore, 0);
                if (flag)
                {
                    binder.inputQueue.Dispatch();
                }
            }
        }

        public bool TryReceive(TimeSpan timeout, out RequestContext requestContext)
        {
            RequestContextWrapper wrapper;
            if (Interlocked.CompareExchange(ref this.pendingOperationSemaphore, 1, 0) == 0)
            {
                ActionItem.Schedule(tryReceive, this);
            }
            bool flag = this.inputQueue.Dequeue(timeout, out wrapper);
            if (flag && (wrapper != null))
            {
                requestContext = wrapper.RequestContext;
                return flag;
            }
            requestContext = null;
            return flag;
        }

        private static void TryReceiveCallback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                HandleEndTryReceive(result);
            }
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            return this.channelBinder.WaitForMessage(timeout);
        }

        public IChannel Channel
        {
            get
            {
                return this.channelBinder.Channel;
            }
        }

        public bool HasSession
        {
            get
            {
                return this.channelBinder.HasSession;
            }
        }

        public Uri ListenUri
        {
            get
            {
                return this.channelBinder.ListenUri;
            }
        }

        public EndpointAddress LocalAddress
        {
            get
            {
                return this.channelBinder.LocalAddress;
            }
        }

        public EndpointAddress RemoteAddress
        {
            get
            {
                return this.channelBinder.RemoteAddress;
            }
        }

        private class RequestContextWrapper
        {
            public RequestContextWrapper(System.ServiceModel.Channels.RequestContext requestContext)
            {
                this.RequestContext = requestContext;
            }

            public System.ServiceModel.Channels.RequestContext RequestContext { get; private set; }
        }
    }
}

