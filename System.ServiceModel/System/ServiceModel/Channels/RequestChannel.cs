namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Threading;

    internal abstract class RequestChannel : ChannelBase, IRequestChannel, IChannel, ICommunicationObject
    {
        private bool closed;
        private ManualResetEvent closedEvent;
        private bool manualAddressing;
        private List<IRequestBase> outstandingRequests;
        private EndpointAddress to;
        private Uri via;

        protected RequestChannel(ChannelManagerBase channelFactory, EndpointAddress to, Uri via, bool manualAddressing) : base(channelFactory)
        {
            this.outstandingRequests = new List<IRequestBase>();
            if (!manualAddressing && (to == null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("to");
            }
            this.manualAddressing = manualAddressing;
            this.to = to;
            this.via = via;
        }

        protected void AbortPendingRequests()
        {
            IRequestBase[] baseArray = this.CopyPendingRequests(false);
            if (baseArray != null)
            {
                foreach (IRequestBase base2 in baseArray)
                {
                    base2.Abort(this);
                }
            }
        }

        protected virtual void AddHeadersTo(Message message)
        {
            if (!this.manualAddressing && (this.to != null))
            {
                this.to.ApplyTo(message);
            }
        }

        public IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state)
        {
            return this.BeginRequest(message, base.DefaultSendTimeout, callback, state);
        }

        public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", timeout, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
            }
            base.ThrowIfDisposedOrNotOpen();
            this.AddHeadersTo(message);
            IAsyncRequest request = this.CreateAsyncRequest(message, callback, state);
            this.TrackRequest(request);
            bool flag = true;
            try
            {
                request.BeginSendRequest(message, timeout);
                flag = false;
            }
            finally
            {
                if (flag)
                {
                    this.ReleaseRequest(request);
                }
            }
            return request;
        }

        protected IAsyncResult BeginWaitForPendingRequests(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new WaitForPendingRequestsAsyncResult(timeout, this, this.SetupWaitForPendingRequests(), callback, state);
        }

        private IRequestBase[] CopyPendingRequests(bool createEventIfNecessary)
        {
            IRequestBase[] array = null;
            lock (this.outstandingRequests)
            {
                if (this.outstandingRequests.Count > 0)
                {
                    array = new IRequestBase[this.outstandingRequests.Count];
                    this.outstandingRequests.CopyTo(array);
                    this.outstandingRequests.Clear();
                    if (createEventIfNecessary && (this.closedEvent == null))
                    {
                        this.closedEvent = new ManualResetEvent(false);
                    }
                }
            }
            return array;
        }

        protected abstract IAsyncRequest CreateAsyncRequest(Message message, AsyncCallback callback, object state);
        protected abstract IRequest CreateRequest(Message message);
        public Message EndRequest(IAsyncResult result)
        {
            Message message2;
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }
            IAsyncRequest request = result as IAsyncRequest;
            if (request == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("result", System.ServiceModel.SR.GetString("InvalidAsyncResult"));
            }
            try
            {
                Message message = request.End();
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    TraceUtility.TraceEvent(TraceEventType.Information, 0x40015, System.ServiceModel.SR.GetString("TraceCodeRequestChannelReplyReceived"), message);
                }
                message2 = message;
            }
            finally
            {
                this.ReleaseRequest(request);
            }
            return message2;
        }

        protected void EndWaitForPendingRequests(IAsyncResult result)
        {
            WaitForPendingRequestsAsyncResult.End(result);
        }

        protected void FaultPendingRequests()
        {
            IRequestBase[] baseArray = this.CopyPendingRequests(false);
            if (baseArray != null)
            {
                foreach (IRequestBase base2 in baseArray)
                {
                    base2.Fault(this);
                }
            }
        }

        private void FinishClose()
        {
            lock (this.outstandingRequests)
            {
                if (!this.closed)
                {
                    this.closed = true;
                    if (this.closedEvent != null)
                    {
                        this.closedEvent.Close();
                    }
                }
            }
        }

        public override T GetProperty<T>() where T: class
        {
            if (typeof(T) == typeof(IRequestChannel))
            {
                return (T) this;
            }
            T property = base.GetProperty<T>();
            if (property != null)
            {
                return property;
            }
            return default(T);
        }

        protected override void OnAbort()
        {
            this.AbortPendingRequests();
        }

        private void ReleaseRequest(IRequestBase request)
        {
            lock (this.outstandingRequests)
            {
                this.outstandingRequests.Remove(request);
                if (((this.outstandingRequests.Count == 0) && !this.closed) && (this.closedEvent != null))
                {
                    this.closedEvent.Set();
                }
            }
        }

        public Message Request(Message message)
        {
            return this.Request(message, base.DefaultSendTimeout);
        }

        public Message Request(Message message, TimeSpan timeout)
        {
            Message message3;
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }
            if (timeout < TimeSpan.Zero)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("timeout", timeout, System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0")));
            }
            base.ThrowIfDisposedOrNotOpen();
            this.AddHeadersTo(message);
            IRequest request = this.CreateRequest(message);
            this.TrackRequest(request);
            try
            {
                Message message2;
                TimeoutHelper helper = new TimeoutHelper(timeout);
                TimeSpan span = helper.RemainingTime();
                try
                {
                    request.SendRequest(message, span);
                }
                catch (TimeoutException exception)
                {
                    throw TraceUtility.ThrowHelperError(new TimeoutException(System.ServiceModel.SR.GetString("RequestChannelSendTimedOut", new object[] { span }), exception), message);
                }
                span = helper.RemainingTime();
                try
                {
                    message2 = request.WaitForReply(span);
                }
                catch (TimeoutException exception2)
                {
                    throw TraceUtility.ThrowHelperError(new TimeoutException(System.ServiceModel.SR.GetString("RequestChannelWaitForReplyTimedOut", new object[] { span }), exception2), message);
                }
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    TraceUtility.TraceEvent(TraceEventType.Information, 0x40015, System.ServiceModel.SR.GetString("TraceCodeRequestChannelReplyReceived"), message2);
                }
                message3 = message2;
            }
            finally
            {
                this.ReleaseRequest(request);
            }
            return message3;
        }

        private IRequestBase[] SetupWaitForPendingRequests()
        {
            return this.CopyPendingRequests(true);
        }

        private void TrackRequest(IRequestBase request)
        {
            lock (this.outstandingRequests)
            {
                base.ThrowIfDisposedOrNotOpen();
                this.outstandingRequests.Add(request);
            }
        }

        protected void WaitForPendingRequests(TimeSpan timeout)
        {
            IRequestBase[] baseArray = this.SetupWaitForPendingRequests();
            if ((baseArray != null) && !this.closedEvent.WaitOne(timeout, false))
            {
                foreach (IRequestBase base2 in baseArray)
                {
                    base2.Abort(this);
                }
            }
            this.FinishClose();
        }

        protected bool ManualAddressing
        {
            get
            {
                return this.manualAddressing;
            }
        }

        public EndpointAddress RemoteAddress
        {
            get
            {
                return this.to;
            }
        }

        public Uri Via
        {
            get
            {
                return this.via;
            }
        }

        private class WaitForPendingRequestsAsyncResult : AsyncResult
        {
            private static WaitOrTimerCallback completeWaitCallBack = new WaitOrTimerCallback(RequestChannel.WaitForPendingRequestsAsyncResult.OnCompleteWaitCallBack);
            private IRequestBase[] pendingRequests;
            private RequestChannel requestChannel;
            private TimeSpan timeout;
            private RegisteredWaitHandle waitHandle;

            public WaitForPendingRequestsAsyncResult(TimeSpan timeout, RequestChannel requestChannel, IRequestBase[] pendingRequests, AsyncCallback callback, object state) : base(callback, state)
            {
                this.requestChannel = requestChannel;
                this.pendingRequests = pendingRequests;
                this.timeout = timeout;
                if ((this.timeout == TimeSpan.Zero) || (this.pendingRequests == null))
                {
                    this.AbortRequests();
                    this.CleanupEvents();
                    base.Complete(true);
                }
                else
                {
                    this.waitHandle = ThreadPool.UnsafeRegisterWaitForSingleObject(this.requestChannel.closedEvent, completeWaitCallBack, this, TimeoutHelper.ToMilliseconds(timeout), true);
                }
            }

            private void AbortRequests()
            {
                if (this.pendingRequests != null)
                {
                    foreach (IRequestBase base2 in this.pendingRequests)
                    {
                        base2.Abort(this.requestChannel);
                    }
                }
            }

            private void CleanupEvents()
            {
                if (this.requestChannel.closedEvent != null)
                {
                    if (this.waitHandle != null)
                    {
                        this.waitHandle.Unregister(this.requestChannel.closedEvent);
                    }
                    this.requestChannel.FinishClose();
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<RequestChannel.WaitForPendingRequestsAsyncResult>(result);
            }

            private static void OnCompleteWaitCallBack(object state, bool timedOut)
            {
                RequestChannel.WaitForPendingRequestsAsyncResult result = (RequestChannel.WaitForPendingRequestsAsyncResult) state;
                Exception exception = null;
                try
                {
                    if (timedOut)
                    {
                        result.AbortRequests();
                    }
                    result.CleanupEvents();
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    exception = exception2;
                }
                result.Complete(false, exception);
            }
        }
    }
}

