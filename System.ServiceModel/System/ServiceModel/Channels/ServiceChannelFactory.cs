namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.Remoting;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;

    internal abstract class ServiceChannelFactory : ChannelFactoryBase
    {
        private string bindingName;
        private List<IChannel> channelsList;
        private System.ServiceModel.Dispatcher.ClientRuntime clientRuntime;
        private System.ServiceModel.Channels.MessageVersion messageVersion;
        private System.ServiceModel.Channels.RequestReplyCorrelator requestReplyCorrelator = new System.ServiceModel.Channels.RequestReplyCorrelator();
        private IDefaultCommunicationTimeouts timeouts;

        public ServiceChannelFactory(System.ServiceModel.Dispatcher.ClientRuntime clientRuntime, Binding binding)
        {
            if (clientRuntime == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("clientRuntime");
            }
            this.bindingName = binding.Name;
            this.channelsList = new List<IChannel>();
            this.clientRuntime = clientRuntime;
            this.timeouts = new DefaultCommunicationTimeouts(binding);
            this.messageVersion = binding.MessageVersion;
        }

        public static ServiceChannelFactory BuildChannelFactory(ServiceEndpoint serviceEndpoint)
        {
            return BuildChannelFactory(serviceEndpoint, false);
        }

        public static ServiceChannelFactory BuildChannelFactory(ChannelBuilder channelBuilder, System.ServiceModel.Dispatcher.ClientRuntime clientRuntime)
        {
            if (channelBuilder.CanBuildChannelFactory<IDuplexChannel>())
            {
                return new ServiceChannelFactoryOverDuplex(channelBuilder.BuildChannelFactory<IDuplexChannel>(), clientRuntime, channelBuilder.Binding);
            }
            if (channelBuilder.CanBuildChannelFactory<IDuplexSessionChannel>())
            {
                return new ServiceChannelFactoryOverDuplexSession(channelBuilder.BuildChannelFactory<IDuplexSessionChannel>(), clientRuntime, channelBuilder.Binding, false);
            }
            return new ServiceChannelFactoryOverRequestSession(channelBuilder.BuildChannelFactory<IRequestSessionChannel>(), clientRuntime, channelBuilder.Binding, false);
        }

        public static ServiceChannelFactory BuildChannelFactory(ServiceEndpoint serviceEndpoint, bool useActiveAutoClose)
        {
            ChannelRequirements requirements;
            BindingParameterCollection parameters;
            if (serviceEndpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceEndpoint");
            }
            serviceEndpoint.EnsureInvariants();
            serviceEndpoint.ValidateForClient();
            ChannelRequirements.ComputeContractRequirements(serviceEndpoint.Contract, out requirements);
            System.ServiceModel.Dispatcher.ClientRuntime clientRuntime = DispatcherBuilder.BuildProxyBehavior(serviceEndpoint, out parameters);
            Binding binding = serviceEndpoint.Binding;
            System.Type[] requiredChannels = ChannelRequirements.ComputeRequiredChannels(ref requirements);
            CustomBinding binding2 = new CustomBinding(binding);
            BindingContext context = new BindingContext(binding2, parameters);
            InternalDuplexBindingElement internalDuplexBindingElement = null;
            InternalDuplexBindingElement.AddDuplexFactorySupport(context, ref internalDuplexBindingElement);
            binding2 = new CustomBinding(context.RemainingBindingElements);
            binding2.CopyTimeouts(serviceEndpoint.Binding);
            foreach (System.Type type in requiredChannels)
            {
                if ((type == typeof(IOutputChannel)) && binding2.CanBuildChannelFactory<IOutputChannel>(parameters))
                {
                    return new ServiceChannelFactoryOverOutput(binding2.BuildChannelFactory<IOutputChannel>(parameters), clientRuntime, binding);
                }
                if ((type == typeof(IRequestChannel)) && binding2.CanBuildChannelFactory<IRequestChannel>(parameters))
                {
                    return new ServiceChannelFactoryOverRequest(binding2.BuildChannelFactory<IRequestChannel>(parameters), clientRuntime, binding);
                }
                if ((type == typeof(IDuplexChannel)) && binding2.CanBuildChannelFactory<IDuplexChannel>(parameters))
                {
                    if (requirements.usesReply && binding.CreateBindingElements().Find<TransportBindingElement>().ManualAddressing)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("CantCreateChannelWithManualAddressing")));
                    }
                    return new ServiceChannelFactoryOverDuplex(binding2.BuildChannelFactory<IDuplexChannel>(parameters), clientRuntime, binding);
                }
                if ((type == typeof(IOutputSessionChannel)) && binding2.CanBuildChannelFactory<IOutputSessionChannel>(parameters))
                {
                    return new ServiceChannelFactoryOverOutputSession(binding2.BuildChannelFactory<IOutputSessionChannel>(parameters), clientRuntime, binding, false);
                }
                if ((type == typeof(IRequestSessionChannel)) && binding2.CanBuildChannelFactory<IRequestSessionChannel>(parameters))
                {
                    return new ServiceChannelFactoryOverRequestSession(binding2.BuildChannelFactory<IRequestSessionChannel>(parameters), clientRuntime, binding, false);
                }
                if ((type == typeof(IDuplexSessionChannel)) && binding2.CanBuildChannelFactory<IDuplexSessionChannel>(parameters))
                {
                    if (requirements.usesReply && binding.CreateBindingElements().Find<TransportBindingElement>().ManualAddressing)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("CantCreateChannelWithManualAddressing")));
                    }
                    return new ServiceChannelFactoryOverDuplexSession(binding2.BuildChannelFactory<IDuplexSessionChannel>(parameters), clientRuntime, binding, useActiveAutoClose);
                }
            }
            foreach (System.Type type2 in requiredChannels)
            {
                if ((type2 == typeof(IOutputChannel)) && binding2.CanBuildChannelFactory<IOutputSessionChannel>(parameters))
                {
                    return new ServiceChannelFactoryOverOutputSession(binding2.BuildChannelFactory<IOutputSessionChannel>(parameters), clientRuntime, binding, true);
                }
                if ((type2 == typeof(IRequestChannel)) && binding2.CanBuildChannelFactory<IRequestSessionChannel>(parameters))
                {
                    return new ServiceChannelFactoryOverRequestSession(binding2.BuildChannelFactory<IRequestSessionChannel>(parameters), clientRuntime, binding, true);
                }
                if (((type2 == typeof(IRequestSessionChannel)) && binding2.CanBuildChannelFactory<IRequestChannel>(parameters)) && (binding2.GetProperty<IContextSessionProvider>(parameters) != null))
                {
                    return new ServiceChannelFactoryOverRequest(binding2.BuildChannelFactory<IRequestChannel>(parameters), clientRuntime, binding);
                }
            }
            Dictionary<System.Type, byte> dictionary = new Dictionary<System.Type, byte>();
            if (binding2.CanBuildChannelFactory<IOutputChannel>(parameters))
            {
                dictionary.Add(typeof(IOutputChannel), 0);
            }
            if (binding2.CanBuildChannelFactory<IRequestChannel>(parameters))
            {
                dictionary.Add(typeof(IRequestChannel), 0);
            }
            if (binding2.CanBuildChannelFactory<IDuplexChannel>(parameters))
            {
                dictionary.Add(typeof(IDuplexChannel), 0);
            }
            if (binding2.CanBuildChannelFactory<IOutputSessionChannel>(parameters))
            {
                dictionary.Add(typeof(IOutputSessionChannel), 0);
            }
            if (binding2.CanBuildChannelFactory<IRequestSessionChannel>(parameters))
            {
                dictionary.Add(typeof(IRequestSessionChannel), 0);
            }
            if (binding2.CanBuildChannelFactory<IDuplexSessionChannel>(parameters))
            {
                dictionary.Add(typeof(IDuplexSessionChannel), 0);
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ChannelRequirements.CantCreateChannelException(dictionary.Keys, requiredChannels, binding.Name));
        }

        public abstract bool CanCreateChannel<TChannel>();
        public void ChannelCreated(IChannel channel)
        {
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, 0x4001f, System.ServiceModel.SR.GetString("TraceCodeChannelCreated", new object[] { TraceUtility.CreateSourceString(channel) }), this);
            }
            lock (base.ThisLock)
            {
                base.ThrowIfDisposed();
                this.channelsList.Add(channel);
            }
        }

        public void ChannelDisposed(IChannel channel)
        {
            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, 0x40020, System.ServiceModel.SR.GetString("TraceCodeChannelDisposed", new object[] { TraceUtility.CreateSourceString(channel) }), this);
            }
            lock (base.ThisLock)
            {
                this.channelsList.Remove(channel);
            }
        }

        public TChannel CreateChannel<TChannel>(EndpointAddress address)
        {
            return this.CreateChannel<TChannel>(address, null);
        }

        public TChannel CreateChannel<TChannel>(EndpointAddress address, Uri via)
        {
            if (!this.CanCreateChannel<TChannel>())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("CouldnTCreateChannelForChannelType2", new object[] { this.bindingName, typeof(TChannel).Name })));
            }
            return (TChannel) this.CreateChannel(typeof(TChannel), address, via);
        }

        public object CreateChannel(System.Type channelType, EndpointAddress address)
        {
            return this.CreateChannel(channelType, address, null);
        }

        public object CreateChannel(System.Type channelType, EndpointAddress address, Uri via)
        {
            if (via == null)
            {
                via = this.ClientRuntime.Via;
                if (via == null)
                {
                    via = address.Uri;
                }
            }
            ServiceChannel serviceChannel = this.CreateServiceChannel(address, via);
            serviceChannel.Proxy = CreateProxy(channelType, channelType, MessageDirection.Input, serviceChannel);
            serviceChannel.ClientRuntime.GetRuntime().InitializeChannel((IClientChannel) serviceChannel.Proxy);
            OperationContext current = OperationContext.Current;
            if ((current != null) && (current.InstanceContext != null))
            {
                current.InstanceContext.WmiChannels.Add((IChannel) serviceChannel.Proxy);
                serviceChannel.WmiInstanceContext = current.InstanceContext;
            }
            return serviceChannel.Proxy;
        }

        protected abstract IChannelBinder CreateInnerChannelBinder(EndpointAddress address, Uri via);
        [SecuritySafeCritical]
        internal static object CreateProxy(System.Type interfaceType, System.Type proxiedType, MessageDirection direction, ServiceChannel serviceChannel)
        {
            if (!proxiedType.IsInterface)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxChannelFactoryTypeMustBeInterface")));
            }
            ServiceChannelProxy proxy = new ServiceChannelProxy(interfaceType, proxiedType, direction, serviceChannel);
            return proxy.GetTransparentProxy();
        }

        public virtual ServiceChannel CreateServiceChannel(EndpointAddress address, Uri via)
        {
            IChannelBinder binder = this.CreateInnerChannelBinder(address, via);
            ServiceChannel channel = new ServiceChannel(this, binder);
            if (binder is DuplexChannelBinder)
            {
                DuplexChannelBinder binder2 = binder as DuplexChannelBinder;
                binder2.ChannelHandler = new ChannelHandler(this.messageVersion, binder, channel);
                binder2.DefaultCloseTimeout = this.DefaultCloseTimeout;
                binder2.DefaultSendTimeout = this.DefaultSendTimeout;
                binder2.IdentityVerifier = this.clientRuntime.IdentityVerifier;
            }
            return channel;
        }

        [SecuritySafeCritical]
        internal static ServiceChannel GetServiceChannel(object transparentProxy)
        {
            IChannelBaseProxy proxy = transparentProxy as IChannelBaseProxy;
            if (proxy != null)
            {
                return proxy.GetServiceChannel();
            }
            ServiceChannelProxy realProxy = RemotingServices.GetRealProxy(transparentProxy) as ServiceChannelProxy;
            if (realProxy != null)
            {
                return realProxy.GetServiceChannel();
            }
            return null;
        }

        protected override void OnAbort()
        {
            IChannel item = null;
            lock (base.ThisLock)
            {
                item = (this.channelsList.Count > 0) ? this.channelsList[this.channelsList.Count - 1] : null;
                goto Label_00A5;
            }
        Label_0049:
            item.Abort();
            lock (base.ThisLock)
            {
                this.channelsList.Remove(item);
                item = (this.channelsList.Count > 0) ? this.channelsList[this.channelsList.Count - 1] : null;
            }
        Label_00A5:
            if (item != null)
            {
                goto Label_0049;
            }
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            List<ICommunicationObject> list;
            lock (base.ThisLock)
            {
                list = new List<ICommunicationObject>();
                for (int i = 0; i < this.channelsList.Count; i++)
                {
                    list.Add(this.channelsList[i]);
                }
            }
            return new CloseCollectionAsyncResult(timeout, callback, state, list);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            while (true)
            {
                IChannel channel;
                lock (base.ThisLock)
                {
                    if (this.channelsList.Count == 0)
                    {
                        return;
                    }
                    channel = this.channelsList[0];
                }
                channel.Close(helper.RemainingTime());
            }
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CloseCollectionAsyncResult.End(result);
        }

        protected override void OnOpened()
        {
            base.OnOpened();
            this.clientRuntime.LockDownProperties();
        }

        public System.ServiceModel.Dispatcher.ClientRuntime ClientRuntime
        {
            get
            {
                base.ThrowIfDisposed();
                return this.clientRuntime;
            }
        }

        protected override TimeSpan DefaultCloseTimeout
        {
            get
            {
                return this.timeouts.CloseTimeout;
            }
        }

        protected override TimeSpan DefaultOpenTimeout
        {
            get
            {
                return this.timeouts.OpenTimeout;
            }
        }

        protected override TimeSpan DefaultReceiveTimeout
        {
            get
            {
                return this.timeouts.ReceiveTimeout;
            }
        }

        protected override TimeSpan DefaultSendTimeout
        {
            get
            {
                return this.timeouts.SendTimeout;
            }
        }

        public System.ServiceModel.Channels.MessageVersion MessageVersion
        {
            get
            {
                return this.messageVersion;
            }
        }

        internal System.ServiceModel.Channels.RequestReplyCorrelator RequestReplyCorrelator
        {
            get
            {
                base.ThrowIfDisposed();
                return this.requestReplyCorrelator;
            }
        }

        private class DefaultCommunicationTimeouts : IDefaultCommunicationTimeouts
        {
            private TimeSpan closeTimeout;
            private TimeSpan openTimeout;
            private TimeSpan receiveTimeout;
            private TimeSpan sendTimeout;

            public DefaultCommunicationTimeouts(IDefaultCommunicationTimeouts timeouts)
            {
                this.closeTimeout = timeouts.CloseTimeout;
                this.openTimeout = timeouts.OpenTimeout;
                this.receiveTimeout = timeouts.ReceiveTimeout;
                this.sendTimeout = timeouts.SendTimeout;
            }

            public TimeSpan CloseTimeout
            {
                get
                {
                    return this.closeTimeout;
                }
            }

            public TimeSpan OpenTimeout
            {
                get
                {
                    return this.openTimeout;
                }
            }

            public TimeSpan ReceiveTimeout
            {
                get
                {
                    return this.receiveTimeout;
                }
            }

            public TimeSpan SendTimeout
            {
                get
                {
                    return this.sendTimeout;
                }
            }
        }

        private class ServiceChannelFactoryOverDuplex : ServiceChannelFactory.TypedServiceChannelFactory<IDuplexChannel>
        {
            public ServiceChannelFactoryOverDuplex(IChannelFactory<IDuplexChannel> innerChannelFactory, ClientRuntime clientRuntime, Binding binding) : base(innerChannelFactory, clientRuntime, binding)
            {
            }

            public override bool CanCreateChannel<TChannel>()
            {
                if (!(typeof(TChannel) == typeof(IOutputChannel)) && !(typeof(TChannel) == typeof(IRequestChannel)))
                {
                    return (typeof(TChannel) == typeof(IDuplexChannel));
                }
                return true;
            }

            protected override IChannelBinder CreateInnerChannelBinder(EndpointAddress to, Uri via)
            {
                return new DuplexChannelBinder(base.InnerChannelFactory.CreateChannel(to, via), base.RequestReplyCorrelator);
            }
        }

        private class ServiceChannelFactoryOverDuplexSession : ServiceChannelFactory.TypedServiceChannelFactory<IDuplexSessionChannel>
        {
            private bool useActiveAutoClose;

            public ServiceChannelFactoryOverDuplexSession(IChannelFactory<IDuplexSessionChannel> innerChannelFactory, ClientRuntime clientRuntime, Binding binding, bool useActiveAutoClose) : base(innerChannelFactory, clientRuntime, binding)
            {
                this.useActiveAutoClose = useActiveAutoClose;
            }

            public override bool CanCreateChannel<TChannel>()
            {
                if (((!(typeof(TChannel) == typeof(IOutputChannel)) && !(typeof(TChannel) == typeof(IRequestChannel))) && (!(typeof(TChannel) == typeof(IDuplexChannel)) && !(typeof(TChannel) == typeof(IOutputSessionChannel)))) && !(typeof(TChannel) == typeof(IRequestSessionChannel)))
                {
                    return (typeof(TChannel) == typeof(IDuplexSessionChannel));
                }
                return true;
            }

            protected override IChannelBinder CreateInnerChannelBinder(EndpointAddress to, Uri via)
            {
                return new DuplexChannelBinder(base.InnerChannelFactory.CreateChannel(to, via), base.RequestReplyCorrelator, this.useActiveAutoClose);
            }
        }

        private class ServiceChannelFactoryOverOutput : ServiceChannelFactory.TypedServiceChannelFactory<IOutputChannel>
        {
            public ServiceChannelFactoryOverOutput(IChannelFactory<IOutputChannel> innerChannelFactory, ClientRuntime clientRuntime, Binding binding) : base(innerChannelFactory, clientRuntime, binding)
            {
            }

            public override bool CanCreateChannel<TChannel>()
            {
                if (!(typeof(TChannel) == typeof(IOutputChannel)))
                {
                    return (typeof(TChannel) == typeof(IRequestChannel));
                }
                return true;
            }

            protected override IChannelBinder CreateInnerChannelBinder(EndpointAddress to, Uri via)
            {
                return new OutputChannelBinder(base.InnerChannelFactory.CreateChannel(to, via));
            }
        }

        private class ServiceChannelFactoryOverOutputSession : ServiceChannelFactory.TypedServiceChannelFactory<IOutputSessionChannel>
        {
            private bool datagramAdapter;

            public ServiceChannelFactoryOverOutputSession(IChannelFactory<IOutputSessionChannel> innerChannelFactory, ClientRuntime clientRuntime, Binding binding, bool datagramAdapter) : base(innerChannelFactory, clientRuntime, binding)
            {
                this.datagramAdapter = datagramAdapter;
            }

            public override bool CanCreateChannel<TChannel>()
            {
                if ((!(typeof(TChannel) == typeof(IOutputChannel)) && !(typeof(TChannel) == typeof(IOutputSessionChannel))) && !(typeof(TChannel) == typeof(IRequestChannel)))
                {
                    return (typeof(TChannel) == typeof(IRequestSessionChannel));
                }
                return true;
            }

            protected override IChannelBinder CreateInnerChannelBinder(EndpointAddress to, Uri via)
            {
                IOutputChannel outputChannel;
                DatagramAdapter.Source<IOutputSessionChannel> channelSource = null;
                if (this.datagramAdapter)
                {
                    if (channelSource == null)
                    {
                        channelSource = () => this.InnerChannelFactory.CreateChannel(to, via);
                    }
                    outputChannel = DatagramAdapter.GetOutputChannel(channelSource, base.timeouts);
                }
                else
                {
                    outputChannel = base.InnerChannelFactory.CreateChannel(to, via);
                }
                return new OutputChannelBinder(outputChannel);
            }
        }

        private class ServiceChannelFactoryOverRequest : ServiceChannelFactory.TypedServiceChannelFactory<IRequestChannel>
        {
            public ServiceChannelFactoryOverRequest(IChannelFactory<IRequestChannel> innerChannelFactory, ClientRuntime clientRuntime, Binding binding) : base(innerChannelFactory, clientRuntime, binding)
            {
            }

            public override bool CanCreateChannel<TChannel>()
            {
                if (!(typeof(TChannel) == typeof(IOutputChannel)))
                {
                    return (typeof(TChannel) == typeof(IRequestChannel));
                }
                return true;
            }

            protected override IChannelBinder CreateInnerChannelBinder(EndpointAddress to, Uri via)
            {
                return new RequestChannelBinder(base.InnerChannelFactory.CreateChannel(to, via));
            }
        }

        private class ServiceChannelFactoryOverRequestSession : ServiceChannelFactory.TypedServiceChannelFactory<IRequestSessionChannel>
        {
            private bool datagramAdapter;

            public ServiceChannelFactoryOverRequestSession(IChannelFactory<IRequestSessionChannel> innerChannelFactory, ClientRuntime clientRuntime, Binding binding, bool datagramAdapter) : base(innerChannelFactory, clientRuntime, binding)
            {
                this.datagramAdapter = datagramAdapter;
            }

            public override bool CanCreateChannel<TChannel>()
            {
                if ((!(typeof(TChannel) == typeof(IOutputChannel)) && !(typeof(TChannel) == typeof(IOutputSessionChannel))) && !(typeof(TChannel) == typeof(IRequestChannel)))
                {
                    return (typeof(TChannel) == typeof(IRequestSessionChannel));
                }
                return true;
            }

            protected override IChannelBinder CreateInnerChannelBinder(EndpointAddress to, Uri via)
            {
                IRequestChannel requestChannel;
                DatagramAdapter.Source<IRequestSessionChannel> channelSource = null;
                if (this.datagramAdapter)
                {
                    if (channelSource == null)
                    {
                        channelSource = () => this.InnerChannelFactory.CreateChannel(to, via);
                    }
                    requestChannel = DatagramAdapter.GetRequestChannel(channelSource, base.timeouts);
                }
                else
                {
                    requestChannel = base.InnerChannelFactory.CreateChannel(to, via);
                }
                return new RequestChannelBinder(requestChannel);
            }
        }

        private abstract class TypedServiceChannelFactory<TChannel> : ServiceChannelFactory where TChannel: class, IChannel
        {
            private IChannelFactory<TChannel> innerChannelFactory;

            protected TypedServiceChannelFactory(IChannelFactory<TChannel> innerChannelFactory, ClientRuntime clientRuntime, Binding binding) : base(clientRuntime, binding)
            {
                this.innerChannelFactory = innerChannelFactory;
            }

            public override T GetProperty<T>() where T: class
            {
                if (typeof(T) == typeof(ServiceChannelFactory.TypedServiceChannelFactory<TChannel>))
                {
                    return (T) this;
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
                base.OnAbort();
                this.innerChannelFactory.Abort();
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new ChainedAsyncResult(timeout, callback, state, new ChainedBeginHandler(this.OnBeginClose), new ChainedEndHandler(this.OnEndClose), new ChainedBeginHandler(this.innerChannelFactory.BeginClose), new ChainedEndHandler(this.innerChannelFactory.EndClose));
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return this.innerChannelFactory.BeginOpen(timeout, callback, state);
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
                this.innerChannelFactory.EndOpen(result);
            }

            protected override void OnOpen(TimeSpan timeout)
            {
                this.innerChannelFactory.Open(timeout);
            }

            protected IChannelFactory<TChannel> InnerChannelFactory
            {
                get
                {
                    return this.innerChannelFactory;
                }
            }
        }
    }
}

