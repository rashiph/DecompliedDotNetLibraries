namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Threading;

    internal class MultipleReceiveBinder : IChannelBinder
    {
        private IChannelBinder channelBinder;
        private static AsyncCallback onInnerReceiveCompleted = Fx.ThunkCallback(new AsyncCallback(MultipleReceiveBinder.OnInnerReceiveCompleted));
        private bool ordered;
        private MultipleReceiveAsyncResult outstanding;
        private ReceiveScopeQueue pendingResults;

        public MultipleReceiveBinder(IChannelBinder channelBinder, int size, bool ordered)
        {
            this.ordered = ordered;
            this.channelBinder = channelBinder;
            this.pendingResults = new ReceiveScopeQueue(size);
        }

        public void Abort()
        {
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
            IAsyncResult result2;
            Fx.AssertAndThrow(this.outstanding == null, "BeginTryReceive should not have a pending result.");
            MultipleReceiveAsyncResult result = new MultipleReceiveAsyncResult(callback, state);
            this.outstanding = result;
            this.EnsurePump(timeout);
            if (this.pendingResults.TryDequeueHead(out result2))
            {
                this.HandleReceiveRequestComplete(result2, true);
            }
            return result;
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.channelBinder.BeginWaitForMessage(timeout, callback, state);
        }

        public void CloseAfterFault(TimeSpan timeout)
        {
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
            return MultipleReceiveAsyncResult.End(result, out requestContext);
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            return this.channelBinder.EndWaitForMessage(result);
        }

        private void EnsurePump(TimeSpan timeout)
        {
            while (!this.pendingResults.IsFull)
            {
                ReceiveScopeSignalGate receiveScope = new ReceiveScopeSignalGate(this);
                this.pendingResults.Enqueue(receiveScope);
                IAsyncResult nestedResult = this.channelBinder.BeginTryReceive(timeout, onInnerReceiveCompleted, receiveScope);
                if (nestedResult.CompletedSynchronously)
                {
                    this.SignalReceiveCompleted(nestedResult);
                }
            }
        }

        private void HandleReceiveAndSignalCompletion(IAsyncResult nestedResult, bool completedSynchronosly)
        {
            if (this.SignalReceiveCompleted(nestedResult))
            {
                this.HandleReceiveRequestComplete(nestedResult, completedSynchronosly);
            }
        }

        private void HandleReceiveRequestComplete(IAsyncResult innerResult, bool completedSynchronously)
        {
            MultipleReceiveAsyncResult outstanding = this.outstanding;
            Exception completionException = null;
            try
            {
                RequestContext context;
                Fx.AssertAndThrow(outstanding != null, "HandleReceive invoked without an outstanding result");
                this.outstanding = null;
                outstanding.Valid = this.channelBinder.EndTryReceive(innerResult, out context);
                outstanding.RequestContext = context;
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                completionException = exception2;
            }
            outstanding.Complete(completedSynchronously, completionException);
        }

        private static void OnInnerReceiveCompleted(IAsyncResult nestedResult)
        {
            if (!nestedResult.CompletedSynchronously)
            {
                ReceiveScopeSignalGate asyncState = nestedResult.AsyncState as ReceiveScopeSignalGate;
                asyncState.Binder.HandleReceiveAndSignalCompletion(nestedResult, false);
            }
        }

        public Message Request(Message message, TimeSpan timeout)
        {
            return this.channelBinder.Request(message, timeout);
        }

        public void Send(Message message, TimeSpan timeout)
        {
            this.channelBinder.Send(message, timeout);
        }

        private bool SignalReceiveCompleted(IAsyncResult nestedResult)
        {
            if (this.ordered)
            {
                return this.pendingResults.TrySignal((ReceiveScopeSignalGate) nestedResult.AsyncState, nestedResult);
            }
            return this.pendingResults.TrySignalPending(nestedResult);
        }

        public bool TryReceive(TimeSpan timeout, out RequestContext requestContext)
        {
            return this.channelBinder.TryReceive(timeout, out requestContext);
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

        private class MultipleReceiveAsyncResult : AsyncResult
        {
            public MultipleReceiveAsyncResult(AsyncCallback callback, object state) : base(callback, state)
            {
            }

            public void Complete(bool completedSynchronously, Exception completionException)
            {
                base.Complete(completedSynchronously, completionException);
            }

            public static bool End(IAsyncResult result, out System.ServiceModel.Channels.RequestContext context)
            {
                MultipleReceiveBinder.MultipleReceiveAsyncResult result2 = AsyncResult.End<MultipleReceiveBinder.MultipleReceiveAsyncResult>(result);
                context = result2.RequestContext;
                return result2.Valid;
            }

            public System.ServiceModel.Channels.RequestContext RequestContext { get; set; }

            public bool Valid { get; set; }
        }

        internal static class MultipleReceiveDefaults
        {
            internal const int MaxPendingReceives = 1;
        }

        private class ReceiveScopeQueue
        {
            private int count;
            private int head;
            private MultipleReceiveBinder.ReceiveScopeSignalGate[] items;
            private int pending;
            private readonly int size;

            public ReceiveScopeQueue(int size)
            {
                this.size = size;
                this.head = 0;
                this.count = 0;
                this.pending = 0;
                this.items = new MultipleReceiveBinder.ReceiveScopeSignalGate[size];
            }

            private void Dequeue()
            {
                Fx.AssertAndThrow(this.count > 0, "Cannot Dequeue and empty queue.");
                this.items[this.head] = null;
                this.head = (this.head + 1) % this.size;
                this.count--;
            }

            internal void Enqueue(MultipleReceiveBinder.ReceiveScopeSignalGate receiveScope)
            {
                Fx.AssertAndThrow(this.count < this.size, "Cannot Enqueue into a full queue.");
                this.items[(this.head + this.count) % this.size] = receiveScope;
                this.count++;
            }

            private int GetNextPending()
            {
                int pending = this.pending;
                while (pending != (pending = Interlocked.CompareExchange(ref this.pending, (pending + 1) % this.size, pending)))
                {
                }
                return pending;
            }

            internal bool TryDequeueHead(out IAsyncResult result)
            {
                Fx.AssertAndThrow(this.count > 0, "Cannot unlock item when queue is empty");
                if (this.items[this.head].Unlock(out result))
                {
                    this.Dequeue();
                    return true;
                }
                return false;
            }

            public bool TrySignal(MultipleReceiveBinder.ReceiveScopeSignalGate scope, IAsyncResult nestedResult)
            {
                if (scope.Signal(nestedResult))
                {
                    this.Dequeue();
                    return true;
                }
                return false;
            }

            public bool TrySignalPending(IAsyncResult result)
            {
                int nextPending = this.GetNextPending();
                if (this.items[nextPending].Signal(result))
                {
                    this.Dequeue();
                    return true;
                }
                return false;
            }

            internal bool IsFull
            {
                get
                {
                    return (this.count == this.size);
                }
            }
        }

        private class ReceiveScopeSignalGate : SignalGate<IAsyncResult>
        {
            public ReceiveScopeSignalGate(MultipleReceiveBinder binder)
            {
                this.Binder = binder;
            }

            public MultipleReceiveBinder Binder { get; private set; }
        }
    }
}

