namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    internal class CoordinationService
    {
        private CoordinationServiceConfiguration config;
        private ChannelMruCache<IDatagramService> datagramChannelCache;
        private GlobalAclOperationRequirement globalAclAuthz;
        private Uri httpsBaseAddressUri;
        private Microsoft.Transactions.Wsat.Messaging.InteropActivationBinding interopActivationBinding;
        private ChannelFactory<IRequestReplyService> interopActivationChannelFactory;
        private Microsoft.Transactions.Wsat.Messaging.InteropDatagramBinding interopDatagramBinding;
        private ChannelFactory<IDatagramService> interopDatagramChannelFactory;
        private Microsoft.Transactions.Wsat.Messaging.InteropRegistrationBinding interopRegistrationBinding;
        private ChannelFactory<IRequestReplyService> interopRegistrationChannelFactory;
        private NamedPipeBinding namedPipeActivationBinding;
        private ChannelFactory<IRequestReplyService> namedPipeActivationChannelFactory;
        private Uri namedPipeBaseAddressUri;
        private Microsoft.Transactions.Wsat.Protocol.ProtocolVersion protocolVersion;
        private ChannelMruCache<IRequestReplyService> requestReplyChannelCache;
        private CoordinationServiceSecurity security;
        private WindowsRequestReplyBinding windowsActivationBinding;
        private ChannelFactory<IRequestReplyService> windowsActivationChannelFactory;

        static CoordinationService()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CoordinationService.UnhandledExceptionHandler);
        }

        public CoordinationService(CoordinationServiceConfiguration config, Microsoft.Transactions.Wsat.Protocol.ProtocolVersion protocolVersion)
        {
            this.protocolVersion = protocolVersion;
            DebugTrace.TraceEnter(this, "CoordinationService");
            try
            {
                this.Initialize(config);
            }
            catch
            {
                this.Cleanup();
                throw;
            }
            finally
            {
                DebugTrace.TraceLeave(this, "CoordinationService");
            }
        }

        public ICoordinationListener Add(IActivationCoordinator serviceInstance)
        {
            DebugTrace.TraceEnter("CoordinationService.Add (IActivationCoordinator)");
            this.AssertProtocolServiceMode();
            IWSActivationCoordinator dispatcher = ActivationCoordinatorDispatcher.Instance(this, serviceInstance);
            ICoordinationListener listener = this.CreateService(dispatcher, dispatcher.ContractType, BindingStrings.ActivationCoordinatorSuffix(this.protocolVersion));
            DebugTrace.TraceLeave("CoordinationService.Add (IActivationCoordinator)");
            return listener;
        }

        public ICoordinationListener Add(ICompletionCoordinator serviceInstance)
        {
            DebugTrace.TraceEnter("CoordinationService.Add (ICompletionCoordinator)");
            this.AssertProtocolServiceMode();
            IWSCompletionCoordinator dispatcher = CompletionCoordinatorDispatcher.Instance(this, serviceInstance);
            ICoordinationListener listener = this.CreateService(dispatcher, dispatcher.ContractType, BindingStrings.CompletionCoordinatorSuffix(this.protocolVersion));
            DebugTrace.TraceLeave("CoordinationService.Add (ICompletionCoordinator)");
            return listener;
        }

        public ICoordinationListener Add(ICompletionParticipant serviceInstance)
        {
            DebugTrace.TraceEnter("CoordinationService.Add (ICompletionParticipant)");
            this.AssertProtocolServiceMode();
            IWSCompletionParticipant dispatcher = CompletionParticipantDispatcher.Instance(this, serviceInstance);
            ICoordinationListener listener = this.CreateService(dispatcher, dispatcher.ContractType, BindingStrings.CompletionParticipantSuffix(this.protocolVersion));
            DebugTrace.TraceLeave("CoordinationService.Add (ICompletionParticipant)");
            return listener;
        }

        public ICoordinationListener Add(IRegistrationCoordinator serviceInstance)
        {
            DebugTrace.TraceEnter("CoordinationService.Add (IRegistrationCoordinator)");
            this.AssertProtocolServiceMode();
            IWSRegistrationCoordinator dispatcher = RegistrationCoordinatorDispatcher.Instance(this, serviceInstance);
            ICoordinationListener listener = this.CreateService(dispatcher, dispatcher.ContractType, BindingStrings.RegistrationCoordinatorSuffix(this.protocolVersion));
            DebugTrace.TraceLeave("CoordinationService.Add (IRegistrationCoordinator)");
            return listener;
        }

        public ICoordinationListener Add(ITwoPhaseCommitCoordinator serviceInstance)
        {
            DebugTrace.TraceEnter("CoordinationService.Add (ITwoPhaseCommitCoordinator)");
            this.AssertProtocolServiceMode();
            IWSTwoPhaseCommitCoordinator dispatcher = TwoPhaseCommitCoordinatorDispatcher.Instance(this, serviceInstance);
            ICoordinationListener listener = this.CreateService(dispatcher, dispatcher.ContractType, BindingStrings.TwoPhaseCommitCoordinatorSuffix(this.protocolVersion));
            DebugTrace.TraceLeave("CoordinationService.Add (ITwoPhaseCommitCoordinator)");
            return listener;
        }

        public ICoordinationListener Add(ITwoPhaseCommitParticipant serviceInstance)
        {
            DebugTrace.TraceEnter("CoordinationService.Add (ITwoPhaseCommitParticipant)");
            this.AssertProtocolServiceMode();
            IWSTwoPhaseCommitParticipant dispatcher = TwoPhaseCommitParticipantDispatcher.Instance(this, serviceInstance);
            ICoordinationListener listener = this.CreateService(dispatcher, dispatcher.ContractType, BindingStrings.TwoPhaseCommitParticipantSuffix(this.protocolVersion));
            DebugTrace.TraceLeave("CoordinationService.Add (ITwoPhaseCommitParticipant)");
            return listener;
        }

        private void AssertProtocolServiceMode()
        {
            if ((this.config.Mode & CoordinationServiceMode.ProtocolService) == 0)
            {
                DiagnosticUtility.FailFast("Must be in protocol service mode to create a proxy");
            }
        }

        public void Cleanup()
        {
            DebugTrace.TraceEnter(this, "Cleanup");
            if (this.namedPipeActivationChannelFactory != null)
            {
                this.CloseChannelFactory(this.namedPipeActivationChannelFactory);
                this.namedPipeActivationChannelFactory = null;
            }
            if (this.windowsActivationChannelFactory != null)
            {
                this.CloseChannelFactory(this.windowsActivationChannelFactory);
                this.windowsActivationChannelFactory = null;
            }
            if (this.interopDatagramChannelFactory != null)
            {
                this.CloseChannelFactory(this.interopDatagramChannelFactory);
                this.interopDatagramChannelFactory = null;
            }
            if (this.interopRegistrationChannelFactory != null)
            {
                this.CloseChannelFactory(this.interopRegistrationChannelFactory);
                this.interopRegistrationChannelFactory = null;
            }
            if (this.interopActivationChannelFactory != null)
            {
                this.CloseChannelFactory(this.interopActivationChannelFactory);
                this.interopActivationChannelFactory = null;
            }
            DebugTrace.TraceLeave(this, "Cleanup");
        }

        private void CloseChannelFactory(ChannelFactory cf)
        {
            try
            {
                cf.Close();
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

        public ActivationProxy CreateActivationProxy(EndpointAddress to, bool interop)
        {
            if ((this.config.Mode & CoordinationServiceMode.Formatter) == 0)
            {
                DiagnosticUtility.FailFast("Must be in formatter mode to create an activation proxy");
            }
            if (interop)
            {
                return new InteropActivationProxy(this, to);
            }
            return new WindowsActivationProxy(this, to);
        }

        private ChannelFactory<T> CreateChannelFactory<T>(Binding binding)
        {
            ChannelFactory<T> factory2;
            try
            {
                ChannelFactory<T> factory = new ChannelFactory<T>(binding);
                factory.Endpoint.Behaviors.Add(DisableTransactionFlowBehavior.Instance);
                factory2 = factory;
            }
            catch (CommunicationException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessagingInitializationException(Microsoft.Transactions.SR.GetString("FailedToCreateChannelFactory"), exception));
            }
            return factory2;
        }

        public CompletionCoordinatorProxy CreateCompletionCoordinatorProxy(EndpointAddress to, EndpointAddress from)
        {
            this.AssertProtocolServiceMode();
            return new CompletionCoordinatorProxy(this, to, from);
        }

        public CompletionParticipantProxy CreateCompletionParticipantProxy(EndpointAddress to)
        {
            this.AssertProtocolServiceMode();
            return new CompletionParticipantProxy(this, to);
        }

        public RegistrationProxy CreateRegistrationProxy(EndpointAddress to)
        {
            this.AssertProtocolServiceMode();
            return new RegistrationProxy(this, to);
        }

        private CoordinationServiceHost CreateService(object dispatcher, System.Type contract, string pathSuffix)
        {
            Binding namedPipeActivationBinding;
            ServiceCredentials serviceCredentials;
            CoordinationServiceHost host = new CoordinationServiceHost(this, dispatcher) {
                InternalBaseAddresses = { this.httpsBaseAddressUri }
            };
            ServiceAuthorizationBehavior behavior = host.Description.Behaviors.Find<ServiceAuthorizationBehavior>();
            behavior.PrincipalPermissionMode = PrincipalPermissionMode.None;
            behavior.ServiceAuthorizationManager = this.globalAclAuthz;
            if (dispatcher is IWSActivationCoordinator)
            {
                host.InternalBaseAddresses.Add(this.namedPipeBaseAddressUri);
                namedPipeActivationBinding = this.namedPipeActivationBinding;
                host.AddServiceEndpoint(contract, namedPipeActivationBinding, pathSuffix);
                if (this.config.RemoteClientsEnabled)
                {
                    namedPipeActivationBinding = this.windowsActivationBinding;
                    host.AddServiceEndpoint(contract, namedPipeActivationBinding, pathSuffix + "Remote/");
                }
                namedPipeActivationBinding = this.interopActivationBinding;
                serviceCredentials = new DefaultServiceCredentials();
            }
            else if (dispatcher is IWSRegistrationCoordinator)
            {
                namedPipeActivationBinding = this.interopRegistrationBinding;
                if (this.config.SupportingTokensEnabled)
                {
                    serviceCredentials = this.interopRegistrationBinding.SupportingTokenBindingElement.ServiceCredentials;
                }
                else
                {
                    serviceCredentials = new DefaultServiceCredentials();
                }
            }
            else
            {
                namedPipeActivationBinding = this.interopDatagramBinding;
                serviceCredentials = new DefaultServiceCredentials();
            }
            host.AddServiceEndpoint(contract, namedPipeActivationBinding, pathSuffix);
            serviceCredentials.WindowsAuthentication.IncludeWindowsGroups = true;
            serviceCredentials.ServiceCertificate.Certificate = this.config.X509Certificate;
            serviceCredentials.ClientCertificate.Certificate = this.config.X509Certificate;
            host.Description.Behaviors.Add(serviceCredentials);
            ServiceMetadataBehavior behavior2 = host.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (behavior2 != null)
            {
                if (DebugTrace.Verbose)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "Disabling WS-MeX support");
                }
                behavior2.HttpGetEnabled = false;
                behavior2.HttpsGetEnabled = false;
            }
            ServiceDebugBehavior behavior3 = host.Description.Behaviors.Find<ServiceDebugBehavior>();
            if (behavior3 != null)
            {
                if (DebugTrace.Verbose)
                {
                    DebugTrace.Trace(TraceLevel.Verbose, "Disabling WS-MeX support");
                }
                behavior3.HttpHelpPageEnabled = false;
                behavior3.HttpsHelpPageEnabled = false;
            }
            return host;
        }

        public TwoPhaseCommitCoordinatorProxy CreateTwoPhaseCommitCoordinatorProxy(EndpointAddress to, EndpointAddress from)
        {
            this.AssertProtocolServiceMode();
            return new TwoPhaseCommitCoordinatorProxy(this, to, from);
        }

        public TwoPhaseCommitParticipantProxy CreateTwoPhaseCommitParticipantProxy(EndpointAddress to, EndpointAddress from)
        {
            this.AssertProtocolServiceMode();
            return new TwoPhaseCommitParticipantProxy(this, to, from);
        }

        private void Initialize(CoordinationServiceConfiguration config)
        {
            DebugTrace.TraceEnter(this, "Initialize");
            this.config = config;
            this.security = new CoordinationServiceSecurity();
            if ((config.Mode == 0) || ((config.Mode & ~(CoordinationServiceMode.ProtocolService | CoordinationServiceMode.Formatter)) != 0))
            {
                DiagnosticUtility.FailFast("Invalid CoordinationServiceMode");
            }
            if ((config.Mode & CoordinationServiceMode.ProtocolService) == 0)
            {
                if (!string.IsNullOrEmpty(config.BasePath))
                {
                    DiagnosticUtility.FailFast("A base path must not be provided if protocol service mode is not enabled");
                }
                if (!string.IsNullOrEmpty(config.HostName))
                {
                    DiagnosticUtility.FailFast("A hostname must not be provided if protocol service mode is not enabled");
                }
            }
            else
            {
                if (string.IsNullOrEmpty(config.BasePath))
                {
                    DiagnosticUtility.FailFast("A base path must be provided if protocol service mode is enabled");
                }
                if (string.IsNullOrEmpty(config.HostName))
                {
                    DiagnosticUtility.FailFast("A hostname must be provided if protocol service mode is enabled");
                }
                if (config.X509Certificate == null)
                {
                    DiagnosticUtility.FailFast("No authentication mechanism was provided for the protocol service");
                }
            }
            this.globalAclAuthz = new GlobalAclOperationRequirement(config.GlobalAclWindowsIdentities, config.GlobalAclX509CertificateThumbprints, this.protocolVersion);
            if ((this.config.Mode & CoordinationServiceMode.ProtocolService) != 0)
            {
                this.httpsBaseAddressUri = new UriBuilder(Uri.UriSchemeHttps, this.config.HostName, this.config.HttpsPort, this.config.BasePath).Uri;
                this.namedPipeBaseAddressUri = new UriBuilder(Uri.UriSchemeNetPipe, "localhost", -1, this.config.HostName + "/" + this.config.BasePath).Uri;
            }
            this.namedPipeActivationBinding = new NamedPipeBinding(this.protocolVersion);
            if (this.config.RemoteClientsEnabled)
            {
                this.windowsActivationBinding = new WindowsRequestReplyBinding(this.protocolVersion);
            }
            this.interopDatagramBinding = new Microsoft.Transactions.Wsat.Messaging.InteropDatagramBinding(this.protocolVersion);
            this.interopRegistrationBinding = new Microsoft.Transactions.Wsat.Messaging.InteropRegistrationBinding(this.httpsBaseAddressUri, this.config.SupportingTokensEnabled, this.protocolVersion);
            this.interopActivationBinding = new Microsoft.Transactions.Wsat.Messaging.InteropActivationBinding(this.httpsBaseAddressUri, this.protocolVersion);
            ClientCredentials item = new ClientCredentials {
                ClientCertificate = { Certificate = this.config.X509Certificate },
                ServiceCertificate = { DefaultCertificate = this.config.X509Certificate }
            };
            if ((this.config.Mode & CoordinationServiceMode.ProtocolService) != 0)
            {
                this.interopDatagramChannelFactory = this.CreateChannelFactory<IDatagramService>(this.interopDatagramBinding);
                this.interopDatagramChannelFactory.Endpoint.Behaviors.Remove<ClientCredentials>();
                this.interopDatagramChannelFactory.Endpoint.Behaviors.Add(item);
                this.OpenChannelFactory<IDatagramService>(this.interopDatagramChannelFactory);
                this.interopRegistrationChannelFactory = this.CreateChannelFactory<IRequestReplyService>(this.interopRegistrationBinding);
                this.interopRegistrationChannelFactory.Endpoint.Behaviors.Remove<ClientCredentials>();
                this.interopRegistrationChannelFactory.Endpoint.Behaviors.Add(item);
                this.OpenChannelFactory<IRequestReplyService>(this.interopRegistrationChannelFactory);
            }
            if ((config.Mode & CoordinationServiceMode.Formatter) != 0)
            {
                if (this.config.X509Certificate != null)
                {
                    this.interopActivationChannelFactory = this.CreateChannelFactory<IRequestReplyService>(this.interopActivationBinding);
                    this.interopActivationChannelFactory.Endpoint.Behaviors.Remove<ClientCredentials>();
                    this.interopActivationChannelFactory.Endpoint.Behaviors.Add(item);
                    this.OpenChannelFactory<IRequestReplyService>(this.interopActivationChannelFactory);
                }
                this.namedPipeActivationChannelFactory = this.CreateChannelFactory<IRequestReplyService>(this.namedPipeActivationBinding);
                this.OpenChannelFactory<IRequestReplyService>(this.namedPipeActivationChannelFactory);
                if (this.config.RemoteClientsEnabled)
                {
                    this.windowsActivationChannelFactory = this.CreateChannelFactory<IRequestReplyService>(this.windowsActivationBinding);
                    this.OpenChannelFactory<IRequestReplyService>(this.windowsActivationChannelFactory);
                }
            }
            this.requestReplyChannelCache = new ChannelMruCache<IRequestReplyService>();
            if ((this.config.Mode & CoordinationServiceMode.ProtocolService) != 0)
            {
                this.datagramChannelCache = new ChannelMruCache<IDatagramService>();
            }
            DebugTrace.TraceLeave(this, "Initialize");
        }

        private void OpenChannelFactory<T>(ChannelFactory<T> cf)
        {
            try
            {
                cf.Open();
            }
            catch (ArgumentException exception)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessagingInitializationException(Microsoft.Transactions.SR.GetString("FailedToOpenChannelFactory"), exception));
            }
            catch (CommunicationException exception2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new MessagingInitializationException(Microsoft.Transactions.SR.GetString("FailedToOpenChannelFactory"), exception2));
            }
        }

        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            if (DebugTrace.Error)
            {
                Exception exceptionObject = (Exception) args.ExceptionObject;
                DebugTrace.Trace(TraceLevel.Error, "Unhandled {0} thrown: {1}", exceptionObject.GetType().Name, exceptionObject);
            }
        }

        public CoordinationServiceConfiguration Config
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.config;
            }
        }

        public ChannelMruCache<IDatagramService> DatagramChannelCache
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.datagramChannelCache;
            }
        }

        public GlobalAclOperationRequirement GlobalAcl
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.globalAclAuthz;
            }
        }

        public InteropRequestReplyBinding InteropActivationBinding
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.interopActivationBinding;
            }
        }

        public IChannelFactory<IRequestReplyService> InteropActivationChannelFactory
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.interopActivationChannelFactory;
            }
        }

        public Microsoft.Transactions.Wsat.Messaging.InteropDatagramBinding InteropDatagramBinding
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.interopDatagramBinding;
            }
        }

        public IChannelFactory<IDatagramService> InteropDatagramChannelFactory
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.interopDatagramChannelFactory;
            }
        }

        public InteropRequestReplyBinding InteropRegistrationBinding
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.interopRegistrationBinding;
            }
        }

        public IChannelFactory<IRequestReplyService> InteropRegistrationChannelFactory
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.interopRegistrationChannelFactory;
            }
        }

        public NamedPipeBinding NamedPipeActivationBinding
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.namedPipeActivationBinding;
            }
        }

        public IChannelFactory<IRequestReplyService> NamedPipeActivationChannelFactory
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.namedPipeActivationChannelFactory;
            }
        }

        public Microsoft.Transactions.Wsat.Protocol.ProtocolVersion ProtocolVersion
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.protocolVersion;
            }
        }

        public ChannelMruCache<IRequestReplyService> RequestReplyChannelCache
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.requestReplyChannelCache;
            }
        }

        public CoordinationServiceSecurity Security
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.security;
            }
        }

        public WindowsRequestReplyBinding WindowsActivationBinding
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.windowsActivationBinding;
            }
        }

        public IChannelFactory<IRequestReplyService> WindowsActivationChannelFactory
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.windowsActivationChannelFactory;
            }
        }

        private class DisableTransactionFlowBehavior : IEndpointBehavior
        {
            internal static CoordinationService.DisableTransactionFlowBehavior Instance = new CoordinationService.DisableTransactionFlowBehavior();

            void IEndpointBehavior.AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection parameters)
            {
            }

            void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime runtime)
            {
                runtime.AddTransactionFlowProperties = false;
            }

            void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointListener)
            {
            }

            void IEndpointBehavior.Validate(ServiceEndpoint serviceEndpoint)
            {
            }
        }
    }
}

