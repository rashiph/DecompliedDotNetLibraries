namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.Xml;

    internal abstract class ReliableChannelListenerBase<TChannel> : DelegatingChannelListener<TChannel>, IReliableFactorySettings where TChannel: class, IChannel
    {
        private TimeSpan acknowledgementInterval;
        private bool closed;
        private System.ServiceModel.Channels.FaultHelper faultHelper;
        private bool flowControlEnabled;
        private TimeSpan inactivityTimeout;
        private IMessageFilterTable<EndpointAddress> localAddresses;
        private int maxPendingChannels;
        private int maxRetryCount;
        private int maxTransferWindowSize;
        private System.ServiceModel.Channels.MessageVersion messageVersion;
        private bool ordered;
        private System.ServiceModel.ReliableMessagingVersion reliableMessagingVersion;

        protected ReliableChannelListenerBase(ReliableSessionBindingElement settings, Binding binding) : base(true, binding)
        {
            this.acknowledgementInterval = settings.AcknowledgementInterval;
            this.flowControlEnabled = settings.FlowControlEnabled;
            this.inactivityTimeout = settings.InactivityTimeout;
            this.maxPendingChannels = settings.MaxPendingChannels;
            this.maxRetryCount = settings.MaxRetryCount;
            this.maxTransferWindowSize = settings.MaxTransferWindowSize;
            this.messageVersion = binding.MessageVersion;
            this.ordered = settings.Ordered;
            this.reliableMessagingVersion = settings.ReliableMessagingVersion;
        }

        protected virtual void AbortInnerListener()
        {
            this.faultHelper.Abort();
            this.InnerChannelListener.Abort();
        }

        protected virtual IAsyncResult BeginCloseInnerListener(TimeSpan timeout, AsyncCallback callback, object state)
        {
            OperationWithTimeoutBeginCallback[] callbackArray3 = new OperationWithTimeoutBeginCallback[2];
            callbackArray3[0] = new OperationWithTimeoutBeginCallback(this.faultHelper.BeginClose);
            IChannelListener innerChannelListener = this.InnerChannelListener;
            callbackArray3[1] = new OperationWithTimeoutBeginCallback(innerChannelListener.BeginClose);
            OperationWithTimeoutBeginCallback[] beginOperations = callbackArray3;
            OperationEndCallback[] callbackArray4 = new OperationEndCallback[2];
            callbackArray4[0] = new OperationEndCallback(this.faultHelper.EndClose);
            IChannelListener listener2 = this.InnerChannelListener;
            callbackArray4[1] = new OperationEndCallback(listener2.EndClose);
            OperationEndCallback[] endOperations = callbackArray4;
            return OperationWithTimeoutComposer.BeginComposeAsyncOperations(timeout, beginOperations, endOperations, callback, state);
        }

        protected virtual void CloseInnerListener(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.faultHelper.Close(helper.RemainingTime());
            this.InnerChannelListener.Close(helper.RemainingTime());
        }

        protected virtual void EndCloseInnerListener(IAsyncResult result)
        {
            OperationWithTimeoutComposer.EndComposeAsyncOperations(result);
        }

        protected abstract bool HasChannels();
        protected abstract bool IsLastChannel(UniqueId inputId);
        protected override void OnAbort()
        {
            bool flag;
            lock (base.ThisLock)
            {
                this.closed = true;
                flag = !this.HasChannels();
            }
            if (flag)
            {
                this.AbortInnerListener();
            }
            base.OnAbort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseAsyncResult<TChannel>((ReliableChannelListenerBase<TChannel>) this, new OperationWithTimeoutBeginCallback(this.OnBeginClose), new OperationEndCallback(this.OnEndClose), timeout, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            OperationWithTimeoutBeginCallback[] beginOperations = new OperationWithTimeoutBeginCallback[2];
            beginOperations[0] = new OperationWithTimeoutBeginCallback(this.OnBeginOpen);
            IChannelListener innerChannelListener = this.InnerChannelListener;
            beginOperations[1] = new OperationWithTimeoutBeginCallback(innerChannelListener.BeginOpen);
            OperationEndCallback[] endOperations = new OperationEndCallback[2];
            endOperations[0] = new OperationEndCallback(this.OnEndOpen);
            IChannelListener listener2 = this.InnerChannelListener;
            endOperations[1] = new OperationEndCallback(listener2.EndOpen);
            return OperationWithTimeoutComposer.BeginComposeAsyncOperations(timeout, beginOperations, endOperations, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (this.ShouldCloseOnChannelListenerClose())
            {
                this.CloseInnerListener(helper.RemainingTime());
                this.closed = true;
            }
            base.OnClose(helper.RemainingTime());
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CloseAsyncResult<TChannel>.End(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            OperationWithTimeoutComposer.EndComposeAsyncOperations(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            base.OnOpen(helper.RemainingTime());
            this.InnerChannelListener.Open(helper.RemainingTime());
        }

        public void OnReliableChannelAbort(UniqueId inputId, UniqueId outputId)
        {
            lock (base.ThisLock)
            {
                this.RemoveChannel(inputId, outputId);
                if (!this.closed || this.HasChannels())
                {
                    return;
                }
            }
            this.AbortInnerListener();
        }

        public IAsyncResult OnReliableChannelBeginClose(UniqueId inputId, UniqueId outputId, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OnReliableChannelCloseAsyncResult<TChannel>((ReliableChannelListenerBase<TChannel>) this, inputId, outputId, timeout, callback, state);
        }

        public void OnReliableChannelClose(UniqueId inputId, UniqueId outputId, TimeSpan timeout)
        {
            if (this.ShouldCloseOnReliableChannelClose(inputId, outputId))
            {
                this.CloseInnerListener(timeout);
                lock (base.ThisLock)
                {
                    this.RemoveChannel(inputId, outputId);
                }
            }
        }

        public void OnReliableChannelEndClose(IAsyncResult result)
        {
            OnReliableChannelCloseAsyncResult<TChannel>.End(result);
        }

        protected abstract void RemoveChannel(UniqueId inputId, UniqueId outputId);
        private bool ShouldCloseOnChannelListenerClose()
        {
            lock (base.ThisLock)
            {
                if (!this.HasChannels())
                {
                    return true;
                }
                this.closed = true;
                return false;
            }
        }

        private bool ShouldCloseOnReliableChannelClose(UniqueId inputId, UniqueId outputId)
        {
            lock (base.ThisLock)
            {
                if (this.closed && this.IsLastChannel(inputId))
                {
                    return true;
                }
                this.RemoveChannel(inputId, outputId);
                return false;
            }
        }

        public TimeSpan AcknowledgementInterval
        {
            get
            {
                return this.acknowledgementInterval;
            }
        }

        protected abstract bool Duplex { get; }

        protected System.ServiceModel.Channels.FaultHelper FaultHelper
        {
            get
            {
                return this.faultHelper;
            }
            set
            {
                this.faultHelper = value;
            }
        }

        public bool FlowControlEnabled
        {
            get
            {
                return this.flowControlEnabled;
            }
        }

        public TimeSpan InactivityTimeout
        {
            get
            {
                return this.inactivityTimeout;
            }
        }

        protected bool IsAccepting
        {
            get
            {
                return (base.State == CommunicationState.Opened);
            }
        }

        public IMessageFilterTable<EndpointAddress> LocalAddresses
        {
            get
            {
                return this.localAddresses;
            }
            set
            {
                this.localAddresses = value;
            }
        }

        public int MaxPendingChannels
        {
            get
            {
                return this.maxPendingChannels;
            }
        }

        public int MaxRetryCount
        {
            get
            {
                return this.maxRetryCount;
            }
        }

        public int MaxTransferWindowSize
        {
            get
            {
                return this.maxTransferWindowSize;
            }
        }

        public System.ServiceModel.Channels.MessageVersion MessageVersion
        {
            get
            {
                return this.messageVersion;
            }
        }

        public bool Ordered
        {
            get
            {
                return this.ordered;
            }
        }

        public System.ServiceModel.ReliableMessagingVersion ReliableMessagingVersion
        {
            get
            {
                return this.reliableMessagingVersion;
            }
        }

        public TimeSpan SendTimeout
        {
            get
            {
                return base.InternalSendTimeout;
            }
        }

        private class CloseAsyncResult : AsyncResult
        {
            private OperationWithTimeoutBeginCallback baseBeginClose;
            private OperationEndCallback baseEndClose;
            private static AsyncCallback onBaseChannelListenerCloseComplete;
            private static AsyncCallback onInnerChannelListenerCloseComplete;
            private ReliableChannelListenerBase<TChannel> parent;
            private TimeoutHelper timeoutHelper;

            static CloseAsyncResult()
            {
                ReliableChannelListenerBase<TChannel>.CloseAsyncResult.onBaseChannelListenerCloseComplete = Fx.ThunkCallback(new AsyncCallback(ReliableChannelListenerBase<TChannel>.CloseAsyncResult.OnBaseChannelListenerCloseCompleteStatic));
                ReliableChannelListenerBase<TChannel>.CloseAsyncResult.onInnerChannelListenerCloseComplete = Fx.ThunkCallback(new AsyncCallback(ReliableChannelListenerBase<TChannel>.CloseAsyncResult.OnInnerChannelListenerCloseCompleteStatic));
            }

            public CloseAsyncResult(ReliableChannelListenerBase<TChannel> parent, OperationWithTimeoutBeginCallback baseBeginClose, OperationEndCallback baseEndClose, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.parent = parent;
                this.baseBeginClose = baseBeginClose;
                this.baseEndClose = baseEndClose;
                bool flag = false;
                if (this.parent.ShouldCloseOnChannelListenerClose())
                {
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    IAsyncResult result = this.parent.BeginCloseInnerListener(this.timeoutHelper.RemainingTime(), ReliableChannelListenerBase<TChannel>.CloseAsyncResult.onInnerChannelListenerCloseComplete, this);
                    if (result.CompletedSynchronously)
                    {
                        flag = this.CompleteInnerChannelListenerClose(result);
                    }
                }
                else
                {
                    flag = this.CloseBaseChannelListener(timeout);
                }
                if (flag)
                {
                    base.Complete(true);
                }
            }

            private bool CloseBaseChannelListener(TimeSpan timeout)
            {
                IAsyncResult result = this.baseBeginClose(timeout, ReliableChannelListenerBase<TChannel>.CloseAsyncResult.onBaseChannelListenerCloseComplete, this);
                if (result.CompletedSynchronously)
                {
                    this.baseEndClose(result);
                    return true;
                }
                return false;
            }

            private bool CompleteInnerChannelListenerClose(IAsyncResult result)
            {
                this.parent.EndCloseInnerListener(result);
                this.parent.closed = true;
                this.parent.faultHelper.Abort();
                return this.CloseBaseChannelListener(this.timeoutHelper.RemainingTime());
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<ReliableChannelListenerBase<TChannel>.CloseAsyncResult>(result);
            }

            private void OnBaseChannelListenerCloseComplete(IAsyncResult result)
            {
                Exception exception = null;
                try
                {
                    this.baseEndClose(result);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    exception = exception2;
                }
                base.Complete(false, exception);
            }

            private static void OnBaseChannelListenerCloseCompleteStatic(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ((ReliableChannelListenerBase<TChannel>.CloseAsyncResult) result.AsyncState).OnBaseChannelListenerCloseComplete(result);
                }
            }

            private void OnInnerChannelListenerCloseComplete(IAsyncResult result)
            {
                bool flag;
                Exception exception = null;
                try
                {
                    flag = this.CompleteInnerChannelListenerClose(result);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    flag = true;
                    exception = exception2;
                }
                if (flag)
                {
                    base.Complete(false, exception);
                }
            }

            private static void OnInnerChannelListenerCloseCompleteStatic(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ((ReliableChannelListenerBase<TChannel>.CloseAsyncResult) result.AsyncState).OnInnerChannelListenerCloseComplete(result);
                }
            }
        }

        private class OnReliableChannelCloseAsyncResult : AsyncResult
        {
            private ReliableChannelListenerBase<TChannel> channelListener;
            private UniqueId inputId;
            private static AsyncCallback onInnerChannelListenerCloseComplete;
            private UniqueId outputId;

            static OnReliableChannelCloseAsyncResult()
            {
                ReliableChannelListenerBase<TChannel>.OnReliableChannelCloseAsyncResult.onInnerChannelListenerCloseComplete = Fx.ThunkCallback(new AsyncCallback(ReliableChannelListenerBase<TChannel>.OnReliableChannelCloseAsyncResult.OnInnerChannelListenerCloseCompleteStatic));
            }

            public OnReliableChannelCloseAsyncResult(ReliableChannelListenerBase<TChannel> channelListener, UniqueId inputId, UniqueId outputId, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                if (!channelListener.ShouldCloseOnReliableChannelClose(inputId, outputId))
                {
                    base.Complete(true);
                }
                else
                {
                    this.channelListener = channelListener;
                    this.inputId = inputId;
                    this.outputId = outputId;
                    IAsyncResult result = this.channelListener.BeginCloseInnerListener(timeout, ReliableChannelListenerBase<TChannel>.OnReliableChannelCloseAsyncResult.onInnerChannelListenerCloseComplete, this);
                    if (result.CompletedSynchronously)
                    {
                        this.CompleteInnerChannelListenerClose(result);
                        base.Complete(true);
                    }
                }
            }

            private void CompleteInnerChannelListenerClose(IAsyncResult result)
            {
                this.channelListener.EndCloseInnerListener(result);
                lock (this.channelListener.ThisLock)
                {
                    this.channelListener.RemoveChannel(this.inputId, this.outputId);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<ReliableChannelListenerBase<TChannel>.OnReliableChannelCloseAsyncResult>(result);
            }

            private void OnInnerChannelListenerCloseComplete(IAsyncResult result)
            {
                Exception exception = null;
                try
                {
                    this.CompleteInnerChannelListenerClose(result);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    exception = exception2;
                }
                base.Complete(false, exception);
            }

            private static void OnInnerChannelListenerCloseCompleteStatic(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ((ReliableChannelListenerBase<TChannel>.OnReliableChannelCloseAsyncResult) result.AsyncState).OnInnerChannelListenerCloseComplete(result);
                }
            }
        }
    }
}

