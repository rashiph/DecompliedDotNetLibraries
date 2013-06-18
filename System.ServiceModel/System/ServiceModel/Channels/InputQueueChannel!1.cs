namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Diagnostics;

    internal abstract class InputQueueChannel<TDisposable> : ChannelBase where TDisposable: class, IDisposable
    {
        private InputQueue<TDisposable> inputQueue;

        protected InputQueueChannel(ChannelManagerBase channelManager) : base(channelManager)
        {
            this.inputQueue = TraceUtility.CreateInputQueue<TDisposable>();
        }

        protected IAsyncResult BeginDequeue(TimeSpan timeout, AsyncCallback callback, object state)
        {
            base.ThrowIfNotOpened();
            return this.inputQueue.BeginDequeue(timeout, callback, state);
        }

        protected IAsyncResult BeginWaitForItem(TimeSpan timeout, AsyncCallback callback, object state)
        {
            base.ThrowIfNotOpened();
            return this.inputQueue.BeginWaitForItem(timeout, callback, state);
        }

        protected bool Dequeue(TimeSpan timeout, out TDisposable item)
        {
            base.ThrowIfNotOpened();
            bool flag = this.inputQueue.Dequeue(timeout, out item);
            if (((TDisposable) item) == null)
            {
                base.ThrowIfFaulted();
                base.ThrowIfAborted();
            }
            return flag;
        }

        public void Dispatch()
        {
            this.inputQueue.Dispatch();
        }

        protected bool EndDequeue(IAsyncResult result, out TDisposable item)
        {
            bool flag = this.inputQueue.EndDequeue(result, out item);
            if (((TDisposable) item) == null)
            {
                base.ThrowIfFaulted();
                base.ThrowIfAborted();
            }
            return flag;
        }

        protected bool EndWaitForItem(IAsyncResult result)
        {
            bool flag = this.inputQueue.EndWaitForItem(result);
            base.ThrowIfFaulted();
            base.ThrowIfAborted();
            return flag;
        }

        public void EnqueueAndDispatch(TDisposable item)
        {
            this.EnqueueAndDispatch(item, null);
        }

        public void EnqueueAndDispatch(TDisposable item, Action dequeuedCallback)
        {
            this.OnEnqueueItem(item);
            this.inputQueue.EnqueueAndDispatch(item, dequeuedCallback);
        }

        public void EnqueueAndDispatch(TDisposable item, Action dequeuedCallback, bool canDispatchOnThisThread)
        {
            this.OnEnqueueItem(item);
            this.inputQueue.EnqueueAndDispatch(item, dequeuedCallback, canDispatchOnThisThread);
        }

        public void EnqueueAndDispatch(Exception exception, Action dequeuedCallback, bool canDispatchOnThisThread)
        {
            this.inputQueue.EnqueueAndDispatch(exception, dequeuedCallback, canDispatchOnThisThread);
        }

        public bool EnqueueWithoutDispatch(Exception exception, Action dequeuedCallback)
        {
            return this.inputQueue.EnqueueWithoutDispatch(exception, dequeuedCallback);
        }

        public bool EnqueueWithoutDispatch(TDisposable item, Action dequeuedCallback)
        {
            this.OnEnqueueItem(item);
            return this.inputQueue.EnqueueWithoutDispatch(item, dequeuedCallback);
        }

        protected override void OnAbort()
        {
            this.inputQueue.Close();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.inputQueue.Close();
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            this.inputQueue.Close();
        }

        protected override void OnClosing()
        {
            base.OnClosing();
            this.inputQueue.Shutdown(() => base.GetPendingException());
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected virtual void OnEnqueueItem(TDisposable item)
        {
        }

        protected override void OnFaulted()
        {
            base.OnFaulted();
            this.inputQueue.Shutdown(() => base.GetPendingException());
        }

        public void Shutdown()
        {
            this.inputQueue.Shutdown();
        }

        protected bool WaitForItem(TimeSpan timeout)
        {
            base.ThrowIfNotOpened();
            bool flag = this.inputQueue.WaitForItem(timeout);
            base.ThrowIfFaulted();
            base.ThrowIfAborted();
            return flag;
        }

        public int InternalPendingItems
        {
            get
            {
                return this.inputQueue.PendingCount;
            }
        }

        public int PendingItems
        {
            get
            {
                base.ThrowIfDisposedOrNotOpen();
                return this.InternalPendingItems;
            }
        }
    }
}

