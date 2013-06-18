namespace System.ServiceModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.ServiceModel.Administration;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.Text;
    using System.Threading;

    public abstract class ServiceHostBase : CommunicationObject, IExtensibleObject<ServiceHostBase>, IDisposable
    {
        private UriSchemeKeyedCollection baseAddresses;
        private ChannelDispatcherCollection channelDispatchers;
        private TimeSpan closeTimeout = ServiceDefaults.ServiceHostCloseTimeout;
        private DefaultPerformanceCounters defaultPerformanceCounters;
        private System.ServiceModel.Description.ServiceDescription description;
        internal static readonly Uri EmptyUri = new Uri(string.Empty, UriKind.RelativeOrAbsolute);
        private ExtensionCollection<ServiceHostBase> extensions;
        private ReadOnlyCollection<Uri> externalBaseAddresses;
        private IDictionary<string, ContractDescription> implementedContracts;
        private bool initializeDescriptionHasFinished;
        private IInstanceContextManager instances;
        private TimeSpan openTimeout = ServiceDefaults.OpenTimeout;
        private ServiceAuthenticationBehavior readOnlyAuthentication;
        private ServiceAuthorizationBehavior readOnlyAuthorization;
        private ServiceCredentials readOnlyCredentials;
        private ServicePerformanceCountersBase servicePerformanceCounters;
        private System.ServiceModel.Dispatcher.ServiceThrottle serviceThrottle;

        public event EventHandler<UnknownMessageReceivedEventArgs> UnknownMessageReceived;

        protected ServiceHostBase()
        {
            TraceUtility.SetEtwProviderId();
            this.baseAddresses = new UriSchemeKeyedCollection(base.ThisLock);
            this.channelDispatchers = new ChannelDispatcherCollection(this, base.ThisLock);
            this.extensions = new ExtensionCollection<ServiceHostBase>(this, base.ThisLock);
            this.instances = new InstanceContextManager(base.ThisLock);
            this.serviceThrottle = new System.ServiceModel.Dispatcher.ServiceThrottle(this);
            base.TraceOpenAndClose = true;
            base.Faulted += new EventHandler(this.OnServiceHostFaulted);
        }

        protected void AddBaseAddress(Uri baseAddress)
        {
            if (this.initializeDescriptionHasFinished)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxCannotCallAddBaseAddress")));
            }
            this.baseAddresses.Add(baseAddress);
        }

        public virtual ReadOnlyCollection<ServiceEndpoint> AddDefaultEndpoints()
        {
            List<ServiceEndpoint> defaultEndpoints = new List<ServiceEndpoint>();
            foreach (Uri uri in this.InternalBaseAddresses)
            {
                ProtocolMappingItem protocolMapping = ConfigLoader.LookupProtocolMapping(uri.Scheme);
                if (protocolMapping != null)
                {
                    Binding defaultBinding = ConfigLoader.LookupBinding(protocolMapping.Binding, protocolMapping.BindingConfiguration);
                    if (defaultBinding == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Exception(System.ServiceModel.SR.GetString("BindingProtocolMappingNotDefined", new object[] { uri.Scheme })));
                    }
                    this.AddDefaultEndpoints(defaultBinding, defaultEndpoints);
                }
            }
            if (DiagnosticUtility.ShouldTraceInformation && (defaultEndpoints.Count > 0))
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                dictionary["ServiceConfigurationName"] = this.description.ConfigurationName;
                TraceUtility.TraceEvent(TraceEventType.Information, 0x80046, System.ServiceModel.SR.GetString("TraceCodeDefaultEndpointsAdded"), (TraceRecord) new DictionaryTraceRecord(dictionary));
            }
            return new ReadOnlyCollection<ServiceEndpoint>(defaultEndpoints);
        }

        internal virtual void AddDefaultEndpoints(Binding defaultBinding, List<ServiceEndpoint> defaultEndpoints)
        {
        }

        public virtual void AddServiceEndpoint(ServiceEndpoint endpoint)
        {
            if (endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
            }
            if ((base.State != CommunicationState.Created) && (base.State != CommunicationState.Opening))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxServiceHostBaseCannotAddEndpointAfterOpen")));
            }
            if (this.Description == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxServiceHostBaseCannotAddEndpointWithoutDescription")));
            }
            if (endpoint.Address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("SFxEndpointAddressNotSpecified"));
            }
            if (endpoint.Contract == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("SFxEndpointContractNotSpecified"));
            }
            if (endpoint.Binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("SFxEndpointBindingNotSpecified"));
            }
            if (!endpoint.IsSystemEndpoint || (endpoint.Contract.ContractType == typeof(IMetadataExchange)))
            {
                new ConfigLoader(this.GetContractResolver(this.implementedContracts)).LookupContract(endpoint.Contract.ConfigurationName, this.Description.Name);
            }
            this.Description.Endpoints.Add(endpoint);
        }

        public ServiceEndpoint AddServiceEndpoint(string implementedContract, Binding binding, string address)
        {
            return this.AddServiceEndpoint(implementedContract, binding, address, null);
        }

        public ServiceEndpoint AddServiceEndpoint(string implementedContract, Binding binding, Uri address)
        {
            return this.AddServiceEndpoint(implementedContract, binding, address, null);
        }

        public ServiceEndpoint AddServiceEndpoint(string implementedContract, Binding binding, string address, Uri listenUri)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("address"));
            }
            ServiceEndpoint endpoint = this.AddServiceEndpoint(implementedContract, binding, new Uri(address, UriKind.RelativeOrAbsolute));
            if (listenUri != null)
            {
                endpoint.UnresolvedListenUri = listenUri;
                listenUri = this.MakeAbsoluteUri(listenUri, binding);
                endpoint.ListenUri = listenUri;
            }
            return endpoint;
        }

        public ServiceEndpoint AddServiceEndpoint(string implementedContract, Binding binding, Uri address, Uri listenUri)
        {
            if (address == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("address"));
            }
            if (binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("binding"));
            }
            if (implementedContract == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("implementedContract"));
            }
            if ((base.State != CommunicationState.Created) && (base.State != CommunicationState.Opening))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxServiceHostBaseCannotAddEndpointAfterOpen")));
            }
            if (this.Description == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxServiceHostBaseCannotAddEndpointWithoutDescription")));
            }
            Uri uri = this.MakeAbsoluteUri(address, binding);
            ConfigLoader loader = new ConfigLoader(this.GetContractResolver(this.implementedContracts));
            ServiceEndpoint item = new ServiceEndpoint(loader.LookupContract(implementedContract, this.Description.Name), binding, new EndpointAddress(uri, new AddressHeader[0]));
            this.Description.Endpoints.Add(item);
            item.UnresolvedAddress = address;
            if (listenUri != null)
            {
                item.UnresolvedListenUri = listenUri;
                listenUri = this.MakeAbsoluteUri(listenUri, binding);
                item.ListenUri = listenUri;
            }
            return item;
        }

        protected virtual void ApplyConfiguration()
        {
            if (this.Description == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxServiceHostBaseCannotApplyConfigurationWithoutDescription")));
            }
            ConfigLoader configLoader = new ConfigLoader(this.GetContractResolver(this.implementedContracts));
            this.LoadConfigurationSectionInternal(configLoader, this.Description, this.Description.ConfigurationName);
            this.EnsureAuthentication(this.description);
            this.EnsureAuthorization(this.description);
            this.EnsureDebug(this.description);
        }

        internal virtual void BindInstance(System.ServiceModel.InstanceContext instance)
        {
            this.instances.Add(instance);
            if (this.servicePerformanceCounters != null)
            {
                lock (base.ThisLock)
                {
                    if (this.servicePerformanceCounters != null)
                    {
                        this.servicePerformanceCounters.ServiceInstanceCreated();
                    }
                }
            }
        }

        protected abstract System.ServiceModel.Description.ServiceDescription CreateDescription(out IDictionary<string, ContractDescription> implementedContracts);
        private ServiceAuthenticationBehavior EnsureAuthentication(System.ServiceModel.Description.ServiceDescription description)
        {
            ServiceAuthenticationBehavior item = description.Behaviors.Find<ServiceAuthenticationBehavior>();
            if (item == null)
            {
                item = new ServiceAuthenticationBehavior();
                description.Behaviors.Add(item);
            }
            return item;
        }

        private ServiceAuthorizationBehavior EnsureAuthorization(System.ServiceModel.Description.ServiceDescription description)
        {
            ServiceAuthorizationBehavior item = description.Behaviors.Find<ServiceAuthorizationBehavior>();
            if (item == null)
            {
                item = new ServiceAuthorizationBehavior();
                description.Behaviors.Add(item);
            }
            return item;
        }

        private ServiceCredentials EnsureCredentials(System.ServiceModel.Description.ServiceDescription description)
        {
            ServiceCredentials item = description.Behaviors.Find<ServiceCredentials>();
            if (item == null)
            {
                item = new ServiceCredentials();
                description.Behaviors.Add(item);
            }
            return item;
        }

        private ServiceDebugBehavior EnsureDebug(System.ServiceModel.Description.ServiceDescription description)
        {
            ServiceDebugBehavior item = description.Behaviors.Find<ServiceDebugBehavior>();
            if (item == null)
            {
                item = new ServiceDebugBehavior();
                description.Behaviors.Add(item);
            }
            return item;
        }

        internal void FaultInternal()
        {
            base.Fault();
        }

        internal string GetBaseAddressSchemes()
        {
            return GetBaseAddressSchemes(this.baseAddresses);
        }

        internal static string GetBaseAddressSchemes(UriSchemeKeyedCollection uriSchemeKeyedCollection)
        {
            StringBuilder builder = new StringBuilder();
            bool flag = true;
            foreach (Uri uri in uriSchemeKeyedCollection)
            {
                if (flag)
                {
                    builder.Append(uri.Scheme);
                    flag = false;
                }
                else
                {
                    builder.Append(CultureInfo.CurrentCulture.TextInfo.ListSeparator).Append(uri.Scheme);
                }
            }
            return builder.ToString();
        }

        internal virtual IContractResolver GetContractResolver(IDictionary<string, ContractDescription> implementedContracts)
        {
            ServiceAndBehaviorsContractResolver resolver = new ServiceAndBehaviorsContractResolver(new ImplementedContractsContractResolver(implementedContracts));
            resolver.AddBehaviorContractsToResolver((this.description == null) ? null : this.description.Behaviors);
            return resolver;
        }

        internal ReadOnlyCollection<System.ServiceModel.InstanceContext> GetInstanceContexts()
        {
            return Array.AsReadOnly<System.ServiceModel.InstanceContext>(this.instances.ToArray());
        }

        internal static Uri GetUri(Uri baseUri, string path)
        {
            if (path.StartsWith("/", StringComparison.Ordinal) || path.StartsWith(@"\", StringComparison.Ordinal))
            {
                int startIndex = 1;
                while (startIndex < path.Length)
                {
                    if ((path[startIndex] != '/') && (path[startIndex] != '\\'))
                    {
                        break;
                    }
                    startIndex++;
                }
                path = path.Substring(startIndex);
            }
            if (path.Length == 0)
            {
                return baseUri;
            }
            if (!baseUri.AbsoluteUri.EndsWith("/", StringComparison.Ordinal))
            {
                baseUri = new Uri(baseUri.AbsoluteUri + "/");
            }
            return new Uri(baseUri, path);
        }

        internal static Uri GetUri(Uri baseUri, Uri relativeUri)
        {
            return GetUri(baseUri, relativeUri.OriginalString);
        }

        internal Uri GetVia(string scheme, Uri address)
        {
            return GetVia(scheme, address, this.InternalBaseAddresses);
        }

        internal static Uri GetVia(string scheme, Uri address, UriSchemeKeyedCollection baseAddresses)
        {
            Uri uri = address;
            if (uri.IsAbsoluteUri)
            {
                return uri;
            }
            if (!baseAddresses.Contains(scheme))
            {
                return null;
            }
            return GetUri(baseAddresses[scheme], address);
        }

        public int IncrementManualFlowControlLimit(int incrementBy)
        {
            return this.ServiceThrottle.IncrementManualFlowControlLimit(incrementBy);
        }

        protected void InitializeDescription(UriSchemeKeyedCollection baseAddresses)
        {
            foreach (Uri uri in baseAddresses)
            {
                this.baseAddresses.Add(uri);
            }
            IDictionary<string, ContractDescription> implementedContracts = null;
            System.ServiceModel.Description.ServiceDescription description = this.CreateDescription(out implementedContracts);
            this.description = description;
            this.implementedContracts = implementedContracts;
            this.ApplyConfiguration();
            this.initializeDescriptionHasFinished = true;
        }

        protected virtual void InitializeRuntime()
        {
            if (this.Description == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxServiceHostBaseCannotInitializeRuntimeWithoutDescription")));
            }
            if (this.Description.Endpoints.Count == 0)
            {
                this.AddDefaultEndpoints();
            }
            new DispatcherBuilder().InitializeServiceHost(this.description, this);
            SecurityValidationBehavior.Instance.AfterBuildTimeValidation(this.description);
        }

        protected void LoadConfigurationSection(ServiceElement serviceSection)
        {
            if (serviceSection == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceSection");
            }
            if (this.Description == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxServiceHostBaseCannotLoadConfigurationSectionWithoutDescription")));
            }
            ConfigLoader configLoader = new ConfigLoader(this.GetContractResolver(this.ImplementedContracts));
            this.LoadConfigurationSectionInternal(configLoader, this.Description, serviceSection);
        }

        internal void LoadConfigurationSectionHelper(Uri baseAddress)
        {
            this.AddBaseAddress(baseAddress);
        }

        [SecuritySafeCritical]
        private void LoadConfigurationSectionInternal(ConfigLoader configLoader, System.ServiceModel.Description.ServiceDescription description, ServiceElement serviceSection)
        {
            configLoader.LoadServiceDescription(this, description, serviceSection, new Action<Uri>(this.LoadConfigurationSectionHelper));
        }

        [SecuritySafeCritical]
        private void LoadConfigurationSectionInternal(ConfigLoader configLoader, System.ServiceModel.Description.ServiceDescription description, string configurationName)
        {
            ServiceElement service = configLoader.LookupService(configurationName);
            this.LoadConfigurationSectionInternal(configLoader, description, service);
        }

        internal Uri MakeAbsoluteUri(Uri relativeOrAbsoluteUri, Binding binding)
        {
            return MakeAbsoluteUri(relativeOrAbsoluteUri, binding, this.InternalBaseAddresses);
        }

        internal static Uri MakeAbsoluteUri(Uri relativeOrAbsoluteUri, Binding binding, UriSchemeKeyedCollection baseAddresses)
        {
            Uri address = relativeOrAbsoluteUri;
            if (!address.IsAbsoluteUri)
            {
                if (binding.Scheme == string.Empty)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxCustomBindingWithoutTransport")));
                }
                address = GetVia(binding.Scheme, address, baseAddresses);
                if (address == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxEndpointNoMatchingScheme", new object[] { binding.Scheme, binding.Name, GetBaseAddressSchemes(baseAddresses) })));
                }
            }
            return address;
        }

        protected override void OnAbort()
        {
            this.instances.Abort();
            foreach (ChannelDispatcherBase base2 in this.ChannelDispatchers)
            {
                if (base2.Listener != null)
                {
                    base2.Listener.Abort();
                }
                base2.Abort();
            }
            ThreadTrace.StopTracing();
        }

        internal void OnAddChannelDispatcher(ChannelDispatcherBase channelDispatcher)
        {
            lock (base.ThisLock)
            {
                base.ThrowIfClosedOrOpened();
                channelDispatcher.AttachInternal(this);
                channelDispatcher.Faulted += new EventHandler(this.OnChannelDispatcherFaulted);
            }
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseAsyncResult(timeout, callback, state, this);
        }

        private void OnBeginOpen()
        {
            this.TraceBaseAddresses();
            MessageLogger.EnsureInitialized();
            this.InitializeRuntime();
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            this.OnBeginOpen();
            return new OpenCollectionAsyncResult(timeout, callback, state, this.SnapshotChannelDispatchers());
        }

        private void OnChannelDispatcherFaulted(object sender, EventArgs e)
        {
            base.Fault();
        }

        protected override void OnClose(TimeSpan timeout)
        {
            try
            {
                TimeoutHelper helper = new TimeoutHelper(timeout);
                if (ManagementExtension.IsEnabled && (this.Description != null))
                {
                    ManagementExtension.OnServiceClosing(this);
                }
                for (int i = 0; i < this.ChannelDispatchers.Count; i++)
                {
                    ChannelDispatcherBase base2 = this.ChannelDispatchers[i];
                    if (base2.Listener != null)
                    {
                        base2.Listener.Close(helper.RemainingTime());
                    }
                }
                for (int j = 0; j < this.ChannelDispatchers.Count; j++)
                {
                    this.ChannelDispatchers[j].CloseInput(helper.RemainingTime());
                }
                this.instances.CloseInput(helper.RemainingTime());
                this.instances.Close(helper.RemainingTime());
                for (int k = 0; k < this.ChannelDispatchers.Count; k++)
                {
                    this.ChannelDispatchers[k].Close(helper.RemainingTime());
                }
                this.TraceBaseAddresses();
                ThreadTrace.StopTracing();
            }
            catch (TimeoutException exception)
            {
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, 0x8002e, System.ServiceModel.SR.GetString("TraceCodeServiceHostTimeoutOnClose"), this, exception);
                }
                base.Abort();
            }
        }

        protected override void OnClosed()
        {
            try
            {
                for (int i = 0; i < this.ChannelDispatchers.Count; i++)
                {
                    ChannelDispatcher dispatcher = this.ChannelDispatchers[i] as ChannelDispatcher;
                    if (dispatcher != null)
                    {
                        dispatcher.ReleasePerformanceCounters();
                    }
                }
            }
            finally
            {
                base.OnClosed();
            }
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            try
            {
                CloseAsyncResult.End(result);
                this.TraceBaseAddresses();
                ThreadTrace.StopTracing();
            }
            catch (TimeoutException exception)
            {
                if (DiagnosticUtility.ShouldTraceWarning)
                {
                    TraceUtility.TraceEvent(TraceEventType.Warning, 0x8002e, System.ServiceModel.SR.GetString("TraceCodeServiceHostTimeoutOnClose"), this, exception);
                }
                base.Abort();
            }
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            OpenCollectionAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            this.OnBeginOpen();
            for (int i = 0; i < this.ChannelDispatchers.Count; i++)
            {
                this.ChannelDispatchers[i].Open(helper.RemainingTime());
            }
        }

        protected override void OnOpened()
        {
            if (this.Description != null)
            {
                ServiceCredentials credentials = this.description.Behaviors.Find<ServiceCredentials>();
                if (credentials != null)
                {
                    ServiceCredentials credentials2 = credentials.Clone();
                    credentials2.MakeReadOnly();
                    this.readOnlyCredentials = credentials2;
                }
                ServiceAuthorizationBehavior behavior = this.description.Behaviors.Find<ServiceAuthorizationBehavior>();
                if (behavior != null)
                {
                    ServiceAuthorizationBehavior behavior2 = behavior.Clone();
                    behavior2.MakeReadOnly();
                    this.readOnlyAuthorization = behavior2;
                }
                ServiceAuthenticationBehavior behavior3 = this.description.Behaviors.Find<ServiceAuthenticationBehavior>();
                if (behavior3 != null)
                {
                    ServiceAuthenticationBehavior behavior4 = behavior3.Clone();
                    behavior3.MakeReadOnly();
                    this.readOnlyAuthentication = behavior4;
                }
                if (ManagementExtension.IsEnabled)
                {
                    ManagementExtension.OnServiceOpened(this);
                }
            }
            base.OnOpened();
        }

        internal void OnRemoveChannelDispatcher(ChannelDispatcherBase channelDispatcher)
        {
            lock (base.ThisLock)
            {
                base.ThrowIfClosedOrOpened();
                channelDispatcher.DetachInternal(this);
            }
        }

        private void OnServiceHostFaulted(object sender, EventArgs args)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                TraceUtility.TraceEvent(TraceEventType.Warning, 0x8002f, System.ServiceModel.SR.GetString("TraceCodeServiceHostFaulted"), this);
            }
            foreach (ICommunicationObject obj2 in this.SnapshotChannelDispatchers())
            {
                if (obj2.State == CommunicationState.Opened)
                {
                    obj2.Abort();
                }
            }
        }

        internal void RaiseUnknownMessageReceived(Message message)
        {
            try
            {
                EventHandler<UnknownMessageReceivedEventArgs> unknownMessageReceived = this.UnknownMessageReceived;
                if (unknownMessageReceived != null)
                {
                    unknownMessageReceived(this, new UnknownMessageReceivedEventArgs(message));
                }
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(exception);
            }
        }

        protected void ReleasePerformanceCounters()
        {
        }

        public void SetEndpointAddress(ServiceEndpoint endpoint, string relativeAddress)
        {
            if (endpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpoint");
            }
            if (relativeAddress == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("relativeAddress");
            }
            if (endpoint.Binding == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(System.ServiceModel.SR.GetString("SFxEndpointBindingNotSpecified"));
            }
            Uri uri = this.MakeAbsoluteUri(new Uri(relativeAddress, UriKind.Relative), endpoint.Binding);
            endpoint.Address = new EndpointAddress(uri, new AddressHeader[0]);
        }

        private ICommunicationObject[] SnapshotChannelDispatchers()
        {
            lock (base.ThisLock)
            {
                ICommunicationObject[] objArray = new ICommunicationObject[this.ChannelDispatchers.Count];
                for (int i = 0; i < objArray.Length; i++)
                {
                    objArray[i] = this.ChannelDispatchers[i];
                }
                return objArray;
            }
        }

        void IDisposable.Dispose()
        {
            base.Close();
        }

        private void TraceBaseAddresses()
        {
            if ((DiagnosticUtility.ShouldTraceInformation && (this.baseAddresses != null)) && (this.baseAddresses.Count > 0))
            {
                TraceUtility.TraceEvent(TraceEventType.Information, 0x8002d, System.ServiceModel.SR.GetString("TraceCodeServiceHostBaseAddresses"), new CollectionTraceRecord("BaseAddresses", "Address", this.baseAddresses), this, null);
            }
        }

        internal virtual void UnbindInstance(System.ServiceModel.InstanceContext instance)
        {
            this.instances.Remove(instance);
            if (this.servicePerformanceCounters != null)
            {
                lock (base.ThisLock)
                {
                    if (this.servicePerformanceCounters != null)
                    {
                        this.servicePerformanceCounters.ServiceInstanceRemoved();
                    }
                }
            }
        }

        public ServiceAuthenticationBehavior Authentication
        {
            get
            {
                if (this.Description == null)
                {
                    return null;
                }
                if ((base.State != CommunicationState.Created) && (base.State != CommunicationState.Opening))
                {
                    return this.readOnlyAuthentication;
                }
                return this.EnsureAuthentication(this.Description);
            }
        }

        public ServiceAuthorizationBehavior Authorization
        {
            get
            {
                if (this.Description == null)
                {
                    return null;
                }
                if ((base.State != CommunicationState.Created) && (base.State != CommunicationState.Opening))
                {
                    return this.readOnlyAuthorization;
                }
                return this.EnsureAuthorization(this.Description);
            }
        }

        public ReadOnlyCollection<Uri> BaseAddresses
        {
            get
            {
                this.externalBaseAddresses = new ReadOnlyCollection<Uri>(new List<Uri>(this.baseAddresses));
                return this.externalBaseAddresses;
            }
        }

        public ChannelDispatcherCollection ChannelDispatchers
        {
            get
            {
                return this.channelDispatchers;
            }
        }

        public TimeSpan CloseTimeout
        {
            get
            {
                return this.closeTimeout;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    string message = System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0");
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", message));
                }
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", System.ServiceModel.SR.GetString("SFxTimeoutOutOfRangeTooBig")));
                }
                lock (base.ThisLock)
                {
                    base.ThrowIfClosedOrOpened();
                    this.closeTimeout = value;
                }
            }
        }

        internal ServicePerformanceCountersBase Counters
        {
            get
            {
                return this.servicePerformanceCounters;
            }
            set
            {
                this.servicePerformanceCounters = value;
                this.serviceThrottle.SetServicePerformanceCounters(this.servicePerformanceCounters);
            }
        }

        public ServiceCredentials Credentials
        {
            get
            {
                if (this.Description == null)
                {
                    return null;
                }
                if ((base.State != CommunicationState.Created) && (base.State != CommunicationState.Opening))
                {
                    return this.readOnlyCredentials;
                }
                return this.EnsureCredentials(this.Description);
            }
        }

        protected override TimeSpan DefaultCloseTimeout
        {
            get
            {
                return this.CloseTimeout;
            }
        }

        internal DefaultPerformanceCounters DefaultCounters
        {
            get
            {
                return this.defaultPerformanceCounters;
            }
            set
            {
                this.defaultPerformanceCounters = value;
            }
        }

        protected override TimeSpan DefaultOpenTimeout
        {
            get
            {
                return this.OpenTimeout;
            }
        }

        public System.ServiceModel.Description.ServiceDescription Description
        {
            get
            {
                return this.description;
            }
        }

        internal virtual object DisposableInstance
        {
            get
            {
                return null;
            }
        }

        public IExtensionCollection<ServiceHostBase> Extensions
        {
            get
            {
                return this.extensions;
            }
        }

        protected internal IDictionary<string, ContractDescription> ImplementedContracts
        {
            get
            {
                return this.implementedContracts;
            }
        }

        internal UriSchemeKeyedCollection InternalBaseAddresses
        {
            get
            {
                return this.baseAddresses;
            }
        }

        public int ManualFlowControlLimit
        {
            get
            {
                return this.ServiceThrottle.ManualFlowControlLimit;
            }
            set
            {
                this.ServiceThrottle.ManualFlowControlLimit = value;
            }
        }

        public TimeSpan OpenTimeout
        {
            get
            {
                return this.openTimeout;
            }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    string message = System.ServiceModel.SR.GetString("SFxTimeoutOutOfRange0");
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", message));
                }
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", System.ServiceModel.SR.GetString("SFxTimeoutOutOfRangeTooBig")));
                }
                lock (base.ThisLock)
                {
                    base.ThrowIfClosedOrOpened();
                    this.openTimeout = value;
                }
            }
        }

        internal System.ServiceModel.Dispatcher.ServiceThrottle ServiceThrottle
        {
            get
            {
                return this.serviceThrottle;
            }
        }

        private class CloseAsyncResult : AsyncResult
        {
            private ServiceHostBase serviceHost;
            private TimeoutHelper timeoutHelper;

            public CloseAsyncResult(TimeSpan timeout, AsyncCallback callback, object state, ServiceHostBase serviceHost) : base(callback, state)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.serviceHost = serviceHost;
                if (ManagementExtension.IsEnabled && (serviceHost.Description != null))
                {
                    ManagementExtension.OnServiceClosing(serviceHost);
                }
                this.CloseListeners(true);
            }

            private void CallComplete(bool completedSynchronously, Exception exception)
            {
                base.Complete(completedSynchronously, exception);
            }

            private void CloseChannelDispatchers(bool completedSynchronously)
            {
                IList<ICommunicationObject> collection = this.serviceHost.SnapshotChannelDispatchers();
                AsyncCallback otherCallback = Fx.ThunkCallback(new AsyncCallback(this.CloseChannelDispatchersCallback));
                TimeSpan timeout = this.timeoutHelper.RemainingTime();
                Exception exception = null;
                IAsyncResult result = null;
                try
                {
                    result = new CloseCollectionAsyncResult(timeout, otherCallback, this, collection);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2) || completedSynchronously)
                    {
                        throw;
                    }
                    exception = exception2;
                }
                if (exception != null)
                {
                    this.CallComplete(completedSynchronously, exception);
                }
                else if (result.CompletedSynchronously)
                {
                    this.FinishCloseChannelDispatchers(result, completedSynchronously);
                }
            }

            private void CloseChannelDispatchersCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ((ServiceHostBase.CloseAsyncResult) result.AsyncState).FinishCloseChannelDispatchers(result, false);
                }
            }

            private void CloseInput(bool completedSynchronously)
            {
                for (int i = 0; i < this.serviceHost.ChannelDispatchers.Count; i++)
                {
                    this.serviceHost.ChannelDispatchers[i].CloseInput(this.timeoutHelper.RemainingTime());
                }
                AsyncCallback callback = Fx.ThunkCallback(new AsyncCallback(this.CloseInputCallback));
                TimeSpan timeout = this.timeoutHelper.RemainingTime();
                Exception exception = null;
                IAsyncResult result = null;
                try
                {
                    result = this.serviceHost.instances.BeginCloseInput(timeout, callback, this);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2) || completedSynchronously)
                    {
                        throw;
                    }
                }
                if (exception != null)
                {
                    this.CallComplete(completedSynchronously, exception);
                }
                else if (result.CompletedSynchronously)
                {
                    this.FinishCloseInput(result, completedSynchronously);
                }
            }

            private void CloseInputCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ((ServiceHostBase.CloseAsyncResult) result.AsyncState).FinishCloseInput(result, false);
                }
            }

            private void CloseInstances(bool completedSynchronously)
            {
                AsyncCallback callback = Fx.ThunkCallback(new AsyncCallback(this.CloseInstancesCallback));
                TimeSpan timeout = this.timeoutHelper.RemainingTime();
                Exception exception = null;
                IAsyncResult result = null;
                try
                {
                    result = this.serviceHost.instances.BeginClose(timeout, callback, this);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2) || completedSynchronously)
                    {
                        throw;
                    }
                    exception = exception2;
                }
                if (exception != null)
                {
                    this.CallComplete(completedSynchronously, exception);
                }
                else if (result.CompletedSynchronously)
                {
                    this.FinishCloseInstances(result, completedSynchronously);
                }
            }

            private void CloseInstancesCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ((ServiceHostBase.CloseAsyncResult) result.AsyncState).FinishCloseInstances(result, false);
                }
            }

            private void CloseListeners(bool completedSynchronously)
            {
                List<ICommunicationObject> collection = new List<ICommunicationObject>();
                for (int i = 0; i < this.serviceHost.ChannelDispatchers.Count; i++)
                {
                    if (this.serviceHost.ChannelDispatchers[i].Listener != null)
                    {
                        collection.Add(this.serviceHost.ChannelDispatchers[i].Listener);
                    }
                }
                AsyncCallback otherCallback = Fx.ThunkCallback(new AsyncCallback(this.CloseListenersCallback));
                TimeSpan timeout = this.timeoutHelper.RemainingTime();
                Exception exception = null;
                IAsyncResult result = null;
                try
                {
                    result = new CloseCollectionAsyncResult(timeout, otherCallback, this, collection);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2) || completedSynchronously)
                    {
                        throw;
                    }
                    exception = exception2;
                }
                if (exception != null)
                {
                    this.CallComplete(completedSynchronously, exception);
                }
                else if (result.CompletedSynchronously)
                {
                    this.FinishCloseListeners(result, completedSynchronously);
                }
            }

            private void CloseListenersCallback(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ((ServiceHostBase.CloseAsyncResult) result.AsyncState).FinishCloseListeners(result, false);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<ServiceHostBase.CloseAsyncResult>(result);
            }

            private void FinishCloseChannelDispatchers(IAsyncResult result, bool completedSynchronously)
            {
                Exception exception = null;
                try
                {
                    CloseCollectionAsyncResult.End(result);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2) || completedSynchronously)
                    {
                        throw;
                    }
                    exception = exception2;
                }
                this.CallComplete(completedSynchronously, exception);
            }

            private void FinishCloseInput(IAsyncResult result, bool completedSynchronously)
            {
                Exception exception = null;
                try
                {
                    this.serviceHost.instances.EndCloseInput(result);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2) || completedSynchronously)
                    {
                        throw;
                    }
                    exception = exception2;
                }
                if (exception != null)
                {
                    this.CallComplete(completedSynchronously, exception);
                }
                else
                {
                    this.CloseInstances(completedSynchronously);
                }
            }

            private void FinishCloseInstances(IAsyncResult result, bool completedSynchronously)
            {
                Exception exception = null;
                try
                {
                    this.serviceHost.instances.EndClose(result);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2) || completedSynchronously)
                    {
                        throw;
                    }
                    exception = exception2;
                }
                if (exception != null)
                {
                    this.CallComplete(completedSynchronously, exception);
                }
                else
                {
                    this.CloseChannelDispatchers(completedSynchronously);
                }
            }

            private void FinishCloseListeners(IAsyncResult result, bool completedSynchronously)
            {
                Exception exception = null;
                try
                {
                    CloseCollectionAsyncResult.End(result);
                }
                catch (Exception exception2)
                {
                    if (Fx.IsFatal(exception2) || completedSynchronously)
                    {
                        throw;
                    }
                    exception = exception2;
                }
                if (exception != null)
                {
                    this.CallComplete(completedSynchronously, exception);
                }
                else
                {
                    this.CloseInput(completedSynchronously);
                }
            }
        }

        private class ImplementedContractsContractResolver : IContractResolver
        {
            private IDictionary<string, ContractDescription> implementedContracts;

            public ImplementedContractsContractResolver(IDictionary<string, ContractDescription> implementedContracts)
            {
                this.implementedContracts = implementedContracts;
            }

            public ContractDescription ResolveContract(string contractName)
            {
                if ((this.implementedContracts != null) && this.implementedContracts.ContainsKey(contractName))
                {
                    return this.implementedContracts[contractName];
                }
                return null;
            }
        }

        internal class ServiceAndBehaviorsContractResolver : IContractResolver
        {
            private Dictionary<string, ContractDescription> behaviorContracts;
            private IContractResolver serviceResolver;

            public ServiceAndBehaviorsContractResolver(IContractResolver serviceResolver)
            {
                this.serviceResolver = serviceResolver;
                this.behaviorContracts = new Dictionary<string, ContractDescription>();
            }

            public void AddBehaviorContractsToResolver(KeyedByTypeCollection<IServiceBehavior> behaviors)
            {
                if ((behaviors != null) && behaviors.Contains(typeof(ServiceMetadataBehavior)))
                {
                    behaviors.Find<ServiceMetadataBehavior>().AddImplementedContracts(this);
                }
            }

            public ContractDescription ResolveContract(string contractName)
            {
                ContractDescription description = this.serviceResolver.ResolveContract(contractName);
                if (description == null)
                {
                    description = this.behaviorContracts.ContainsKey(contractName) ? this.behaviorContracts[contractName] : null;
                }
                return description;
            }

            public Dictionary<string, ContractDescription> BehaviorContracts
            {
                get
                {
                    return this.behaviorContracts;
                }
            }
        }
    }
}

