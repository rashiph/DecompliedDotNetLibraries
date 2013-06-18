namespace System.ServiceModel.Security
{
    using System;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Xml;

    internal abstract class NegotiationTokenProvider<T> : IssuanceTokenProviderBase<T> where T: IssuanceTokenProviderState
    {
        private BindingContext issuanceBindingContext;
        private System.ServiceModel.Channels.MessageVersion messageVersion;
        private bool requiresManualReplyAddressing;
        private IChannelFactory<IRequestChannel> rstChannelFactory;

        protected NegotiationTokenProvider()
        {
        }

        protected override IAsyncResult BeginInitializeChannelFactories(EndpointAddress target, TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override IRequestChannel CreateClientChannel(EndpointAddress target, Uri via)
        {
            if (via != null)
            {
                return this.rstChannelFactory.CreateChannel(target, via);
            }
            return this.rstChannelFactory.CreateChannel(target);
        }

        protected override void EndInitializeChannelFactories(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected abstract IChannelFactory<IRequestChannel> GetNegotiationChannelFactory(IChannelFactory<IRequestChannel> transportChannelFactory, ChannelBuilder channelBuilder);
        protected override void InitializeChannelFactories(EndpointAddress target, TimeSpan timeout)
        {
        }

        public override void OnAbort()
        {
            if (this.rstChannelFactory != null)
            {
                this.rstChannelFactory.Abort();
                this.rstChannelFactory = null;
            }
            base.OnAbort();
        }

        public override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            if (this.rstChannelFactory != null)
            {
                this.rstChannelFactory.Close(timeout);
                this.rstChannelFactory = null;
            }
            base.OnClose(helper.RemainingTime());
        }

        public override void OnOpen(TimeSpan timeout)
        {
            if (this.IssuerBindingContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("IssuerBuildContextNotSet", new object[] { base.GetType() })));
            }
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.SetupRstChannelFactory();
            this.rstChannelFactory.Open(timeout);
            base.OnOpen(helper.RemainingTime());
        }

        private void SetupRstChannelFactory()
        {
            IChannelFactory<IRequestChannel> transportChannelFactory = null;
            ChannelBuilder channelBuilder = new ChannelBuilder(this.IssuerBindingContext.Clone(), true);
            if (channelBuilder.CanBuildChannelFactory<IRequestChannel>())
            {
                transportChannelFactory = channelBuilder.BuildChannelFactory<IRequestChannel>();
                this.requiresManualReplyAddressing = true;
            }
            else
            {
                ClientRuntime clientRuntime = new ClientRuntime("RequestSecurityTokenContract", "http://tempuri.org/") {
                    ValidateMustUnderstand = false
                };
                ServiceChannelFactory serviceChannelFactory = ServiceChannelFactory.BuildChannelFactory(channelBuilder, clientRuntime);
                serviceChannelFactory.ClientRuntime.UseSynchronizationContext = false;
                serviceChannelFactory.ClientRuntime.AddTransactionFlowProperties = false;
                ClientOperation item = new ClientOperation(serviceChannelFactory.ClientRuntime, "RequestSecurityToken", this.RequestSecurityTokenAction.Value) {
                    Formatter = MessageOperationFormatter.Instance
                };
                serviceChannelFactory.ClientRuntime.Operations.Add(item);
                if (this.IsMultiLegNegotiation)
                {
                    ClientOperation operation2 = new ClientOperation(serviceChannelFactory.ClientRuntime, "RequestSecurityTokenResponse", this.RequestSecurityTokenResponseAction.Value) {
                        Formatter = MessageOperationFormatter.Instance
                    };
                    serviceChannelFactory.ClientRuntime.Operations.Add(operation2);
                }
                this.requiresManualReplyAddressing = false;
                transportChannelFactory = new SecuritySessionSecurityTokenProvider.RequestChannelFactory(serviceChannelFactory);
            }
            this.rstChannelFactory = this.GetNegotiationChannelFactory(transportChannelFactory, channelBuilder);
            this.messageVersion = channelBuilder.Binding.MessageVersion;
        }

        protected override bool WillInitializeChannelFactoriesCompleteSynchronously(EndpointAddress target)
        {
            return true;
        }

        public BindingContext IssuerBindingContext
        {
            get
            {
                return this.issuanceBindingContext;
            }
            set
            {
                base.CommunicationObject.ThrowIfDisposedOrImmutable();
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }
                this.issuanceBindingContext = value.Clone();
            }
        }

        protected override System.ServiceModel.Channels.MessageVersion MessageVersion
        {
            get
            {
                return this.messageVersion;
            }
        }

        public override XmlDictionaryString RequestSecurityTokenAction
        {
            get
            {
                return base.StandardsManager.TrustDriver.RequestSecurityTokenAction;
            }
        }

        public override XmlDictionaryString RequestSecurityTokenResponseAction
        {
            get
            {
                return base.StandardsManager.TrustDriver.RequestSecurityTokenResponseAction;
            }
        }

        protected override bool RequiresManualReplyAddressing
        {
            get
            {
                base.ThrowIfCreated();
                return this.requiresManualReplyAddressing;
            }
        }
    }
}

