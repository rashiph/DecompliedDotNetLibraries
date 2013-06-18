namespace System.ServiceModel
{
    using System;
    using System.Configuration;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;

    public abstract class ChannelFactory : CommunicationObject, IChannelFactory, ICommunicationObject, IDisposable
    {
        private string configurationName;
        private IChannelFactory innerFactory;
        private object openLock = new object();
        private ClientCredentials readOnlyClientCredentials;
        private ServiceEndpoint serviceEndpoint;

        protected ChannelFactory()
        {
            TraceUtility.SetEtwProviderId();
            base.TraceOpenAndClose = true;
        }

        protected virtual void ApplyConfiguration(string configurationName)
        {
            this.ApplyConfiguration(configurationName, null);
        }

        private void ApplyConfiguration(string configurationName, System.Configuration.Configuration configuration)
        {
            if (this.Endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxChannelFactoryCannotApplyConfigurationWithoutEndpoint")));
            }
            if (!this.Endpoint.IsFullyConfigured)
            {
                ConfigLoader loader;
                if (configuration != null)
                {
                    loader = new ConfigLoader(configuration.EvaluationContext);
                }
                else
                {
                    loader = new ConfigLoader();
                }
                if (configurationName == null)
                {
                    loader.LoadCommonClientBehaviors(this.Endpoint);
                }
                else
                {
                    loader.LoadChannelBehaviors(this.Endpoint, configurationName);
                }
            }
        }

        protected abstract ServiceEndpoint CreateDescription();
        internal EndpointAddress CreateEndpointAddress(ServiceEndpoint endpoint)
        {
            if (endpoint.Address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxChannelFactoryEndpointAddressUri")));
            }
            return endpoint.Address;
        }

        protected virtual IChannelFactory CreateFactory()
        {
            if (this.Endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxChannelFactoryCannotCreateFactoryWithoutDescription")));
            }
            if (this.Endpoint.Binding != null)
            {
                return ServiceChannelFactory.BuildChannelFactory(this.Endpoint, this.UseActiveAutoClose);
            }
            if (this.configurationName != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxChannelFactoryNoBindingFoundInConfig1", new object[] { this.configurationName })));
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxChannelFactoryNoBindingFoundInConfigOrCode")));
        }

        private ClientCredentials EnsureCredentials(ServiceEndpoint endpoint)
        {
            ClientCredentials item = endpoint.Behaviors.Find<ClientCredentials>();
            if (item == null)
            {
                item = new ClientCredentials();
                endpoint.Behaviors.Add(item);
            }
            return item;
        }

        protected internal void EnsureOpened()
        {
            base.ThrowIfDisposed();
            if (base.State != CommunicationState.Opened)
            {
                lock (this.openLock)
                {
                    if (base.State != CommunicationState.Opened)
                    {
                        base.Open();
                    }
                }
            }
        }

        private void EnsureSecurityCredentialsManager(ServiceEndpoint endpoint)
        {
            if (endpoint.Behaviors.Find<SecurityCredentialsManager>() == null)
            {
                endpoint.Behaviors.Add(new ClientCredentials());
            }
        }

        public T GetProperty<T>() where T: class
        {
            if (this.innerFactory != null)
            {
                return this.innerFactory.GetProperty<T>();
            }
            return default(T);
        }

        internal bool HasDuplexOperations()
        {
            OperationDescriptionCollection operations = this.Endpoint.Contract.Operations;
            for (int i = 0; i < operations.Count; i++)
            {
                OperationDescription description = operations[i];
                if (description.IsServerInitiated())
                {
                    return true;
                }
            }
            return false;
        }

        protected void InitializeEndpoint(ServiceEndpoint endpoint)
        {
            if (endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
            }
            this.serviceEndpoint = endpoint;
            this.ApplyConfiguration(null);
            this.EnsureSecurityCredentialsManager(this.serviceEndpoint);
        }

        protected void InitializeEndpoint(Binding binding, EndpointAddress address)
        {
            this.serviceEndpoint = this.CreateDescription();
            if (binding != null)
            {
                this.Endpoint.Binding = binding;
            }
            if (address != null)
            {
                this.Endpoint.Address = address;
            }
            this.ApplyConfiguration(null);
            this.EnsureSecurityCredentialsManager(this.serviceEndpoint);
        }

        protected void InitializeEndpoint(string configurationName, EndpointAddress address)
        {
            this.serviceEndpoint = this.CreateDescription();
            ServiceEndpoint endpoint = null;
            if (configurationName != null)
            {
                endpoint = ConfigLoader.LookupEndpoint(configurationName, address, this.serviceEndpoint.Contract);
            }
            if (endpoint != null)
            {
                this.serviceEndpoint = endpoint;
            }
            else
            {
                if (address != null)
                {
                    this.Endpoint.Address = address;
                }
                this.ApplyConfiguration(configurationName);
            }
            this.configurationName = configurationName;
            this.EnsureSecurityCredentialsManager(this.serviceEndpoint);
        }

        internal void InitializeEndpoint(string configurationName, EndpointAddress address, System.Configuration.Configuration configuration)
        {
            this.serviceEndpoint = this.CreateDescription();
            ServiceEndpoint endpoint = null;
            if (configurationName != null)
            {
                endpoint = ConfigLoader.LookupEndpoint(configurationName, address, this.serviceEndpoint.Contract, configuration.EvaluationContext);
            }
            if (endpoint != null)
            {
                this.serviceEndpoint = endpoint;
            }
            else
            {
                if (address != null)
                {
                    this.Endpoint.Address = address;
                }
                this.ApplyConfiguration(configurationName, configuration);
            }
            this.configurationName = configurationName;
            this.EnsureSecurityCredentialsManager(this.serviceEndpoint);
        }

        protected override void OnAbort()
        {
            if (this.innerFactory != null)
            {
                this.innerFactory.Abort();
            }
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseAsyncResult(this.innerFactory, timeout, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OpenAsyncResult(this.innerFactory, timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            if (this.innerFactory != null)
            {
                this.innerFactory.Close(timeout);
            }
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CloseAsyncResult.End(result);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            OpenAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            this.innerFactory.Open(timeout);
        }

        protected override void OnOpened()
        {
            if (this.Endpoint != null)
            {
                ClientCredentials credentials = this.Endpoint.Behaviors.Find<ClientCredentials>();
                if (credentials != null)
                {
                    ClientCredentials credentials2 = credentials.Clone();
                    credentials2.MakeReadOnly();
                    this.readOnlyClientCredentials = credentials2;
                }
            }
            base.OnOpened();
        }

        protected override void OnOpening()
        {
            base.OnOpening();
            this.innerFactory = this.CreateFactory();
            if (this.innerFactory == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("InnerChannelFactoryWasNotSet")));
            }
        }

        void IDisposable.Dispose()
        {
            base.Close();
        }

        public ClientCredentials Credentials
        {
            get
            {
                if (this.Endpoint == null)
                {
                    return null;
                }
                if ((base.State == CommunicationState.Created) || (base.State == CommunicationState.Opening))
                {
                    return this.EnsureCredentials(this.Endpoint);
                }
                if (this.readOnlyClientCredentials == null)
                {
                    ClientCredentials credentials = new ClientCredentials();
                    credentials.MakeReadOnly();
                    this.readOnlyClientCredentials = credentials;
                }
                return this.readOnlyClientCredentials;
            }
        }

        protected override TimeSpan DefaultCloseTimeout
        {
            get
            {
                if ((this.Endpoint != null) && (this.Endpoint.Binding != null))
                {
                    return this.Endpoint.Binding.CloseTimeout;
                }
                return ServiceDefaults.CloseTimeout;
            }
        }

        protected override TimeSpan DefaultOpenTimeout
        {
            get
            {
                if ((this.Endpoint != null) && (this.Endpoint.Binding != null))
                {
                    return this.Endpoint.Binding.OpenTimeout;
                }
                return ServiceDefaults.OpenTimeout;
            }
        }

        public ServiceEndpoint Endpoint
        {
            get
            {
                return this.serviceEndpoint;
            }
        }

        internal IChannelFactory InnerFactory
        {
            get
            {
                return this.innerFactory;
            }
        }

        internal bool UseActiveAutoClose { get; set; }

        private class CloseAsyncResult : AsyncResult
        {
            private ICommunicationObject communicationObject;
            private static AsyncCallback onCloseComplete = Fx.ThunkCallback(new AsyncCallback(ChannelFactory.CloseAsyncResult.OnCloseComplete));

            public CloseAsyncResult(ICommunicationObject communicationObject, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.communicationObject = communicationObject;
                if (this.communicationObject == null)
                {
                    base.Complete(true);
                }
                else
                {
                    IAsyncResult result = this.communicationObject.BeginClose(timeout, onCloseComplete, this);
                    if (result.CompletedSynchronously)
                    {
                        this.communicationObject.EndClose(result);
                        base.Complete(true);
                    }
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<ChannelFactory.CloseAsyncResult>(result);
            }

            private static void OnCloseComplete(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ChannelFactory.CloseAsyncResult asyncState = (ChannelFactory.CloseAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.communicationObject.EndClose(result);
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

        private class OpenAsyncResult : AsyncResult
        {
            private ICommunicationObject communicationObject;
            private static AsyncCallback onOpenComplete = Fx.ThunkCallback(new AsyncCallback(ChannelFactory.OpenAsyncResult.OnOpenComplete));

            public OpenAsyncResult(ICommunicationObject communicationObject, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.communicationObject = communicationObject;
                if (this.communicationObject == null)
                {
                    base.Complete(true);
                }
                else
                {
                    IAsyncResult result = this.communicationObject.BeginOpen(timeout, onOpenComplete, this);
                    if (result.CompletedSynchronously)
                    {
                        this.communicationObject.EndOpen(result);
                        base.Complete(true);
                    }
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<ChannelFactory.OpenAsyncResult>(result);
            }

            private static void OnOpenComplete(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ChannelFactory.OpenAsyncResult asyncState = (ChannelFactory.OpenAsyncResult) result.AsyncState;
                    Exception exception = null;
                    try
                    {
                        asyncState.communicationObject.EndOpen(result);
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

