namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Transactions;

    internal class ListenerHandler : CommunicationObject, ISessionThrottleNotification
    {
        private static AsyncCallback acceptCallback = Fx.ThunkCallback(new AsyncCallback(ListenerHandler.AcceptCallback));
        private bool acceptedNull;
        private readonly ErrorHandlingAcceptor acceptor;
        private ListenerChannel channel;
        private readonly System.ServiceModel.Dispatcher.ChannelDispatcher channelDispatcher;
        private bool doneAccepting;
        private EndpointDispatcherTable endpoints;
        private readonly ServiceHostBase host;
        private ServiceChannel.SessionIdleManager idleManager;
        private static Action<object> initiateChannelPump = new Action<object>(ListenerHandler.InitiateChannelPump);
        private readonly IListenerBinder listenerBinder;
        private readonly ServiceThrottle throttle;
        private IDefaultCommunicationTimeouts timeouts;
        private WrappedTransaction wrappedTransaction;

        internal ListenerHandler(IListenerBinder listenerBinder, System.ServiceModel.Dispatcher.ChannelDispatcher channelDispatcher, ServiceHostBase host, ServiceThrottle throttle, IDefaultCommunicationTimeouts timeouts)
        {
            this.listenerBinder = listenerBinder;
            if (this.listenerBinder == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("listenerBinder");
            }
            this.channelDispatcher = channelDispatcher;
            if (this.channelDispatcher == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channelDispatcher");
            }
            this.host = host;
            if (this.host == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("host");
            }
            this.throttle = throttle;
            if (this.throttle == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("throttle");
            }
            this.timeouts = timeouts;
            this.endpoints = channelDispatcher.EndpointDispatcherTable;
            this.acceptor = new ErrorHandlingAcceptor(listenerBinder, channelDispatcher);
        }

        private void AbortChannels()
        {
            IChannel[] channelArray = this.channelDispatcher.Channels.ToArray();
            for (int i = 0; i < channelArray.Length; i++)
            {
                channelArray[i].Abort();
            }
        }

        private bool AcceptAndAcquireThrottle()
        {
            IAsyncResult result = this.acceptor.BeginTryAccept(TimeSpan.MaxValue, acceptCallback, this);
            return (result.CompletedSynchronously && this.HandleEndAccept(result));
        }

        private static void AcceptCallback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ListenerHandler asyncState = (ListenerHandler) result.AsyncState;
                if (asyncState.HandleEndAccept(result))
                {
                    asyncState.Dispatch();
                    asyncState.ChannelPump();
                }
            }
        }

        private void AcceptedNull()
        {
            this.acceptedNull = true;
        }

        private bool AcquireThrottle()
        {
            if (((this.channel != null) && (this.throttle != null)) && this.channelDispatcher.Session)
            {
                return this.throttle.AcquireSession(this);
            }
            return true;
        }

        private void CancelPendingIdleManager()
        {
            ServiceChannel.SessionIdleManager idleManager = this.idleManager;
            if (idleManager != null)
            {
                idleManager.CancelTimer();
            }
        }

        private void ChannelPump()
        {
            IChannelListener listener = this.listenerBinder.Listener;
        Label_000C:
            if (this.acceptedNull || (listener.State == CommunicationState.Faulted))
            {
                this.DoneAccepting();
            }
            else if (this.AcceptAndAcquireThrottle())
            {
                this.Dispatch();
                goto Label_000C;
            }
        }

        private void CloseChannel(IChannel channel, TimeSpan timeout)
        {
            try
            {
                if ((channel.State != CommunicationState.Closing) && (channel.State != CommunicationState.Closed))
                {
                    CloseChannelState state = new CloseChannelState(this, channel);
                    if (channel is ISessionChannel<IDuplexSession>)
                    {
                        IDuplexSession session = ((ISessionChannel<IDuplexSession>) channel).Session;
                        IAsyncResult result = session.BeginCloseOutputSession(timeout, Fx.ThunkCallback(new AsyncCallback(ListenerHandler.CloseOutputSessionCallback)), state);
                        if (result.CompletedSynchronously)
                        {
                            session.EndCloseOutputSession(result);
                        }
                    }
                    else
                    {
                        IAsyncResult result2 = channel.BeginClose(timeout, Fx.ThunkCallback(new AsyncCallback(ListenerHandler.CloseChannelCallback)), state);
                        if (result2.CompletedSynchronously)
                        {
                            channel.EndClose(result2);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                this.HandleError(exception);
                if (channel is ISessionChannel<IDuplexSession>)
                {
                    channel.Abort();
                }
            }
        }

        private static void CloseChannelCallback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                CloseChannelState asyncState = (CloseChannelState) result.AsyncState;
                try
                {
                    asyncState.Channel.EndClose(result);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    asyncState.ListenerHandler.HandleError(exception);
                }
            }
        }

        private void CloseChannels(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            IChannel[] channelArray = this.channelDispatcher.Channels.ToArray();
            for (int i = 0; i < channelArray.Length; i++)
            {
                this.CloseChannel(channelArray[i], helper.RemainingTime());
            }
        }

        public void CloseInput(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            foreach (IChannel channel in this.channelDispatcher.Channels.ToArray())
            {
                if (!this.IsSessionChannel(channel))
                {
                    try
                    {
                        channel.Close(helper.RemainingTime());
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                        this.HandleError(exception);
                    }
                }
            }
        }

        private static void CloseOutputSessionCallback(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                CloseChannelState asyncState = (CloseChannelState) result.AsyncState;
                try
                {
                    ((ISessionChannel<IDuplexSession>) asyncState.Channel).Session.EndCloseOutputSession(result);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    asyncState.ListenerHandler.HandleError(exception);
                    asyncState.Channel.Abort();
                }
            }
        }

        private ListenerChannel CompleteAccept(IAsyncResult result)
        {
            IChannelBinder binder;
            if (this.acceptor.EndTryAccept(result, out binder))
            {
                if (binder != null)
                {
                    return new ListenerChannel(binder);
                }
                this.AcceptedNull();
            }
            return null;
        }

        private void Dispatch()
        {
            ListenerChannel channel = this.channel;
            ServiceChannel.SessionIdleManager idleManager = this.idleManager;
            this.channel = null;
            this.idleManager = null;
            try
            {
                if (channel != null)
                {
                    ChannelHandler handler = new ChannelHandler(this.listenerBinder.MessageVersion, channel.Binder, this.throttle, this, channel.Throttle != null, this.wrappedTransaction, idleManager);
                    if (!channel.Binder.HasSession)
                    {
                        this.channelDispatcher.Channels.Add(channel.Binder.Channel);
                    }
                    if (channel.Binder is DuplexChannelBinder)
                    {
                        DuplexChannelBinder binder = channel.Binder as DuplexChannelBinder;
                        binder.ChannelHandler = handler;
                        binder.DefaultCloseTimeout = this.DefaultCloseTimeout;
                        if (this.timeouts == null)
                        {
                            binder.DefaultSendTimeout = ServiceDefaults.SendTimeout;
                        }
                        else
                        {
                            binder.DefaultSendTimeout = this.timeouts.SendTimeout;
                        }
                    }
                    ChannelHandler.Register(handler);
                    channel = null;
                    idleManager = null;
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                this.HandleError(exception);
            }
            finally
            {
                if (channel != null)
                {
                    channel.Binder.Channel.Abort();
                    if ((this.throttle != null) && this.channelDispatcher.Session)
                    {
                        this.throttle.DeactivateChannel();
                    }
                    if (idleManager != null)
                    {
                        idleManager.CancelTimer();
                    }
                }
            }
        }

        private void DoneAccepting()
        {
            lock (this.ThisLock)
            {
                if (!this.doneAccepting)
                {
                    this.doneAccepting = true;
                    this.channelDispatcher.Channels.DecrementActivityCount();
                }
            }
        }

        private bool HandleEndAccept(IAsyncResult result)
        {
            this.channel = this.CompleteAccept(result);
            if (this.channel != null)
            {
                this.idleManager = ServiceChannel.SessionIdleManager.CreateIfNeeded(this.channel.Binder, this.channelDispatcher.DefaultCommunicationTimeouts.ReceiveTimeout);
                return this.AcquireThrottle();
            }
            this.DoneAccepting();
            return true;
        }

        private bool HandleError(Exception e)
        {
            return this.channelDispatcher.HandleError(e);
        }

        private static void InitiateChannelPump(object state)
        {
            ListenerHandler handler = state as ListenerHandler;
            if (handler.ChannelDispatcher.IsTransactedAccept)
            {
                handler.TransactedChannelPump();
            }
            else
            {
                handler.ChannelPump();
            }
        }

        private bool IsSessionChannel(IChannel channel)
        {
            return (((channel is ISessionChannel<IDuplexSession>) || (channel is ISessionChannel<IInputSession>)) || (channel is ISessionChannel<IOutputSession>));
        }

        internal void NewChannelPump()
        {
            ActionItem.Schedule(initiateChannelPump, this);
        }

        protected override void OnAbort()
        {
            this.CancelPendingIdleManager();
            this.channelDispatcher.Channels.CloseInput();
            this.AbortChannels();
            this.channelDispatcher.Channels.Abort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.CancelPendingIdleManager();
            this.channelDispatcher.Channels.CloseInput();
            this.CloseChannels(helper.RemainingTime());
            return this.channelDispatcher.Channels.BeginClose(helper.RemainingTime(), callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.CancelPendingIdleManager();
            this.channelDispatcher.Channels.CloseInput();
            this.CloseChannels(helper.RemainingTime());
            this.channelDispatcher.Channels.Close(helper.RemainingTime());
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            this.channelDispatcher.Channels.EndClose(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
        }

        protected override void OnOpened()
        {
            base.OnOpened();
            this.channelDispatcher.Channels.IncrementActivityCount();
            if ((this.channelDispatcher.IsTransactedReceive && this.channelDispatcher.ReceiveContextEnabled) && (this.channelDispatcher.MaxTransactedBatchSize > 0))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("IncompatibleBehaviors")));
            }
            this.NewChannelPump();
        }

        public void ThrottleAcquired()
        {
            this.Dispatch();
            this.NewChannelPump();
        }

        private bool TransactedAccept(out Transaction tx)
        {
            tx = null;
            try
            {
                tx = TransactionBehavior.CreateTransaction(this.ChannelDispatcher.TransactionIsolationLevel, this.ChannelDispatcher.TransactionTimeout);
                IChannelBinder channelBinder = null;
                using (TransactionScope scope = new TransactionScope(tx))
                {
                    TimeSpan timeout = TimeoutHelper.Min(this.ChannelDispatcher.TransactionTimeout, this.ChannelDispatcher.DefaultCommunicationTimeouts.ReceiveTimeout);
                    if (!this.acceptor.TryAccept(TransactionBehavior.NormalizeTimeout(timeout), out channelBinder))
                    {
                        return false;
                    }
                    scope.Complete();
                }
                if (channelBinder != null)
                {
                    this.channel = new ListenerChannel(channelBinder);
                    this.idleManager = ServiceChannel.SessionIdleManager.CreateIfNeeded(this.channel.Binder, this.channelDispatcher.DefaultCommunicationTimeouts.ReceiveTimeout);
                    return true;
                }
                this.AcceptedNull();
                tx = null;
                return false;
            }
            catch (CommunicationException exception)
            {
                if (null != tx)
                {
                    try
                    {
                        tx.Rollback();
                    }
                    catch (TransactionException exception2)
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                        }
                    }
                }
                tx = null;
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                }
                return false;
            }
            catch (TransactionException exception3)
            {
                tx = null;
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                }
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void TransactedChannelPump()
        {
            IChannelListener listener = this.listenerBinder.Listener;
        Label_000C:
            if (this.acceptedNull || (listener.State == CommunicationState.Faulted))
            {
                this.DoneAccepting();
            }
            else
            {
                Transaction transaction;
                this.acceptor.WaitForChannel();
                if (!this.TransactedAccept(out transaction) || (null == transaction))
                {
                    goto Label_000C;
                }
                this.wrappedTransaction = new WrappedTransaction(transaction);
                if (this.AcquireThrottle())
                {
                    this.Dispatch();
                    goto Label_000C;
                }
            }
        }

        internal ListenerChannel Channel
        {
            get
            {
                return this.channel;
            }
        }

        internal System.ServiceModel.Dispatcher.ChannelDispatcher ChannelDispatcher
        {
            get
            {
                return this.channelDispatcher;
            }
        }

        protected override TimeSpan DefaultCloseTimeout
        {
            get
            {
                return this.host.CloseTimeout;
            }
        }

        protected override TimeSpan DefaultOpenTimeout
        {
            get
            {
                return this.host.OpenTimeout;
            }
        }

        internal EndpointDispatcherTable Endpoints
        {
            get
            {
                return this.endpoints;
            }
            set
            {
                this.endpoints = value;
            }
        }

        internal ServiceHostBase Host
        {
            get
            {
                return this.host;
            }
        }

        internal object ThisLock
        {
            get
            {
                return base.ThisLock;
            }
        }

        private class CloseChannelState
        {
            private IChannel channel;
            private System.ServiceModel.Dispatcher.ListenerHandler listenerHandler;

            internal CloseChannelState(System.ServiceModel.Dispatcher.ListenerHandler listenerHandler, IChannel channel)
            {
                this.listenerHandler = listenerHandler;
                this.channel = channel;
            }

            internal IChannel Channel
            {
                get
                {
                    return this.channel;
                }
            }

            internal System.ServiceModel.Dispatcher.ListenerHandler ListenerHandler
            {
                get
                {
                    return this.listenerHandler;
                }
            }
        }
    }
}

