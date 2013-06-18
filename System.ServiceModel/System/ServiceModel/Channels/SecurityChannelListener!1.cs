namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Authentication.ExtendedProtection;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Security;
    using System.ServiceModel.Security.Tokens;
    using System.Threading;

    internal sealed class SecurityChannelListener<TChannel> : DelegatingChannelListener<TChannel> where TChannel: class, IChannel
    {
        private System.ServiceModel.Channels.ChannelBuilder channelBuilder;
        private bool extendedProtectionPolicyHasSupport;
        private bool hasSecurityStateReference;
        private EndpointIdentity identity;
        private ISecurityCapabilities securityCapabilities;
        private System.ServiceModel.Security.SecurityProtocolFactory securityProtocolFactory;
        private bool sendUnsecuredFaults;
        private bool sessionMode;
        private SecuritySessionServerSettings sessionServerSettings;
        private SecurityListenerSettingsLifetimeManager settingsLifetimeManager;

        public SecurityChannelListener(SecurityBindingElement bindingElement, BindingContext context) : base(true, context.Binding)
        {
            this.sendUnsecuredFaults = true;
            this.securityCapabilities = bindingElement.GetProperty<ISecurityCapabilities>(context);
            this.extendedProtectionPolicyHasSupport = System.ServiceModel.Security.SecurityUtils.IsSecurityBindingSuitableForChannelBinding(bindingElement as TransportSecurityBindingElement);
        }

        internal SecurityChannelListener(System.ServiceModel.Security.SecurityProtocolFactory protocolFactory, IChannelListener innerChannelListener) : base(true, null, innerChannelListener)
        {
            this.sendUnsecuredFaults = true;
            this.securityProtocolFactory = protocolFactory;
        }

        private void ComputeEndpointIdentity()
        {
            EndpointIdentity identityOfSelf = null;
            if (base.State == CommunicationState.Opened)
            {
                if (this.SecurityProtocolFactory != null)
                {
                    identityOfSelf = this.SecurityProtocolFactory.GetIdentityOfSelf();
                }
                else if ((this.SessionServerSettings != null) && (this.SessionServerSettings.SessionProtocolFactory != null))
                {
                    identityOfSelf = this.SessionServerSettings.SessionProtocolFactory.GetIdentityOfSelf();
                }
            }
            if (identityOfSelf == null)
            {
                identityOfSelf = base.GetProperty<EndpointIdentity>();
            }
            this.identity = identityOfSelf;
        }

        private void EnableChannelBindingSupport()
        {
            ExtendedProtectionPolicy property = this.InnerChannelListener.GetProperty<ExtendedProtectionPolicy>();
            if (property != null)
            {
                if (property.CustomChannelBinding != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("ExtendedProtectionPolicyCustomChannelBindingNotSupported")));
                }
                if (property.PolicyEnforcement == PolicyEnforcement.Never)
                {
                    return;
                }
                IChannelBindingProvider provider = this.InnerChannelListener.GetProperty<IChannelBindingProvider>();
                if ((property.PolicyEnforcement == PolicyEnforcement.Always) && ((System.ServiceModel.Security.SecurityUtils.IsChannelBindingDisabled || !this.extendedProtectionPolicyHasSupport) || ((provider == null) && (property.ProtectionScenario != ProtectionScenario.TrustedProxy))))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecurityChannelListenerChannelExtendedProtectionNotSupported")));
                }
                if (System.ServiceModel.Security.SecurityUtils.IsChannelBindingDisabled || !this.extendedProtectionPolicyHasSupport)
                {
                    return;
                }
                if (provider != null)
                {
                    provider.EnableChannelBindingSupport();
                }
            }
            if (this.securityProtocolFactory != null)
            {
                this.securityProtocolFactory.ExtendedProtectionPolicy = property;
            }
        }

        public override T GetProperty<T>() where T: class
        {
            if (typeof(T) == typeof(System.ServiceModel.Security.SecurityProtocolFactory))
            {
                return (T) this.SecurityProtocolFactory;
            }
            if (this.SessionMode && (typeof(T) == typeof(IListenerSecureConversationSessionSettings)))
            {
                return (T) this.SessionServerSettings;
            }
            if (typeof(T) == typeof(EndpointIdentity))
            {
                return (T) this.identity;
            }
            if (typeof(T) == typeof(Collection<ISecurityContextSecurityTokenCache>))
            {
                if (this.SecurityProtocolFactory != null)
                {
                    return this.SecurityProtocolFactory.GetProperty<Collection<ISecurityContextSecurityTokenCache>>();
                }
                return base.GetProperty<Collection<ISecurityContextSecurityTokenCache>>();
            }
            if (typeof(T) == typeof(ISecurityCapabilities))
            {
                return (T) this.securityCapabilities;
            }
            if (!(typeof(T) == typeof(ILogonTokenCacheManager)))
            {
                return base.GetProperty<T>();
            }
            List<ILogonTokenCacheManager> list = new List<ILogonTokenCacheManager>();
            if ((this.SecurityProtocolFactory != null) && (this.securityProtocolFactory.ChannelSupportingTokenAuthenticatorSpecification.Count > 0))
            {
                foreach (SupportingTokenAuthenticatorSpecification specification in this.securityProtocolFactory.ChannelSupportingTokenAuthenticatorSpecification)
                {
                    if (specification.TokenAuthenticator is ILogonTokenCacheManager)
                    {
                        list.Add(specification.TokenAuthenticator as ILogonTokenCacheManager);
                    }
                }
            }
            if ((this.SessionServerSettings.SessionProtocolFactory != null) && (this.SessionServerSettings.SessionTokenAuthenticator is ILogonTokenCacheManager))
            {
                list.Add(this.SessionServerSettings.SessionTokenAuthenticator as ILogonTokenCacheManager);
            }
            return (T) new AggregateLogonTokenCacheManager<TChannel>(new ReadOnlyCollection<ILogonTokenCacheManager>(list));
        }

        internal void InitializeListener(System.ServiceModel.Channels.ChannelBuilder channelBuilder)
        {
            this.channelBuilder = channelBuilder;
            if (this.SessionMode)
            {
                this.sessionServerSettings.ChannelBuilder = this.ChannelBuilder;
                this.InnerChannelListener = this.sessionServerSettings.CreateInnerChannelListener();
                base.Acceptor = this.sessionServerSettings.CreateAcceptor<TChannel>();
            }
            else
            {
                this.InnerChannelListener = this.ChannelBuilder.BuildChannelListener<TChannel>();
                base.Acceptor = new SecurityChannelAcceptor<TChannel>(this, (IChannelListener<TChannel>) this.InnerChannelListener, this.securityProtocolFactory.CreateListenerSecurityState());
            }
        }

        private void InitializeListenerSecurityState()
        {
            if (this.SessionMode)
            {
                this.SessionServerSettings.SessionProtocolFactory.ListenUri = this.Uri;
                this.SessionServerSettings.SecurityChannelListener = this;
            }
            else
            {
                this.ThrowIfProtocolFactoryNotSet();
                this.securityProtocolFactory.ListenUri = this.Uri;
            }
            this.settingsLifetimeManager = new SecurityListenerSettingsLifetimeManager(this.securityProtocolFactory, this.sessionServerSettings, this.sessionMode, this.InnerChannelListener);
            if (this.sessionServerSettings != null)
            {
                this.sessionServerSettings.SettingsLifetimeManager = this.settingsLifetimeManager;
            }
            this.hasSecurityStateReference = true;
        }

        protected override void OnAbort()
        {
            lock (base.ThisLock)
            {
                if (this.hasSecurityStateReference)
                {
                    this.hasSecurityStateReference = false;
                    if (this.settingsLifetimeManager != null)
                    {
                        this.settingsLifetimeManager.Abort();
                    }
                }
            }
            base.OnAbort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.SessionMode && (this.sessionServerSettings != null))
            {
                this.sessionServerSettings.StopAcceptingNewWork();
            }
            return new ChainedAsyncResult(timeout, callback, state, new ChainedBeginHandler(this.OnBeginCloseSharedState), new ChainedEndHandler(this.OnEndCloseSharedState), new ChainedBeginHandler(this.OnBeginClose), new ChainedEndHandler(this.OnEndClose));
        }

        private IAsyncResult OnBeginCloseSharedState(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseSharedStateAsyncResult<TChannel>((SecurityChannelListener<TChannel>) this, timeout, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            base.ThrowIfInnerListenerNotSet();
            this.EnableChannelBindingSupport();
            return new ChainedAsyncResult(timeout, callback, state, new ChainedBeginHandler(this.OnBeginOpen), new ChainedEndHandler(this.OnEndOpen), new ChainedBeginHandler(this.OnBeginOpenListenerState), new ChainedEndHandler(this.OnEndOpenListenerState));
        }

        internal IAsyncResult OnBeginOpenListenerState(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OpenListenerStateAsyncResult<TChannel>((SecurityChannelListener<TChannel>) this, timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (this.sessionServerSettings != null)
            {
                this.sessionServerSettings.StopAcceptingNewWork();
            }
            lock (base.ThisLock)
            {
                if (this.hasSecurityStateReference)
                {
                    this.hasSecurityStateReference = false;
                    this.settingsLifetimeManager.Close(helper.RemainingTime());
                }
            }
            base.OnClose(helper.RemainingTime());
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        private void OnEndCloseSharedState(IAsyncResult result)
        {
            CloseSharedStateAsyncResult<TChannel>.End(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        internal void OnEndOpenListenerState(IAsyncResult result)
        {
            OpenListenerStateAsyncResult<TChannel>.End(result);
        }

        protected override void OnFaulted()
        {
            lock (base.ThisLock)
            {
                if (this.hasSecurityStateReference)
                {
                    this.hasSecurityStateReference = false;
                    if (this.settingsLifetimeManager != null)
                    {
                        this.settingsLifetimeManager.Abort();
                    }
                }
            }
            base.OnFaulted();
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            base.ThrowIfInnerListenerNotSet();
            this.EnableChannelBindingSupport();
            TimeoutHelper helper = new TimeoutHelper(timeout);
            base.OnOpen(helper.RemainingTime());
            lock (base.ThisLock)
            {
                if ((base.State == CommunicationState.Closing) && (base.State == CommunicationState.Closed))
                {
                    return;
                }
                this.InitializeListenerSecurityState();
            }
            this.settingsLifetimeManager.Open(helper.RemainingTime());
        }

        protected override void OnOpened()
        {
            base.OnOpened();
            this.ComputeEndpointIdentity();
        }

        private void ThrowIfProtocolFactoryNotSet()
        {
            if (this.securityProtocolFactory == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SecurityProtocolFactoryShouldBeSetBeforeThisOperation")));
            }
        }

        public System.ServiceModel.Channels.ChannelBuilder ChannelBuilder
        {
            get
            {
                base.ThrowIfDisposed();
                return this.channelBuilder;
            }
        }

        public System.ServiceModel.Security.SecurityProtocolFactory SecurityProtocolFactory
        {
            get
            {
                base.ThrowIfDisposed();
                return this.securityProtocolFactory;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                base.ThrowIfDisposedOrImmutable();
                this.securityProtocolFactory = value;
            }
        }

        public bool SendUnsecuredFaults
        {
            get
            {
                return this.sendUnsecuredFaults;
            }
            set
            {
                base.ThrowIfDisposedOrImmutable();
                this.sendUnsecuredFaults = value;
            }
        }

        public bool SessionMode
        {
            get
            {
                return this.sessionMode;
            }
            set
            {
                base.ThrowIfDisposedOrImmutable();
                this.sessionMode = value;
            }
        }

        public SecuritySessionServerSettings SessionServerSettings
        {
            get
            {
                base.ThrowIfDisposed();
                if (this.sessionServerSettings == null)
                {
                    lock (base.ThisLock)
                    {
                        if (this.sessionServerSettings == null)
                        {
                            SecuritySessionServerSettings settings = new SecuritySessionServerSettings();
                            Thread.MemoryBarrier();
                            this.sessionServerSettings = settings;
                        }
                    }
                }
                return this.sessionServerSettings;
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

        private class AggregateLogonTokenCacheManager : ILogonTokenCacheManager
        {
            private ReadOnlyCollection<ILogonTokenCacheManager> cacheManagers;

            public AggregateLogonTokenCacheManager(ReadOnlyCollection<ILogonTokenCacheManager> cacheManagers)
            {
                if (cacheManagers == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("cacheManagers");
                }
                this.cacheManagers = cacheManagers;
            }

            public void FlushLogonTokenCache()
            {
                if (this.cacheManagers != null)
                {
                    for (int i = 0; i < this.cacheManagers.Count; i++)
                    {
                        this.cacheManagers[i].FlushLogonTokenCache();
                    }
                }
            }

            public bool RemoveCachedLogonToken(string username)
            {
                bool flag = false;
                if (!flag && (this.cacheManagers != null))
                {
                    for (int i = 0; i < this.cacheManagers.Count; i++)
                    {
                        flag = this.cacheManagers[i].RemoveCachedLogonToken(username);
                        if (flag)
                        {
                            return flag;
                        }
                    }
                }
                return flag;
            }
        }

        private class CloseSharedStateAsyncResult : AsyncResult
        {
            private static AsyncCallback lifetimeManagerCloseCallback;
            private SecurityChannelListener<TChannel> securityListener;

            static CloseSharedStateAsyncResult()
            {
                SecurityChannelListener<TChannel>.CloseSharedStateAsyncResult.lifetimeManagerCloseCallback = Fx.ThunkCallback(new AsyncCallback(SecurityChannelListener<TChannel>.CloseSharedStateAsyncResult.LifetimeManagerCloseCallback));
            }

            public CloseSharedStateAsyncResult(SecurityChannelListener<TChannel> securityListener, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.securityListener = securityListener;
                lock (this.securityListener.ThisLock)
                {
                    if (this.securityListener.hasSecurityStateReference)
                    {
                        this.securityListener.hasSecurityStateReference = false;
                        IAsyncResult result = this.securityListener.settingsLifetimeManager.BeginClose(timeout, SecurityChannelListener<TChannel>.CloseSharedStateAsyncResult.lifetimeManagerCloseCallback, this);
                        if (!result.CompletedSynchronously)
                        {
                            return;
                        }
                        this.securityListener.settingsLifetimeManager.EndClose(result);
                    }
                }
                base.Complete(true);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<SecurityChannelListener<TChannel>.CloseSharedStateAsyncResult>(result);
            }

            private static void LifetimeManagerCloseCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    SecurityChannelListener<TChannel>.CloseSharedStateAsyncResult asyncState = (SecurityChannelListener<TChannel>.CloseSharedStateAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.securityListener.settingsLifetimeManager.EndClose(result);
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

        private sealed class DuplexSessionReceiveMessageAndVerifySecurityAsyncResult : SecurityChannelListener<TChannel>.ReceiveItemAndVerifySecurityAsyncResult<Message, IInputChannel>
        {
            private SecurityChannelListener<TChannel>.SecurityDuplexSessionChannel channel;
            private IDuplexChannel innerChannel;

            public DuplexSessionReceiveMessageAndVerifySecurityAsyncResult(SecurityChannelListener<TChannel>.SecurityDuplexSessionChannel channel, IDuplexChannel innerChannel, TimeSpan timeout, AsyncCallback callback, object state) : base(channel, timeout, callback, state)
            {
                this.innerChannel = innerChannel;
                this.channel = channel;
                ActionItem.Schedule(new Action<object>(SecurityChannelListener<TChannel>.DuplexSessionReceiveMessageAndVerifySecurityAsyncResult.ReceiveMessage), this);
            }

            protected override void AbortInnerItem(Message innerItem)
            {
            }

            protected override IAsyncResult BeginSendFault(Message innerItem, Message faultMessage, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.innerChannel.BeginSend(faultMessage, timeout, callback, state);
            }

            protected override IAsyncResult BeginTryReceiveItem(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.innerChannel.BeginTryReceive(timeout, callback, state);
            }

            protected override void CloseInnerItem(Message innerItem, TimeSpan timeout)
            {
                innerItem.Close();
            }

            protected override Message CreateFaultMessage(MessageFault fault, Message innerItem)
            {
                Message message = Message.CreateMessage(innerItem.Version, fault, innerItem.Version.Addressing.DefaultFaultAction);
                if (innerItem.Headers.MessageId != null)
                {
                    message.InitializeReply(innerItem);
                }
                return message;
            }

            public static bool End(IAsyncResult result, out Message message)
            {
                SecurityChannelListener<TChannel>.DuplexSessionReceiveMessageAndVerifySecurityAsyncResult result2 = AsyncResult.End<SecurityChannelListener<TChannel>.DuplexSessionReceiveMessageAndVerifySecurityAsyncResult>(result);
                message = result2.Item;
                return result2.ReceiveCompleted;
            }

            protected override void EndSendFault(Message innerItem, IAsyncResult result)
            {
                this.innerChannel.EndSend(result);
            }

            protected override bool EndTryReceiveItem(IAsyncResult result, out Message innerItem)
            {
                return this.innerChannel.EndTryReceive(result, out innerItem);
            }

            protected override Message ProcessInnerItem(Message innerItem, TimeSpan timeout)
            {
                if (innerItem == null)
                {
                    return null;
                }
                Message message = innerItem;
                this.channel.VerifyIncomingMessage(ref message, timeout);
                return message;
            }

            private static void ReceiveMessage(object state)
            {
                SecurityChannelListener<TChannel>.DuplexSessionReceiveMessageAndVerifySecurityAsyncResult result = state as SecurityChannelListener<TChannel>.DuplexSessionReceiveMessageAndVerifySecurityAsyncResult;
                if (result == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException());
                }
                result.Start();
            }

            protected override bool CanSendFault
            {
                get
                {
                    return this.channel.SendUnsecuredFaults;
                }
            }

            protected override SecurityStandardsManager StandardsManager
            {
                get
                {
                    return this.channel.SecurityProtocol.SecurityProtocolFactory.StandardsManager;
                }
            }
        }

        private sealed class InputChannelReceiveMessageAndVerifySecurityAsyncResult : SecurityChannelListener<TChannel>.ReceiveItemAndVerifySecurityAsyncResult<Message, IInputChannel>
        {
            private SecurityChannelListener<TChannel>.SecurityInputChannel channel;
            private IInputChannel innerChannel;

            public InputChannelReceiveMessageAndVerifySecurityAsyncResult(SecurityChannelListener<TChannel>.SecurityInputChannel channel, IInputChannel innerChannel, TimeSpan timeout, AsyncCallback callback, object state) : base(channel, timeout, callback, state)
            {
                this.innerChannel = innerChannel;
                this.channel = channel;
                ActionItem.Schedule(new Action<object>(SecurityChannelListener<TChannel>.InputChannelReceiveMessageAndVerifySecurityAsyncResult.ReceiveMessage), this);
            }

            protected override void AbortInnerItem(Message innerItem)
            {
            }

            protected override IAsyncResult BeginSendFault(Message innerItem, Message faultMessage, TimeSpan timeout, AsyncCallback callback, object state)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            protected override IAsyncResult BeginTryReceiveItem(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.innerChannel.BeginTryReceive(timeout, callback, state);
            }

            protected override void CloseInnerItem(Message innerItem, TimeSpan timeout)
            {
                innerItem.Close();
            }

            protected override Message CreateFaultMessage(MessageFault fault, Message innerItem)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            public static bool End(IAsyncResult result, out Message message)
            {
                SecurityChannelListener<TChannel>.InputChannelReceiveMessageAndVerifySecurityAsyncResult result2 = AsyncResult.End<SecurityChannelListener<TChannel>.InputChannelReceiveMessageAndVerifySecurityAsyncResult>(result);
                message = result2.Item;
                return result2.ReceiveCompleted;
            }

            protected override void EndSendFault(Message innerItem, IAsyncResult result)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException());
            }

            protected override bool EndTryReceiveItem(IAsyncResult result, out Message innerItem)
            {
                return this.innerChannel.EndTryReceive(result, out innerItem);
            }

            protected override Message ProcessInnerItem(Message innerItem, TimeSpan timeout)
            {
                if (innerItem == null)
                {
                    return null;
                }
                Message message = innerItem;
                this.channel.VerifyIncomingMessage(ref message, timeout);
                return message;
            }

            private static void ReceiveMessage(object state)
            {
                SecurityChannelListener<TChannel>.InputChannelReceiveMessageAndVerifySecurityAsyncResult result = state as SecurityChannelListener<TChannel>.InputChannelReceiveMessageAndVerifySecurityAsyncResult;
                if (result == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException());
                }
                result.Start();
            }

            protected override bool CanSendFault
            {
                get
                {
                    return false;
                }
            }

            protected override SecurityStandardsManager StandardsManager
            {
                get
                {
                    return this.channel.SecurityProtocol.SecurityProtocolFactory.StandardsManager;
                }
            }
        }

        private class OpenListenerStateAsyncResult : AsyncResult
        {
            private static AsyncCallback lifetimeManagerOpenCallback;
            private SecurityChannelListener<TChannel> securityListener;

            static OpenListenerStateAsyncResult()
            {
                SecurityChannelListener<TChannel>.OpenListenerStateAsyncResult.lifetimeManagerOpenCallback = Fx.ThunkCallback(new AsyncCallback(SecurityChannelListener<TChannel>.OpenListenerStateAsyncResult.LifetimeManagerOpenCallback));
            }

            public OpenListenerStateAsyncResult(SecurityChannelListener<TChannel> securityListener, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                bool flag;
                this.securityListener = securityListener;
                lock (this.securityListener.ThisLock)
                {
                    if ((this.securityListener.State == CommunicationState.Closed) || (this.securityListener.State == CommunicationState.Closing))
                    {
                        flag = false;
                    }
                    else
                    {
                        flag = true;
                        this.securityListener.InitializeListenerSecurityState();
                    }
                }
                if (flag)
                {
                    IAsyncResult result = this.securityListener.settingsLifetimeManager.BeginOpen(timeout, SecurityChannelListener<TChannel>.OpenListenerStateAsyncResult.lifetimeManagerOpenCallback, this);
                    if (!result.CompletedSynchronously)
                    {
                        return;
                    }
                    this.securityListener.settingsLifetimeManager.EndOpen(result);
                }
                base.Complete(true);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<SecurityChannelListener<TChannel>.OpenListenerStateAsyncResult>(result);
            }

            private static void LifetimeManagerOpenCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    SecurityChannelListener<TChannel>.OpenListenerStateAsyncResult asyncState = (SecurityChannelListener<TChannel>.OpenListenerStateAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.securityListener.settingsLifetimeManager.EndOpen(result);
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

        private abstract class ReceiveItemAndVerifySecurityAsyncResult<TItem, UChannel> : AsyncResult where TItem: class where UChannel: class, IChannel
        {
            private SecurityChannelListener<TChannel>.ServerSecurityChannel<UChannel> channel;
            private Message faultMessage;
            private TItem innerItem;
            private static AsyncCallback innerTryReceiveCompletedCallback;
            private TItem item;
            protected bool receiveCompleted;
            protected TimeoutHelper timeoutHelper;

            static ReceiveItemAndVerifySecurityAsyncResult()
            {
                SecurityChannelListener<TChannel>.ReceiveItemAndVerifySecurityAsyncResult<TItem, UChannel>.innerTryReceiveCompletedCallback = Fx.ThunkCallback(new AsyncCallback(SecurityChannelListener<TChannel>.ReceiveItemAndVerifySecurityAsyncResult<TItem, UChannel>.InnerTryReceiveCompletedCallback));
            }

            public ReceiveItemAndVerifySecurityAsyncResult(SecurityChannelListener<TChannel>.ServerSecurityChannel<UChannel> channel, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.channel = channel;
            }

            protected abstract void AbortInnerItem(TItem innerItem);
            protected abstract IAsyncResult BeginSendFault(TItem innerItem, Message faultMessage, TimeSpan timeout, AsyncCallback callback, object state);
            protected abstract IAsyncResult BeginTryReceiveItem(TimeSpan timeout, AsyncCallback callback, object state);
            protected abstract void CloseInnerItem(TItem innerItem, TimeSpan timeout);
            protected abstract Message CreateFaultMessage(MessageFault fault, TItem innerItem);
            protected abstract void EndSendFault(TItem innerItem, IAsyncResult result);
            protected abstract bool EndTryReceiveItem(IAsyncResult result, out TItem innerItem);
            private static void InnerTryReceiveCompletedCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    SecurityChannelListener<TChannel>.ReceiveItemAndVerifySecurityAsyncResult<TItem, UChannel> asyncState = (SecurityChannelListener<TChannel>.ReceiveItemAndVerifySecurityAsyncResult<TItem, UChannel>) result.AsyncState;
                    bool flag = false;
                    Exception exception = null;
                    try
                    {
                        if (!asyncState.EndTryReceiveItem(result, out asyncState.innerItem))
                        {
                            asyncState.receiveCompleted = false;
                            flag = true;
                        }
                        else
                        {
                            flag = asyncState.OnInnerReceiveDone();
                        }
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
                        asyncState.Complete(false, exception);
                    }
                }
            }

            private bool OnFaultSent()
            {
                this.innerItem = default(TItem);
                if (this.timeoutHelper.RemainingTime() == TimeSpan.Zero)
                {
                    this.receiveCompleted = false;
                    return true;
                }
                return this.StartInnerReceive();
            }

            private bool OnInnerReceiveDone()
            {
                this.channel.InternalThrowIfFaulted();
                Exception e = null;
                try
                {
                    this.item = this.ProcessInnerItem(this.innerItem, this.timeoutHelper.RemainingTime());
                    this.receiveCompleted = true;
                }
                catch (MessageSecurityException exception2)
                {
                    e = exception2;
                }
                if (e == null)
                {
                    return true;
                }
                if (this.CanSendFault && !this.OnSecurityException(e))
                {
                    return false;
                }
                return this.OnFaultSent();
            }

            private bool OnSecurityException(Exception e)
            {
                MessageFault fault = System.ServiceModel.Security.SecurityUtils.CreateSecurityMessageFault(e, this.StandardsManager);
                if (fault == null)
                {
                    return true;
                }
                this.faultMessage = this.CreateFaultMessage(fault, this.innerItem);
                return this.SendFault(this.faultMessage, e);
            }

            protected abstract TItem ProcessInnerItem(TItem innerItem, TimeSpan timeout);
            private bool SendFault(Message faultMessage, Exception e)
            {
                bool flag = false;
                try
                {
                    IAsyncResult result = this.BeginSendFault(this.innerItem, faultMessage, this.timeoutHelper.RemainingTime(), Fx.ThunkCallback(new AsyncCallback(this.SendFaultCallback)), e);
                    if (!result.CompletedSynchronously)
                    {
                        return false;
                    }
                    flag = true;
                    this.EndSendFault(this.innerItem, result);
                    this.CloseInnerItem(this.innerItem, this.timeoutHelper.RemainingTime());
                }
                catch (Exception exception)
                {
                    if (faultMessage != null)
                    {
                        faultMessage.Close();
                    }
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                }
                finally
                {
                    if (flag)
                    {
                        this.AbortInnerItem(this.innerItem);
                        if (faultMessage != null)
                        {
                            faultMessage.Close();
                        }
                    }
                }
                return true;
            }

            private void SendFaultCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    Exception asyncState = (Exception) result.AsyncState;
                    try
                    {
                        this.EndSendFault(this.innerItem, result);
                        this.CloseInnerItem(this.innerItem, this.timeoutHelper.RemainingTime());
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
                            throw;
                        }
                    }
                    finally
                    {
                        if (this.faultMessage != null)
                        {
                            this.faultMessage.Close();
                        }
                        this.AbortInnerItem(this.innerItem);
                    }
                    bool flag = false;
                    Exception exception2 = null;
                    try
                    {
                        flag = this.OnFaultSent();
                    }
                    catch (Exception exception3)
                    {
                        if (Fx.IsFatal(exception3))
                        {
                            throw;
                        }
                        flag = true;
                        exception2 = exception3;
                    }
                    if (flag)
                    {
                        base.Complete(false, exception2);
                    }
                }
            }

            protected void Start()
            {
                if (this.StartInnerReceive())
                {
                    base.Complete(false);
                }
            }

            private bool StartInnerReceive()
            {
                this.channel.InternalThrowIfFaulted();
                if (this.channel.State == CommunicationState.Closed)
                {
                    this.item = default(TItem);
                    this.receiveCompleted = true;
                    return true;
                }
                IAsyncResult result = this.BeginTryReceiveItem(this.timeoutHelper.RemainingTime(), SecurityChannelListener<TChannel>.ReceiveItemAndVerifySecurityAsyncResult<TItem, UChannel>.innerTryReceiveCompletedCallback, this);
                if (!result.CompletedSynchronously)
                {
                    return false;
                }
                if (!this.EndTryReceiveItem(result, out this.innerItem))
                {
                    this.receiveCompleted = false;
                    return true;
                }
                return this.OnInnerReceiveDone();
            }

            protected abstract bool CanSendFault { get; }

            protected TItem Item
            {
                get
                {
                    return this.item;
                }
            }

            protected bool ReceiveCompleted
            {
                get
                {
                    return this.receiveCompleted;
                }
            }

            protected abstract SecurityStandardsManager StandardsManager { get; }
        }

        private sealed class ReceiveRequestAndVerifySecurityAsyncResult : SecurityChannelListener<TChannel>.ReceiveItemAndVerifySecurityAsyncResult<RequestContext, IReplyChannel>
        {
            private SecurityChannelListener<TChannel>.SecurityReplyChannel channel;
            private IReplyChannel innerChannel;

            public ReceiveRequestAndVerifySecurityAsyncResult(SecurityChannelListener<TChannel>.SecurityReplyChannel channel, IReplyChannel innerChannel, TimeSpan timeout, AsyncCallback callback, object state) : base(channel, timeout, callback, state)
            {
                this.channel = channel;
                this.innerChannel = innerChannel;
                ActionItem.Schedule(new Action<object>(SecurityChannelListener<TChannel>.ReceiveRequestAndVerifySecurityAsyncResult.ReceiveMessage), this);
            }

            protected override void AbortInnerItem(RequestContext innerItem)
            {
                innerItem.Abort();
            }

            protected override IAsyncResult BeginSendFault(RequestContext innerItem, Message faultMessage, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return innerItem.BeginReply(faultMessage, timeout, callback, state);
            }

            protected override IAsyncResult BeginTryReceiveItem(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.innerChannel.BeginTryReceiveRequest(timeout, callback, state);
            }

            protected override void CloseInnerItem(RequestContext innerItem, TimeSpan timeout)
            {
                innerItem.Close(timeout);
            }

            protected override Message CreateFaultMessage(MessageFault fault, RequestContext innerItem)
            {
                Message requestMessage = innerItem.RequestMessage;
                Message message2 = Message.CreateMessage(requestMessage.Version, fault, requestMessage.Version.Addressing.DefaultFaultAction);
                if (requestMessage.Headers.MessageId != null)
                {
                    message2.InitializeReply(requestMessage);
                }
                return message2;
            }

            public static bool End(IAsyncResult result, out RequestContext requestContext)
            {
                SecurityChannelListener<TChannel>.ReceiveRequestAndVerifySecurityAsyncResult result2 = AsyncResult.End<SecurityChannelListener<TChannel>.ReceiveRequestAndVerifySecurityAsyncResult>(result);
                requestContext = result2.Item;
                return result2.ReceiveCompleted;
            }

            protected override void EndSendFault(RequestContext innerItem, IAsyncResult result)
            {
                innerItem.EndReply(result);
            }

            protected override bool EndTryReceiveItem(IAsyncResult result, out RequestContext innerItem)
            {
                return this.innerChannel.EndTryReceiveRequest(result, out innerItem);
            }

            protected override RequestContext ProcessInnerItem(RequestContext innerItem, TimeSpan timeout)
            {
                return this.channel.ProcessReceivedRequest(innerItem, timeout);
            }

            private static void ReceiveMessage(object state)
            {
                SecurityChannelListener<TChannel>.ReceiveRequestAndVerifySecurityAsyncResult result = state as SecurityChannelListener<TChannel>.ReceiveRequestAndVerifySecurityAsyncResult;
                if (result == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentException());
                }
                result.Start();
            }

            protected override bool CanSendFault
            {
                get
                {
                    return this.channel.SendUnsecuredFaults;
                }
            }

            protected override SecurityStandardsManager StandardsManager
            {
                get
                {
                    return this.channel.SecurityProtocol.SecurityProtocolFactory.StandardsManager;
                }
            }
        }

        internal sealed class SecurityChannelAcceptor : LayeredChannelAcceptor<TChannel, TChannel>
        {
            private readonly object listenerSecurityProtocolState;

            public SecurityChannelAcceptor(ChannelManagerBase channelManager, IChannelListener<TChannel> innerListener, object listenerSecurityProtocolState) : base(channelManager, innerListener)
            {
                this.listenerSecurityProtocolState = listenerSecurityProtocolState;
            }

            protected override TChannel OnAcceptChannel(TChannel innerChannel)
            {
                object obj2;
                SecurityChannelListener<TChannel> securityChannelListener = this.SecurityChannelListener;
                SecurityProtocol securityProtocol = securityChannelListener.SecurityProtocolFactory.CreateSecurityProtocol(null, null, this.listenerSecurityProtocolState, (typeof(TChannel) == typeof(IReplyChannel)) || (typeof(TChannel) == typeof(IReplySessionChannel)), TimeSpan.Zero);
                if (typeof(TChannel) == typeof(IInputChannel))
                {
                    obj2 = new SecurityChannelListener<TChannel>.SecurityInputChannel(securityChannelListener, (IInputChannel) innerChannel, securityProtocol, securityChannelListener.settingsLifetimeManager);
                }
                else if (typeof(TChannel) == typeof(IInputSessionChannel))
                {
                    obj2 = new SecurityChannelListener<TChannel>.SecurityInputSessionChannel(securityChannelListener, (IInputSessionChannel) innerChannel, securityProtocol, securityChannelListener.settingsLifetimeManager);
                }
                else if (securityChannelListener.SupportsDuplex && (typeof(TChannel) == typeof(IDuplexChannel)))
                {
                    obj2 = new SecurityChannelListener<TChannel>.SecurityDuplexChannel(securityChannelListener, (IDuplexChannel) innerChannel, securityProtocol, securityChannelListener.settingsLifetimeManager);
                }
                else if (securityChannelListener.SupportsDuplex && (typeof(TChannel) == typeof(IDuplexSessionChannel)))
                {
                    obj2 = new SecurityChannelListener<TChannel>.SecurityDuplexSessionChannel(securityChannelListener, (IDuplexSessionChannel) innerChannel, securityProtocol, securityChannelListener.settingsLifetimeManager);
                }
                else if (securityChannelListener.SupportsRequestReply && (typeof(TChannel) == typeof(IReplyChannel)))
                {
                    obj2 = new SecurityChannelListener<TChannel>.SecurityReplyChannel(securityChannelListener, (IReplyChannel) innerChannel, securityProtocol, securityChannelListener.settingsLifetimeManager);
                }
                else
                {
                    if (!securityChannelListener.SupportsRequestReply || (typeof(TChannel) != typeof(IReplySessionChannel)))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("UnsupportedChannelInterfaceType", new object[] { typeof(TChannel) })));
                    }
                    obj2 = new SecurityChannelListener<TChannel>.SecurityReplySessionChannel(securityChannelListener, (IReplySessionChannel) innerChannel, securityProtocol, securityChannelListener.settingsLifetimeManager);
                }
                return (TChannel) obj2;
            }

            private SecurityChannelListener<TChannel> SecurityChannelListener
            {
                get
                {
                    return (SecurityChannelListener<TChannel>) base.ChannelManager;
                }
            }
        }

        private class SecurityDuplexChannel : SecurityChannelListener<TChannel>.SecurityInputChannel, IDuplexChannel, IInputChannel, IOutputChannel, IChannel, ICommunicationObject
        {
            private readonly IDuplexChannel innerDuplexChannel;

            public SecurityDuplexChannel(ChannelManagerBase channelManager, IDuplexChannel innerChannel, SecurityProtocol securityProtocol, SecurityListenerSettingsLifetimeManager settingsLifetimeManager) : base(channelManager, innerChannel, securityProtocol, settingsLifetimeManager)
            {
                this.innerDuplexChannel = innerChannel;
            }

            public IAsyncResult BeginSend(Message message, AsyncCallback callback, object state)
            {
                return this.BeginSend(message, base.DefaultSendTimeout, callback, state);
            }

            public IAsyncResult BeginSend(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                base.ThrowIfFaulted();
                base.ThrowIfDisposedOrNotOpen(message);
                return new SecurityChannel<IInputChannel>.OutputChannelSendAsyncResult(message, base.SecurityProtocol, this.innerDuplexChannel, timeout, callback, state);
            }

            public void EndSend(IAsyncResult result)
            {
                SecurityChannel<IInputChannel>.OutputChannelSendAsyncResult.End(result);
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
                this.innerDuplexChannel.Send(message, helper.RemainingTime());
            }

            protected IDuplexChannel InnerDuplexChannel
            {
                get
                {
                    return this.innerDuplexChannel;
                }
            }

            public EndpointAddress RemoteAddress
            {
                get
                {
                    return this.innerDuplexChannel.RemoteAddress;
                }
            }

            public Uri Via
            {
                get
                {
                    return this.innerDuplexChannel.Via;
                }
            }
        }

        private sealed class SecurityDuplexSessionChannel : SecurityChannelListener<TChannel>.SecurityDuplexChannel, IDuplexSessionChannel, IDuplexChannel, IInputChannel, IOutputChannel, IChannel, ICommunicationObject, ISessionChannel<IDuplexSession>
        {
            private bool sendUnsecuredFaults;

            public SecurityDuplexSessionChannel(SecurityChannelListener<TChannel> channelManager, IDuplexSessionChannel innerChannel, SecurityProtocol securityProtocol, SecurityListenerSettingsLifetimeManager settingsLifetimeManager) : base(channelManager, innerChannel, securityProtocol, settingsLifetimeManager)
            {
                this.sendUnsecuredFaults = channelManager.SendUnsecuredFaults;
            }

            public override IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
            {
                if (base.DoneReceivingInCurrentState())
                {
                    return new DoneReceivingAsyncResult(callback, state);
                }
                return new SecurityChannelListener<TChannel>.DuplexSessionReceiveMessageAndVerifySecurityAsyncResult((SecurityChannelListener<TChannel>.SecurityDuplexSessionChannel) this, base.InnerDuplexChannel, timeout, callback, state);
            }

            public override bool EndTryReceive(IAsyncResult result, out Message message)
            {
                DoneReceivingAsyncResult result2 = result as DoneReceivingAsyncResult;
                if (result2 != null)
                {
                    return DoneReceivingAsyncResult.End(result2, out message);
                }
                return SecurityChannelListener<TChannel>.DuplexSessionReceiveMessageAndVerifySecurityAsyncResult.End(result, out message);
            }

            private void SendFaultIfRequired(Exception e, Message unverifiedMessage, TimeSpan timeout)
            {
                if (this.sendUnsecuredFaults)
                {
                    MessageFault fault = System.ServiceModel.Security.SecurityUtils.CreateSecurityMessageFault(e, base.SecurityProtocol.SecurityProtocolFactory.StandardsManager);
                    if (fault != null)
                    {
                        try
                        {
                            using (Message message = Message.CreateMessage(unverifiedMessage.Version, fault, unverifiedMessage.Version.Addressing.DefaultFaultAction))
                            {
                                if (unverifiedMessage.Headers.MessageId != null)
                                {
                                    message.InitializeReply(unverifiedMessage);
                                }
                                ((IDuplexChannel) base.InnerChannel).Send(message, timeout);
                            }
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                        }
                    }
                }
            }

            public override bool TryReceive(TimeSpan timeout, out Message message)
            {
                if (base.DoneReceivingInCurrentState())
                {
                    message = null;
                    return true;
                }
                TimeoutHelper helper = new TimeoutHelper(timeout);
            Label_0015:
                if ((base.State == CommunicationState.Closed) || (base.State == CommunicationState.Faulted))
                {
                    message = null;
                }
                else
                {
                    if (base.InnerChannel.TryReceive(helper.RemainingTime(), out message))
                    {
                        Message unverifiedMessage = message;
                        Exception e = null;
                        try
                        {
                            base.VerifyIncomingMessage(ref message, helper.RemainingTime());
                            goto Label_0087;
                        }
                        catch (MessageSecurityException exception2)
                        {
                            message = null;
                            e = exception2;
                        }
                        if (e == null)
                        {
                            goto Label_0015;
                        }
                        this.SendFaultIfRequired(e, unverifiedMessage, helper.RemainingTime());
                        if (!(helper.RemainingTime() == TimeSpan.Zero))
                        {
                            goto Label_0015;
                        }
                    }
                    return false;
                }
            Label_0087:
                base.ThrowIfFaulted();
                return true;
            }

            public bool SendUnsecuredFaults
            {
                get
                {
                    return this.sendUnsecuredFaults;
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

        private class SecurityInputChannel : SecurityChannelListener<TChannel>.ServerSecurityChannel<IInputChannel>, IInputChannel, IChannel, ICommunicationObject
        {
            public SecurityInputChannel(ChannelManagerBase channelManager, IInputChannel innerChannel, SecurityProtocol securityProtocol, SecurityListenerSettingsLifetimeManager settingsLifetimeManager) : base(channelManager, innerChannel, securityProtocol, settingsLifetimeManager)
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
                return new SecurityChannelListener<TChannel>.InputChannelReceiveMessageAndVerifySecurityAsyncResult((SecurityChannelListener<TChannel>.SecurityInputChannel) this, base.InnerChannel, timeout, callback, state);
            }

            public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return base.InnerChannel.BeginWaitForMessage(timeout, callback, state);
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
                return SecurityChannelListener<TChannel>.InputChannelReceiveMessageAndVerifySecurityAsyncResult.End(result, out message);
            }

            public bool EndWaitForMessage(IAsyncResult result)
            {
                return base.InnerChannel.EndWaitForMessage(result);
            }

            public Message Receive()
            {
                return this.Receive(base.DefaultReceiveTimeout);
            }

            public Message Receive(TimeSpan timeout)
            {
                return InputChannel.HelpReceive(this, timeout);
            }

            public virtual bool TryReceive(TimeSpan timeout, out Message message)
            {
                if (base.DoneReceivingInCurrentState())
                {
                    message = null;
                    return true;
                }
                TimeoutHelper helper = new TimeoutHelper(timeout);
            Label_0015:
                if ((base.State == CommunicationState.Closed) || (base.State == CommunicationState.Faulted))
                {
                    message = null;
                }
                else
                {
                    if (!base.InnerChannel.TryReceive(helper.RemainingTime(), out message))
                    {
                        return false;
                    }
                    try
                    {
                        base.VerifyIncomingMessage(ref message, helper.RemainingTime());
                    }
                    catch (MessageSecurityException)
                    {
                        message = null;
                        if (helper.RemainingTime() == TimeSpan.Zero)
                        {
                            return false;
                        }
                        goto Label_0015;
                    }
                }
                base.ThrowIfFaulted();
                return true;
            }

            public bool WaitForMessage(TimeSpan timeout)
            {
                return base.InnerChannel.WaitForMessage(timeout);
            }

            public EndpointAddress LocalAddress
            {
                get
                {
                    return base.InnerChannel.LocalAddress;
                }
            }
        }

        private sealed class SecurityInputSessionChannel : SecurityChannelListener<TChannel>.SecurityInputChannel, IInputSessionChannel, IInputChannel, IChannel, ICommunicationObject, ISessionChannel<IInputSession>
        {
            public SecurityInputSessionChannel(ChannelManagerBase channelManager, IInputSessionChannel innerChannel, SecurityProtocol securityProtocol, SecurityListenerSettingsLifetimeManager settingsLifetimeManager) : base(channelManager, innerChannel, securityProtocol, settingsLifetimeManager)
            {
            }

            public IInputSession Session
            {
                get
                {
                    return ((IInputSessionChannel) base.InnerChannel).Session;
                }
            }
        }

        private class SecurityReplyChannel : SecurityChannelListener<TChannel>.ServerSecurityChannel<IReplyChannel>, IReplyChannel, IChannel, ICommunicationObject
        {
            private bool sendUnsecuredFaults;

            public SecurityReplyChannel(SecurityChannelListener<TChannel> channelManager, IReplyChannel innerChannel, SecurityProtocol securityProtocol, SecurityListenerSettingsLifetimeManager settingsLifetimeManager) : base(channelManager, innerChannel, securityProtocol, settingsLifetimeManager)
            {
                this.sendUnsecuredFaults = channelManager.SendUnsecuredFaults;
            }

            public IAsyncResult BeginReceiveRequest(AsyncCallback callback, object state)
            {
                return this.BeginReceiveRequest(base.DefaultReceiveTimeout, callback, state);
            }

            public IAsyncResult BeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return ReplyChannel.HelpBeginReceiveRequest(this, timeout, callback, state);
            }

            public IAsyncResult BeginTryReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
            {
                if (base.DoneReceivingInCurrentState())
                {
                    return new DoneReceivingAsyncResult(callback, state);
                }
                return new SecurityChannelListener<TChannel>.ReceiveRequestAndVerifySecurityAsyncResult((SecurityChannelListener<TChannel>.SecurityReplyChannel) this, base.InnerChannel, timeout, callback, state);
            }

            public IAsyncResult BeginWaitForRequest(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return base.InnerChannel.BeginWaitForRequest(timeout, callback, state);
            }

            public RequestContext EndReceiveRequest(IAsyncResult result)
            {
                return ReplyChannel.HelpEndReceiveRequest(result);
            }

            public bool EndTryReceiveRequest(IAsyncResult result, out RequestContext requestContext)
            {
                DoneReceivingAsyncResult result2 = result as DoneReceivingAsyncResult;
                if (result2 != null)
                {
                    return DoneReceivingAsyncResult.End(result2, out requestContext);
                }
                return SecurityChannelListener<TChannel>.ReceiveRequestAndVerifySecurityAsyncResult.End(result, out requestContext);
            }

            public bool EndWaitForRequest(IAsyncResult result)
            {
                return base.InnerChannel.EndWaitForRequest(result);
            }

            internal RequestContext ProcessReceivedRequest(RequestContext requestContext, TimeSpan timeout)
            {
                if (requestContext == null)
                {
                    return null;
                }
                Message requestMessage = requestContext.RequestMessage;
                if (requestMessage == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new CommunicationException(System.ServiceModel.SR.GetString("ReceivedMessageInRequestContextNull", new object[] { base.InnerChannel })));
                }
                return new SecurityChannelListener<TChannel>.SecurityRequestContext(requestMessage, requestContext, base.SecurityProtocol, base.VerifyIncomingMessage(ref requestMessage, timeout, null), base.DefaultSendTimeout, this.DefaultCloseTimeout);
            }

            public RequestContext ReceiveRequest()
            {
                return this.ReceiveRequest(base.DefaultReceiveTimeout);
            }

            public RequestContext ReceiveRequest(TimeSpan timeout)
            {
                return ReplyChannel.HelpReceiveRequest(this, timeout);
            }

            private void SendFaultIfRequired(Exception e, RequestContext innerContext, TimeSpan timeout)
            {
                if (this.sendUnsecuredFaults)
                {
                    MessageFault fault = System.ServiceModel.Security.SecurityUtils.CreateSecurityMessageFault(e, base.SecurityProtocol.SecurityProtocolFactory.StandardsManager);
                    if (fault != null)
                    {
                        Message requestMessage = innerContext.RequestMessage;
                        Message message = Message.CreateMessage(requestMessage.Version, fault, requestMessage.Version.Addressing.DefaultFaultAction);
                        if (requestMessage.Headers.MessageId != null)
                        {
                            message.InitializeReply(requestMessage);
                        }
                        try
                        {
                            TimeoutHelper helper = new TimeoutHelper(timeout);
                            innerContext.Reply(message, helper.RemainingTime());
                            innerContext.Close(helper.RemainingTime());
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                        }
                        finally
                        {
                            message.Close();
                            innerContext.Abort();
                        }
                    }
                }
            }

            public bool TryReceiveRequest(TimeSpan timeout, out RequestContext requestContext)
            {
                if (base.DoneReceivingInCurrentState())
                {
                    requestContext = null;
                    return true;
                }
                requestContext = null;
                TimeoutHelper helper = new TimeoutHelper(timeout);
            Label_0018:
                if ((base.State == CommunicationState.Closed) || (base.State == CommunicationState.Faulted))
                {
                    requestContext = null;
                }
                else
                {
                    RequestContext context;
                    if (!base.InnerChannel.TryReceiveRequest(helper.RemainingTime(), out context))
                    {
                        requestContext = null;
                        return false;
                    }
                    Exception e = null;
                    try
                    {
                        requestContext = this.ProcessReceivedRequest(context, helper.RemainingTime());
                        goto Label_008A;
                    }
                    catch (MessageSecurityException exception2)
                    {
                        e = exception2;
                    }
                    if (e == null)
                    {
                        goto Label_0018;
                    }
                    this.SendFaultIfRequired(e, context, helper.RemainingTime());
                    if (!(helper.RemainingTime() == TimeSpan.Zero))
                    {
                        goto Label_0018;
                    }
                    return false;
                }
            Label_008A:
                base.ThrowIfFaulted();
                return true;
            }

            public bool WaitForRequest(TimeSpan timeout)
            {
                return base.InnerChannel.WaitForRequest(timeout);
            }

            public EndpointAddress LocalAddress
            {
                get
                {
                    return base.InnerChannel.LocalAddress;
                }
            }

            public bool SendUnsecuredFaults
            {
                get
                {
                    return this.sendUnsecuredFaults;
                }
            }
        }

        private sealed class SecurityReplySessionChannel : SecurityChannelListener<TChannel>.SecurityReplyChannel, IReplySessionChannel, IReplyChannel, IChannel, ICommunicationObject, ISessionChannel<IInputSession>
        {
            public SecurityReplySessionChannel(SecurityChannelListener<TChannel> channelManager, IReplySessionChannel innerChannel, SecurityProtocol securityProtocol, SecurityListenerSettingsLifetimeManager settingsLifetimeManager) : base(channelManager, innerChannel, securityProtocol, settingsLifetimeManager)
            {
            }

            public IInputSession Session
            {
                get
                {
                    return ((IReplySessionChannel) base.InnerChannel).Session;
                }
            }
        }

        private sealed class SecurityRequestContext : RequestContextBase
        {
            private readonly SecurityProtocolCorrelationState correlationState;
            private readonly RequestContext innerContext;
            private readonly SecurityProtocol securityProtocol;

            public SecurityRequestContext(Message requestMessage, RequestContext innerContext, SecurityProtocol securityProtocol, SecurityProtocolCorrelationState correlationState, TimeSpan defaultSendTimeout, TimeSpan defaultCloseTimeout) : base(requestMessage, defaultCloseTimeout, defaultSendTimeout)
            {
                this.innerContext = innerContext;
                this.securityProtocol = securityProtocol;
                this.correlationState = correlationState;
            }

            protected override void OnAbort()
            {
                this.innerContext.Abort();
            }

            protected override IAsyncResult OnBeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                if (message != null)
                {
                    return new RequestContextSendAsyncResult<TChannel>(message, this.securityProtocol, this.innerContext, timeout, callback, state, this.correlationState);
                }
                return this.innerContext.BeginReply(message, timeout, callback, state);
            }

            protected override void OnClose(TimeSpan timeout)
            {
                this.innerContext.Close(timeout);
            }

            protected override void OnEndReply(IAsyncResult result)
            {
                if (result is RequestContextSendAsyncResult<TChannel>)
                {
                    RequestContextSendAsyncResult<TChannel>.End(result);
                }
                else
                {
                    this.innerContext.EndReply(result);
                }
            }

            protected override void OnReply(Message message, TimeSpan timeout)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                if (message != null)
                {
                    this.securityProtocol.SecureOutgoingMessage(ref message, helper.RemainingTime(), this.correlationState);
                }
                this.innerContext.Reply(message, helper.RemainingTime());
            }

            private sealed class RequestContextSendAsyncResult : ApplySecurityAndSendAsyncResult<RequestContext>
            {
                public RequestContextSendAsyncResult(Message message, SecurityProtocol protocol, RequestContext context, TimeSpan timeout, AsyncCallback callback, object state, SecurityProtocolCorrelationState correlationState) : base(protocol, context, timeout, callback, state)
                {
                    base.Begin(message, correlationState);
                }

                protected override IAsyncResult BeginSendCore(RequestContext context, Message message, TimeSpan timeout, AsyncCallback callback, object state)
                {
                    return context.BeginReply(message, timeout, callback, state);
                }

                internal static void End(IAsyncResult result)
                {
                    SecurityChannelListener<TChannel>.SecurityRequestContext.RequestContextSendAsyncResult self = result as SecurityChannelListener<TChannel>.SecurityRequestContext.RequestContextSendAsyncResult;
                    ApplySecurityAndSendAsyncResult<RequestContext>.OnEnd(self);
                }

                protected override void EndSendCore(RequestContext context, IAsyncResult result)
                {
                    context.EndReply(result);
                }

                protected override void OnSendCompleteCore(TimeSpan timeout)
                {
                }
            }
        }

        private abstract class ServerSecurityChannel<UChannel> : SecurityChannel<UChannel> where UChannel: class, IChannel
        {
            private bool hasSecurityStateReference;
            private string secureConversationCloseAction;
            private static MessageFault secureConversationCloseNotSupportedFault;
            private SecurityListenerSettingsLifetimeManager settingsLifetimeManager;

            protected ServerSecurityChannel(ChannelManagerBase channelManager, UChannel innerChannel, SecurityProtocol securityProtocol, SecurityListenerSettingsLifetimeManager settingsLifetimeManager) : base(channelManager, innerChannel, securityProtocol)
            {
                if (settingsLifetimeManager == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("settingsLifetimeManager");
                }
                this.settingsLifetimeManager = settingsLifetimeManager;
            }

            [SecurityCritical]
            private IDisposable ApplyHostingIntegrationContext(Message message)
            {
                IDisposable disposable = null;
                IAspNetMessageProperty hostingProperty = AspNetEnvironment.Current.GetHostingProperty(message);
                if (hostingProperty != null)
                {
                    disposable = hostingProperty.ApplyIntegrationContext();
                }
                return disposable;
            }

            private static MessageFault GetSecureConversationCloseNotSupportedFault()
            {
                if (SecurityChannelListener<TChannel>.ServerSecurityChannel<UChannel>.secureConversationCloseNotSupportedFault == null)
                {
                    FaultCode code = FaultCode.CreateSenderFaultCode("SecureConversationCancellationNotAllowed", "http://schemas.microsoft.com/ws/2006/05/security");
                    FaultReason reason = new FaultReason(System.ServiceModel.SR.GetString("SecureConversationCancelNotAllowedFaultReason"), CultureInfo.InvariantCulture);
                    SecurityChannelListener<TChannel>.ServerSecurityChannel<UChannel>.secureConversationCloseNotSupportedFault = MessageFault.CreateFault(code, reason);
                }
                return SecurityChannelListener<TChannel>.ServerSecurityChannel<UChannel>.secureConversationCloseNotSupportedFault;
            }

            internal void InternalThrowIfFaulted()
            {
                base.ThrowIfFaulted();
            }

            protected override void OnAbort()
            {
                lock (base.ThisLock)
                {
                    if (this.hasSecurityStateReference)
                    {
                        this.hasSecurityStateReference = false;
                        this.settingsLifetimeManager.Abort();
                    }
                }
                base.OnAbort();
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new ChainedAsyncResult(timeout, callback, state, new ChainedBeginHandler(this.OnBeginCloseSharedState), new ChainedEndHandler(this.OnEndCloseSharedState), new ChainedBeginHandler(this.OnBeginClose), new ChainedEndHandler(this.OnEndClose));
            }

            private IAsyncResult OnBeginCloseSharedState(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new CloseSharedStateAsyncResult<TChannel, UChannel>((SecurityChannelListener<TChannel>.ServerSecurityChannel<UChannel>) this, timeout, callback, state);
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                base.SecurityProtocol.Open(helper.RemainingTime());
                return base.OnBeginOpen(helper.RemainingTime(), callback, state);
            }

            protected override void OnClose(TimeSpan timeout)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                lock (base.ThisLock)
                {
                    if (this.hasSecurityStateReference)
                    {
                        this.hasSecurityStateReference = false;
                        this.settingsLifetimeManager.Close(helper.RemainingTime());
                    }
                }
                base.OnClose(helper.RemainingTime());
            }

            protected override void OnEndClose(IAsyncResult result)
            {
                ChainedAsyncResult.End(result);
            }

            private void OnEndCloseSharedState(IAsyncResult result)
            {
                CloseSharedStateAsyncResult<TChannel, UChannel>.End(result);
            }

            protected override void OnEndOpen(IAsyncResult result)
            {
                base.OnEndOpen(result);
                lock (base.ThisLock)
                {
                    if ((base.State != CommunicationState.Closed) && (base.State != CommunicationState.Closing))
                    {
                        this.hasSecurityStateReference = true;
                        this.settingsLifetimeManager.AddReference();
                    }
                }
            }

            protected override void OnFaulted()
            {
                lock (base.ThisLock)
                {
                    if (this.hasSecurityStateReference)
                    {
                        this.hasSecurityStateReference = false;
                        this.settingsLifetimeManager.Abort();
                    }
                }
                base.OnFaulted();
            }

            protected override void OnOpen(TimeSpan timeout)
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                base.SecurityProtocol.Open(helper.RemainingTime());
                base.OnOpen(helper.RemainingTime());
                lock (base.ThisLock)
                {
                    if ((base.State != CommunicationState.Closed) && (base.State != CommunicationState.Closing))
                    {
                        this.hasSecurityStateReference = true;
                        this.settingsLifetimeManager.AddReference();
                    }
                }
            }

            protected override void OnOpened()
            {
                base.OnOpened();
                this.secureConversationCloseAction = base.SecurityProtocol.SecurityProtocolFactory.StandardsManager.SecureConversationDriver.CloseAction.Value;
            }

            private void ThrowIfSecureConversationCloseMessage(Message message)
            {
                if (message.Headers.Action == this.secureConversationCloseAction)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperWarning(new MessageSecurityException(System.ServiceModel.SR.GetString("SecureConversationCancelNotAllowedFaultReason"), null, SecurityChannelListener<TChannel>.ServerSecurityChannel<UChannel>.GetSecureConversationCloseNotSupportedFault()));
                }
            }

            [SecuritySafeCritical]
            internal void VerifyIncomingMessage(ref Message message, TimeSpan timeout)
            {
                if (message != null)
                {
                    this.ThrowIfSecureConversationCloseMessage(message);
                    using (this.ApplyHostingIntegrationContext(message))
                    {
                        base.SecurityProtocol.VerifyIncomingMessage(ref message, timeout);
                    }
                }
            }

            [SecuritySafeCritical]
            internal SecurityProtocolCorrelationState VerifyIncomingMessage(ref Message message, TimeSpan timeout, params SecurityProtocolCorrelationState[] correlationState)
            {
                if (message == null)
                {
                    return null;
                }
                this.ThrowIfSecureConversationCloseMessage(message);
                using (this.ApplyHostingIntegrationContext(message))
                {
                    return base.SecurityProtocol.VerifyIncomingMessage(ref message, timeout, correlationState);
                }
            }

            private class CloseSharedStateAsyncResult : AsyncResult
            {
                private static AsyncCallback lifetimeManagerCloseCallback;
                private SecurityChannelListener<TChannel>.ServerSecurityChannel<UChannel> securityChannel;

                static CloseSharedStateAsyncResult()
                {
                    SecurityChannelListener<TChannel>.ServerSecurityChannel<UChannel>.CloseSharedStateAsyncResult.lifetimeManagerCloseCallback = Fx.ThunkCallback(new AsyncCallback(SecurityChannelListener<TChannel>.ServerSecurityChannel<UChannel>.CloseSharedStateAsyncResult.LifetimeManagerCloseCallback));
                }

                public CloseSharedStateAsyncResult(SecurityChannelListener<TChannel>.ServerSecurityChannel<UChannel> securityChannel, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.securityChannel = securityChannel;
                    lock (this.securityChannel.ThisLock)
                    {
                        if (this.securityChannel.hasSecurityStateReference)
                        {
                            this.securityChannel.hasSecurityStateReference = false;
                            IAsyncResult result = this.securityChannel.settingsLifetimeManager.BeginClose(timeout, SecurityChannelListener<TChannel>.ServerSecurityChannel<UChannel>.CloseSharedStateAsyncResult.lifetimeManagerCloseCallback, this);
                            if (!result.CompletedSynchronously)
                            {
                                return;
                            }
                            this.securityChannel.settingsLifetimeManager.EndClose(result);
                        }
                    }
                    base.Complete(true);
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<SecurityChannelListener<TChannel>.ServerSecurityChannel<UChannel>.CloseSharedStateAsyncResult>(result);
                }

                private static void LifetimeManagerCloseCallback(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        SecurityChannelListener<TChannel>.ServerSecurityChannel<UChannel>.CloseSharedStateAsyncResult asyncState = (SecurityChannelListener<TChannel>.ServerSecurityChannel<UChannel>.CloseSharedStateAsyncResult) result.AsyncState;
                        Exception exception = null;
                        try
                        {
                            asyncState.securityChannel.settingsLifetimeManager.EndClose(result);
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

