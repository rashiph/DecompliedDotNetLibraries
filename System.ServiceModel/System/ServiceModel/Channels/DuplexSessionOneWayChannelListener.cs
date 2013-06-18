namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal class DuplexSessionOneWayChannelListener : DelegatingChannelListener<IInputChannel>
    {
        private object acceptLock;
        private bool acceptPending;
        private int activeChannels;
        private Action<object> handleAcceptCallback;
        private TimeSpan idleTimeout;
        private IChannelListener<IDuplexSessionChannel> innerChannelListener;
        private DuplexSessionOneWayInputChannelAcceptor inputChannelAcceptor;
        private int maxAcceptedChannels;
        private static AsyncCallback onAcceptInnerChannel = Fx.ThunkCallback(new AsyncCallback(DuplexSessionOneWayChannelListener.OnAcceptInnerChannel));
        private Action onExceptionDequeued;
        private EventHandler onInnerChannelClosed;
        private AsyncCallback onOpenInnerChannel;
        private bool ownsInnerListener;
        private bool packetRoutable;

        public DuplexSessionOneWayChannelListener(OneWayBindingElement bindingElement, BindingContext context) : base(true, context.Binding, context.BuildInnerChannelListener<IDuplexSessionChannel>())
        {
            this.acceptLock = new object();
            this.inputChannelAcceptor = new DuplexSessionOneWayInputChannelAcceptor(this);
            this.packetRoutable = bindingElement.PacketRoutable;
            this.maxAcceptedChannels = bindingElement.MaxAcceptedChannels;
            base.Acceptor = this.inputChannelAcceptor;
            this.idleTimeout = bindingElement.ChannelPoolSettings.IdleTimeout;
            this.onOpenInnerChannel = Fx.ThunkCallback(new AsyncCallback(this.OnOpenInnerChannel));
            this.ownsInnerListener = true;
            this.onInnerChannelClosed = new EventHandler(this.OnInnerChannelClosed);
        }

        private void AcceptLoop(IAsyncResult pendingResult)
        {
            IDuplexSessionChannel channel = null;
            if (pendingResult != null)
            {
                if (!this.ProcessEndAccept(pendingResult, out channel))
                {
                    return;
                }
                pendingResult = null;
            }
            lock (this.acceptLock)
            {
                while (this.IsAcceptNecessary)
                {
                    Exception exception = null;
                    try
                    {
                        IAsyncResult result = null;
                        try
                        {
                            result = this.innerChannelListener.BeginAcceptChannel(TimeSpan.MaxValue, onAcceptInnerChannel, this);
                        }
                        catch (CommunicationException exception2)
                        {
                            if (DiagnosticUtility.ShouldTraceInformation)
                            {
                                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                            }
                            continue;
                        }
                        this.acceptPending = true;
                        if (!result.CompletedSynchronously)
                        {
                            break;
                        }
                        if (this.handleAcceptCallback == null)
                        {
                            this.handleAcceptCallback = new Action<object>(this.HandleAcceptCallback);
                        }
                        if (channel != null)
                        {
                            ActionItem.Schedule(this.handleAcceptCallback, channel);
                            channel = null;
                        }
                        IDuplexSessionChannel channel2 = null;
                        if (this.ProcessEndAccept(result, out channel2))
                        {
                            if (channel2 != null)
                            {
                                ActionItem.Schedule(this.handleAcceptCallback, channel2);
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                    catch (Exception exception3)
                    {
                        if (Fx.IsFatal(exception3))
                        {
                            throw;
                        }
                        exception = exception3;
                    }
                    if (exception != null)
                    {
                        this.inputChannelAcceptor.Enqueue(exception, null, false);
                    }
                }
            }
            if (channel != null)
            {
                this.HandleAcceptComplete(channel);
            }
        }

        private void AcceptLoop(object state)
        {
            this.AcceptLoop((IAsyncResult) null);
        }

        private void CompleteOpen(IDuplexSessionChannel channel, IAsyncResult result)
        {
            Exception exception = null;
            bool flag = false;
            try
            {
                channel.EndOpen(result);
                flag = true;
            }
            catch (CommunicationException exception2)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                }
            }
            catch (TimeoutException exception3)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                }
            }
            catch (Exception exception4)
            {
                if (Fx.IsFatal(exception4))
                {
                    throw;
                }
                exception = exception4;
            }
            finally
            {
                if (!flag)
                {
                    channel.Abort();
                }
            }
            if (flag)
            {
                this.inputChannelAcceptor.AcceptInnerChannel(this, channel);
            }
            else if (exception != null)
            {
                this.inputChannelAcceptor.Enqueue(exception, null);
            }
        }

        private void HandleAcceptCallback(object state)
        {
            this.HandleAcceptComplete((IDuplexSessionChannel) state);
        }

        private void HandleAcceptComplete(IDuplexSessionChannel channel)
        {
            Exception exception = null;
            bool flag = false;
            this.inputChannelAcceptor.PrepareChannel(channel);
            IAsyncResult result = null;
            try
            {
                result = channel.BeginOpen(this.idleTimeout, this.onOpenInnerChannel, channel);
                flag = true;
            }
            catch (CommunicationException exception2)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                }
            }
            catch (TimeoutException exception3)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                }
            }
            catch (Exception exception4)
            {
                if (Fx.IsFatal(exception4))
                {
                    throw;
                }
                exception = exception4;
            }
            finally
            {
                if (!flag && (channel != null))
                {
                    channel.Abort();
                }
            }
            if (flag)
            {
                if (result.CompletedSynchronously)
                {
                    this.CompleteOpen(channel, result);
                }
            }
            else if (exception != null)
            {
                this.inputChannelAcceptor.Enqueue(exception, null);
            }
        }

        protected override void OnAbort()
        {
            base.OnAbort();
            if (this.ownsInnerListener && (this.innerChannelListener != null))
            {
                this.innerChannelListener.Abort();
            }
        }

        private static void OnAcceptInnerChannel(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ((DuplexSessionOneWayChannelListener) result.AsyncState).AcceptLoop(result);
            }
        }

        private void OnExceptionDequeued()
        {
            lock (this.acceptLock)
            {
                this.acceptPending = false;
            }
            this.AcceptLoop((IAsyncResult) null);
        }

        private void OnInnerChannelClosed(object sender, EventArgs e)
        {
            IDuplexSessionChannel channel = (IDuplexSessionChannel) sender;
            channel.Closed -= this.onInnerChannelClosed;
            lock (this.acceptLock)
            {
                this.activeChannels--;
            }
            this.AcceptLoop((IAsyncResult) null);
        }

        protected override void OnOpened()
        {
            base.OnOpened();
            ActionItem.Schedule(new Action<object>(this.AcceptLoop), null);
        }

        protected override void OnOpening()
        {
            this.innerChannelListener = (IChannelListener<IDuplexSessionChannel>) this.InnerChannelListener;
            this.inputChannelAcceptor.TransferInnerChannelListener(this.innerChannelListener);
            this.ownsInnerListener = false;
            base.OnOpening();
        }

        private void OnOpenInnerChannel(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                IDuplexSessionChannel asyncState = (IDuplexSessionChannel) result.AsyncState;
                this.CompleteOpen(asyncState, result);
            }
        }

        private bool ProcessEndAccept(IAsyncResult result, out IDuplexSessionChannel channel)
        {
            channel = null;
            Exception exception = null;
            bool flag = false;
            try
            {
                channel = this.innerChannelListener.EndAcceptChannel(result);
                flag = true;
            }
            catch (CommunicationException exception2)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                }
            }
            catch (Exception exception3)
            {
                if (Fx.IsFatal(exception3))
                {
                    throw;
                }
                exception = exception3;
            }
            if (flag)
            {
                if (channel == null)
                {
                    this.inputChannelAcceptor.Close();
                    return false;
                }
                channel.Closed += this.onInnerChannelClosed;
                bool flag2 = false;
                lock (this.acceptLock)
                {
                    this.acceptPending = false;
                    this.activeChannels++;
                    if (this.activeChannels >= this.maxAcceptedChannels)
                    {
                        flag2 = true;
                    }
                }
                if (DiagnosticUtility.ShouldTraceWarning && flag2)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, 0x40025, System.ServiceModel.SR.GetString("TraceCodeMaxAcceptedChannelsReached"), new StringTraceRecord("MaxAcceptedChannels", this.maxAcceptedChannels.ToString(CultureInfo.InvariantCulture)), this, null);
                }
            }
            else if (exception != null)
            {
                bool canDispatchOnThisThread = this.innerChannelListener.State != CommunicationState.Opened;
                if (this.onExceptionDequeued == null)
                {
                    this.onExceptionDequeued = new Action(this.OnExceptionDequeued);
                }
                this.inputChannelAcceptor.Enqueue(exception, this.onExceptionDequeued, canDispatchOnThisThread);
            }
            else
            {
                lock (this.acceptLock)
                {
                    this.acceptPending = false;
                }
            }
            return true;
        }

        private bool IsAcceptNecessary
        {
            get
            {
                return ((!this.acceptPending && (this.activeChannels < this.maxAcceptedChannels)) && (this.innerChannelListener.State == CommunicationState.Opened));
            }
        }

        private class ChannelReceiver
        {
            private DuplexSessionOneWayChannelListener.DuplexSessionOneWayInputChannelAcceptor acceptor;
            private IDuplexSessionChannel channel;
            private TimeSpan idleTimeout;
            private Action<object> onDispatchItemsLater;
            private Action onMessageDequeued;
            private static AsyncCallback onReceive = Fx.ThunkCallback(new AsyncCallback(DuplexSessionOneWayChannelListener.ChannelReceiver.OnReceive));
            private Action<object> onStartReceiveLater;
            private static Action<object> startReceivingCallback;
            private bool validateHeader;

            public ChannelReceiver(DuplexSessionOneWayChannelListener parent, IDuplexSessionChannel channel)
            {
                this.channel = channel;
                this.acceptor = parent.inputChannelAcceptor;
                this.idleTimeout = parent.idleTimeout;
                this.validateHeader = parent.packetRoutable;
                this.onMessageDequeued = new Action(this.OnMessageDequeued);
            }

            private void Dispatch()
            {
                this.acceptor.DispatchItems();
            }

            private bool EnqueueMessage(Message message)
            {
                if (this.validateHeader)
                {
                    if (!PacketRoutableHeader.TryValidateMessage(message))
                    {
                        this.channel.Abort();
                        message.Close();
                        return false;
                    }
                    this.validateHeader = false;
                }
                return this.acceptor.EnqueueWithoutDispatch(message, this.onMessageDequeued);
            }

            private bool OnCompleteReceive(IAsyncResult result, out bool dispatchLater)
            {
                Exception exception = null;
                Message message = null;
                bool flag = false;
                dispatchLater = false;
                try
                {
                    if (!this.channel.EndTryReceive(result, out message))
                    {
                        this.channel.Abort();
                    }
                    else if (message == null)
                    {
                        this.channel.Close();
                    }
                }
                catch (CommunicationException exception2)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                    }
                    flag = this.channel.State == CommunicationState.Opened;
                }
                catch (TimeoutException exception3)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Information);
                    }
                    flag = this.channel.State == CommunicationState.Opened;
                }
                catch (Exception exception4)
                {
                    if (Fx.IsFatal(exception4))
                    {
                        throw;
                    }
                    exception = exception4;
                }
                if (message != null)
                {
                    dispatchLater = this.EnqueueMessage(message);
                    return flag;
                }
                if (exception != null)
                {
                    dispatchLater = this.acceptor.EnqueueWithoutDispatch(exception, this.onMessageDequeued);
                }
                return flag;
            }

            private void OnDispatchItemsLater(object state)
            {
                this.Dispatch();
            }

            private void OnMessageDequeued()
            {
                IAsyncResult result = null;
                Exception exception = null;
                try
                {
                    result = this.channel.BeginTryReceive(this.idleTimeout, onReceive, this);
                }
                catch (CommunicationException exception2)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                    }
                }
                catch (Exception exception3)
                {
                    if (Fx.IsFatal(exception3))
                    {
                        throw;
                    }
                    exception = exception3;
                }
                if (result != null)
                {
                    if (result.CompletedSynchronously)
                    {
                        bool flag;
                        if (this.OnCompleteReceive(result, out flag))
                        {
                            if (this.onStartReceiveLater == null)
                            {
                                this.onStartReceiveLater = new Action<object>(this.OnStartReceiveLater);
                            }
                            ActionItem.Schedule(this.onStartReceiveLater, null);
                        }
                        if (flag)
                        {
                            if (this.onDispatchItemsLater == null)
                            {
                                this.onDispatchItemsLater = new Action<object>(this.OnDispatchItemsLater);
                            }
                            ActionItem.Schedule(this.onDispatchItemsLater, null);
                        }
                    }
                }
                else if (exception != null)
                {
                    this.acceptor.Enqueue(exception, this.onMessageDequeued, false);
                }
                else if (this.channel.State == CommunicationState.Opened)
                {
                    if (startReceivingCallback == null)
                    {
                        startReceivingCallback = new Action<object>(this.StartReceivingCallback);
                    }
                    ActionItem.Schedule(startReceivingCallback, this);
                }
            }

            private static void OnReceive(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    bool flag;
                    DuplexSessionOneWayChannelListener.ChannelReceiver asyncState = (DuplexSessionOneWayChannelListener.ChannelReceiver) result.AsyncState;
                    if (asyncState.OnCompleteReceive(result, out flag))
                    {
                        asyncState.StartReceiving();
                    }
                    if (flag)
                    {
                        asyncState.Dispatch();
                    }
                }
            }

            private void OnStartReceiveLater(object state)
            {
                this.StartReceiving();
            }

            public void StartReceiving()
            {
                Exception exception = null;
            Label_0002:
                if (this.channel.State != CommunicationState.Opened)
                {
                    this.channel.Abort();
                }
                else
                {
                    IAsyncResult result = null;
                    try
                    {
                        result = this.channel.BeginTryReceive(this.idleTimeout, onReceive, this);
                    }
                    catch (CommunicationException exception2)
                    {
                        if (DiagnosticUtility.ShouldTraceInformation)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                        }
                    }
                    catch (Exception exception3)
                    {
                        if (Fx.IsFatal(exception3))
                        {
                            throw;
                        }
                        exception = exception3;
                        goto Label_0082;
                    }
                    if (result == null)
                    {
                        goto Label_0002;
                    }
                    if (result.CompletedSynchronously)
                    {
                        bool flag;
                        bool flag2 = this.OnCompleteReceive(result, out flag);
                        if (flag)
                        {
                            this.Dispatch();
                        }
                        if (flag2)
                        {
                            goto Label_0002;
                        }
                    }
                }
            Label_0082:
                if (exception != null)
                {
                    this.acceptor.Enqueue(exception, this.onMessageDequeued);
                }
            }

            private void StartReceivingCallback(object state)
            {
                ((DuplexSessionOneWayChannelListener.ChannelReceiver) state).StartReceiving();
            }
        }

        private class DuplexSessionOneWayInputChannelAcceptor : InputChannelAcceptor
        {
            private IChannelListener<IDuplexSessionChannel> innerChannelListener;
            private ChannelTracker<IDuplexSessionChannel, DuplexSessionOneWayChannelListener.ChannelReceiver> receivers;

            public DuplexSessionOneWayInputChannelAcceptor(DuplexSessionOneWayChannelListener listener) : base(listener)
            {
                this.receivers = new ChannelTracker<IDuplexSessionChannel, DuplexSessionOneWayChannelListener.ChannelReceiver>();
            }

            public void AcceptInnerChannel(DuplexSessionOneWayChannelListener listener, IDuplexSessionChannel channel)
            {
                DuplexSessionOneWayChannelListener.ChannelReceiver channelReceiver = new DuplexSessionOneWayChannelListener.ChannelReceiver(listener, channel);
                this.receivers.Add(channel, channelReceiver);
                channelReceiver.StartReceiving();
            }

            protected override void OnAbort()
            {
                base.OnAbort();
                if (!this.TransferReceivers())
                {
                    this.receivers.Abort();
                    if (this.innerChannelListener != null)
                    {
                        this.innerChannelListener.Abort();
                    }
                }
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                List<ICommunicationObject> collection = new List<ICommunicationObject>();
                if (!this.TransferReceivers())
                {
                    collection.Add(this.receivers);
                    collection.Add(this.innerChannelListener);
                }
                return new ChainedCloseAsyncResult(timeout, callback, state, new ChainedBeginHandler(this.OnBeginClose), new ChainedEndHandler(this.OnEndClose), collection);
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new ChainedOpenAsyncResult(timeout, callback, state, new ChainedBeginHandler(this.OnBeginOpen), new ChainedEndHandler(this.OnEndOpen), new ICommunicationObject[] { this.receivers, this.innerChannelListener });
            }

            protected override void OnClose(TimeSpan timeout)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                base.OnClose(helper.RemainingTime());
                if (!this.TransferReceivers())
                {
                    this.receivers.Close(helper.RemainingTime());
                    this.innerChannelListener.Close(helper.RemainingTime());
                }
            }

            protected override InputChannel OnCreateChannel()
            {
                return new DuplexSessionOneWayInputChannel(base.ChannelManager, null);
            }

            protected override void OnEndClose(IAsyncResult result)
            {
                ChainedAsyncResult.End(result);
            }

            protected override void OnEndOpen(IAsyncResult result)
            {
                ChainedAsyncResult.End(result);
            }

            protected override void OnOpen(TimeSpan timeout)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                base.OnOpen(helper.RemainingTime());
                this.receivers.Open(helper.RemainingTime());
                this.innerChannelListener.Open(helper.RemainingTime());
            }

            public void PrepareChannel(IDuplexSessionChannel channel)
            {
                this.receivers.PrepareChannel(channel);
            }

            public void TransferInnerChannelListener(IChannelListener<IDuplexSessionChannel> innerChannelListener)
            {
                bool flag = false;
                lock (base.ThisLock)
                {
                    this.innerChannelListener = innerChannelListener;
                    if ((base.State == CommunicationState.Closing) || (base.State == CommunicationState.Closed))
                    {
                        flag = true;
                    }
                }
                if (flag)
                {
                    innerChannelListener.Abort();
                }
            }

            private bool TransferReceivers()
            {
                DuplexSessionOneWayInputChannel currentChannel = (DuplexSessionOneWayInputChannel) base.GetCurrentChannel();
                if (currentChannel == null)
                {
                    return false;
                }
                return currentChannel.TransferReceivers(this.receivers, this.innerChannelListener);
            }

            private class DuplexSessionOneWayInputChannel : InputChannel
            {
                private IChannelListener<IDuplexSessionChannel> innerChannelListener;
                private ChannelTracker<IDuplexSessionChannel, DuplexSessionOneWayChannelListener.ChannelReceiver> receivers;

                public DuplexSessionOneWayInputChannel(ChannelManagerBase channelManager, EndpointAddress localAddress) : base(channelManager, localAddress)
                {
                }

                protected override void OnAbort()
                {
                    if (this.receivers != null)
                    {
                        this.receivers.Abort();
                        this.innerChannelListener.Abort();
                    }
                    base.OnAbort();
                }

                protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
                {
                    List<ICommunicationObject> collection = new List<ICommunicationObject>();
                    if (this.receivers != null)
                    {
                        collection.Add(this.receivers);
                        collection.Add(this.innerChannelListener);
                    }
                    return new ChainedCloseAsyncResult(timeout, callback, state, new ChainedBeginHandler(this.OnBeginClose), new ChainedEndHandler(this.OnEndClose), collection);
                }

                protected override void OnClose(TimeSpan timeout)
                {
                    TimeoutHelper helper = new TimeoutHelper(timeout);
                    if (this.receivers != null)
                    {
                        this.receivers.Close(helper.RemainingTime());
                        this.innerChannelListener.Close(helper.RemainingTime());
                    }
                    base.OnClose(helper.RemainingTime());
                }

                protected override void OnEndClose(IAsyncResult result)
                {
                    ChainedAsyncResult.End(result);
                }

                public bool TransferReceivers(ChannelTracker<IDuplexSessionChannel, DuplexSessionOneWayChannelListener.ChannelReceiver> receivers, IChannelListener<IDuplexSessionChannel> innerChannelListener)
                {
                    lock (base.ThisLock)
                    {
                        if (base.State != CommunicationState.Opened)
                        {
                            return false;
                        }
                        this.receivers = receivers;
                        this.innerChannelListener = innerChannelListener;
                        return true;
                    }
                }
            }
        }
    }
}

