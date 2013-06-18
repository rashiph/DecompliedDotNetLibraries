namespace System.ServiceModel
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;

    public class ChannelFactory<TChannel> : ChannelFactory, IChannelFactory<TChannel>, IChannelFactory, ICommunicationObject
    {
        private InstanceContext callbackInstance;
        private System.Type callbackType;
        private System.Type channelType;
        private System.ServiceModel.Description.TypeLoader typeLoader;

        public ChannelFactory() : this(typeof(TChannel))
        {
            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivityConstructChannelFactory", new object[] { typeof(TChannel).FullName }), ActivityType.Construct);
                }
                base.InitializeEndpoint((string) null, null);
            }
        }

        public ChannelFactory(Binding binding) : this(binding, (EndpointAddress) null)
        {
        }

        public ChannelFactory(ServiceEndpoint endpoint) : this(typeof(TChannel))
        {
            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivityConstructChannelFactory", new object[] { typeof(TChannel).FullName }), ActivityType.Construct);
                }
                if (endpoint == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
                }
                base.InitializeEndpoint(endpoint);
            }
        }

        public ChannelFactory(string endpointConfigurationName) : this(endpointConfigurationName, null)
        {
        }

        protected ChannelFactory(System.Type channelType)
        {
            if (channelType == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channelType");
            }
            if (!channelType.IsInterface)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxChannelFactoryTypeMustBeInterface")));
            }
            this.channelType = channelType;
        }

        public ChannelFactory(Binding binding, EndpointAddress remoteAddress) : this(typeof(TChannel))
        {
            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivityConstructChannelFactory", new object[] { typeof(TChannel).FullName }), ActivityType.Construct);
                }
                if (binding == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
                }
                base.InitializeEndpoint(binding, remoteAddress);
            }
        }

        public ChannelFactory(Binding binding, string remoteAddress) : this(binding, new EndpointAddress(remoteAddress))
        {
        }

        public ChannelFactory(string endpointConfigurationName, EndpointAddress remoteAddress) : this(typeof(TChannel))
        {
            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivityConstructChannelFactory", new object[] { typeof(TChannel).FullName }), ActivityType.Construct);
                }
                if (endpointConfigurationName == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointConfigurationName");
                }
                base.InitializeEndpoint(endpointConfigurationName, remoteAddress);
            }
        }

        internal bool CanCreateChannel<UChannel>()
        {
            base.EnsureOpened();
            return this.ServiceChannelFactory.CanCreateChannel<UChannel>();
        }

        public TChannel CreateChannel()
        {
            return this.CreateChannel(base.CreateEndpointAddress(base.Endpoint), null);
        }

        public TChannel CreateChannel(EndpointAddress address)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }
            return this.CreateChannel(address, address.Uri);
        }

        internal UChannel CreateChannel<UChannel>(EndpointAddress address)
        {
            base.EnsureOpened();
            return this.ServiceChannelFactory.CreateChannel<UChannel>(address);
        }

        protected static TChannel CreateChannel(string endpointConfigurationName)
        {
            ChannelFactory<TChannel> factory = new ChannelFactory<TChannel>(endpointConfigurationName);
            if (factory.HasDuplexOperations())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxInvalidStaticOverloadCalledForDuplexChannelFactory1", new object[] { factory.channelType.Name })));
            }
            TChannel channel = factory.CreateChannel();
            ChannelFactory<TChannel>.SetFactoryToAutoClose(channel);
            return channel;
        }

        public static TChannel CreateChannel(Binding binding, EndpointAddress endpointAddress)
        {
            ChannelFactory<TChannel> factory = new ChannelFactory<TChannel>(binding, endpointAddress);
            if (factory.HasDuplexOperations())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxInvalidStaticOverloadCalledForDuplexChannelFactory1", new object[] { factory.channelType.Name })));
            }
            TChannel channel = factory.CreateChannel();
            ChannelFactory<TChannel>.SetFactoryToAutoClose(channel);
            return channel;
        }

        public virtual TChannel CreateChannel(EndpointAddress address, Uri via)
        {
            TChannel local;
            bool traceOpenAndClose = base.TraceOpenAndClose;
            try
            {
                using (ServiceModelActivity activity = (DiagnosticUtility.ShouldUseActivity && base.TraceOpenAndClose) ? ServiceModelActivity.CreateBoundedActivity() : null)
                {
                    if (DiagnosticUtility.ShouldUseActivity)
                    {
                        ServiceModelActivity.Start(activity, this.OpenActivityName, this.OpenActivityType);
                        base.TraceOpenAndClose = false;
                    }
                    if (address == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
                    }
                    if (base.HasDuplexOperations())
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxCreateNonDuplexChannel1", new object[] { base.Endpoint.Contract.Name })));
                    }
                    base.EnsureOpened();
                    local = (TChannel) this.ServiceChannelFactory.CreateChannel(typeof(TChannel), address, via);
                }
            }
            finally
            {
                base.TraceOpenAndClose = traceOpenAndClose;
            }
            return local;
        }

        internal UChannel CreateChannel<UChannel>(EndpointAddress address, Uri via)
        {
            base.EnsureOpened();
            return this.ServiceChannelFactory.CreateChannel<UChannel>(address, via);
        }

        public static TChannel CreateChannel(Binding binding, EndpointAddress endpointAddress, Uri via)
        {
            ChannelFactory<TChannel> factory = new ChannelFactory<TChannel>(binding);
            if (factory.HasDuplexOperations())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxInvalidStaticOverloadCalledForDuplexChannelFactory1", new object[] { factory.channelType.Name })));
            }
            TChannel channel = factory.CreateChannel(endpointAddress, via);
            ChannelFactory<TChannel>.SetFactoryToAutoClose(channel);
            return channel;
        }

        protected override ServiceEndpoint CreateDescription()
        {
            ServiceEndpoint endpoint = new ServiceEndpoint(this.TypeLoader.LoadContractDescription(this.channelType));
            this.ReflectOnCallbackInstance(endpoint);
            this.TypeLoader.AddBehaviorsSFx(endpoint, this.channelType);
            return endpoint;
        }

        private void ReflectOnCallbackInstance(ServiceEndpoint endpoint)
        {
            if (this.callbackType != null)
            {
                if (endpoint.Contract.CallbackContractType == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SfxCallbackTypeCannotBeNull", new object[] { endpoint.Contract.ContractType.FullName })));
                }
                this.TypeLoader.AddBehaviorsFromImplementationType(endpoint, this.callbackType);
            }
            else if ((this.CallbackInstance != null) && (this.CallbackInstance.UserObject != null))
            {
                if (endpoint.Contract.CallbackContractType == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SfxCallbackTypeCannotBeNull", new object[] { endpoint.Contract.ContractType.FullName })));
                }
                object userObject = this.CallbackInstance.UserObject;
                System.Type implementationType = userObject.GetType();
                this.TypeLoader.AddBehaviorsFromImplementationType(endpoint, implementationType);
                IEndpointBehavior item = userObject as IEndpointBehavior;
                if (item != null)
                {
                    endpoint.Behaviors.Add(item);
                }
                IContractBehavior behavior2 = userObject as IContractBehavior;
                if (behavior2 != null)
                {
                    endpoint.Contract.Behaviors.Add(behavior2);
                }
            }
        }

        internal static void SetFactoryToAutoClose(TChannel channel)
        {
            System.ServiceModel.Channels.ServiceChannelFactory.GetServiceChannel(channel).CloseFactory = true;
        }

        internal InstanceContext CallbackInstance
        {
            get
            {
                return this.callbackInstance;
            }
            set
            {
                this.callbackInstance = value;
            }
        }

        internal System.Type CallbackType
        {
            get
            {
                return this.callbackType;
            }
            set
            {
                this.callbackType = value;
            }
        }

        internal override string CloseActivityName
        {
            get
            {
                return System.ServiceModel.SR.GetString("ActivityCloseChannelFactory", new object[] { typeof(TChannel).FullName });
            }
        }

        internal override string OpenActivityName
        {
            get
            {
                return System.ServiceModel.SR.GetString("ActivityOpenChannelFactory", new object[] { typeof(TChannel).FullName });
            }
        }

        internal override ActivityType OpenActivityType
        {
            get
            {
                return ActivityType.OpenClient;
            }
        }

        internal System.ServiceModel.Channels.ServiceChannelFactory ServiceChannelFactory
        {
            get
            {
                return (System.ServiceModel.Channels.ServiceChannelFactory) base.InnerFactory;
            }
        }

        internal System.ServiceModel.Description.TypeLoader TypeLoader
        {
            get
            {
                if (this.typeLoader == null)
                {
                    this.typeLoader = new System.ServiceModel.Description.TypeLoader();
                }
                return this.typeLoader;
            }
        }
    }
}

