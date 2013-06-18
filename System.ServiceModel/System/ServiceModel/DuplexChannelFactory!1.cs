namespace System.ServiceModel
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;

    public class DuplexChannelFactory<TChannel> : ChannelFactory<TChannel>
    {
        public DuplexChannelFactory(object callbackObject) : base(typeof(TChannel))
        {
            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivityConstructChannelFactory", new object[] { TraceUtility.CreateSourceString(this) }), ActivityType.Construct);
                }
                if (callbackObject == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackObject");
                }
                this.CheckAndAssignCallbackInstance(callbackObject);
                base.InitializeEndpoint((string) null, null);
            }
        }

        public DuplexChannelFactory(InstanceContext callbackInstance) : this(callbackInstance)
        {
        }

        public DuplexChannelFactory(System.Type callbackInstanceType) : this(callbackInstanceType)
        {
        }

        public DuplexChannelFactory(object callbackObject, Binding binding) : this(callbackObject, binding, (EndpointAddress) null)
        {
        }

        public DuplexChannelFactory(object callbackObject, ServiceEndpoint endpoint) : base(typeof(TChannel))
        {
            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivityConstructChannelFactory", new object[] { TraceUtility.CreateSourceString(this) }), ActivityType.Construct);
                }
                if (callbackObject == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackObject");
                }
                if (endpoint == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
                }
                this.CheckAndAssignCallbackInstance(callbackObject);
                base.InitializeEndpoint(endpoint);
            }
        }

        public DuplexChannelFactory(object callbackObject, string endpointConfigurationName) : this(callbackObject, endpointConfigurationName, (EndpointAddress) null)
        {
        }

        public DuplexChannelFactory(InstanceContext callbackInstance, Binding binding) : this(callbackInstance, binding)
        {
        }

        public DuplexChannelFactory(InstanceContext callbackInstance, ServiceEndpoint endpoint) : this(callbackInstance, endpoint)
        {
        }

        public DuplexChannelFactory(InstanceContext callbackInstance, string endpointConfigurationName) : this(callbackInstance, endpointConfigurationName)
        {
        }

        public DuplexChannelFactory(System.Type callbackInstanceType, Binding binding) : this(callbackInstanceType, binding)
        {
        }

        public DuplexChannelFactory(System.Type callbackInstanceType, ServiceEndpoint endpoint) : this(callbackInstanceType, endpoint)
        {
        }

        public DuplexChannelFactory(System.Type callbackInstanceType, string endpointConfigurationName) : this(callbackInstanceType, endpointConfigurationName)
        {
        }

        public DuplexChannelFactory(object callbackObject, Binding binding, EndpointAddress remoteAddress) : base(typeof(TChannel))
        {
            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivityConstructChannelFactory", new object[] { TraceUtility.CreateSourceString(this) }), ActivityType.Construct);
                }
                if (callbackObject == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackObject");
                }
                if (binding == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("binding");
                }
                this.CheckAndAssignCallbackInstance(callbackObject);
                base.InitializeEndpoint(binding, remoteAddress);
            }
        }

        public DuplexChannelFactory(object callbackObject, Binding binding, string remoteAddress) : this(callbackObject, binding, new EndpointAddress(remoteAddress))
        {
        }

        public DuplexChannelFactory(object callbackObject, string endpointConfigurationName, EndpointAddress remoteAddress) : base(typeof(TChannel))
        {
            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivityConstructChannelFactory", new object[] { TraceUtility.CreateSourceString(this) }), ActivityType.Construct);
                }
                if (callbackObject == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackObject");
                }
                if (endpointConfigurationName == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointConfigurationName");
                }
                this.CheckAndAssignCallbackInstance(callbackObject);
                base.InitializeEndpoint(endpointConfigurationName, remoteAddress);
            }
        }

        public DuplexChannelFactory(InstanceContext callbackInstance, Binding binding, EndpointAddress remoteAddress) : this(callbackInstance, binding, remoteAddress)
        {
        }

        public DuplexChannelFactory(InstanceContext callbackInstance, Binding binding, string remoteAddress) : this(callbackInstance, binding, new EndpointAddress(remoteAddress))
        {
        }

        public DuplexChannelFactory(InstanceContext callbackInstance, string endpointConfigurationName, EndpointAddress remoteAddress) : this(callbackInstance, endpointConfigurationName, remoteAddress)
        {
        }

        public DuplexChannelFactory(System.Type callbackInstanceType, Binding binding, EndpointAddress remoteAddress) : this(callbackInstanceType, binding, remoteAddress)
        {
        }

        public DuplexChannelFactory(System.Type callbackInstanceType, Binding binding, string remoteAddress) : this(callbackInstanceType, binding, new EndpointAddress(remoteAddress))
        {
        }

        public DuplexChannelFactory(System.Type callbackInstanceType, string endpointConfigurationName, EndpointAddress remoteAddress) : this(callbackInstanceType, endpointConfigurationName, remoteAddress)
        {
        }

        internal void CheckAndAssignCallbackInstance(object callbackInstance)
        {
            if (callbackInstance is System.Type)
            {
                base.CallbackType = (System.Type) callbackInstance;
            }
            else if (callbackInstance is InstanceContext)
            {
                base.CallbackInstance = (InstanceContext) callbackInstance;
            }
            else
            {
                base.CallbackInstance = new InstanceContext(callbackInstance);
            }
        }

        public TChannel CreateChannel(InstanceContext callbackInstance)
        {
            return this.CreateChannel(callbackInstance, base.CreateEndpointAddress(base.Endpoint), null);
        }

        public static TChannel CreateChannel(object callbackObject, string endpointConfigurationName)
        {
            return DuplexChannelFactory<TChannel>.CreateChannel(DuplexChannelFactory<TChannel>.GetInstanceContextForObject(callbackObject), endpointConfigurationName);
        }

        public override TChannel CreateChannel(EndpointAddress address, Uri via)
        {
            return this.CreateChannel(base.CallbackInstance, address, via);
        }

        public TChannel CreateChannel(InstanceContext callbackInstance, EndpointAddress address)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }
            return this.CreateChannel(callbackInstance, address, address.Uri);
        }

        public static TChannel CreateChannel(InstanceContext callbackInstance, string endpointConfigurationName)
        {
            TChannel channel = new DuplexChannelFactory<TChannel>(callbackInstance, endpointConfigurationName).CreateChannel();
            ChannelFactory<TChannel>.SetFactoryToAutoClose(channel);
            return channel;
        }

        public static TChannel CreateChannel(object callbackObject, Binding binding, EndpointAddress endpointAddress)
        {
            return DuplexChannelFactory<TChannel>.CreateChannel(DuplexChannelFactory<TChannel>.GetInstanceContextForObject(callbackObject), binding, endpointAddress);
        }

        public static TChannel CreateChannel(InstanceContext callbackInstance, Binding binding, EndpointAddress endpointAddress)
        {
            TChannel channel = new DuplexChannelFactory<TChannel>(callbackInstance, binding, endpointAddress).CreateChannel();
            ChannelFactory<TChannel>.SetFactoryToAutoClose(channel);
            return channel;
        }

        public virtual TChannel CreateChannel(InstanceContext callbackInstance, EndpointAddress address, Uri via)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("address");
            }
            if ((base.CallbackType != null) && (callbackInstance == null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxCreateDuplexChannelNoCallback1")));
            }
            if (callbackInstance == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxCreateDuplexChannelNoCallback")));
            }
            if (callbackInstance.UserObject == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxCreateDuplexChannelNoCallbackUserObject")));
            }
            if (!base.HasDuplexOperations())
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxCreateDuplexChannel1", new object[] { base.Endpoint.Contract.Name })));
            }
            System.Type c = callbackInstance.UserObject.GetType();
            System.Type callbackContractType = base.Endpoint.Contract.CallbackContractType;
            if ((callbackContractType != null) && !callbackContractType.IsAssignableFrom(c))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxCreateDuplexChannelBadCallbackUserObject", new object[] { callbackContractType })));
            }
            base.EnsureOpened();
            TChannel local = (TChannel) base.ServiceChannelFactory.CreateChannel(typeof(TChannel), address, via);
            IDuplexContextChannel channel = local as IDuplexContextChannel;
            if (channel != null)
            {
                channel.CallbackInstance = callbackInstance;
            }
            return local;
        }

        public static TChannel CreateChannel(object callbackObject, Binding binding, EndpointAddress endpointAddress, Uri via)
        {
            return DuplexChannelFactory<TChannel>.CreateChannel(DuplexChannelFactory<TChannel>.GetInstanceContextForObject(callbackObject), binding, endpointAddress, via);
        }

        public static TChannel CreateChannel(InstanceContext callbackInstance, Binding binding, EndpointAddress endpointAddress, Uri via)
        {
            TChannel channel = new DuplexChannelFactory<TChannel>(callbackInstance, binding).CreateChannel(endpointAddress, via);
            ChannelFactory<TChannel>.SetFactoryToAutoClose(channel);
            return channel;
        }

        private static InstanceContext GetInstanceContextForObject(object callbackObject)
        {
            if (callbackObject is InstanceContext)
            {
                return (InstanceContext) callbackObject;
            }
            return new InstanceContext(callbackObject);
        }
    }
}

