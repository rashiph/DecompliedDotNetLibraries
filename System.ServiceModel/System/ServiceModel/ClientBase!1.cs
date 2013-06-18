namespace System.ServiceModel
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Remoting.Messaging;
    using System.Security;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.Threading;

    public abstract class ClientBase<TChannel> : ICommunicationObject, IDisposable where TChannel: class
    {
        private bool canShareFactory;
        private TChannel channel;
        private ChannelFactoryRef<TChannel> channelFactoryRef;
        private bool channelFactoryRefReleased;
        private EndpointTrait<TChannel> endpointTrait;
        private static ChannelFactoryRefCache<TChannel> factoryRefCache;
        private object finalizeLock;
        private static AsyncCallback onAsyncCallCompleted;
        private bool releasedLastRef;
        private bool sharingFinalized;
        private static object staticLock;
        private object syncRoot;
        private bool useCachedFactory;

        event EventHandler ICommunicationObject.Closed
        {
            add
            {
                this.InnerChannel.Closed += value;
            }
            remove
            {
                this.InnerChannel.Closed -= value;
            }
        }

        event EventHandler ICommunicationObject.Closing
        {
            add
            {
                this.InnerChannel.Closing += value;
            }
            remove
            {
                this.InnerChannel.Closing -= value;
            }
        }

        event EventHandler ICommunicationObject.Faulted
        {
            add
            {
                this.InnerChannel.Faulted += value;
            }
            remove
            {
                this.InnerChannel.Faulted -= value;
            }
        }

        event EventHandler ICommunicationObject.Opened
        {
            add
            {
                this.InnerChannel.Opened += value;
            }
            remove
            {
                this.InnerChannel.Opened -= value;
            }
        }

        event EventHandler ICommunicationObject.Opening
        {
            add
            {
                this.InnerChannel.Opening += value;
            }
            remove
            {
                this.InnerChannel.Opening -= value;
            }
        }

        static ClientBase()
        {
            ClientBase<TChannel>.factoryRefCache = new ChannelFactoryRefCache<TChannel>(0x20);
            ClientBase<TChannel>.staticLock = new object();
            ClientBase<TChannel>.onAsyncCallCompleted = Fx.ThunkCallback(new AsyncCallback(ClientBase<TChannel>.OnAsyncCallCompleted));
        }

        protected ClientBase()
        {
            this.canShareFactory = true;
            this.syncRoot = new object();
            this.finalizeLock = new object();
            this.endpointTrait = new EndpointTrait<TChannel>("*", null, null);
            this.InitializeChannelFactoryRef();
        }

        protected ClientBase(ServiceEndpoint endpoint)
        {
            this.canShareFactory = true;
            this.syncRoot = new object();
            this.finalizeLock = new object();
            if (endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
            }
            this.channelFactoryRef = new ChannelFactoryRef<TChannel>(new ChannelFactory<TChannel>(endpoint));
            this.channelFactoryRef.ChannelFactory.TraceOpenAndClose = false;
            this.TryDisableSharing();
        }

        protected ClientBase(InstanceContext callbackInstance)
        {
            this.canShareFactory = true;
            this.syncRoot = new object();
            this.finalizeLock = new object();
            if (callbackInstance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackInstance");
            }
            this.endpointTrait = new EndpointTrait<TChannel>("*", null, callbackInstance);
            this.InitializeChannelFactoryRef();
        }

        protected ClientBase(string endpointConfigurationName)
        {
            this.canShareFactory = true;
            this.syncRoot = new object();
            this.finalizeLock = new object();
            if (endpointConfigurationName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointConfigurationName");
            }
            this.endpointTrait = new EndpointTrait<TChannel>(endpointConfigurationName, null, null);
            this.InitializeChannelFactoryRef();
        }

        protected ClientBase(Binding binding, EndpointAddress remoteAddress)
        {
            this.canShareFactory = true;
            this.syncRoot = new object();
            this.finalizeLock = new object();
            if (binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            }
            if (remoteAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("remoteAddress");
            }
            this.channelFactoryRef = new ChannelFactoryRef<TChannel>(new ChannelFactory<TChannel>(binding, remoteAddress));
            this.channelFactoryRef.ChannelFactory.TraceOpenAndClose = false;
            this.TryDisableSharing();
        }

        protected ClientBase(InstanceContext callbackInstance, ServiceEndpoint endpoint)
        {
            this.canShareFactory = true;
            this.syncRoot = new object();
            this.finalizeLock = new object();
            if (callbackInstance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackInstance");
            }
            if (endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
            }
            this.channelFactoryRef = new ChannelFactoryRef<TChannel>(new DuplexChannelFactory<TChannel>(callbackInstance, endpoint));
            this.channelFactoryRef.ChannelFactory.TraceOpenAndClose = false;
            this.TryDisableSharing();
        }

        protected ClientBase(InstanceContext callbackInstance, string endpointConfigurationName)
        {
            this.canShareFactory = true;
            this.syncRoot = new object();
            this.finalizeLock = new object();
            if (callbackInstance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackInstance");
            }
            if (endpointConfigurationName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointConfigurationName");
            }
            this.endpointTrait = new EndpointTrait<TChannel>(endpointConfigurationName, null, callbackInstance);
            this.InitializeChannelFactoryRef();
        }

        protected ClientBase(string endpointConfigurationName, EndpointAddress remoteAddress)
        {
            this.canShareFactory = true;
            this.syncRoot = new object();
            this.finalizeLock = new object();
            if (endpointConfigurationName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointConfigurationName");
            }
            if (remoteAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("remoteAddress");
            }
            this.endpointTrait = new EndpointTrait<TChannel>(endpointConfigurationName, remoteAddress, null);
            this.InitializeChannelFactoryRef();
        }

        protected ClientBase(string endpointConfigurationName, string remoteAddress)
        {
            this.canShareFactory = true;
            this.syncRoot = new object();
            this.finalizeLock = new object();
            if (endpointConfigurationName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointConfigurationName");
            }
            if (remoteAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("remoteAddress");
            }
            this.endpointTrait = new EndpointTrait<TChannel>(endpointConfigurationName, new EndpointAddress(remoteAddress), null);
            this.InitializeChannelFactoryRef();
        }

        protected ClientBase(InstanceContext callbackInstance, Binding binding, EndpointAddress remoteAddress)
        {
            this.canShareFactory = true;
            this.syncRoot = new object();
            this.finalizeLock = new object();
            if (callbackInstance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackInstance");
            }
            if (binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
            }
            if (remoteAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("remoteAddress");
            }
            this.channelFactoryRef = new ChannelFactoryRef<TChannel>(new DuplexChannelFactory<TChannel>(callbackInstance, binding, remoteAddress));
            this.channelFactoryRef.ChannelFactory.TraceOpenAndClose = false;
            this.TryDisableSharing();
        }

        protected ClientBase(InstanceContext callbackInstance, string endpointConfigurationName, EndpointAddress remoteAddress)
        {
            this.canShareFactory = true;
            this.syncRoot = new object();
            this.finalizeLock = new object();
            if (callbackInstance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackInstance");
            }
            if (endpointConfigurationName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointConfigurationName");
            }
            if (remoteAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("remoteAddress");
            }
            this.endpointTrait = new EndpointTrait<TChannel>(endpointConfigurationName, remoteAddress, callbackInstance);
            this.InitializeChannelFactoryRef();
        }

        protected ClientBase(InstanceContext callbackInstance, string endpointConfigurationName, string remoteAddress)
        {
            this.canShareFactory = true;
            this.syncRoot = new object();
            this.finalizeLock = new object();
            if (callbackInstance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackInstance");
            }
            if (endpointConfigurationName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointConfigurationName");
            }
            if (remoteAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("remoteAddress");
            }
            this.endpointTrait = new EndpointTrait<TChannel>(endpointConfigurationName, new EndpointAddress(remoteAddress), callbackInstance);
            this.InitializeChannelFactoryRef();
        }

        public void Abort()
        {
            IChannel channel = (IChannel) this.channel;
            if (channel != null)
            {
                channel.Abort();
            }
            if (!this.channelFactoryRefReleased)
            {
                lock (ClientBase<TChannel>.staticLock)
                {
                    if (!this.channelFactoryRefReleased)
                    {
                        if (this.channelFactoryRef.Release())
                        {
                            this.releasedLastRef = true;
                        }
                        this.channelFactoryRefReleased = true;
                    }
                }
            }
            if (this.releasedLastRef)
            {
                this.channelFactoryRef.Abort();
            }
        }

        internal IAsyncResult BeginChannelClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.channel != null)
            {
                return this.InnerChannel.BeginClose(timeout, callback, state);
            }
            return new CompletedAsyncResult(callback, state);
        }

        internal IAsyncResult BeginChannelOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.InnerChannel.BeginOpen(timeout, callback, state);
        }

        internal IAsyncResult BeginFactoryClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.useCachedFactory)
            {
                return new CompletedAsyncResult(callback, state);
            }
            return this.GetChannelFactory().BeginClose(timeout, callback, state);
        }

        internal IAsyncResult BeginFactoryOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (this.useCachedFactory)
            {
                return new CompletedAsyncResult(callback, state);
            }
            return this.GetChannelFactory().BeginOpen(timeout, callback, state);
        }

        public void Close()
        {
            ((ICommunicationObject) this).Close(this.GetChannelFactory().InternalCloseTimeout);
        }

        private static void CompleteAsyncCall(AsyncOperationContext<TChannel> context, object[] results, Exception error)
        {
            if (context.CompletionCallback != null)
            {
                InvokeAsyncCompletedEventArgs<TChannel> arg = new InvokeAsyncCompletedEventArgs<TChannel>(results, error, false, context.AsyncOperation.UserSuppliedState);
                context.AsyncOperation.PostOperationCompleted(context.CompletionCallback, arg);
            }
            else
            {
                context.AsyncOperation.OperationCompleted();
            }
        }

        protected virtual TChannel CreateChannel()
        {
            if (this.sharingFinalized)
            {
                return this.GetChannelFactory().CreateChannel();
            }
            lock (this.finalizeLock)
            {
                this.sharingFinalized = true;
                return this.GetChannelFactory().CreateChannel();
            }
        }

        private static ChannelFactoryRef<TChannel> CreateChannelFactoryRef(EndpointTrait<TChannel> endpointTrait)
        {
            ChannelFactory<TChannel> channelFactory = endpointTrait.CreateChannelFactory();
            channelFactory.TraceOpenAndClose = false;
            return new ChannelFactoryRef<TChannel>(channelFactory);
        }

        private void CreateChannelInternal()
        {
            try
            {
                this.channel = this.CreateChannel();
                if ((this.sharingFinalized && this.canShareFactory) && !this.useCachedFactory)
                {
                    this.TryAddChannelFactoryToCache();
                }
            }
            finally
            {
                if (!this.sharingFinalized)
                {
                    this.TryDisableSharing();
                }
            }
        }

        public void DisplayInitializationUI()
        {
            this.InnerChannel.DisplayInitializationUI();
        }

        internal void EndChannelClose(IAsyncResult result)
        {
            if (typeof(CompletedAsyncResult).IsAssignableFrom(result.GetType()))
            {
                CompletedAsyncResult.End(result);
            }
            else
            {
                this.InnerChannel.EndClose(result);
            }
        }

        internal void EndChannelOpen(IAsyncResult result)
        {
            this.InnerChannel.EndOpen(result);
        }

        internal void EndFactoryClose(IAsyncResult result)
        {
            if (typeof(CompletedAsyncResult).IsAssignableFrom(result.GetType()))
            {
                CompletedAsyncResult.End(result);
            }
            else
            {
                this.GetChannelFactory().EndClose(result);
            }
        }

        internal void EndFactoryOpen(IAsyncResult result)
        {
            if (this.useCachedFactory)
            {
                CompletedAsyncResult.End(result);
            }
            else
            {
                this.GetChannelFactory().EndOpen(result);
            }
        }

        private ChannelFactory<TChannel> GetChannelFactory()
        {
            return this.channelFactoryRef.ChannelFactory;
        }

        protected T GetDefaultValueForInitialization<T>()
        {
            return default(T);
        }

        private void InitializeChannelFactoryRef()
        {
            lock (ClientBase<TChannel>.staticLock)
            {
                ChannelFactoryRef<TChannel> ref2;
                if (ClientBase<TChannel>.factoryRefCache.TryGetValue(this.endpointTrait, out ref2))
                {
                    if (ref2.ChannelFactory.State != CommunicationState.Opened)
                    {
                        ClientBase<TChannel>.factoryRefCache.Remove(this.endpointTrait);
                    }
                    else
                    {
                        this.channelFactoryRef = ref2;
                        this.channelFactoryRef.AddRef();
                        this.useCachedFactory = true;
                        return;
                    }
                }
            }
            if (this.channelFactoryRef == null)
            {
                this.channelFactoryRef = ClientBase<TChannel>.CreateChannelFactoryRef(this.endpointTrait);
            }
        }

        private void InvalidateCacheAndCreateChannel()
        {
            this.RemoveFactoryFromCache();
            this.TryDisableSharing();
            this.CreateChannelInternal();
        }

        protected void InvokeAsync(BeginOperationDelegate<TChannel> beginOperationDelegate, object[] inValues, EndOperationDelegate<TChannel> endOperationDelegate, SendOrPostCallback operationCompletedCallback, object userState)
        {
            if (beginOperationDelegate == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("beginOperationDelegate");
            }
            if (endOperationDelegate == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endOperationDelegate");
            }
            AsyncOperationContext<TChannel> state = new AsyncOperationContext<TChannel>(AsyncOperationManager.CreateOperation(userState), endOperationDelegate, operationCompletedCallback);
            Exception error = null;
            object[] results = null;
            IAsyncResult result = null;
            try
            {
                result = beginOperationDelegate(inValues, ClientBase<TChannel>.onAsyncCallCompleted, state);
                if (result.CompletedSynchronously)
                {
                    results = endOperationDelegate(result);
                }
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                error = exception2;
            }
            if ((error != null) || result.CompletedSynchronously)
            {
                ClientBase<TChannel>.CompleteAsyncCall(state, results, error);
            }
        }

        private static void OnAsyncCallCompleted(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                AsyncOperationContext<TChannel> asyncState = (AsyncOperationContext<TChannel>) result.AsyncState;
                Exception error = null;
                object[] results = null;
                try
                {
                    results = asyncState.EndDelegate(result);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2))
                    {
                        throw;
                    }
                    error = exception2;
                }
                ClientBase<TChannel>.CompleteAsyncCall(asyncState, results, error);
            }
        }

        public void Open()
        {
            ((ICommunicationObject) this).Open(this.GetChannelFactory().InternalOpenTimeout);
        }

        private void RemoveFactoryFromCache()
        {
            lock (ClientBase<TChannel>.staticLock)
            {
                ChannelFactoryRef<TChannel> ref2;
                if (ClientBase<TChannel>.factoryRefCache.TryGetValue(this.endpointTrait, out ref2) && object.ReferenceEquals(this.channelFactoryRef, ref2))
                {
                    ClientBase<TChannel>.factoryRefCache.Remove(this.endpointTrait);
                }
            }
        }

        void IDisposable.Dispose()
        {
            this.Close();
        }

        IAsyncResult ICommunicationObject.BeginClose(AsyncCallback callback, object state)
        {
            return ((ICommunicationObject) this).BeginClose(this.GetChannelFactory().InternalCloseTimeout, callback, state);
        }

        IAsyncResult ICommunicationObject.BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedAsyncResult(timeout, callback, state, new ChainedBeginHandler(this.BeginChannelClose), new ChainedEndHandler(this.EndChannelClose), new ChainedBeginHandler(this.BeginFactoryClose), new ChainedEndHandler(this.EndFactoryClose));
        }

        IAsyncResult ICommunicationObject.BeginOpen(AsyncCallback callback, object state)
        {
            return ((ICommunicationObject) this).BeginOpen(this.GetChannelFactory().InternalOpenTimeout, callback, state);
        }

        IAsyncResult ICommunicationObject.BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new ChainedAsyncResult(timeout, callback, state, new ChainedBeginHandler(this.BeginFactoryOpen), new ChainedEndHandler(this.EndFactoryOpen), new ChainedBeginHandler(this.BeginChannelOpen), new ChainedEndHandler(this.EndChannelOpen));
        }

        void ICommunicationObject.Close(TimeSpan timeout)
        {
            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivityCloseClientBase", new object[] { typeof(TChannel).FullName }), ActivityType.Close);
                }
                TimeoutHelper helper = new TimeoutHelper(timeout);
                if (this.channel != null)
                {
                    this.InnerChannel.Close(helper.RemainingTime());
                }
                if (!this.channelFactoryRefReleased)
                {
                    lock (ClientBase<TChannel>.staticLock)
                    {
                        if (!this.channelFactoryRefReleased)
                        {
                            if (this.channelFactoryRef.Release())
                            {
                                this.releasedLastRef = true;
                            }
                            this.channelFactoryRefReleased = true;
                        }
                    }
                    if (this.releasedLastRef)
                    {
                        if (this.useCachedFactory)
                        {
                            this.channelFactoryRef.Abort();
                        }
                        else
                        {
                            this.channelFactoryRef.Close(helper.RemainingTime());
                        }
                    }
                }
            }
        }

        void ICommunicationObject.EndClose(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        void ICommunicationObject.EndOpen(IAsyncResult result)
        {
            ChainedAsyncResult.End(result);
        }

        void ICommunicationObject.Open(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (!this.useCachedFactory)
            {
                this.GetChannelFactory().Open(helper.RemainingTime());
            }
            this.InnerChannel.Open(helper.RemainingTime());
        }

        private void TryAddChannelFactoryToCache()
        {
            lock (ClientBase<TChannel>.staticLock)
            {
                ChannelFactoryRef<TChannel> ref2;
                if (!ClientBase<TChannel>.factoryRefCache.TryGetValue(this.endpointTrait, out ref2))
                {
                    this.channelFactoryRef.AddRef();
                    ClientBase<TChannel>.factoryRefCache.Add(this.endpointTrait, this.channelFactoryRef);
                    this.useCachedFactory = true;
                }
            }
        }

        private void TryDisableSharing()
        {
            if (!this.sharingFinalized)
            {
                lock (this.finalizeLock)
                {
                    if (!this.sharingFinalized)
                    {
                        this.canShareFactory = false;
                        this.sharingFinalized = true;
                        if (this.useCachedFactory)
                        {
                            ChannelFactoryRef<TChannel> channelFactoryRef = this.channelFactoryRef;
                            this.channelFactoryRef = ClientBase<TChannel>.CreateChannelFactoryRef(this.endpointTrait);
                            this.useCachedFactory = false;
                            lock (ClientBase<TChannel>.staticLock)
                            {
                                if (!channelFactoryRef.Release())
                                {
                                    channelFactoryRef = null;
                                }
                            }
                            if (channelFactoryRef != null)
                            {
                                channelFactoryRef.Abort();
                            }
                        }
                    }
                }
            }
        }

        protected TChannel Channel
        {
            get
            {
                if (this.channel == null)
                {
                    lock (this.ThisLock)
                    {
                        if (this.channel == null)
                        {
                            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
                            {
                                if (DiagnosticUtility.ShouldUseActivity)
                                {
                                    ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivityOpenClientBase", new object[] { typeof(TChannel).FullName }), ActivityType.OpenClient);
                                }
                                if (this.useCachedFactory)
                                {
                                    try
                                    {
                                        this.CreateChannelInternal();
                                    }
                                    catch (Exception exception)
                                    {
                                        if (!this.useCachedFactory || ((!(exception is CommunicationException) && !(exception is ObjectDisposedException)) && !(exception is TimeoutException)))
                                        {
                                            throw;
                                        }
                                        DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                                        this.InvalidateCacheAndCreateChannel();
                                    }
                                }
                                else
                                {
                                    this.CreateChannelInternal();
                                }
                            }
                        }
                    }
                }
                return this.channel;
            }
        }

        public ChannelFactory<TChannel> ChannelFactory
        {
            get
            {
                this.TryDisableSharing();
                return this.GetChannelFactory();
            }
        }

        public System.ServiceModel.Description.ClientCredentials ClientCredentials
        {
            get
            {
                this.TryDisableSharing();
                return this.ChannelFactory.Credentials;
            }
        }

        public ServiceEndpoint Endpoint
        {
            get
            {
                this.TryDisableSharing();
                return this.GetChannelFactory().Endpoint;
            }
        }

        public IClientChannel InnerChannel
        {
            get
            {
                return (IClientChannel) this.Channel;
            }
        }

        public CommunicationState State
        {
            get
            {
                IChannel channel = (IChannel) this.channel;
                if (channel != null)
                {
                    return channel.State;
                }
                if (!this.useCachedFactory)
                {
                    return this.GetChannelFactory().State;
                }
                return CommunicationState.Created;
            }
        }

        private object ThisLock
        {
            get
            {
                return this.syncRoot;
            }
        }

        private class AsyncOperationContext
        {
            private System.ComponentModel.AsyncOperation asyncOperation;
            private SendOrPostCallback completionCallback;
            private ClientBase<TChannel>.EndOperationDelegate endDelegate;

            internal AsyncOperationContext(System.ComponentModel.AsyncOperation asyncOperation, ClientBase<TChannel>.EndOperationDelegate endDelegate, SendOrPostCallback completionCallback)
            {
                this.asyncOperation = asyncOperation;
                this.endDelegate = endDelegate;
                this.completionCallback = completionCallback;
            }

            internal System.ComponentModel.AsyncOperation AsyncOperation
            {
                get
                {
                    return this.asyncOperation;
                }
            }

            internal SendOrPostCallback CompletionCallback
            {
                get
                {
                    return this.completionCallback;
                }
            }

            internal ClientBase<TChannel>.EndOperationDelegate EndDelegate
            {
                get
                {
                    return this.endDelegate;
                }
            }
        }

        protected delegate IAsyncResult BeginOperationDelegate(object[] inValues, AsyncCallback asyncCallback, object state);

        protected class ChannelBase<T> : IClientChannel, IContextChannel, IExtensibleObject<IContextChannel>, IDisposable, IOutputChannel, IRequestChannel, IChannel, ICommunicationObject, IChannelBaseProxy where T: class
        {
            private ServiceChannel channel;
            private ImmutableClientRuntime runtime;

            event EventHandler<UnknownMessageReceivedEventArgs> IClientChannel.UnknownMessageReceived
            {
                add
                {
                    this.channel.UnknownMessageReceived += value;
                }
                remove
                {
                    this.channel.UnknownMessageReceived -= value;
                }
            }

            event EventHandler ICommunicationObject.Closed
            {
                add
                {
                    this.channel.Closed += value;
                }
                remove
                {
                    this.channel.Closed -= value;
                }
            }

            event EventHandler ICommunicationObject.Closing
            {
                add
                {
                    this.channel.Closing += value;
                }
                remove
                {
                    this.channel.Closing -= value;
                }
            }

            event EventHandler ICommunicationObject.Faulted
            {
                add
                {
                    this.channel.Faulted += value;
                }
                remove
                {
                    this.channel.Faulted -= value;
                }
            }

            event EventHandler ICommunicationObject.Opened
            {
                add
                {
                    this.channel.Opened += value;
                }
                remove
                {
                    this.channel.Opened -= value;
                }
            }

            event EventHandler ICommunicationObject.Opening
            {
                add
                {
                    this.channel.Opening += value;
                }
                remove
                {
                    this.channel.Opening -= value;
                }
            }

            protected ChannelBase(ClientBase<T> client)
            {
                if (client.Endpoint.Address == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxChannelFactoryEndpointAddressUri")));
                }
                ChannelFactory<T> channelFactory = client.ChannelFactory;
                channelFactory.EnsureOpened();
                this.channel = channelFactory.ServiceChannelFactory.CreateServiceChannel(client.Endpoint.Address, client.Endpoint.Address.Uri);
                this.channel.InstanceContext = channelFactory.CallbackInstance;
                this.runtime = this.channel.ClientRuntime.GetRuntime();
            }

            [SecuritySafeCritical]
            protected IAsyncResult BeginInvoke(string methodName, object[] args, AsyncCallback callback, object state)
            {
                object[] destinationArray = new object[args.Length + 2];
                Array.Copy(args, destinationArray, args.Length);
                destinationArray[destinationArray.Length - 2] = callback;
                destinationArray[destinationArray.Length - 1] = state;
                IMethodCallMessage methodCall = new MethodCallMessage<TChannel, T>(destinationArray);
                ProxyOperationRuntime operationByName = this.GetOperationByName(methodName);
                object[] ins = operationByName.MapAsyncBeginInputs(methodCall, out callback, out state);
                return this.channel.BeginCall(operationByName.Action, operationByName.IsOneWay, operationByName, ins, callback, state);
            }

            [SecuritySafeCritical]
            protected object EndInvoke(string methodName, object[] args, IAsyncResult result)
            {
                object[] objArray2;
                object[] destinationArray = new object[args.Length + 1];
                Array.Copy(args, destinationArray, args.Length);
                destinationArray[destinationArray.Length - 1] = result;
                IMethodCallMessage methodCall = new MethodCallMessage<TChannel, T>(destinationArray);
                ProxyOperationRuntime operationByName = this.GetOperationByName(methodName);
                operationByName.MapAsyncEndInputs(methodCall, out result, out objArray2);
                object ret = this.channel.EndCall(operationByName.Action, objArray2, result);
                object[] sourceArray = operationByName.MapAsyncOutputs(methodCall, objArray2, ref ret);
                if (sourceArray != null)
                {
                    Array.Copy(sourceArray, args, args.Length);
                }
                return ret;
            }

            private ProxyOperationRuntime GetOperationByName(string methodName)
            {
                ProxyOperationRuntime operationByName = this.runtime.GetOperationByName(methodName);
                if (operationByName == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(System.ServiceModel.SR.GetString("SFxMethodNotSupported1", new object[] { methodName })));
                }
                return operationByName;
            }

            void IDisposable.Dispose()
            {
                ((IDisposable) this.channel).Dispose();
            }

            TProperty IChannel.GetProperty<TProperty>() where TProperty: class
            {
                return this.channel.GetProperty<TProperty>();
            }

            IAsyncResult IOutputChannel.BeginSend(System.ServiceModel.Channels.Message message, AsyncCallback callback, object state)
            {
                return this.channel.BeginSend(message, callback, state);
            }

            IAsyncResult IOutputChannel.BeginSend(System.ServiceModel.Channels.Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.channel.BeginSend(message, timeout, callback, state);
            }

            void IOutputChannel.EndSend(IAsyncResult result)
            {
                this.channel.EndSend(result);
            }

            void IOutputChannel.Send(System.ServiceModel.Channels.Message message)
            {
                this.channel.Send(message);
            }

            void IOutputChannel.Send(System.ServiceModel.Channels.Message message, TimeSpan timeout)
            {
                this.channel.Send(message, timeout);
            }

            IAsyncResult IRequestChannel.BeginRequest(System.ServiceModel.Channels.Message message, AsyncCallback callback, object state)
            {
                return this.channel.BeginRequest(message, callback, state);
            }

            IAsyncResult IRequestChannel.BeginRequest(System.ServiceModel.Channels.Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.channel.BeginRequest(message, timeout, callback, state);
            }

            System.ServiceModel.Channels.Message IRequestChannel.EndRequest(IAsyncResult result)
            {
                return this.channel.EndRequest(result);
            }

            System.ServiceModel.Channels.Message IRequestChannel.Request(System.ServiceModel.Channels.Message message)
            {
                return this.channel.Request(message);
            }

            System.ServiceModel.Channels.Message IRequestChannel.Request(System.ServiceModel.Channels.Message message, TimeSpan timeout)
            {
                return this.channel.Request(message, timeout);
            }

            ServiceChannel IChannelBaseProxy.GetServiceChannel()
            {
                return this.channel;
            }

            IAsyncResult IClientChannel.BeginDisplayInitializationUI(AsyncCallback callback, object state)
            {
                return this.channel.BeginDisplayInitializationUI(callback, state);
            }

            void IClientChannel.DisplayInitializationUI()
            {
                this.channel.DisplayInitializationUI();
            }

            void IClientChannel.EndDisplayInitializationUI(IAsyncResult result)
            {
                this.channel.EndDisplayInitializationUI(result);
            }

            void ICommunicationObject.Abort()
            {
                this.channel.Abort();
            }

            IAsyncResult ICommunicationObject.BeginClose(AsyncCallback callback, object state)
            {
                return this.channel.BeginClose(callback, state);
            }

            IAsyncResult ICommunicationObject.BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.channel.BeginClose(timeout, callback, state);
            }

            IAsyncResult ICommunicationObject.BeginOpen(AsyncCallback callback, object state)
            {
                return this.channel.BeginOpen(callback, state);
            }

            IAsyncResult ICommunicationObject.BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.channel.BeginOpen(timeout, callback, state);
            }

            void ICommunicationObject.Close()
            {
                this.channel.Close();
            }

            void ICommunicationObject.Close(TimeSpan timeout)
            {
                this.channel.Close(timeout);
            }

            void ICommunicationObject.EndClose(IAsyncResult result)
            {
                this.channel.EndClose(result);
            }

            void ICommunicationObject.EndOpen(IAsyncResult result)
            {
                this.channel.EndOpen(result);
            }

            void ICommunicationObject.Open()
            {
                this.channel.Open();
            }

            void ICommunicationObject.Open(TimeSpan timeout)
            {
                this.channel.Open(timeout);
            }

            EndpointAddress IOutputChannel.RemoteAddress
            {
                get
                {
                    return this.channel.RemoteAddress;
                }
            }

            Uri IOutputChannel.Via
            {
                get
                {
                    return this.channel.Via;
                }
            }

            EndpointAddress IRequestChannel.RemoteAddress
            {
                get
                {
                    return this.channel.RemoteAddress;
                }
            }

            Uri IRequestChannel.Via
            {
                get
                {
                    return this.channel.Via;
                }
            }

            bool IClientChannel.AllowInitializationUI
            {
                get
                {
                    return ((IClientChannel) this.channel).AllowInitializationUI;
                }
                set
                {
                    ((IClientChannel) this.channel).AllowInitializationUI = value;
                }
            }

            bool IClientChannel.DidInteractiveInitialization
            {
                get
                {
                    return ((IClientChannel) this.channel).DidInteractiveInitialization;
                }
            }

            Uri IClientChannel.Via
            {
                get
                {
                    return this.channel.Via;
                }
            }

            CommunicationState ICommunicationObject.State
            {
                get
                {
                    return this.channel.State;
                }
            }

            bool IContextChannel.AllowOutputBatching
            {
                get
                {
                    return ((IContextChannel) this.channel).AllowOutputBatching;
                }
                set
                {
                    ((IContextChannel) this.channel).AllowOutputBatching = value;
                }
            }

            IInputSession IContextChannel.InputSession
            {
                get
                {
                    return ((IContextChannel) this.channel).InputSession;
                }
            }

            EndpointAddress IContextChannel.LocalAddress
            {
                get
                {
                    return this.channel.LocalAddress;
                }
            }

            TimeSpan IContextChannel.OperationTimeout
            {
                get
                {
                    return this.channel.OperationTimeout;
                }
                set
                {
                    this.channel.OperationTimeout = value;
                }
            }

            IOutputSession IContextChannel.OutputSession
            {
                get
                {
                    return ((IContextChannel) this.channel).OutputSession;
                }
            }

            EndpointAddress IContextChannel.RemoteAddress
            {
                get
                {
                    return this.channel.RemoteAddress;
                }
            }

            string IContextChannel.SessionId
            {
                get
                {
                    return ((IContextChannel) this.channel).SessionId;
                }
            }

            IExtensionCollection<IContextChannel> IExtensibleObject<IContextChannel>.Extensions
            {
                get
                {
                    return this.channel.Extensions;
                }
            }

            private class MethodCallMessage : IMethodCallMessage, IMethodMessage, IMessage
            {
                private readonly object[] args;

                public MethodCallMessage(object[] args)
                {
                    this.args = args;
                }

                public object GetArg(int argNum)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
                }

                public string GetArgName(int index)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
                }

                public object GetInArg(int argNum)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
                }

                public string GetInArgName(int index)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
                }

                public int ArgCount
                {
                    get
                    {
                        return this.args.Length;
                    }
                }

                public object[] Args
                {
                    get
                    {
                        return this.args;
                    }
                }

                public bool HasVarArgs
                {
                    get
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
                    }
                }

                public int InArgCount
                {
                    get
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
                    }
                }

                public object[] InArgs
                {
                    get
                    {
                        return this.args;
                    }
                }

                public System.Runtime.Remoting.Messaging.LogicalCallContext LogicalCallContext
                {
                    get
                    {
                        return null;
                    }
                }

                public System.Reflection.MethodBase MethodBase
                {
                    get
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
                    }
                }

                public string MethodName
                {
                    get
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
                    }
                }

                public object MethodSignature
                {
                    get
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
                    }
                }

                public IDictionary Properties
                {
                    get
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
                    }
                }

                public string TypeName
                {
                    get
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
                    }
                }

                public string Uri
                {
                    get
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotImplementedException());
                    }
                }
            }
        }

        protected delegate object[] EndOperationDelegate(IAsyncResult result);

        protected class InvokeAsyncCompletedEventArgs : AsyncCompletedEventArgs
        {
            private object[] results;

            internal InvokeAsyncCompletedEventArgs(object[] results, Exception error, bool cancelled, object userState) : base(error, cancelled, userState)
            {
                this.results = results;
            }

            public object[] Results
            {
                get
                {
                    return this.results;
                }
            }
        }
    }
}

