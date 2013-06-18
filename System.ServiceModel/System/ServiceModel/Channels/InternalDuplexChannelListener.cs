namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal sealed class InternalDuplexChannelListener : DelegatingChannelListener<IDuplexChannel>
    {
        private IChannelFactory<IOutputChannel> innerChannelFactory;
        private bool providesCorrelation;

        internal InternalDuplexChannelListener(InternalDuplexBindingElement bindingElement, BindingContext context) : base(context.Binding, context.Clone().BuildInnerChannelListener<IInputChannel>())
        {
            this.innerChannelFactory = context.BuildInnerChannelFactory<IOutputChannel>();
            this.providesCorrelation = bindingElement.ProvidesCorrelation;
        }

        private IOutputChannel GetOutputChannel(Uri to, TimeoutHelper timeoutHelper)
        {
            IOutputChannel channel = this.innerChannelFactory.CreateChannel(new EndpointAddress(to, new AddressHeader[0]));
            channel.Open(timeoutHelper.RemainingTime());
            return channel;
        }

        public override T GetProperty<T>() where T: class
        {
            if (typeof(T) == typeof(IChannelFactory))
            {
                return (T) this.innerChannelFactory;
            }
            if ((typeof(T) == typeof(ISecurityCapabilities)) && !this.providesCorrelation)
            {
                return InternalDuplexBindingElement.GetSecurityCapabilities<T>(base.GetProperty<ISecurityCapabilities>());
            }
            T property = base.GetProperty<T>();
            if (property != null)
            {
                return property;
            }
            return this.innerChannelFactory.GetProperty<T>();
        }

        protected override void OnAbort()
        {
            try
            {
                this.innerChannelFactory.Abort();
            }
            finally
            {
                base.OnAbort();
            }
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedCloseAsyncResult(timeout, callback, state, new ChainedBeginHandler(this.OnBeginClose), new ChainedEndHandler(this.OnEndClose), new ICommunicationObject[] { this.innerChannelFactory });
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedOpenAsyncResult(timeout, callback, state, new ChainedBeginHandler(this.OnBeginOpen), new ChainedEndHandler(this.OnEndOpen), new ICommunicationObject[] { this.innerChannelFactory });
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            base.OnClose(helper.RemainingTime());
            this.innerChannelFactory.Close(helper.RemainingTime());
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
            this.innerChannelFactory.Open(helper.RemainingTime());
        }

        protected override void OnOpening()
        {
            base.OnOpening();
            base.Acceptor = new CompositeDuplexChannelAcceptor(this, (IChannelListener<IInputChannel>) this.InnerChannelListener);
        }

        private sealed class CompositeDuplexChannelAcceptor : LayeredChannelAcceptor<IDuplexChannel, IInputChannel>
        {
            public CompositeDuplexChannelAcceptor(InternalDuplexChannelListener listener, IChannelListener<IInputChannel> innerListener) : base(listener, innerListener)
            {
            }

            protected override IDuplexChannel OnAcceptChannel(IInputChannel innerChannel)
            {
                return new InternalDuplexChannelListener.ServerCompositeDuplexChannel((InternalDuplexChannelListener) base.ChannelManager, innerChannel);
            }
        }

        private sealed class ServerCompositeDuplexChannel : ChannelBase, IDuplexChannel, IInputChannel, IOutputChannel, IChannel, ICommunicationObject
        {
            private IInputChannel innerInputChannel;
            private TimeSpan sendTimeout;

            public ServerCompositeDuplexChannel(InternalDuplexChannelListener listener, IInputChannel innerInputChannel) : base(listener)
            {
                this.innerInputChannel = innerInputChannel;
                this.sendTimeout = listener.DefaultSendTimeout;
            }

            public IAsyncResult BeginReceive(AsyncCallback callback, object state)
            {
                return this.BeginReceive(base.DefaultReceiveTimeout, callback, state);
            }

            public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return InputChannel.HelpBeginReceive(this, timeout, callback, state);
            }

            public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
            {
                return this.BeginSend(message, base.DefaultSendTimeout, callback, state);
            }

            public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new SendAsyncResult(this, message, timeout, callback, state);
            }

            public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.innerInputChannel.BeginTryReceive(timeout, callback, state);
            }

            public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.innerInputChannel.BeginWaitForMessage(timeout, callback, state);
            }

            public Message EndReceive(IAsyncResult result)
            {
                return InputChannel.HelpEndReceive(result);
            }

            public void EndSend(IAsyncResult result)
            {
                SendAsyncResult.End(result);
            }

            public bool EndTryReceive(IAsyncResult result, out Message message)
            {
                return this.innerInputChannel.EndTryReceive(result, out message);
            }

            public bool EndWaitForMessage(IAsyncResult result)
            {
                return this.innerInputChannel.EndWaitForMessage(result);
            }

            protected override void OnAbort()
            {
                this.innerInputChannel.Abort();
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.innerInputChannel.BeginClose(timeout, callback, state);
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.innerInputChannel.BeginOpen(callback, state);
            }

            protected override void OnClose(TimeSpan timeout)
            {
                if (this.innerInputChannel.State == CommunicationState.Opened)
                {
                    this.innerInputChannel.Close(timeout);
                }
            }

            protected override void OnEndClose(IAsyncResult result)
            {
                this.innerInputChannel.EndClose(result);
            }

            protected override void OnEndOpen(IAsyncResult result)
            {
                this.innerInputChannel.EndOpen(result);
            }

            protected override void OnOpen(TimeSpan timeout)
            {
                this.innerInputChannel.Open(timeout);
            }

            public Message Receive()
            {
                return this.Receive(base.DefaultReceiveTimeout);
            }

            public Message Receive(TimeSpan timeout)
            {
                return InputChannel.HelpReceive(this, timeout);
            }

            public void Send(Message message)
            {
                this.Send(message, base.DefaultSendTimeout);
            }

            public void Send(Message message, TimeSpan timeout)
            {
                TimeoutHelper timeoutHelper = new TimeoutHelper(timeout);
                IOutputChannel channel = this.ValidateStateAndGetOutputChannel(message, timeoutHelper);
                try
                {
                    channel.Send(message, timeoutHelper.RemainingTime());
                    channel.Close(timeoutHelper.RemainingTime());
                }
                finally
                {
                    channel.Abort();
                }
            }

            public bool TryReceive(TimeSpan timeout, out Message message)
            {
                return this.innerInputChannel.TryReceive(timeout, out message);
            }

            private IOutputChannel ValidateStateAndGetOutputChannel(Message message, TimeoutHelper timeoutHelper)
            {
                base.ThrowIfDisposedOrNotOpen();
                if (message == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
                }
                Uri via = message.Properties.Via;
                if (via == null)
                {
                    via = message.Headers.To;
                    if (via == null)
                    {
                        throw TraceUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("MessageMustHaveViaOrToSetForSendingOnServerSideCompositeDuplexChannels")), message);
                    }
                    if (via.Equals(EndpointAddress.AnonymousUri) || via.Equals(message.Version.Addressing.AnonymousUri))
                    {
                        throw TraceUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("MessageToCannotBeAddressedToAnonymousOnServerSideCompositeDuplexChannels", new object[] { via })), message);
                    }
                }
                else if (via.Equals(EndpointAddress.AnonymousUri) || via.Equals(message.Version.Addressing.AnonymousUri))
                {
                    throw TraceUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("MessageViaCannotBeAddressedToAnonymousOnServerSideCompositeDuplexChannels", new object[] { via })), message);
                }
                return this.Listener.GetOutputChannel(via, timeoutHelper);
            }

            public bool WaitForMessage(TimeSpan timeout)
            {
                return this.innerInputChannel.WaitForMessage(timeout);
            }

            private InternalDuplexChannelListener Listener
            {
                get
                {
                    return (InternalDuplexChannelListener) base.Manager;
                }
            }

            public EndpointAddress LocalAddress
            {
                get
                {
                    return this.innerInputChannel.LocalAddress;
                }
            }

            public EndpointAddress RemoteAddress
            {
                get
                {
                    return null;
                }
            }

            public Uri Via
            {
                get
                {
                    return null;
                }
            }

            private class SendAsyncResult : AsyncResult
            {
                private IOutputChannel outputChannel;
                private static AsyncCallback sendCompleteCallback = Fx.ThunkCallback(new AsyncCallback(InternalDuplexChannelListener.ServerCompositeDuplexChannel.SendAsyncResult.SendCompleteCallback));
                private TimeoutHelper timeoutHelper;

                public SendAsyncResult(InternalDuplexChannelListener.ServerCompositeDuplexChannel outer, Message message, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.outputChannel = outer.ValidateStateAndGetOutputChannel(message, this.timeoutHelper);
                    bool flag = false;
                    try
                    {
                        IAsyncResult result = this.outputChannel.BeginSend(message, this.timeoutHelper.RemainingTime(), sendCompleteCallback, this);
                        if (result.CompletedSynchronously)
                        {
                            this.CompleteSend(result);
                            base.Complete(true);
                        }
                        flag = true;
                    }
                    finally
                    {
                        if (!flag)
                        {
                            this.outputChannel.Abort();
                        }
                    }
                }

                private void CompleteSend(IAsyncResult result)
                {
                    try
                    {
                        this.outputChannel.EndSend(result);
                        this.outputChannel.Close();
                    }
                    finally
                    {
                        this.outputChannel.Abort();
                    }
                }

                internal static void End(IAsyncResult result)
                {
                    AsyncResult.End<InternalDuplexChannelListener.ServerCompositeDuplexChannel.SendAsyncResult>(result);
                }

                private static void SendCompleteCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        InternalDuplexChannelListener.ServerCompositeDuplexChannel.SendAsyncResult asyncState = (InternalDuplexChannelListener.ServerCompositeDuplexChannel.SendAsyncResult) result.AsyncState;
                        Exception exception = null;
                        try
                        {
                            asyncState.CompleteSend(result);
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            exception = exception2;
                        }
                        asyncState.Complete(false, exception);
                    }
                }
            }
        }
    }
}

