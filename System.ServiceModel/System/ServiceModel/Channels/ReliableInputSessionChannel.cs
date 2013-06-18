namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.Xml;

    internal abstract class ReliableInputSessionChannel : InputChannel, IInputSessionChannel, IInputChannel, IChannel, ICommunicationObject, ISessionChannel<IInputSession>
    {
        private bool advertisedZero;
        private static Action<object> asyncReceiveComplete = new Action<object>(ReliableInputSessionChannel.AsyncReceiveCompleteStatic);
        private IServerReliableChannelBinder binder;
        private ReliableInputConnection connection;
        private DeliveryStrategy<Message> deliveryStrategy;
        private ReliableChannelListenerBase<IInputSessionChannel> listener;
        private static AsyncCallback onReceiveCompleted = Fx.ThunkCallback(new AsyncCallback(ReliableInputSessionChannel.OnReceiveCompletedStatic));
        protected string perfCounterId;
        private ServerReliableSession session;

        protected ReliableInputSessionChannel(ReliableChannelListenerBase<IInputSessionChannel> listener, IServerReliableChannelBinder binder, FaultHelper faultHelper, UniqueId inputID) : base(listener, binder.LocalAddress)
        {
            this.binder = binder;
            this.listener = listener;
            this.connection = new ReliableInputConnection();
            this.connection.ReliableMessagingVersion = listener.ReliableMessagingVersion;
            this.session = new ServerReliableSession(this, listener, binder, faultHelper, inputID, null);
            this.session.UnblockChannelCloseCallback = new ChannelReliableSession.UnblockChannelCloseHandler(this.UnblockClose);
            if (listener.Ordered)
            {
                this.deliveryStrategy = new OrderedDeliveryStrategy<Message>(this, listener.MaxTransferWindowSize, false);
            }
            else
            {
                this.deliveryStrategy = new UnorderedDeliveryStrategy<Message>(this, listener.MaxTransferWindowSize);
            }
            this.binder.Faulted += new BinderExceptionHandler(this.OnBinderFaulted);
            this.binder.OnException += new BinderExceptionHandler(this.OnBinderException);
            this.session.Open(TimeSpan.Zero);
            if (PerformanceCounters.PerformanceCountersEnabled)
            {
                this.perfCounterId = this.listener.Uri.ToString().ToUpperInvariant();
            }
        }

        protected virtual void AbortGuards()
        {
        }

        protected void AddAcknowledgementHeader(Message message)
        {
            int bufferRemaining = -1;
            if (this.Listener.FlowControlEnabled)
            {
                bufferRemaining = this.Listener.MaxTransferWindowSize - this.deliveryStrategy.EnqueuedCount;
                this.AdvertisedZero = bufferRemaining == 0;
            }
            WsrmUtilities.AddAcknowledgementHeader(this.listener.ReliableMessagingVersion, message, this.session.InputID, this.connection.Ranges, this.connection.IsLastKnown, bufferRemaining);
        }

        protected virtual void AggregateAsyncCloseOperations(List<OperationWithTimeoutBeginCallback> beginOperations, List<OperationEndCallback> endOperations)
        {
            beginOperations.Add(new OperationWithTimeoutBeginCallback(this.session.BeginClose));
            endOperations.Add(new OperationEndCallback(this.session.EndClose));
        }

        private static void AsyncReceiveCompleteStatic(object state)
        {
            IAsyncResult result = (IAsyncResult) state;
            ReliableInputSessionChannel asyncState = (ReliableInputSessionChannel) result.AsyncState;
            try
            {
                if (asyncState.HandleReceiveComplete(result))
                {
                    asyncState.StartReceiving(true);
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                asyncState.ReliableSession.OnUnknownException(exception);
            }
        }

        private IAsyncResult BeginCloseBinder(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.binder.BeginClose(timeout, MaskingMode.Handled, callback, state);
        }

        protected virtual IAsyncResult BeginCloseGuards(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        private IAsyncResult BeginUnregisterChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.listener.OnReliableChannelBeginClose(this.ReliableSession.InputID, null, timeout, callback, state);
        }

        protected virtual void CloseGuards(TimeSpan timeout)
        {
        }

        protected Message CreateAcknowledgmentMessage()
        {
            int bufferRemaining = -1;
            if (this.Listener.FlowControlEnabled)
            {
                bufferRemaining = this.Listener.MaxTransferWindowSize - this.deliveryStrategy.EnqueuedCount;
                this.AdvertisedZero = bufferRemaining == 0;
            }
            return WsrmUtilities.CreateAcknowledgmentMessage(this.listener.MessageVersion, this.listener.ReliableMessagingVersion, this.session.InputID, this.connection.Ranges, this.connection.IsLastKnown, bufferRemaining);
        }

        private void EndCloseBinder(IAsyncResult result)
        {
            this.binder.EndClose(result);
        }

        protected virtual void EndCloseGuards(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        private void EndUnregisterChannel(IAsyncResult result)
        {
            this.listener.OnReliableChannelEndClose(result);
        }

        public override T GetProperty<T>() where T: class
        {
            if (typeof(T) == typeof(IInputSessionChannel))
            {
                return (T) this;
            }
            T property = base.GetProperty<T>();
            if (property != null)
            {
                return property;
            }
            T local2 = this.binder.Channel.GetProperty<T>();
            if ((local2 == null) && (typeof(T) == typeof(FaultConverter)))
            {
                return (T) FaultConverter.GetDefaultFaultConverter(this.listener.MessageVersion);
            }
            return local2;
        }

        protected abstract bool HandleReceiveComplete(IAsyncResult result);
        protected override void OnAbort()
        {
            this.connection.Abort(this);
            this.AbortGuards();
            this.session.Abort();
            this.listener.OnReliableChannelAbort(this.ReliableSession.InputID, null);
            base.OnAbort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.ThrowIfCloseInvalid();
            OperationWithTimeoutBeginCallback[] beginOperations = new OperationWithTimeoutBeginCallback[] { new OperationWithTimeoutBeginCallback(this.connection.BeginClose), new OperationWithTimeoutBeginCallback(this.session.BeginClose), new OperationWithTimeoutBeginCallback(this.BeginCloseGuards), new OperationWithTimeoutBeginCallback(this.BeginCloseBinder), new OperationWithTimeoutBeginCallback(this.BeginUnregisterChannel), new OperationWithTimeoutBeginCallback(this.OnBeginClose) };
            OperationEndCallback[] endOperations = new OperationEndCallback[] { new OperationEndCallback(this.connection.EndClose), new OperationEndCallback(this.session.EndClose), new OperationEndCallback(this.EndCloseGuards), new OperationEndCallback(this.EndCloseBinder), new OperationEndCallback(this.EndUnregisterChannel), new OperationEndCallback(this.OnEndClose) };
            return OperationWithTimeoutComposer.BeginComposeAsyncOperations(timeout, beginOperations, endOperations, callback, state);
        }

        private void OnBinderException(IReliableChannelBinder sender, Exception exception)
        {
            if (exception is QuotaExceededException)
            {
                this.session.OnLocalFault(exception, SequenceTerminatedFault.CreateQuotaExceededFault(this.session.OutputID), null);
            }
            else
            {
                base.EnqueueAndDispatch(exception, null, false);
            }
        }

        private void OnBinderFaulted(IReliableChannelBinder sender, Exception exception)
        {
            this.binder.Abort();
            exception = new CommunicationException(System.ServiceModel.SR.GetString("EarlySecurityFaulted"), exception);
            this.session.OnLocalFault(exception, (Message) null, null);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            this.ThrowIfCloseInvalid();
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.connection.Close(helper.RemainingTime());
            this.session.Close(helper.RemainingTime());
            this.CloseGuards(helper.RemainingTime());
            this.binder.Close(helper.RemainingTime(), MaskingMode.Handled);
            this.listener.OnReliableChannelClose(this.ReliableSession.InputID, null, helper.RemainingTime());
            base.OnClose(helper.RemainingTime());
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            this.binder.Faulted -= new BinderExceptionHandler(this.OnBinderFaulted);
            this.deliveryStrategy.Dispose();
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            OperationWithTimeoutComposer.EndComposeAsyncOperations(result);
        }

        protected override void OnFaulted()
        {
            this.session.OnFaulted();
            this.UnblockClose();
            base.OnFaulted();
            if (PerformanceCounters.PerformanceCountersEnabled)
            {
                PerformanceCounters.SessionFaulted(this.perfCounterId);
            }
        }

        protected virtual void OnQuotaAvailable()
        {
        }

        private static void OnReceiveCompletedStatic(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ReliableInputSessionChannel asyncState = (ReliableInputSessionChannel) result.AsyncState;
                try
                {
                    if (asyncState.HandleReceiveComplete(result))
                    {
                        asyncState.StartReceiving(true);
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    asyncState.ReliableSession.OnUnknownException(exception);
                }
            }
        }

        protected void ShutdownCallback(object state)
        {
            base.Shutdown();
        }

        protected void StartReceiving(bool canBlock)
        {
            IAsyncResult result;
        Label_0000:
            result = this.Binder.BeginTryReceive(TimeSpan.MaxValue, onReceiveCompleted, this);
            if (result.CompletedSynchronously)
            {
                if (!canBlock)
                {
                    ActionItem.Schedule(asyncReceiveComplete, result);
                }
                else if (this.HandleReceiveComplete(result))
                {
                    goto Label_0000;
                }
            }
        }

        private void ThrowIfCloseInvalid()
        {
            bool flag = false;
            if (this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessagingFebruary2005)
            {
                if ((this.DeliveryStrategy.EnqueuedCount > 0) || (this.Connection.Ranges.Count > 1))
                {
                    flag = true;
                }
            }
            else if ((this.listener.ReliableMessagingVersion == ReliableMessagingVersion.WSReliableMessaging11) && (this.DeliveryStrategy.EnqueuedCount > 0))
            {
                flag = true;
            }
            if (flag)
            {
                WsrmFault fault = SequenceTerminatedFault.CreateProtocolFault(this.session.InputID, System.ServiceModel.SR.GetString("SequenceTerminatedSessionClosedBeforeDone"), System.ServiceModel.SR.GetString("SessionClosedBeforeDone"));
                this.session.OnLocalFault(null, fault, null);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(fault.CreateException());
            }
        }

        private void UnblockClose()
        {
            this.connection.Fault(this);
        }

        protected bool AdvertisedZero
        {
            get
            {
                return this.advertisedZero;
            }
            set
            {
                this.advertisedZero = value;
            }
        }

        public IServerReliableChannelBinder Binder
        {
            get
            {
                return this.binder;
            }
        }

        protected ReliableInputConnection Connection
        {
            get
            {
                return this.connection;
            }
        }

        protected DeliveryStrategy<Message> DeliveryStrategy
        {
            get
            {
                return this.deliveryStrategy;
            }
        }

        protected ReliableChannelListenerBase<IInputSessionChannel> Listener
        {
            get
            {
                return this.listener;
            }
        }

        protected ChannelReliableSession ReliableSession
        {
            get
            {
                return this.session;
            }
        }

        public IInputSession Session
        {
            get
            {
                return this.session;
            }
        }
    }
}

