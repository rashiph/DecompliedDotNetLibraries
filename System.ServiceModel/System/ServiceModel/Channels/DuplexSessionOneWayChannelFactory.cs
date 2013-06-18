namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;

    internal class DuplexSessionOneWayChannelFactory : LayeredChannelFactory<IOutputChannel>
    {
        private ChannelPool<IDuplexSessionChannel> channelPool;
        private ChannelPoolSettings channelPoolSettings;
        private bool packetRoutable;

        public DuplexSessionOneWayChannelFactory(OneWayBindingElement bindingElement, BindingContext context) : base(context.Binding, context.BuildInnerChannelFactory<IDuplexSessionChannel>())
        {
            this.packetRoutable = bindingElement.PacketRoutable;
            ISecurityCapabilities property = base.InnerChannelFactory.GetProperty<ISecurityCapabilities>();
            if ((property != null) && property.SupportsClientAuthentication)
            {
                this.channelPoolSettings = bindingElement.ChannelPoolSettings.Clone();
            }
            else
            {
                this.channelPool = new ChannelPool<IDuplexSessionChannel>(bindingElement.ChannelPoolSettings);
            }
        }

        internal ChannelPool<IDuplexSessionChannel> GetChannelPool(out bool cleanupChannelPool)
        {
            if (this.channelPool != null)
            {
                cleanupChannelPool = false;
                return this.channelPool;
            }
            cleanupChannelPool = true;
            return new ChannelPool<IDuplexSessionChannel>(this.channelPoolSettings);
        }

        protected override void OnAbort()
        {
            if (this.channelPool != null)
            {
                this.channelPool.Close(TimeSpan.Zero);
            }
            base.OnAbort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (this.channelPool != null)
            {
                this.channelPool.Close(helper.RemainingTime());
            }
            return base.OnBeginClose(helper.RemainingTime(), callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (this.channelPool != null)
            {
                this.channelPool.Close(helper.RemainingTime());
            }
            base.OnClose(helper.RemainingTime());
        }

        protected override IOutputChannel OnCreateChannel(EndpointAddress address, Uri via)
        {
            return new DuplexSessionOutputChannel(this, address, via);
        }

        private class DuplexSessionOutputChannel : OutputChannel
        {
            private ChannelPool<IDuplexSessionChannel> channelPool;
            private bool cleanupChannelPool;
            private IChannelFactory<IDuplexSessionChannel> innerFactory;
            private AsyncCallback onReceive;
            private bool packetRoutable;
            private EndpointAddress remoteAddress;
            private Uri via;

            public DuplexSessionOutputChannel(DuplexSessionOneWayChannelFactory factory, EndpointAddress remoteAddress, Uri via) : base(factory)
            {
                this.channelPool = factory.GetChannelPool(out this.cleanupChannelPool);
                this.packetRoutable = factory.packetRoutable;
                this.innerFactory = (IChannelFactory<IDuplexSessionChannel>) factory.InnerChannelFactory;
                this.remoteAddress = remoteAddress;
                this.via = via;
            }

            private void CleanupChannel(IDuplexSessionChannel channel, bool connectionStillGood, ChannelPoolKey key, bool isConnectionFromPool, ref TimeoutHelper timeoutHelper)
            {
                if (isConnectionFromPool)
                {
                    this.channelPool.ReturnConnection(key, channel, connectionStillGood, timeoutHelper.RemainingTime());
                }
                else if (connectionStillGood)
                {
                    this.channelPool.AddConnection(key, channel, timeoutHelper.RemainingTime());
                }
                else
                {
                    channel.Abort();
                }
            }

            private IDuplexSessionChannel GetChannelFromPool(ref TimeoutHelper timeoutHelper, out ChannelPoolKey key, out bool isConnectionFromPool)
            {
                isConnectionFromPool = true;
                while (true)
                {
                    IDuplexSessionChannel connection = this.channelPool.TakeConnection(this.RemoteAddress, this.Via, timeoutHelper.RemainingTime(), out key);
                    if (connection == null)
                    {
                        isConnectionFromPool = false;
                        return this.innerFactory.CreateChannel(this.RemoteAddress, this.Via);
                    }
                    if (connection.State == CommunicationState.Opened)
                    {
                        return connection;
                    }
                    this.channelPool.ReturnConnection(key, connection, false, timeoutHelper.RemainingTime());
                }
            }

            protected override void OnAbort()
            {
                if (this.cleanupChannelPool)
                {
                    this.channelPool.Close(TimeSpan.Zero);
                }
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                if (this.cleanupChannelPool)
                {
                    this.channelPool.Close(timeout);
                }
                return new CompletedAsyncResult(callback, state);
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new CompletedAsyncResult(callback, state);
            }

            protected override IAsyncResult OnBeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new SendAsyncResult(this, message, timeout, callback, state);
            }

            protected override void OnClose(TimeSpan timeout)
            {
                if (this.cleanupChannelPool)
                {
                    this.channelPool.Close(timeout);
                }
            }

            protected override void OnEndClose(IAsyncResult result)
            {
                CompletedAsyncResult.End(result);
            }

            protected override void OnEndOpen(IAsyncResult result)
            {
                CompletedAsyncResult.End(result);
            }

            protected override void OnEndSend(IAsyncResult result)
            {
                SendAsyncResult.End(result);
            }

            protected override void OnOpen(TimeSpan timeout)
            {
            }

            private void OnReceive(IAsyncResult result)
            {
                IDuplexSessionChannel asyncState = (IDuplexSessionChannel) result.AsyncState;
                bool flag = false;
                try
                {
                    Message message = asyncState.EndReceive(result);
                    if (message == null)
                    {
                        asyncState.Close(this.channelPool.IdleTimeout);
                        flag = true;
                    }
                    else
                    {
                        message.Close();
                    }
                }
                catch (CommunicationException exception)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Information);
                    }
                }
                catch (TimeoutException exception2)
                {
                    if (DiagnosticUtility.ShouldTraceInformation)
                    {
                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Information);
                    }
                }
                finally
                {
                    if (!flag)
                    {
                        asyncState.Abort();
                    }
                }
            }

            protected override void OnSend(Message message, TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                ChannelPoolKey key = null;
                bool isConnectionFromPool = true;
                IDuplexSessionChannel channel = this.GetChannelFromPool(ref timeoutHelper, out key, out isConnectionFromPool);
                bool flag2 = false;
                try
                {
                    if (!isConnectionFromPool)
                    {
                        this.StampInitialMessage(message);
                        channel.Open(timeoutHelper.RemainingTime());
                        this.StartBackgroundReceive(channel);
                    }
                    channel.Send(message, timeoutHelper.RemainingTime());
                    flag2 = true;
                }
                finally
                {
                    if (!flag2)
                    {
                        this.CleanupChannel(channel, false, key, isConnectionFromPool, ref timeoutHelper);
                    }
                }
                this.CleanupChannel(channel, true, key, isConnectionFromPool, ref timeoutHelper);
            }

            private void StampInitialMessage(Message message)
            {
                if (this.packetRoutable)
                {
                    PacketRoutableHeader.AddHeadersTo(message, null);
                }
            }

            private void StartBackgroundReceive(IDuplexSessionChannel channel)
            {
                if (this.onReceive == null)
                {
                    this.onReceive = Fx.ThunkCallback(new AsyncCallback(this.OnReceive));
                }
                channel.BeginReceive(TimeSpan.MaxValue, this.onReceive, channel);
            }

            public override EndpointAddress RemoteAddress
            {
                get
                {
                    return this.remoteAddress;
                }
            }

            public override Uri Via
            {
                get
                {
                    return this.via;
                }
            }

            private class SendAsyncResult : AsyncResult
            {
                private IDuplexSessionChannel innerChannel;
                private bool isConnectionFromPool;
                private ChannelPoolKey key;
                private Message message;
                private static AsyncCallback onInnerSend = Fx.ThunkCallback(new AsyncCallback(DuplexSessionOneWayChannelFactory.DuplexSessionOutputChannel.SendAsyncResult.OnInnerSend));
                private static AsyncCallback onOpen;
                private DuplexSessionOneWayChannelFactory.DuplexSessionOutputChannel parent;
                private TimeoutHelper timeoutHelper;

                public SendAsyncResult(DuplexSessionOneWayChannelFactory.DuplexSessionOutputChannel parent, Message message, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.parent = parent;
                    this.message = message;
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.innerChannel = parent.GetChannelFromPool(ref this.timeoutHelper, out this.key, out this.isConnectionFromPool);
                    bool flag = false;
                    bool flag2 = true;
                    try
                    {
                        if (!this.isConnectionFromPool)
                        {
                            flag2 = this.OpenNewChannel();
                        }
                        if (flag2)
                        {
                            flag2 = this.SendMessage();
                        }
                        flag = true;
                    }
                    finally
                    {
                        if (!flag)
                        {
                            this.Cleanup(false);
                        }
                    }
                    if (flag2)
                    {
                        this.Cleanup(true);
                        base.Complete(true);
                    }
                }

                private void Cleanup(bool connectionStillGood)
                {
                    this.parent.CleanupChannel(this.innerChannel, connectionStillGood, this.key, this.isConnectionFromPool, ref this.timeoutHelper);
                }

                private void CompleteOpen(IAsyncResult result)
                {
                    this.innerChannel.EndOpen(result);
                    this.parent.StartBackgroundReceive(this.innerChannel);
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<DuplexSessionOneWayChannelFactory.DuplexSessionOutputChannel.SendAsyncResult>(result);
                }

                private static void OnInnerSend(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        DuplexSessionOneWayChannelFactory.DuplexSessionOutputChannel.SendAsyncResult asyncState = (DuplexSessionOneWayChannelFactory.DuplexSessionOutputChannel.SendAsyncResult) result.AsyncState;
                        Exception exception = null;
                        try
                        {
                            asyncState.innerChannel.EndSend(result);
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            exception = exception2;
                        }
                        asyncState.Cleanup(exception == null);
                        asyncState.Complete(false, exception);
                    }
                }

                private static void OnOpen(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        DuplexSessionOneWayChannelFactory.DuplexSessionOutputChannel.SendAsyncResult asyncState = (DuplexSessionOneWayChannelFactory.DuplexSessionOutputChannel.SendAsyncResult) result.AsyncState;
                        Exception exception = null;
                        bool flag = false;
                        try
                        {
                            asyncState.CompleteOpen(result);
                            flag = asyncState.SendMessage();
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
                            asyncState.Cleanup(exception == null);
                            asyncState.Complete(false, exception);
                        }
                    }
                }

                private bool OpenNewChannel()
                {
                    if (onOpen == null)
                    {
                        onOpen = Fx.ThunkCallback(new AsyncCallback(DuplexSessionOneWayChannelFactory.DuplexSessionOutputChannel.SendAsyncResult.OnOpen));
                    }
                    this.parent.StampInitialMessage(this.message);
                    IAsyncResult result = this.innerChannel.BeginOpen(this.timeoutHelper.RemainingTime(), onOpen, this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                    this.CompleteOpen(result);
                    return true;
                }

                private bool SendMessage()
                {
                    IAsyncResult result = this.innerChannel.BeginSend(this.message, onInnerSend, this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                    this.innerChannel.EndSend(result);
                    return true;
                }
            }
        }
    }
}

