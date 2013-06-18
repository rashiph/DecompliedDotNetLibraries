namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions;
    using System;
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Security.Tokens;

    internal class CoordinationServiceHost : ServiceHost, ICoordinationListener
    {
        private EndpointAddress baseEndpoint;
        private CoordinationService service;

        public CoordinationServiceHost(CoordinationService service, object serviceInstance)
        {
            this.service = service;
            base.InitializeDescription(serviceInstance, new UriSchemeKeyedCollection(new Uri[0]));
        }

        private void CreateBaseEndpointAddress()
        {
            if (DebugTrace.Info)
            {
                DebugTrace.Trace(TraceLevel.Info, "Creating base endpoint for {0}", base.SingletonInstance.GetType().Name);
            }
            if (((base.ChannelDispatchers.Count != 1) || (base.ChannelDispatchers[0] == null)) || ((base.ChannelDispatchers[0] as ChannelDispatcher).Endpoints.Count != 1))
            {
                DiagnosticUtility.FailFast("Must have exactly one endpoint dispatcher");
            }
            EndpointDispatcher dispatcher = (base.ChannelDispatchers[0] as ChannelDispatcher).Endpoints[0];
            this.baseEndpoint = dispatcher.EndpointAddress;
            if (DebugTrace.Info)
            {
                DebugTrace.Trace(TraceLevel.Info, "Listening on {0}", this.baseEndpoint.Uri);
            }
        }

        public EndpointAddress CreateEndpointReference(AddressHeader refParam)
        {
            if (this.baseEndpoint == null)
            {
                DiagnosticUtility.FailFast("Uninitialized base endpoint reference");
            }
            EndpointAddressBuilder builder = new EndpointAddressBuilder(this.baseEndpoint);
            builder.Headers.Clear();
            builder.Headers.Add(refParam);
            return builder.ToEndpointAddress();
        }

        protected override void InitializeRuntime()
        {
            DebugTrace.TraceEnter(this, "OnCreateListeners");
            base.InitializeRuntime();
            if (base.SingletonInstance is IWSActivationCoordinator)
            {
                if (DebugTrace.Info)
                {
                    for (int i = 0; i < base.ChannelDispatchers.Count; i++)
                    {
                        ChannelDispatcher dispatcher = base.ChannelDispatchers[i] as ChannelDispatcher;
                        if (dispatcher != null)
                        {
                            for (int j = 0; j < dispatcher.Endpoints.Count; j++)
                            {
                                EndpointDispatcher dispatcher2 = dispatcher.Endpoints[j];
                                DebugTrace.Trace(TraceLevel.Info, "Listening on {0}", dispatcher2.EndpointAddress.Uri);
                            }
                        }
                    }
                }
            }
            else
            {
                this.CreateBaseEndpointAddress();
            }
            DebugTrace.TraceLeave(this, "OnCreateListeners");
        }

        public void Start()
        {
            try
            {
                if (DebugTrace.Verbose)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "Opening ServiceHost for {0}", base.SingletonInstance.GetType().Name);
                }
                if (this.service.Config.SupportingTokensEnabled && (base.SingletonInstance is IWSRegistrationCoordinator))
                {
                    foreach (ServiceEndpoint endpoint in base.Description.Endpoints)
                    {
                        CustomBinding binding = new CustomBinding(endpoint.Binding);
                        SecurityBindingElement element = binding.Elements.Find<SecurityBindingElement>();
                        CoordinationStrings strings = CoordinationStrings.Version(this.service.ProtocolVersion);
                        if (element != null)
                        {
                            if (!element.OptionalOperationSupportingTokenParameters.ContainsKey(strings.RegisterAction))
                            {
                                element.OptionalOperationSupportingTokenParameters.Add(strings.RegisterAction, new SupportingTokenParameters());
                            }
                            element.OptionalOperationSupportingTokenParameters[strings.RegisterAction].Endorsing.Add(CoordinationServiceSecurity.SecurityContextSecurityTokenParameters);
                            endpoint.Binding = binding;
                        }
                    }
                }
                base.Open();
                if (DebugTrace.Verbose)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "Opened ServiceHost for {0}", base.SingletonInstance.GetType().Name);
                }
            }
            catch (CommunicationException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessagingInitializationException(Microsoft.Transactions.SR.GetString("ListenerCannotBeStarted", new object[] { this.baseEndpoint.Uri, exception.Message }), exception));
            }
        }

        public void Stop()
        {
            try
            {
                base.Close();
            }
            catch (CommunicationException exception)
            {
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Error);
            }
            catch (TimeoutException exception2)
            {
                DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Error);
            }
        }
    }
}

