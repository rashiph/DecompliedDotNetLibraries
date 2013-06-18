namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security;

    internal abstract class ServerReliableChannelBinder<TChannel> : ReliableChannelBinder<TChannel>, IServerReliableChannelBinder, IReliableChannelBinder where TChannel: class, IChannel
    {
        private static string addressedPropertyName;
        private EndpointAddress cachedLocalAddress;
        private IChannelListener<TChannel> listener;
        private static AsyncCallback onAcceptChannelComplete;
        private TChannel pendingChannel;
        private InterruptibleWaitObject pendingChannelEvent;
        private EndpointAddress remoteAddress;

        static ServerReliableChannelBinder()
        {
            ServerReliableChannelBinder<TChannel>.addressedPropertyName = "MessageAddressedByBinderProperty";
            ServerReliableChannelBinder<TChannel>.onAcceptChannelComplete = Fx.ThunkCallback(new AsyncCallback(ServerReliableChannelBinder<TChannel>.OnAcceptChannelCompleteStatic));
        }

        protected ServerReliableChannelBinder(TChannel channel, EndpointAddress cachedLocalAddress, EndpointAddress remoteAddress, MaskingMode maskingMode, TolerateFaultsMode faultMode, TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout) : base(channel, maskingMode, faultMode, defaultCloseTimeout, defaultSendTimeout)
        {
            this.pendingChannelEvent = new InterruptibleWaitObject(false, false);
            this.cachedLocalAddress = cachedLocalAddress;
            this.remoteAddress = remoteAddress;
        }

        protected ServerReliableChannelBinder(ChannelBuilder builder, EndpointAddress remoteAddress, MessageFilter filter, int priority, MaskingMode maskingMode, TolerateFaultsMode faultMode, TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout) : base(default(TChannel), maskingMode, faultMode, defaultCloseTimeout, defaultSendTimeout)
        {
            this.pendingChannelEvent = new InterruptibleWaitObject(false, false);
            this.listener = builder.BuildChannelListener<TChannel>(filter, priority);
            this.remoteAddress = remoteAddress;
        }

        private void AddAddressedProperty(Message message)
        {
            message.Properties.Add(ServerReliableChannelBinder<TChannel>.addressedPropertyName, new object());
        }

        protected override void AddOutputHeaders(Message message)
        {
            if (this.GetAddressedProperty(message) == null)
            {
                this.RemoteAddress.ApplyTo(message);
                this.AddAddressedProperty(message);
            }
        }

        public bool AddressResponse(Message request, Message response)
        {
            if (this.GetAddressedProperty(response) != null)
            {
                throw Fx.AssertAndThrow("The binder can't address a response twice");
            }
            try
            {
                RequestReplyCorrelator.PrepareReply(response, request);
            }
            catch (MessageHeaderException exception)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                }
            }
            bool flag = true;
            try
            {
                flag = RequestReplyCorrelator.AddressReply(response, request);
            }
            catch (MessageHeaderException exception2)
            {
                if (DiagnosticUtility.ShouldTraceInformation)
                {
                    DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                }
            }
            if (flag)
            {
                this.AddAddressedProperty(response);
            }
            return flag;
        }

        protected override IAsyncResult BeginTryGetChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.pendingChannelEvent.BeginTryWait(timeout, callback, state);
        }

        public IAsyncResult BeginWaitForRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (base.DefaultMaskingMode != MaskingMode.None)
            {
                throw Fx.AssertAndThrow("This method was implemented only for the case where we do not mask exceptions.");
            }
            if (base.ValidateInputOperation(timeout))
            {
                return new WaitForRequestAsyncResult<TChannel>((ServerReliableChannelBinder<TChannel>) this, timeout, callback, state);
            }
            return new CompletedAsyncResult(callback, state);
        }

        private bool CompleteAcceptChannel(IAsyncResult result)
        {
            TChannel channel = this.listener.EndAcceptChannel(result);
            if (channel == null)
            {
                return false;
            }
            if (!this.UseNewChannel(channel))
            {
                channel.Abort();
            }
            return true;
        }

        public static IServerReliableChannelBinder CreateBinder(TChannel channel, EndpointAddress cachedLocalAddress, EndpointAddress remoteAddress, TolerateFaultsMode faultMode, TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout)
        {
            System.Type type = typeof(TChannel);
            if (type == typeof(IDuplexChannel))
            {
                return new DuplexServerReliableChannelBinder<TChannel>((IDuplexChannel) channel, cachedLocalAddress, remoteAddress, MaskingMode.All, defaultCloseTimeout, defaultSendTimeout);
            }
            if (type == typeof(IDuplexSessionChannel))
            {
                return new DuplexSessionServerReliableChannelBinder<TChannel>((IDuplexSessionChannel) channel, cachedLocalAddress, remoteAddress, MaskingMode.All, faultMode, defaultCloseTimeout, defaultSendTimeout);
            }
            if (type == typeof(IReplyChannel))
            {
                return new ReplyServerReliableChannelBinder<TChannel>((IReplyChannel) channel, cachedLocalAddress, remoteAddress, MaskingMode.All, defaultCloseTimeout, defaultSendTimeout);
            }
            if (type != typeof(IReplySessionChannel))
            {
                throw Fx.AssertAndThrow("ServerReliableChannelBinder supports creation of IDuplexChannel, IDuplexSessionChannel, IReplyChannel, and IReplySessionChannel only.");
            }
            return new ReplySessionServerReliableChannelBinder<TChannel>((IReplySessionChannel) channel, cachedLocalAddress, remoteAddress, MaskingMode.All, faultMode, defaultCloseTimeout, defaultSendTimeout);
        }

        public static IServerReliableChannelBinder CreateBinder(ChannelBuilder builder, EndpointAddress remoteAddress, MessageFilter filter, int priority, TolerateFaultsMode faultMode, TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout)
        {
            System.Type type = typeof(TChannel);
            if (type == typeof(IDuplexChannel))
            {
                return new DuplexServerReliableChannelBinder<TChannel>(builder, remoteAddress, filter, priority, MaskingMode.None, defaultCloseTimeout, defaultSendTimeout);
            }
            if (type == typeof(IDuplexSessionChannel))
            {
                return new DuplexSessionServerReliableChannelBinder<TChannel>(builder, remoteAddress, filter, priority, MaskingMode.None, faultMode, defaultCloseTimeout, defaultSendTimeout);
            }
            if (type == typeof(IReplyChannel))
            {
                return new ReplyServerReliableChannelBinder<TChannel>(builder, remoteAddress, filter, priority, MaskingMode.None, defaultCloseTimeout, defaultSendTimeout);
            }
            if (type != typeof(IReplySessionChannel))
            {
                throw Fx.AssertAndThrow("ServerReliableChannelBinder supports creation of IDuplexChannel, IDuplexSessionChannel, IReplyChannel, and IReplySessionChannel only.");
            }
            return new ReplySessionServerReliableChannelBinder<TChannel>(builder, remoteAddress, filter, priority, MaskingMode.None, faultMode, defaultCloseTimeout, defaultSendTimeout);
        }

        protected override bool EndTryGetChannel(IAsyncResult result)
        {
            if (!this.pendingChannelEvent.EndTryWait(result))
            {
                return false;
            }
            TChannel pendingChannel = default(TChannel);
            lock (base.ThisLock)
            {
                if (((base.State != CommunicationState.Faulted) && (base.State != CommunicationState.Closing)) && (base.State != CommunicationState.Closed))
                {
                    if (!base.Synchronizer.SetChannel(this.pendingChannel))
                    {
                        pendingChannel = this.pendingChannel;
                    }
                    this.pendingChannel = default(TChannel);
                    this.pendingChannelEvent.Reset();
                }
            }
            if (pendingChannel != null)
            {
                pendingChannel.Abort();
            }
            return true;
        }

        public bool EndWaitForRequest(IAsyncResult result)
        {
            WaitForRequestAsyncResult<TChannel> result2 = result as WaitForRequestAsyncResult<TChannel>;
            if (result2 != null)
            {
                return result2.End();
            }
            CompletedAsyncResult.End(result);
            return true;
        }

        private object GetAddressedProperty(Message message)
        {
            object obj2;
            message.Properties.TryGetValue(ServerReliableChannelBinder<TChannel>.addressedPropertyName, out obj2);
            return obj2;
        }

        protected abstract EndpointAddress GetInnerChannelLocalAddress();
        private bool IsListenerExceptionNullOrHandleable(Exception e)
        {
            if (e == null)
            {
                return true;
            }
            if (this.listener.State == CommunicationState.Faulted)
            {
                return false;
            }
            return base.IsHandleable(e);
        }

        protected override void OnAbort()
        {
            if (this.listener != null)
            {
                this.listener.Abort();
            }
        }

        private void OnAcceptChannelComplete(IAsyncResult result)
        {
            Exception e = null;
            Exception exception2 = null;
            bool flag = false;
            try
            {
                flag = this.CompleteAcceptChannel(result);
            }
            catch (Exception exception3)
            {
                if (Fx.IsFatal(exception3))
                {
                    throw;
                }
                if (base.IsHandleable(exception3))
                {
                    e = exception3;
                }
                else
                {
                    exception2 = exception3;
                }
            }
            if (flag)
            {
                this.StartAccepting();
            }
            else if (exception2 != null)
            {
                base.Fault(exception2);
            }
            else if ((e != null) && (this.listener.State == CommunicationState.Opened))
            {
                this.StartAccepting();
            }
            else if (this.listener.State == CommunicationState.Faulted)
            {
                base.Fault(e);
            }
        }

        private static void OnAcceptChannelCompleteStatic(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ((ServerReliableChannelBinder<TChannel>) result.AsyncState).OnAcceptChannelComplete(result);
            }
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.listener != null)
            {
                return this.listener.BeginClose(timeout, callback, state);
            }
            return new CompletedAsyncResult(callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.listener != null)
            {
                return this.listener.BeginOpen(timeout, callback, state);
            }
            return new CompletedAsyncResult(callback, state);
        }

        protected abstract IAsyncResult OnBeginWaitForRequest(TChannel channel, TimeSpan timeout, AsyncCallback callback, object state);
        protected override void OnClose(TimeSpan timeout)
        {
            if (this.listener != null)
            {
                this.listener.Close(timeout);
            }
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            if (this.listener != null)
            {
                this.listener.EndClose(result);
            }
            else
            {
                CompletedAsyncResult.End(result);
            }
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            if (this.listener != null)
            {
                this.listener.EndOpen(result);
                this.StartAccepting();
            }
            else
            {
                CompletedAsyncResult.End(result);
            }
        }

        protected abstract bool OnEndWaitForRequest(TChannel channel, IAsyncResult result);
        protected override void OnOpen(TimeSpan timeout)
        {
            if (this.listener != null)
            {
                this.listener.Open(timeout);
                this.StartAccepting();
            }
        }

        protected override void OnShutdown()
        {
            TChannel pendingChannel = default(TChannel);
            lock (base.ThisLock)
            {
                pendingChannel = this.pendingChannel;
                this.pendingChannel = default(TChannel);
                this.pendingChannelEvent.Set();
            }
            if (pendingChannel != null)
            {
                pendingChannel.Abort();
            }
        }

        protected abstract bool OnWaitForRequest(TChannel channel, TimeSpan timeout);
        private void StartAccepting()
        {
            Exception e = null;
            Exception exception2 = null;
            while (this.listener.State == CommunicationState.Opened)
            {
                e = null;
                exception2 = null;
                try
                {
                    IAsyncResult result = this.listener.BeginAcceptChannel(TimeSpan.MaxValue, ServerReliableChannelBinder<TChannel>.onAcceptChannelComplete, this);
                    if (!result.CompletedSynchronously)
                    {
                        return;
                    }
                    if (!this.CompleteAcceptChannel(result))
                    {
                        break;
                    }
                    continue;
                }
                catch (Exception exception3)
                {
                    if (Fx.IsFatal(exception3))
                    {
                        throw;
                    }
                    if (base.IsHandleable(exception3))
                    {
                        e = exception3;
                        continue;
                    }
                    exception2 = exception3;
                    break;
                }
            }
            if (exception2 != null)
            {
                base.Fault(exception2);
            }
            else if (this.listener.State == CommunicationState.Faulted)
            {
                base.Fault(e);
            }
        }

        protected override bool TryGetChannel(TimeSpan timeout)
        {
            if (!this.pendingChannelEvent.Wait(timeout))
            {
                return false;
            }
            TChannel pendingChannel = default(TChannel);
            lock (base.ThisLock)
            {
                if (((base.State != CommunicationState.Faulted) && (base.State != CommunicationState.Closing)) && (base.State != CommunicationState.Closed))
                {
                    if (!base.Synchronizer.SetChannel(this.pendingChannel))
                    {
                        pendingChannel = this.pendingChannel;
                    }
                    this.pendingChannel = default(TChannel);
                    this.pendingChannelEvent.Reset();
                }
            }
            if (pendingChannel != null)
            {
                pendingChannel.Abort();
            }
            return true;
        }

        public bool UseNewChannel(IChannel channel)
        {
            TChannel pendingChannel = default(TChannel);
            TChannel local2 = default(TChannel);
            lock (base.ThisLock)
            {
                if ((!base.Synchronizer.TolerateFaults || (base.State == CommunicationState.Faulted)) || ((base.State == CommunicationState.Closing) || (base.State == CommunicationState.Closed)))
                {
                    return false;
                }
                pendingChannel = this.pendingChannel;
                this.pendingChannel = (TChannel) channel;
                local2 = base.Synchronizer.AbortCurentChannel();
            }
            if (pendingChannel != null)
            {
                pendingChannel.Abort();
            }
            this.pendingChannelEvent.Set();
            if (local2 != null)
            {
                local2.Abort();
            }
            return true;
        }

        public bool WaitForRequest(TimeSpan timeout)
        {
            bool aborting;
            bool flag3;
            if (base.DefaultMaskingMode != MaskingMode.None)
            {
                throw Fx.AssertAndThrow("This method was implemented only for the case where we do not mask exceptions.");
            }
            if (!base.ValidateInputOperation(timeout))
            {
                return true;
            }
            TimeoutHelper helper = new TimeoutHelper(timeout);
        Label_0026:
            aborting = false;
            try
            {
                TChannel local;
                bool flag2 = !base.Synchronizer.TryGetChannelForInput(true, helper.RemainingTime(), out local);
                if (local == null)
                {
                    flag3 = flag2;
                }
                else
                {
                    try
                    {
                        flag3 = this.OnWaitForRequest(local, helper.RemainingTime());
                    }
                    finally
                    {
                        aborting = base.Synchronizer.Aborting;
                        base.Synchronizer.ReturnChannel();
                    }
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                if (!base.HandleException(exception, base.DefaultMaskingMode, aborting))
                {
                    throw;
                }
                goto Label_0026;
            }
            return flag3;
        }

        protected override bool CanGetChannelForReceive
        {
            get
            {
                return true;
            }
        }

        public override EndpointAddress LocalAddress
        {
            get
            {
                if (this.cachedLocalAddress != null)
                {
                    return this.cachedLocalAddress;
                }
                return this.GetInnerChannelLocalAddress();
            }
        }

        protected override bool MustCloseChannel
        {
            get
            {
                if (!this.MustOpenChannel)
                {
                    return this.HasSession;
                }
                return true;
            }
        }

        protected override bool MustOpenChannel
        {
            get
            {
                return (this.listener != null);
            }
        }

        public override EndpointAddress RemoteAddress
        {
            get
            {
                return this.remoteAddress;
            }
        }

        private sealed class DuplexServerReliableChannelBinder : ServerReliableChannelBinder<TChannel>.DuplexServerReliableChannelBinder<IDuplexChannel>
        {
            public DuplexServerReliableChannelBinder(IDuplexChannel channel, EndpointAddress cachedLocalAddress, EndpointAddress remoteAddress, MaskingMode maskingMode, TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout) : base(channel, cachedLocalAddress, remoteAddress, maskingMode, TolerateFaultsMode.Never, defaultCloseTimeout, defaultSendTimeout)
            {
            }

            public DuplexServerReliableChannelBinder(ChannelBuilder builder, EndpointAddress remoteAddress, MessageFilter filter, int priority, MaskingMode maskingMode, TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout) : base(builder, remoteAddress, filter, priority, maskingMode, TolerateFaultsMode.Never, defaultCloseTimeout, defaultSendTimeout)
            {
            }

            public override ISession GetInnerSession()
            {
                return null;
            }

            protected override bool HasSecuritySession(IDuplexChannel channel)
            {
                return false;
            }

            protected override void OnMessageReceived(Message message)
            {
            }

            public override bool HasSession
            {
                get
                {
                    return false;
                }
            }
        }

        private abstract class DuplexServerReliableChannelBinder<TDuplexChannel> : ServerReliableChannelBinder<TDuplexChannel> where TDuplexChannel: class, IDuplexChannel
        {
            protected DuplexServerReliableChannelBinder(TDuplexChannel channel, EndpointAddress cachedLocalAddress, EndpointAddress remoteAddress, MaskingMode maskingMode, TolerateFaultsMode faultMode, TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout) : base(channel, cachedLocalAddress, remoteAddress, maskingMode, faultMode, defaultCloseTimeout, defaultSendTimeout)
            {
            }

            protected DuplexServerReliableChannelBinder(ChannelBuilder builder, EndpointAddress remoteAddress, MessageFilter filter, int priority, MaskingMode maskingMode, TolerateFaultsMode faultMode, TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout) : base(builder, remoteAddress, filter, priority, maskingMode, faultMode, defaultCloseTimeout, defaultSendTimeout)
            {
            }

            protected override EndpointAddress GetInnerChannelLocalAddress()
            {
                IDuplexChannel currentChannel = base.Synchronizer.CurrentChannel;
                return ((currentChannel == null) ? null : currentChannel.LocalAddress);
            }

            protected override IAsyncResult OnBeginSend(TDuplexChannel channel, Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return channel.BeginSend(message, timeout, callback, state);
            }

            protected override IAsyncResult OnBeginTryReceive(TDuplexChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return channel.BeginTryReceive(timeout, callback, state);
            }

            protected override IAsyncResult OnBeginWaitForRequest(TDuplexChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return channel.BeginWaitForMessage(timeout, callback, state);
            }

            protected override void OnEndSend(TDuplexChannel channel, IAsyncResult result)
            {
                channel.EndSend(result);
            }

            protected override bool OnEndTryReceive(TDuplexChannel channel, IAsyncResult result, out RequestContext requestContext)
            {
                Message message;
                bool flag = channel.EndTryReceive(result, out message);
                if (flag)
                {
                    this.OnMessageReceived(message);
                }
                requestContext = base.WrapMessage(message);
                return flag;
            }

            protected override bool OnEndWaitForRequest(TDuplexChannel channel, IAsyncResult result)
            {
                return channel.EndWaitForMessage(result);
            }

            protected abstract void OnMessageReceived(Message message);
            protected override void OnSend(TDuplexChannel channel, Message message, TimeSpan timeout)
            {
                channel.Send(message, timeout);
            }

            protected override bool OnTryReceive(TDuplexChannel channel, TimeSpan timeout, out RequestContext requestContext)
            {
                Message message;
                bool flag = channel.TryReceive(timeout, out message);
                if (flag)
                {
                    this.OnMessageReceived(message);
                }
                requestContext = base.WrapMessage(message);
                return flag;
            }

            protected override bool OnWaitForRequest(TDuplexChannel channel, TimeSpan timeout)
            {
                return channel.WaitForMessage(timeout);
            }

            public override bool CanSendAsynchronously
            {
                get
                {
                    return true;
                }
            }
        }

        private sealed class DuplexSessionServerReliableChannelBinder : ServerReliableChannelBinder<TChannel>.DuplexServerReliableChannelBinder<IDuplexSessionChannel>
        {
            public DuplexSessionServerReliableChannelBinder(IDuplexSessionChannel channel, EndpointAddress cachedLocalAddress, EndpointAddress remoteAddress, MaskingMode maskingMode, TolerateFaultsMode faultMode, TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout) : base(channel, cachedLocalAddress, remoteAddress, maskingMode, faultMode, defaultCloseTimeout, defaultSendTimeout)
            {
            }

            public DuplexSessionServerReliableChannelBinder(ChannelBuilder builder, EndpointAddress remoteAddress, MessageFilter filter, int priority, MaskingMode maskingMode, TolerateFaultsMode faultMode, TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout) : base(builder, remoteAddress, filter, priority, maskingMode, faultMode, defaultCloseTimeout, defaultSendTimeout)
            {
            }

            protected override IAsyncResult BeginCloseChannel(IDuplexSessionChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return ReliableChannelBinderHelper.BeginCloseDuplexSessionChannel(this, channel, timeout, callback, state);
            }

            protected override void CloseChannel(IDuplexSessionChannel channel, TimeSpan timeout)
            {
                ReliableChannelBinderHelper.CloseDuplexSessionChannel(this, channel, timeout);
            }

            protected override void EndCloseChannel(IDuplexSessionChannel channel, IAsyncResult result)
            {
                ReliableChannelBinderHelper.EndCloseDuplexSessionChannel(channel, result);
            }

            public override ISession GetInnerSession()
            {
                return base.Synchronizer.CurrentChannel.Session;
            }

            protected override bool HasSecuritySession(IDuplexSessionChannel channel)
            {
                return (channel.Session is ISecuritySession);
            }

            protected override void OnMessageReceived(Message message)
            {
                if (message == null)
                {
                    base.Synchronizer.OnReadEof();
                }
            }

            public override bool HasSession
            {
                get
                {
                    return true;
                }
            }
        }

        private sealed class ReplyServerReliableChannelBinder : ServerReliableChannelBinder<TChannel>.ReplyServerReliableChannelBinder<IReplyChannel>
        {
            public ReplyServerReliableChannelBinder(IReplyChannel channel, EndpointAddress cachedLocalAddress, EndpointAddress remoteAddress, MaskingMode maskingMode, TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout) : base(channel, cachedLocalAddress, remoteAddress, maskingMode, TolerateFaultsMode.Never, defaultCloseTimeout, defaultSendTimeout)
            {
            }

            public ReplyServerReliableChannelBinder(ChannelBuilder builder, EndpointAddress remoteAddress, MessageFilter filter, int priority, MaskingMode maskingMode, TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout) : base(builder, remoteAddress, filter, priority, maskingMode, TolerateFaultsMode.Never, defaultCloseTimeout, defaultSendTimeout)
            {
            }

            public override ISession GetInnerSession()
            {
                return null;
            }

            protected override bool HasSecuritySession(IReplyChannel channel)
            {
                return false;
            }

            public override bool HasSession
            {
                get
                {
                    return false;
                }
            }
        }

        private abstract class ReplyServerReliableChannelBinder<TReplyChannel> : ServerReliableChannelBinder<TReplyChannel> where TReplyChannel: class, IReplyChannel
        {
            public ReplyServerReliableChannelBinder(TReplyChannel channel, EndpointAddress cachedLocalAddress, EndpointAddress remoteAddress, MaskingMode maskingMode, TolerateFaultsMode faultMode, TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout) : base(channel, cachedLocalAddress, remoteAddress, maskingMode, faultMode, defaultCloseTimeout, defaultSendTimeout)
            {
            }

            public ReplyServerReliableChannelBinder(ChannelBuilder builder, EndpointAddress remoteAddress, MessageFilter filter, int priority, MaskingMode maskingMode, TolerateFaultsMode faultMode, TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout) : base(builder, remoteAddress, filter, priority, maskingMode, faultMode, defaultCloseTimeout, defaultSendTimeout)
            {
            }

            protected override EndpointAddress GetInnerChannelLocalAddress()
            {
                IReplyChannel currentChannel = base.Synchronizer.CurrentChannel;
                return ((currentChannel == null) ? null : currentChannel.LocalAddress);
            }

            protected override IAsyncResult OnBeginTryReceive(TReplyChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return channel.BeginTryReceiveRequest(timeout, callback, state);
            }

            protected override IAsyncResult OnBeginWaitForRequest(TReplyChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return channel.BeginWaitForRequest(timeout, callback, state);
            }

            protected override bool OnEndTryReceive(TReplyChannel channel, IAsyncResult result, out RequestContext requestContext)
            {
                bool flag = channel.EndTryReceiveRequest(result, out requestContext);
                if (flag && (requestContext == null))
                {
                    this.OnReadNullMessage();
                }
                requestContext = base.WrapRequestContext(requestContext);
                return flag;
            }

            protected override bool OnEndWaitForRequest(TReplyChannel channel, IAsyncResult result)
            {
                return channel.EndWaitForRequest(result);
            }

            protected virtual void OnReadNullMessage()
            {
            }

            protected override bool OnTryReceive(TReplyChannel channel, TimeSpan timeout, out RequestContext requestContext)
            {
                bool flag = channel.TryReceiveRequest(timeout, out requestContext);
                if (flag && (requestContext == null))
                {
                    this.OnReadNullMessage();
                }
                requestContext = base.WrapRequestContext(requestContext);
                return flag;
            }

            protected override bool OnWaitForRequest(TReplyChannel channel, TimeSpan timeout)
            {
                return channel.WaitForRequest(timeout);
            }

            public override bool CanSendAsynchronously
            {
                get
                {
                    return false;
                }
            }
        }

        private sealed class ReplySessionServerReliableChannelBinder : ServerReliableChannelBinder<TChannel>.ReplyServerReliableChannelBinder<IReplySessionChannel>
        {
            public ReplySessionServerReliableChannelBinder(IReplySessionChannel channel, EndpointAddress cachedLocalAddress, EndpointAddress remoteAddress, MaskingMode maskingMode, TolerateFaultsMode faultMode, TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout) : base(channel, cachedLocalAddress, remoteAddress, maskingMode, faultMode, defaultCloseTimeout, defaultSendTimeout)
            {
            }

            public ReplySessionServerReliableChannelBinder(ChannelBuilder builder, EndpointAddress remoteAddress, MessageFilter filter, int priority, MaskingMode maskingMode, TolerateFaultsMode faultMode, TimeSpan defaultCloseTimeout, TimeSpan defaultSendTimeout) : base(builder, remoteAddress, filter, priority, maskingMode, faultMode, defaultCloseTimeout, defaultSendTimeout)
            {
            }

            protected override IAsyncResult BeginCloseChannel(IReplySessionChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return ReliableChannelBinderHelper.BeginCloseReplySessionChannel(this, channel, timeout, callback, state);
            }

            protected override void CloseChannel(IReplySessionChannel channel, TimeSpan timeout)
            {
                ReliableChannelBinderHelper.CloseReplySessionChannel(this, channel, timeout);
            }

            protected override void EndCloseChannel(IReplySessionChannel channel, IAsyncResult result)
            {
                ReliableChannelBinderHelper.EndCloseReplySessionChannel(channel, result);
            }

            public override ISession GetInnerSession()
            {
                return base.Synchronizer.CurrentChannel.Session;
            }

            protected override bool HasSecuritySession(IReplySessionChannel channel)
            {
                return (channel.Session is ISecuritySession);
            }

            protected override void OnReadNullMessage()
            {
                base.Synchronizer.OnReadEof();
            }

            public override bool HasSession
            {
                get
                {
                    return true;
                }
            }
        }

        private sealed class WaitForRequestAsyncResult : ReliableChannelBinder<TChannel>.InputAsyncResult<ServerReliableChannelBinder<TChannel>>
        {
            public WaitForRequestAsyncResult(ServerReliableChannelBinder<TChannel> binder, TimeSpan timeout, AsyncCallback callback, object state) : base(binder, true, timeout, binder.DefaultMaskingMode, callback, state)
            {
                if (base.Start())
                {
                    base.Complete(true);
                }
            }

            protected override IAsyncResult BeginInput(ServerReliableChannelBinder<TChannel> binder, TChannel channel, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return binder.OnBeginWaitForRequest(channel, timeout, callback, state);
            }

            protected override bool EndInput(ServerReliableChannelBinder<TChannel> binder, TChannel channel, IAsyncResult result, out bool complete)
            {
                complete = true;
                return binder.OnEndWaitForRequest(channel, result);
            }
        }
    }
}

