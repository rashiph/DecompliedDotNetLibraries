namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.ServiceModel.Security;

    internal sealed class SecurityChannelFactory<TChannel> : LayeredChannelFactory<TChannel>
    {
        private System.ServiceModel.Channels.ChannelBuilder channelBuilder;
        private System.ServiceModel.Channels.MessageVersion messageVersion;
        private ISecurityCapabilities securityCapabilities;
        private System.ServiceModel.Security.SecurityProtocolFactory securityProtocolFactory;
        private SecuritySessionClientSettings<TChannel> sessionClientSettings;
        private bool sessionMode;

        internal SecurityChannelFactory(Binding binding, System.ServiceModel.Security.SecurityProtocolFactory protocolFactory, IChannelFactory innerChannelFactory) : base(binding, innerChannelFactory)
        {
            this.securityProtocolFactory = protocolFactory;
        }

        public SecurityChannelFactory(ISecurityCapabilities securityCapabilities, BindingContext context, SecuritySessionClientSettings<TChannel> sessionClientSettings) : this(securityCapabilities, context, sessionClientSettings.ChannelBuilder, sessionClientSettings.CreateInnerChannelFactory())
        {
            this.sessionMode = true;
            this.sessionClientSettings = sessionClientSettings;
        }

        private SecurityChannelFactory(ISecurityCapabilities securityCapabilities, BindingContext context, System.ServiceModel.Channels.ChannelBuilder channelBuilder, IChannelFactory innerChannelFactory) : base(context.Binding, innerChannelFactory)
        {
            this.channelBuilder = channelBuilder;
            this.messageVersion = context.Binding.MessageVersion;
            this.securityCapabilities = securityCapabilities;
        }

        public SecurityChannelFactory(ISecurityCapabilities securityCapabilities, BindingContext context, System.ServiceModel.Channels.ChannelBuilder channelBuilder, System.ServiceModel.Security.SecurityProtocolFactory protocolFactory) : this(securityCapabilities, context, channelBuilder, protocolFactory, channelBuilder.BuildChannelFactory<TChannel>())
        {
        }

        public SecurityChannelFactory(ISecurityCapabilities securityCapabilities, BindingContext context, System.ServiceModel.Channels.ChannelBuilder channelBuilder, System.ServiceModel.Security.SecurityProtocolFactory protocolFactory, IChannelFactory innerChannelFactory) : this(securityCapabilities, context, channelBuilder, innerChannelFactory)
        {
            this.securityProtocolFactory = protocolFactory;
        }

        private void CloseProtocolFactory(bool aborted, TimeSpan timeout)
        {
            if ((this.securityProtocolFactory != null) && !this.SessionMode)
            {
                this.securityProtocolFactory.Close(aborted, timeout);
                this.securityProtocolFactory = null;
            }
        }

        public override T GetProperty<T>() where T: class
        {
            if (this.SessionMode && (typeof(T) == typeof(IChannelSecureConversationSessionSettings)))
            {
                return (T) this.SessionClientSettings;
            }
            if (typeof(T) == typeof(ISecurityCapabilities))
            {
                return (T) this.securityCapabilities;
            }
            return base.GetProperty<T>();
        }

        protected override void OnAbort()
        {
            base.OnAbort();
            this.CloseProtocolFactory(true, TimeSpan.Zero);
            if (this.sessionClientSettings != null)
            {
                this.sessionClientSettings.Abort();
            }
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            List<OperationWithTimeoutBeginCallback> list = new List<OperationWithTimeoutBeginCallback>();
            List<OperationEndCallback> list2 = new List<OperationEndCallback> {
                new OperationWithTimeoutBeginCallback(this.OnBeginClose),
                new OperationEndCallback(this.OnEndClose)
            };
            if ((this.securityProtocolFactory != null) && !this.SessionMode)
            {
                list.Add(new OperationWithTimeoutBeginCallback(this.securityProtocolFactory.BeginClose));
                list2.Add(new OperationEndCallback(this.securityProtocolFactory.EndClose));
            }
            if (this.sessionClientSettings != null)
            {
                list.Add(new OperationWithTimeoutBeginCallback(this.sessionClientSettings.BeginClose));
                list2.Add(new OperationEndCallback(this.sessionClientSettings.EndClose));
            }
            return OperationWithTimeoutComposer.BeginComposeAsyncOperations(timeout, list.ToArray(), list2.ToArray(), callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OperationWithTimeoutAsyncResult(new OperationWithTimeoutCallback(this.OnOpen), timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            base.OnClose(timeout);
            this.CloseProtocolFactory(false, helper.RemainingTime());
            if (this.sessionClientSettings != null)
            {
                this.sessionClientSettings.Close(helper.RemainingTime());
            }
        }

        protected override TChannel OnCreateChannel(EndpointAddress address, Uri via)
        {
            base.ThrowIfDisposed();
            if (this.SessionMode)
            {
                return this.sessionClientSettings.OnCreateChannel(address, via);
            }
            if (typeof(TChannel) == typeof(IOutputChannel))
            {
                return (TChannel) new SecurityOutputChannel<TChannel>(this, this.securityProtocolFactory, ((IChannelFactory<IOutputChannel>) base.InnerChannelFactory).CreateChannel(address, via), address, via);
            }
            if (typeof(TChannel) == typeof(IOutputSessionChannel))
            {
                return (TChannel) new SecurityOutputSessionChannel<TChannel>(this, this.securityProtocolFactory, ((IChannelFactory<IOutputSessionChannel>) base.InnerChannelFactory).CreateChannel(address, via), address, via);
            }
            if (typeof(TChannel) == typeof(IDuplexChannel))
            {
                return (TChannel) new SecurityDuplexChannel<TChannel>(this, this.securityProtocolFactory, ((IChannelFactory<IDuplexChannel>) base.InnerChannelFactory).CreateChannel(address, via), address, via);
            }
            if (typeof(TChannel) == typeof(IDuplexSessionChannel))
            {
                return (TChannel) new SecurityDuplexSessionChannel<TChannel>(this, this.securityProtocolFactory, ((IChannelFactory<IDuplexSessionChannel>) base.InnerChannelFactory).CreateChannel(address, via), address, via);
            }
            if (typeof(TChannel) == typeof(IRequestChannel))
            {
                return (TChannel) new SecurityRequestChannel<TChannel>(this, this.securityProtocolFactory, ((IChannelFactory<IRequestChannel>) base.InnerChannelFactory).CreateChannel(address, via), address, via);
            }
            return (TChannel) new SecurityRequestSessionChannel<TChannel>(this, this.securityProtocolFactory, ((IChannelFactory<IRequestSessionChannel>) base.InnerChannelFactory).CreateChannel(address, via), address, via);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            OperationWithTimeoutComposer.EndComposeAsyncOperations(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            OperationWithTimeoutAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.OnOpenCore(helper.RemainingTime());
            base.OnOpen(helper.RemainingTime());
        }

        private void OnOpenCore(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (this.SessionMode)
            {
                this.SessionClientSettings.Open((SecurityChannelFactory<TChannel>) this, base.InnerChannelFactory, this.ChannelBuilder, helper.RemainingTime());
            }
            else
            {
                this.ThrowIfProtocolFactoryNotSet();
                this.securityProtocolFactory.Open(true, helper.RemainingTime());
            }
        }

        private void ThrowIfDuplexNotSupported()
        {
            if (!this.SupportsDuplex)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecurityProtocolFactoryDoesNotSupportDuplex", new object[] { this.securityProtocolFactory })));
            }
        }

        private void ThrowIfProtocolFactoryNotSet()
        {
            if (this.securityProtocolFactory == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecurityProtocolFactoryShouldBeSetBeforeThisOperation")));
            }
        }

        private void ThrowIfRequestReplyNotSupported()
        {
            if (!this.SupportsRequestReply)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecurityProtocolFactoryDoesNotSupportRequestReply", new object[] { this.securityProtocolFactory })));
            }
        }

        public System.ServiceModel.Channels.ChannelBuilder ChannelBuilder
        {
            get
            {
                return this.channelBuilder;
            }
        }

        public System.ServiceModel.Channels.MessageVersion MessageVersion
        {
            get
            {
                return this.messageVersion;
            }
        }

        public System.ServiceModel.Security.SecurityProtocolFactory SecurityProtocolFactory
        {
            get
            {
                return this.securityProtocolFactory;
            }
        }

        public SecuritySessionClientSettings<TChannel> SessionClientSettings
        {
            get
            {
                return this.sessionClientSettings;
            }
        }

        public bool SessionMode
        {
            get
            {
                return this.sessionMode;
            }
        }

        private bool SupportsDuplex
        {
            get
            {
                this.ThrowIfProtocolFactoryNotSet();
                return this.securityProtocolFactory.SupportsDuplex;
            }
        }

        private bool SupportsRequestReply
        {
            get
            {
                this.ThrowIfProtocolFactoryNotSet();
                return this.securityProtocolFactory.SupportsRequestReply;
            }
        }

        private class ClientDuplexReceiveMessageAndVerifySecurityAsyncResult : ReceiveMessageAndVerifySecurityAsyncResultBase
        {
            private SecurityChannelFactory<TChannel>.SecurityDuplexChannel channel;

            public ClientDuplexReceiveMessageAndVerifySecurityAsyncResult(SecurityChannelFactory<TChannel>.SecurityDuplexChannel channel, IDuplexChannel innerChannel, TimeSpan timeout, AsyncCallback callback, object state) : base(innerChannel, timeout, callback, state)
            {
                this.channel = channel;
            }

            protected override bool OnInnerReceiveDone(ref Message message, TimeSpan timeout)
            {
                message = this.channel.ProcessMessage(message, timeout);
                return true;
            }
        }

        private abstract class ClientSecurityChannel<UChannel> : SecurityChannel<UChannel> where UChannel: class, IChannel
        {
            private ChannelParameterCollection channelParameters;
            private System.ServiceModel.Security.SecurityProtocolFactory securityProtocolFactory;
            private EndpointAddress to;
            private Uri via;

            protected ClientSecurityChannel(ChannelManagerBase factory, System.ServiceModel.Security.SecurityProtocolFactory securityProtocolFactory, UChannel innerChannel, EndpointAddress to, Uri via) : base(factory, innerChannel)
            {
                this.to = to;
                this.via = via;
                this.securityProtocolFactory = securityProtocolFactory;
                this.channelParameters = new ChannelParameterCollection(this);
            }

            private void EnableChannelBindingSupport()
            {
                if (((this.securityProtocolFactory != null) && (this.securityProtocolFactory.ExtendedProtectionPolicy != null)) && (this.securityProtocolFactory.ExtendedProtectionPolicy.CustomChannelBinding != null))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("ExtendedProtectionPolicyCustomChannelBindingNotSupported")));
                }
                if ((!System.ServiceModel.Security.SecurityUtils.IsChannelBindingDisabled && System.ServiceModel.Security.SecurityUtils.IsSecurityBindingSuitableForChannelBinding(this.SecurityProtocolFactory.SecurityBindingElement as TransportSecurityBindingElement)) && (base.InnerChannel != null))
                {
                    IChannelBindingProvider property = base.InnerChannel.GetProperty<IChannelBindingProvider>();
                    if (property != null)
                    {
                        property.EnableChannelBindingSupport();
                    }
                }
            }

            public override T GetProperty<T>() where T: class
            {
                if (typeof(T) == typeof(ChannelParameterCollection))
                {
                    return (T) this.channelParameters;
                }
                return base.GetProperty<T>();
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                this.EnableChannelBindingSupport();
                return new OpenAsyncResult<TChannel, UChannel>((SecurityChannelFactory<TChannel>.ClientSecurityChannel<UChannel>) this, timeout, callback, state);
            }

            protected override void OnEndOpen(IAsyncResult result)
            {
                OpenAsyncResult<TChannel, UChannel>.End(result);
            }

            protected override void OnOpen(TimeSpan timeout)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                this.EnableChannelBindingSupport();
                SecurityProtocol securityProtocol = this.SecurityProtocolFactory.CreateSecurityProtocol(this.to, this.Via, null, typeof(TChannel) == typeof(IRequestChannel), helper.RemainingTime());
                this.OnProtocolCreationComplete(securityProtocol);
                base.SecurityProtocol.Open(helper.RemainingTime());
                base.OnOpen(helper.RemainingTime());
            }

            private void OnProtocolCreationComplete(SecurityProtocol securityProtocol)
            {
                base.SecurityProtocol = securityProtocol;
                base.SecurityProtocol.ChannelParameters = this.channelParameters;
            }

            protected bool TryGetSecurityFaultException(Message faultMessage, out Exception faultException)
            {
                faultException = null;
                if (!faultMessage.IsFault)
                {
                    return false;
                }
                MessageFault fault = MessageFault.CreateFault(faultMessage, 0x4000);
                faultException = System.ServiceModel.Security.SecurityUtils.CreateSecurityFaultException(fault);
                return true;
            }

            public EndpointAddress RemoteAddress
            {
                get
                {
                    return this.to;
                }
            }

            protected System.ServiceModel.Security.SecurityProtocolFactory SecurityProtocolFactory
            {
                get
                {
                    return this.securityProtocolFactory;
                }
            }

            public Uri Via
            {
                get
                {
                    return this.via;
                }
            }

            private sealed class OpenAsyncResult : AsyncResult
            {
                private readonly SecurityChannelFactory<TChannel>.ClientSecurityChannel<UChannel> clientChannel;
                private static readonly AsyncCallback openInnerChannelCallback;
                private static readonly AsyncCallback openSecurityProtocolCallback;
                private TimeoutHelper timeoutHelper;

                static OpenAsyncResult()
                {
                    SecurityChannelFactory<TChannel>.ClientSecurityChannel<UChannel>.OpenAsyncResult.openInnerChannelCallback = Fx.ThunkCallback(new AsyncCallback(SecurityChannelFactory<TChannel>.ClientSecurityChannel<UChannel>.OpenAsyncResult.OpenInnerChannelCallback));
                    SecurityChannelFactory<TChannel>.ClientSecurityChannel<UChannel>.OpenAsyncResult.openSecurityProtocolCallback = Fx.ThunkCallback(new AsyncCallback(SecurityChannelFactory<TChannel>.ClientSecurityChannel<UChannel>.OpenAsyncResult.OpenSecurityProtocolCallback));
                }

                public OpenAsyncResult(SecurityChannelFactory<TChannel>.ClientSecurityChannel<UChannel> clientChannel, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.timeoutHelper = new TimeoutHelper(timeout);
                    this.clientChannel = clientChannel;
                    SecurityProtocol securityProtocol = this.clientChannel.SecurityProtocolFactory.CreateSecurityProtocol(this.clientChannel.to, this.clientChannel.Via, null, typeof(TChannel) == typeof(IRequestChannel), this.timeoutHelper.RemainingTime());
                    if (this.OnCreateSecurityProtocolComplete(securityProtocol))
                    {
                        base.Complete(true);
                    }
                }

                internal static void End(IAsyncResult result)
                {
                    AsyncResult.End<SecurityChannelFactory<TChannel>.ClientSecurityChannel<UChannel>.OpenAsyncResult>(result);
                }

                private bool OnCreateSecurityProtocolComplete(SecurityProtocol securityProtocol)
                {
                    this.clientChannel.OnProtocolCreationComplete(securityProtocol);
                    IAsyncResult result = securityProtocol.BeginOpen(this.timeoutHelper.RemainingTime(), SecurityChannelFactory<TChannel>.ClientSecurityChannel<UChannel>.OpenAsyncResult.openSecurityProtocolCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                    securityProtocol.EndOpen(result);
                    return this.OnSecurityProtocolOpenComplete();
                }

                private bool OnSecurityProtocolOpenComplete()
                {
                    IAsyncResult result = this.clientChannel.InnerChannel.BeginOpen(this.timeoutHelper.RemainingTime(), SecurityChannelFactory<TChannel>.ClientSecurityChannel<UChannel>.OpenAsyncResult.openInnerChannelCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                    this.clientChannel.InnerChannel.EndOpen(result);
                    return true;
                }

                private static void OpenInnerChannelCallback(IAsyncResult result)
                {
                    if (result == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("result"));
                    }
                    if (!result.CompletedSynchronously)
                    {
                        SecurityChannelFactory<TChannel>.ClientSecurityChannel<UChannel>.OpenAsyncResult asyncState = result.AsyncState as SecurityChannelFactory<TChannel>.ClientSecurityChannel<UChannel>.OpenAsyncResult;
                        if (asyncState == null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("InvalidAsyncResult"), "result"));
                        }
                        Exception exception = null;
                        try
                        {
                            asyncState.clientChannel.InnerChannel.EndOpen(result);
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

                private static void OpenSecurityProtocolCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        SecurityChannelFactory<TChannel>.ClientSecurityChannel<UChannel>.OpenAsyncResult asyncState = result.AsyncState as SecurityChannelFactory<TChannel>.ClientSecurityChannel<UChannel>.OpenAsyncResult;
                        if (asyncState == null)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException(System.ServiceModel.SR.GetString("InvalidAsyncResult"), "result"));
                        }
                        Exception exception = null;
                        bool flag = false;
                        try
                        {
                            asyncState.clientChannel.SecurityProtocol.EndOpen(result);
                            flag = asyncState.OnSecurityProtocolOpenComplete();
                        }
                        catch (Exception exception2)
                        {
                            if (Fx.IsFatal(exception2))
                            {
                                throw;
                            }
                            exception = exception2;
                            flag = true;
                        }
                        if (flag)
                        {
                            asyncState.Complete(false, exception);
                        }
                    }
                }
            }
        }

        private sealed class RequestChannelSendAsyncResult : ApplySecurityAndSendAsyncResult<IRequestChannel>
        {
            private Message reply;
            private SecurityChannelFactory<TChannel>.SecurityRequestChannel securityChannel;

            public RequestChannelSendAsyncResult(Message message, SecurityProtocol protocol, IRequestChannel channel, SecurityChannelFactory<TChannel>.SecurityRequestChannel securityChannel, TimeSpan timeout, AsyncCallback callback, object state) : base(protocol, channel, timeout, callback, state)
            {
                this.securityChannel = securityChannel;
                base.Begin(message, null);
            }

            protected override IAsyncResult BeginSendCore(IRequestChannel channel, Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return channel.BeginRequest(message, timeout, callback, state);
            }

            internal static Message End(IAsyncResult result)
            {
                SecurityChannelFactory<TChannel>.RequestChannelSendAsyncResult self = result as SecurityChannelFactory<TChannel>.RequestChannelSendAsyncResult;
                ApplySecurityAndSendAsyncResult<IRequestChannel>.OnEnd(self);
                return self.reply;
            }

            protected override void EndSendCore(IRequestChannel channel, IAsyncResult result)
            {
                this.reply = channel.EndRequest(result);
            }

            protected override void OnSendCompleteCore(TimeSpan timeout)
            {
                this.reply = this.securityChannel.ProcessReply(this.reply, base.CorrelationState, timeout);
            }
        }

        private class SecurityDuplexChannel : SecurityChannelFactory<TChannel>.SecurityOutputChannel, IDuplexChannel, IInputChannel, IOutputChannel, IChannel, ICommunicationObject
        {
            public SecurityDuplexChannel(ChannelManagerBase factory, SecurityProtocolFactory securityProtocolFactory, IDuplexChannel innerChannel, EndpointAddress to, Uri via) : base(factory, securityProtocolFactory, innerChannel, to, via)
            {
            }

            public IAsyncResult BeginReceive(AsyncCallback callback, object state)
            {
                return this.BeginReceive(base.DefaultReceiveTimeout, callback, state);
            }

            public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return InputChannel.HelpBeginReceive(this, timeout, callback, state);
            }

            public virtual IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
            {
                if (base.DoneReceivingInCurrentState())
                {
                    return new DoneReceivingAsyncResult(callback, state);
                }
                SecurityChannelFactory<TChannel>.ClientDuplexReceiveMessageAndVerifySecurityAsyncResult result = new SecurityChannelFactory<TChannel>.ClientDuplexReceiveMessageAndVerifySecurityAsyncResult((SecurityChannelFactory<TChannel>.SecurityDuplexChannel) this, this.InnerDuplexChannel, timeout, callback, state);
                result.Start();
                return result;
            }

            public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.InnerDuplexChannel.BeginWaitForMessage(timeout, callback, state);
            }

            public Message EndReceive(IAsyncResult result)
            {
                return InputChannel.HelpEndReceive(result);
            }

            public virtual bool EndTryReceive(IAsyncResult result, out Message message)
            {
                DoneReceivingAsyncResult result2 = result as DoneReceivingAsyncResult;
                if (result2 != null)
                {
                    return DoneReceivingAsyncResult.End(result2, out message);
                }
                return ReceiveMessageAndVerifySecurityAsyncResultBase.End(result, out message);
            }

            public bool EndWaitForMessage(IAsyncResult result)
            {
                return this.InnerDuplexChannel.EndWaitForMessage(result);
            }

            internal Message ProcessMessage(Message message, TimeSpan timeout)
            {
                if (message == null)
                {
                    return null;
                }
                Message faultMessage = message;
                Exception faultException = null;
                try
                {
                    base.SecurityProtocol.VerifyIncomingMessage(ref message, timeout);
                }
                catch (MessageSecurityException)
                {
                    base.TryGetSecurityFaultException(faultMessage, out faultException);
                    if (faultException == null)
                    {
                        throw;
                    }
                }
                if (faultException == null)
                {
                    return message;
                }
                if (this.AcceptUnsecuredFaults)
                {
                    base.Fault(faultException);
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(faultException);
            }

            public Message Receive()
            {
                return this.Receive(base.DefaultReceiveTimeout);
            }

            public Message Receive(TimeSpan timeout)
            {
                return InputChannel.HelpReceive(this, timeout);
            }

            public bool TryReceive(TimeSpan timeout, out Message message)
            {
                if (base.DoneReceivingInCurrentState())
                {
                    message = null;
                    return true;
                }
                TimeoutHelper helper = new TimeoutHelper(timeout);
                if (!this.InnerDuplexChannel.TryReceive(helper.RemainingTime(), out message))
                {
                    return false;
                }
                message = this.ProcessMessage(message, helper.RemainingTime());
                return true;
            }

            public bool WaitForMessage(TimeSpan timeout)
            {
                return this.InnerDuplexChannel.WaitForMessage(timeout);
            }

            internal virtual bool AcceptUnsecuredFaults
            {
                get
                {
                    return false;
                }
            }

            internal IDuplexChannel InnerDuplexChannel
            {
                get
                {
                    return (IDuplexChannel) base.InnerChannel;
                }
            }

            public EndpointAddress LocalAddress
            {
                get
                {
                    return this.InnerDuplexChannel.LocalAddress;
                }
            }
        }

        private sealed class SecurityDuplexSessionChannel : SecurityChannelFactory<TChannel>.SecurityDuplexChannel, IDuplexSessionChannel, IDuplexChannel, IInputChannel, IOutputChannel, IChannel, ICommunicationObject, ISessionChannel<IDuplexSession>
        {
            public SecurityDuplexSessionChannel(ChannelManagerBase factory, SecurityProtocolFactory securityProtocolFactory, IDuplexSessionChannel innerChannel, EndpointAddress to, Uri via) : base(factory, securityProtocolFactory, innerChannel, to, via)
            {
            }

            internal override bool AcceptUnsecuredFaults
            {
                get
                {
                    return true;
                }
            }

            public IDuplexSession Session
            {
                get
                {
                    return ((IDuplexSessionChannel) base.InnerChannel).Session;
                }
            }
        }

        private class SecurityOutputChannel : SecurityChannelFactory<TChannel>.ClientSecurityChannel<IOutputChannel>, IOutputChannel, IChannel, ICommunicationObject
        {
            public SecurityOutputChannel(ChannelManagerBase factory, SecurityProtocolFactory securityProtocolFactory, IOutputChannel innerChannel, EndpointAddress to, Uri via) : base(factory, securityProtocolFactory, innerChannel, to, via)
            {
            }

            public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
            {
                return this.BeginSend(message, base.DefaultSendTimeout, callback, state);
            }

            public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                base.ThrowIfFaulted();
                base.ThrowIfDisposedOrNotOpen(message);
                return new SecurityChannel<IOutputChannel>.OutputChannelSendAsyncResult(message, base.SecurityProtocol, base.InnerChannel, timeout, callback, state);
            }

            public void EndSend(IAsyncResult result)
            {
                SecurityChannel<IOutputChannel>.OutputChannelSendAsyncResult.End(result);
            }

            public void Send(Message message)
            {
                this.Send(message, base.DefaultSendTimeout);
            }

            public void Send(Message message, TimeSpan timeout)
            {
                base.ThrowIfFaulted();
                base.ThrowIfDisposedOrNotOpen(message);
                TimeoutHelper helper = new TimeoutHelper(timeout);
                base.SecurityProtocol.SecureOutgoingMessage(ref message, helper.RemainingTime());
                base.InnerChannel.Send(message, helper.RemainingTime());
            }
        }

        private sealed class SecurityOutputSessionChannel : SecurityChannelFactory<TChannel>.SecurityOutputChannel, IOutputSessionChannel, IOutputChannel, IChannel, ICommunicationObject, ISessionChannel<IOutputSession>
        {
            public SecurityOutputSessionChannel(ChannelManagerBase factory, SecurityProtocolFactory securityProtocolFactory, IOutputSessionChannel innerChannel, EndpointAddress to, Uri via) : base(factory, securityProtocolFactory, innerChannel, to, via)
            {
            }

            public IOutputSession Session
            {
                get
                {
                    return ((IOutputSessionChannel) base.InnerChannel).Session;
                }
            }
        }

        private class SecurityRequestChannel : SecurityChannelFactory<TChannel>.ClientSecurityChannel<IRequestChannel>, IRequestChannel, IChannel, ICommunicationObject
        {
            public SecurityRequestChannel(ChannelManagerBase factory, SecurityProtocolFactory securityProtocolFactory, IRequestChannel innerChannel, EndpointAddress to, Uri via) : base(factory, securityProtocolFactory, innerChannel, to, via)
            {
            }

            public IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state)
            {
                return this.BeginRequest(message, base.DefaultSendTimeout, callback, state);
            }

            public IAsyncResult BeginRequest(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                base.ThrowIfFaulted();
                base.ThrowIfDisposedOrNotOpen(message);
                return new SecurityChannelFactory<TChannel>.RequestChannelSendAsyncResult(message, base.SecurityProtocol, base.InnerChannel, (SecurityChannelFactory<TChannel>.SecurityRequestChannel) this, timeout, callback, state);
            }

            public Message EndRequest(IAsyncResult result)
            {
                return SecurityChannelFactory<TChannel>.RequestChannelSendAsyncResult.End(result);
            }

            internal Message ProcessReply(Message reply, SecurityProtocolCorrelationState correlationState, TimeSpan timeout)
            {
                if (reply != null)
                {
                    if (DiagnosticUtility.ShouldUseActivity)
                    {
                        ServiceModelActivity activity = TraceUtility.ExtractActivity(reply);
                        if (((activity != null) && (correlationState != null)) && ((correlationState.Activity != null) && (activity.Id != correlationState.Activity.Id)))
                        {
                            using (ServiceModelActivity.BoundOperation(activity))
                            {
                                if (FxTrace.Trace != null)
                                {
                                    FxTrace.Trace.TraceTransfer(correlationState.Activity.Id);
                                }
                                activity.Stop();
                            }
                        }
                    }
                    ServiceModelActivity activity2 = (correlationState == null) ? null : correlationState.Activity;
                    using (ServiceModelActivity.BoundOperation(activity2))
                    {
                        if (DiagnosticUtility.ShouldUseActivity)
                        {
                            TraceUtility.SetActivity(reply, activity2);
                        }
                        Message faultMessage = reply;
                        Exception faultException = null;
                        try
                        {
                            base.SecurityProtocol.VerifyIncomingMessage(ref reply, timeout, new SecurityProtocolCorrelationState[] { correlationState });
                        }
                        catch (MessageSecurityException)
                        {
                            base.TryGetSecurityFaultException(faultMessage, out faultException);
                            if (faultException == null)
                            {
                                throw;
                            }
                        }
                        if (faultException != null)
                        {
                            base.Fault(faultException);
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(faultException);
                        }
                    }
                }
                return reply;
            }

            public Message Request(Message message)
            {
                return this.Request(message, base.DefaultSendTimeout);
            }

            public Message Request(Message message, TimeSpan timeout)
            {
                base.ThrowIfFaulted();
                base.ThrowIfDisposedOrNotOpen(message);
                TimeoutHelper helper = new TimeoutHelper(timeout);
                SecurityProtocolCorrelationState correlationState = base.SecurityProtocol.SecureOutgoingMessage(ref message, helper.RemainingTime(), null);
                Message reply = base.InnerChannel.Request(message, helper.RemainingTime());
                return this.ProcessReply(reply, correlationState, helper.RemainingTime());
            }
        }

        private sealed class SecurityRequestSessionChannel : SecurityChannelFactory<TChannel>.SecurityRequestChannel, IRequestSessionChannel, IRequestChannel, IChannel, ICommunicationObject, ISessionChannel<IOutputSession>
        {
            public SecurityRequestSessionChannel(ChannelManagerBase factory, SecurityProtocolFactory securityProtocolFactory, IRequestSessionChannel innerChannel, EndpointAddress to, Uri via) : base(factory, securityProtocolFactory, innerChannel, to, via)
            {
            }

            public IOutputSession Session
            {
                get
                {
                    return ((IRequestSessionChannel) base.InnerChannel).Session;
                }
            }
        }
    }
}

