namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.Threading;
    using System.Xml;

    internal abstract class ReliableChannelListener<TChannel, TReliableChannel, TInnerChannel> : ReliableChannelListenerBase<TChannel> where TChannel: class, IChannel where TReliableChannel: class, IChannel where TInnerChannel: class, IChannel
    {
        private Dictionary<UniqueId, TReliableChannel> channelsByInput;
        private Dictionary<UniqueId, TReliableChannel> channelsByOutput;
        private InputQueueChannelAcceptor<TChannel> inputQueueChannelAcceptor;
        private static AsyncCallback onAcceptCompleted;
        private IChannelListener<TInnerChannel> typedListener;

        static ReliableChannelListener()
        {
            ReliableChannelListener<TChannel, TReliableChannel, TInnerChannel>.onAcceptCompleted = Fx.ThunkCallback(new AsyncCallback(ReliableChannelListener<TChannel, TReliableChannel, TInnerChannel>.OnAcceptCompletedStatic));
        }

        protected ReliableChannelListener(ReliableSessionBindingElement binding, BindingContext context) : base(binding, context.Binding)
        {
            this.typedListener = context.BuildInnerChannelListener<TInnerChannel>();
            this.inputQueueChannelAcceptor = new InputQueueChannelAcceptor<TChannel>(this);
            base.Acceptor = this.inputQueueChannelAcceptor;
        }

        private IServerReliableChannelBinder CreateBinder(TInnerChannel channel, EndpointAddress localAddress, EndpointAddress remoteAddress)
        {
            return ServerReliableChannelBinder<TInnerChannel>.CreateBinder(channel, localAddress, remoteAddress, TolerateFaultsMode.IfNotSecuritySession, this.DefaultCloseTimeout, this.DefaultSendTimeout);
        }

        protected abstract TReliableChannel CreateChannel(UniqueId id, CreateSequenceInfo createSequenceInfo, IServerReliableChannelBinder binder);
        protected void Dispatch()
        {
            this.inputQueueChannelAcceptor.Dispatch();
        }

        protected bool EnqueueWithoutDispatch(TChannel channel)
        {
            return this.inputQueueChannelAcceptor.EnqueueWithoutDispatch(channel, null);
        }

        protected TReliableChannel GetChannel(WsrmMessageInfo info, out UniqueId id)
        {
            id = WsrmUtilities.GetInputId(info);
            lock (base.ThisLock)
            {
                TReliableChannel local = default(TReliableChannel);
                if (((id == null) || !this.channelsByInput.TryGetValue(id, out local)) && this.Duplex)
                {
                    UniqueId outputId = WsrmUtilities.GetOutputId(base.ReliableMessagingVersion, info);
                    if (outputId != null)
                    {
                        id = outputId;
                        this.channelsByOutput.TryGetValue(id, out local);
                    }
                }
                return local;
            }
        }

        private void HandleAcceptComplete(TInnerChannel channel)
        {
            if (channel != null)
            {
                try
                {
                    this.OnInnerChannelAccepted(channel);
                    channel.Open();
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    if (DiagnosticUtility.ShouldTraceError)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Error);
                    }
                    channel.Abort();
                    return;
                }
                this.ProcessChannel(channel);
            }
        }

        protected bool HandleException(Exception e, ICommunicationObject o)
        {
            if (((e is CommunicationException) || (e is TimeoutException)) && (o.State == CommunicationState.Opened))
            {
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(e, TraceEventType.Warning);
                }
                return true;
            }
            if (DiagnosticUtility.ShouldTraceError)
            {
                DiagnosticUtility.ExceptionUtility.TraceHandledException(e, TraceEventType.Error);
            }
            return false;
        }

        protected override bool HasChannels()
        {
            return ((this.channelsByInput != null) && (this.channelsByInput.Count > 0));
        }

        private bool IsExpectedException(Exception e)
        {
            if (e is ProtocolException)
            {
                return false;
            }
            return (e is CommunicationException);
        }

        protected override bool IsLastChannel(UniqueId inputId)
        {
            if (this.channelsByInput.Count != 1)
            {
                return false;
            }
            return this.channelsByInput.ContainsKey(inputId);
        }

        private void OnAcceptCompleted(IAsyncResult result)
        {
            TInnerChannel channel = default(TInnerChannel);
            Exception exception = null;
            Exception exception2 = null;
            try
            {
                channel = this.typedListener.EndAcceptChannel(result);
            }
            catch (Exception exception3)
            {
                if (Fx.IsFatal(exception3))
                {
                    throw;
                }
                if (this.IsExpectedException(exception3))
                {
                    exception = exception3;
                }
                else
                {
                    exception2 = exception3;
                }
            }
            if (channel != null)
            {
                this.HandleAcceptComplete(channel);
                this.StartAccepting();
            }
            else if (exception2 != null)
            {
                base.Fault(exception2);
            }
            else if ((exception != null) && (this.typedListener.State == CommunicationState.Opened))
            {
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                }
                this.StartAccepting();
            }
            else if (this.typedListener.State == CommunicationState.Faulted)
            {
                base.Fault(exception);
            }
        }

        private static void OnAcceptCompletedStatic(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ReliableChannelListener<TChannel, TReliableChannel, TInnerChannel> asyncState = (ReliableChannelListener<TChannel, TReliableChannel, TInnerChannel>) result.AsyncState;
                try
                {
                    asyncState.OnAcceptCompleted(result);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    asyncState.Fault(exception);
                }
            }
        }

        protected override void OnFaulted()
        {
            this.typedListener.Abort();
            this.inputQueueChannelAcceptor.FaultQueue();
            base.OnFaulted();
        }

        protected virtual void OnInnerChannelAccepted(TInnerChannel channel)
        {
        }

        protected override void OnOpened()
        {
            base.OnOpened();
            this.channelsByInput = new Dictionary<UniqueId, TReliableChannel>();
            if (this.Duplex)
            {
                this.channelsByOutput = new Dictionary<UniqueId, TReliableChannel>();
            }
            if (Thread.CurrentThread.IsThreadPoolThread)
            {
                try
                {
                    this.StartAccepting();
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    base.Fault(exception);
                }
            }
            else
            {
                ActionItem.Schedule(new Action<object>(ReliableChannelListener<TChannel, TReliableChannel, TInnerChannel>.StartAccepting), this);
            }
        }

        protected abstract void ProcessChannel(TInnerChannel channel);
        protected TReliableChannel ProcessCreateSequence(WsrmMessageInfo info, TInnerChannel channel, out bool dispatch, out bool newChannel)
        {
            EndpointAddress address;
            dispatch = false;
            newChannel = false;
            CreateSequenceInfo createSequenceInfo = info.CreateSequenceInfo;
            if (!WsrmUtilities.ValidateCreateSequence<TChannel>(info, this, channel, out address))
            {
                return default(TReliableChannel);
            }
            lock (base.ThisLock)
            {
                TReliableChannel local = default(TReliableChannel);
                if (((createSequenceInfo.OfferIdentifier == null) || !this.Duplex) || !this.channelsByOutput.TryGetValue(createSequenceInfo.OfferIdentifier, out local))
                {
                    if (!base.IsAccepting)
                    {
                        info.FaultReply = WsrmUtilities.CreateEndpointNotFoundFault(base.MessageVersion, System.ServiceModel.SR.GetString("RMEndpointNotFoundReason", new object[] { this.Uri }));
                        return default(TReliableChannel);
                    }
                    if (this.inputQueueChannelAcceptor.PendingCount >= base.MaxPendingChannels)
                    {
                        info.FaultReply = WsrmUtilities.CreateCSRefusedServerTooBusyFault(base.MessageVersion, base.ReliableMessagingVersion, System.ServiceModel.SR.GetString("ServerTooBusy", new object[] { this.Uri }));
                        return default(TReliableChannel);
                    }
                    UniqueId id = WsrmUtilities.NextSequenceId();
                    local = this.CreateChannel(id, createSequenceInfo, this.CreateBinder(channel, address, createSequenceInfo.ReplyTo));
                    this.channelsByInput.Add(id, local);
                    if (this.Duplex)
                    {
                        this.channelsByOutput.Add(createSequenceInfo.OfferIdentifier, local);
                    }
                    dispatch = this.EnqueueWithoutDispatch((TChannel) local);
                    newChannel = true;
                }
                return local;
            }
        }

        protected override void RemoveChannel(UniqueId inputId, UniqueId outputId)
        {
            this.channelsByInput.Remove(inputId);
            if (this.Duplex)
            {
                this.channelsByOutput.Remove(outputId);
            }
        }

        private void StartAccepting()
        {
            Exception exception = null;
            Exception exception2 = null;
            while (this.typedListener.State == CommunicationState.Opened)
            {
                TInnerChannel channel = default(TInnerChannel);
                exception = null;
                exception2 = null;
                try
                {
                    IAsyncResult result = this.typedListener.BeginAcceptChannel(TimeSpan.MaxValue, ReliableChannelListener<TChannel, TReliableChannel, TInnerChannel>.onAcceptCompleted, this);
                    if (!result.CompletedSynchronously)
                    {
                        return;
                    }
                    channel = this.typedListener.EndAcceptChannel(result);
                    if (channel == null)
                    {
                        break;
                    }
                }
                catch (Exception exception3)
                {
                    if (Fx.IsFatal(exception3))
                    {
                        throw;
                    }
                    if (this.IsExpectedException(exception3))
                    {
                        if (DiagnosticUtility.ShouldTraceWarning)
                        {
                            DiagnosticUtility.ExceptionUtility.TraceHandledException(exception3, TraceEventType.Warning);
                        }
                        exception = exception3;
                        continue;
                    }
                    exception2 = exception3;
                    break;
                }
                this.HandleAcceptComplete(channel);
            }
            if (exception2 != null)
            {
                base.Fault(exception2);
            }
            else if (this.typedListener.State == CommunicationState.Faulted)
            {
                base.Fault(exception);
            }
        }

        private static void StartAccepting(object state)
        {
            ReliableChannelListener<TChannel, TReliableChannel, TInnerChannel> listener = (ReliableChannelListener<TChannel, TReliableChannel, TInnerChannel>) state;
            try
            {
                listener.StartAccepting();
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                listener.Fault(exception);
            }
        }

        internal override IChannelListener InnerChannelListener
        {
            get
            {
                return this.typedListener;
            }
            set
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException());
            }
        }
    }
}

