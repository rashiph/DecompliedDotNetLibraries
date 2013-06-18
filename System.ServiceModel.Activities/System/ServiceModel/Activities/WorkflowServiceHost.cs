namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.Activities.Hosting;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.DurableInstancing;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Activities.Description;
    using System.ServiceModel.Activities.Dispatcher;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Xml.Linq;

    public class WorkflowServiceHost : ServiceHostBase
    {
        private System.Activities.Activity activity;
        private static readonly System.Type baseActivityType = typeof(System.Activities.Activity);
        private static readonly System.Type bufferedReceiveServiceBehaviorType = typeof(BufferedReceiveServiceBehavior);
        private IDictionary<XName, Collection<CorrelationQuery>> correlationQueries;
        private static readonly System.Type correlationQueryBehaviorType = typeof(CorrelationQueryBehavior);
        private static readonly TimeSpan defaultFilterResumeTimeout = TimeSpan.FromMinutes(1.0);
        private static readonly TimeSpan defaultPersistTimeout = TimeSpan.FromSeconds(30.0);
        private static readonly TimeSpan defaultTrackTimeout = TimeSpan.FromSeconds(30.0);
        private System.ServiceModel.Activities.Dispatcher.DurableInstanceManager durableInstanceManager;
        private TimeSpan idleTimeToPersist;
        private TimeSpan idleTimeToUnload;
        private IDictionary<XName, ContractDescription> inferredContracts;
        private static readonly System.Type mexBehaviorType = typeof(ServiceMetadataBehavior);
        private static readonly XName mexContractXName = XName.Get("IMetadataExchange", "http://schemas.microsoft.com/2006/04/mex");
        private WorkflowService serviceDefinition;
        private WorkflowUnhandledExceptionAction unhandledExceptionAction;
        private WorkflowServiceHostExtensions workflowExtensions;

        protected WorkflowServiceHost()
        {
            this.InitializeFromConstructor((WorkflowService) null, new Uri[0]);
        }

        public WorkflowServiceHost(System.Activities.Activity activity, params Uri[] baseAddresses)
        {
            if (activity == null)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.ArgumentNull("activity");
            }
            this.InitializeFromConstructor(activity, baseAddresses);
        }

        public WorkflowServiceHost(object serviceImplementation, params Uri[] baseAddresses)
        {
            if (serviceImplementation == null)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.ArgumentNull("serviceImplementation");
            }
            if (serviceImplementation is WorkflowService)
            {
                this.InitializeFromConstructor((WorkflowService) serviceImplementation, baseAddresses);
            }
            else
            {
                System.Activities.Activity activity = serviceImplementation as System.Activities.Activity;
                if (activity == null)
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.Argument("serviceImplementation", System.ServiceModel.Activities.SR.InvalidServiceImplementation);
                }
                this.InitializeFromConstructor(activity, baseAddresses);
            }
        }

        public WorkflowServiceHost(WorkflowService serviceDefinition, params Uri[] baseAddresses)
        {
            if (serviceDefinition == null)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.ArgumentNull("serviceDefinition");
            }
            this.InitializeFromConstructor(serviceDefinition, baseAddresses);
        }

        private void AddCorrelationQueryBehaviorToServiceEndpoint(ServiceEndpoint serviceEndpoint)
        {
            Collection<CorrelationQuery> collection;
            XName key = XName.Get(serviceEndpoint.Contract.Name, serviceEndpoint.Contract.Namespace);
            if ((this.correlationQueries != null) && this.correlationQueries.TryGetValue(key, out collection))
            {
                Collection<CorrelationQuery> queries = new Collection<CorrelationQuery>();
                foreach (CorrelationQuery query in collection)
                {
                    if (!queries.Contains(query))
                    {
                        queries.Add(query);
                    }
                    else if (System.ServiceModel.Activities.TD.DuplicateCorrelationQueryIsEnabled())
                    {
                        System.ServiceModel.Activities.TD.DuplicateCorrelationQuery(query.Where.ToString());
                    }
                }
                CorrelationQueryBehavior item = new CorrelationQueryBehavior(queries) {
                    ServiceContractName = key
                };
                serviceEndpoint.Behaviors.Add(item);
            }
            else if (CorrelationQueryBehavior.BindingHasDefaultQueries(serviceEndpoint.Binding) && !serviceEndpoint.Behaviors.Contains(typeof(CorrelationQueryBehavior)))
            {
                CorrelationQueryBehavior behavior2 = new CorrelationQueryBehavior(new Collection<CorrelationQuery>()) {
                    ServiceContractName = key
                };
                serviceEndpoint.Behaviors.Add(behavior2);
            }
        }

        internal override void AddDefaultEndpoints(Binding defaultBinding, List<ServiceEndpoint> defaultEndpoints)
        {
            if (this.inferredContracts != null)
            {
                foreach (XName name in this.inferredContracts.Keys)
                {
                    ServiceEndpoint endpoint = this.AddServiceEndpoint(name, defaultBinding, string.Empty, null, null);
                    ConfigLoader.LoadDefaultEndpointBehaviors(endpoint);
                    this.AddCorrelationQueryBehaviorToServiceEndpoint(endpoint);
                    defaultEndpoints.Add(endpoint);
                }
            }
        }

        public override void AddServiceEndpoint(ServiceEndpoint endpoint)
        {
            if (!endpoint.IsSystemEndpoint)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.CannotUseAddServiceEndpointOverloadForWorkflowServices));
            }
            base.AddServiceEndpoint(endpoint);
        }

        public ServiceEndpoint AddServiceEndpoint(string implementedContract, Binding binding, string address)
        {
            return base.AddServiceEndpoint(implementedContract, binding, address);
        }

        public ServiceEndpoint AddServiceEndpoint(string implementedContract, Binding binding, Uri address)
        {
            return base.AddServiceEndpoint(implementedContract, binding, address);
        }

        public ServiceEndpoint AddServiceEndpoint(string implementedContract, Binding binding, string address, Uri listenUri)
        {
            return base.AddServiceEndpoint(implementedContract, binding, address, listenUri);
        }

        public ServiceEndpoint AddServiceEndpoint(string implementedContract, Binding binding, Uri address, Uri listenUri)
        {
            return base.AddServiceEndpoint(implementedContract, binding, address, listenUri);
        }

        public ServiceEndpoint AddServiceEndpoint(XName serviceContractName, Binding binding, string address, Uri listenUri = null, string behaviorConfigurationName = null)
        {
            return this.AddServiceEndpoint(serviceContractName, binding, new Uri(address, UriKind.RelativeOrAbsolute), listenUri, behaviorConfigurationName);
        }

        public ServiceEndpoint AddServiceEndpoint(XName serviceContractName, Binding binding, Uri address, Uri listenUri = null, string behaviorConfigurationName = null)
        {
            if (binding == null)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.ArgumentNull("binding");
            }
            if (address == null)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.ArgumentNull("address");
            }
            Uri uri = base.MakeAbsoluteUri(address, binding);
            return this.AddServiceEndpointCore(serviceContractName, binding, new EndpointAddress(uri, new AddressHeader[0]), listenUri, behaviorConfigurationName);
        }

        private ServiceEndpoint AddServiceEndpointCore(XName serviceContractName, Binding binding, EndpointAddress address, Uri listenUri = null, string behaviorConfigurationName = null)
        {
            ServiceEndpoint endpoint;
            ContractDescription description;
            if (serviceContractName == null)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.ArgumentNull("serviceContractName");
            }
            if (this.inferredContracts == null)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.ContractNotFoundInAddServiceEndpoint(serviceContractName.LocalName, serviceContractName.NamespaceName)));
            }
            ContractInferenceHelper.ProvideDefaultNamespace(ref serviceContractName);
            if (this.inferredContracts.TryGetValue(serviceContractName, out description))
            {
                endpoint = new ServiceEndpoint(description, binding, address);
                if (!string.IsNullOrEmpty(behaviorConfigurationName))
                {
                    ConfigLoader.LoadChannelBehaviors(behaviorConfigurationName, null, endpoint.Behaviors);
                }
            }
            else
            {
                if (!(serviceContractName == mexContractXName))
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.ContractNotFoundInAddServiceEndpoint(serviceContractName.LocalName, serviceContractName.NamespaceName)));
                }
                if (!base.Description.Behaviors.Contains(mexBehaviorType))
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.ServiceMetadataBehaviorNotFoundForServiceMetadataEndpoint(base.Description.Name)));
                }
                endpoint = new ServiceMetadataEndpoint(binding, address);
            }
            if (listenUri != null)
            {
                listenUri = base.MakeAbsoluteUri(listenUri, binding);
                endpoint.ListenUri = listenUri;
            }
            base.Description.Endpoints.Add(endpoint);
            if (System.ServiceModel.Activities.TD.ServiceEndpointAddedIsEnabled())
            {
                System.ServiceModel.Activities.TD.ServiceEndpointAdded(address.Uri.ToString(), binding.GetType().ToString(), endpoint.Contract.Name);
            }
            return endpoint;
        }

        private IAsyncResult BeginHostClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return base.OnBeginClose(timeout, callback, state);
        }

        private IAsyncResult BeginHostOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return base.OnBeginOpen(timeout, callback, state);
        }

        protected override System.ServiceModel.Description.ServiceDescription CreateDescription(out IDictionary<string, ContractDescription> implementedContracts)
        {
            Fx.AssertAndThrow(this.serviceDefinition != null, "serviceDefinition is null");
            this.activity = this.serviceDefinition.Body;
            Dictionary<string, ContractDescription> dictionary = new Dictionary<string, ContractDescription>();
            this.inferredContracts = this.serviceDefinition.GetContractDescriptions();
            if (this.inferredContracts != null)
            {
                foreach (ContractDescription description in this.inferredContracts.Values)
                {
                    if (!string.IsNullOrEmpty(description.ConfigurationName))
                    {
                        if (dictionary.ContainsKey(description.ConfigurationName))
                        {
                            throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.DifferentContractsSameConfigName));
                        }
                        dictionary.Add(description.ConfigurationName, description);
                    }
                }
            }
            implementedContracts = dictionary;
            this.correlationQueries = this.serviceDefinition.CorrelationQueries;
            return this.serviceDefinition.GetEmptyServiceDescription();
        }

        private void EndHostClose(IAsyncResult result)
        {
            base.OnEndClose(result);
        }

        private void EndHostOpen(IAsyncResult result)
        {
            base.OnEndOpen(result);
        }

        internal void FaultServiceHostIfNecessary(Exception exception)
        {
            if ((exception is InstancePersistenceException) && !(exception is InstancePersistenceCommandException))
            {
                base.Fault(exception);
            }
        }

        private void FixupEndpoints()
        {
            Dictionary<System.Type, ContractDescription> dictionary = new Dictionary<System.Type, ContractDescription>();
            foreach (ServiceEndpoint endpoint in base.Description.Endpoints)
            {
                if (this.serviceDefinition.AllowBufferedReceive)
                {
                    this.SetupReceiveContextEnabledAttribute(endpoint);
                }
                if (!endpoint.Behaviors.Contains(correlationQueryBehaviorType))
                {
                    this.AddCorrelationQueryBehaviorToServiceEndpoint(endpoint);
                }
                if (endpoint is WorkflowHostingEndpoint)
                {
                    ContractDescription description;
                    if (dictionary.TryGetValue(endpoint.Contract.ContractType, out description))
                    {
                        endpoint.Contract = description;
                    }
                    else
                    {
                        dictionary[endpoint.Contract.ContractType] = endpoint.Contract;
                    }
                }
            }
            if (this.serviceDefinition.AllowBufferedReceive && !base.Description.Behaviors.Contains(bufferedReceiveServiceBehaviorType))
            {
                base.Description.Behaviors.Add(new BufferedReceiveServiceBehavior());
            }
        }

        private void InitializeDescription(WorkflowService serviceDefinition, UriSchemeKeyedCollection baseAddresses)
        {
            this.serviceDefinition = serviceDefinition;
            base.InitializeDescription(baseAddresses);
            foreach (Endpoint endpoint in serviceDefinition.Endpoints)
            {
                if (endpoint.Binding == null)
                {
                    string errorMessageEndpointName = ContractValidationHelper.GetErrorMessageEndpointName(endpoint.Name);
                    string errorMessageEndpointServiceContractName = ContractValidationHelper.GetErrorMessageEndpointServiceContractName(endpoint.ServiceContractName);
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.MissingBindingInEndpoint(errorMessageEndpointName, errorMessageEndpointServiceContractName)));
                }
                ServiceEndpoint endpoint2 = this.AddServiceEndpointCore(endpoint.ServiceContractName, endpoint.Binding, endpoint.GetAddress(this), endpoint.ListenUri, endpoint.BehaviorConfigurationName);
                if (!string.IsNullOrEmpty(endpoint.Name))
                {
                    endpoint2.Name = endpoint.Name;
                }
                endpoint2.UnresolvedAddress = endpoint.AddressUri;
                endpoint2.UnresolvedListenUri = endpoint.ListenUri;
            }
            this.PersistTimeout = defaultPersistTimeout;
            this.TrackTimeout = defaultTrackTimeout;
            this.FilterResumeTimeout = defaultFilterResumeTimeout;
        }

        private void InitializeFromConstructor(System.Activities.Activity activity, params Uri[] baseAddresses)
        {
            WorkflowService serviceDefinition = new WorkflowService {
                Body = activity
            };
            this.InitializeFromConstructor(serviceDefinition, baseAddresses);
        }

        private void InitializeFromConstructor(WorkflowService serviceDefinition, params Uri[] baseAddresses)
        {
            this.idleTimeToPersist = WorkflowIdleBehavior.defaultTimeToPersist;
            this.idleTimeToUnload = WorkflowIdleBehavior.defaultTimeToUnload;
            this.unhandledExceptionAction = WorkflowUnhandledExceptionAction.AbandonAndSuspend;
            this.workflowExtensions = new WorkflowServiceHostExtensions();
            if (System.ServiceModel.Activities.TD.CreateWorkflowServiceHostStartIsEnabled())
            {
                System.ServiceModel.Activities.TD.CreateWorkflowServiceHostStart();
            }
            if (serviceDefinition != null)
            {
                this.InitializeDescription(serviceDefinition, new UriSchemeKeyedCollection(baseAddresses));
            }
            this.durableInstanceManager = new System.ServiceModel.Activities.Dispatcher.DurableInstanceManager(this);
            if (System.ServiceModel.Activities.TD.CreateWorkflowServiceHostStopIsEnabled())
            {
                System.ServiceModel.Activities.TD.CreateWorkflowServiceHostStop();
            }
        }

        protected override void InitializeRuntime()
        {
            if (base.Description != null)
            {
                this.FixupEndpoints();
                if (this.DurableInstancingOptions.ScopeName == null)
                {
                    this.DurableInstancingOptions.ScopeName = XNamespace.Get(base.Description.Namespace).GetName(base.Description.Name);
                }
            }
            base.InitializeRuntime();
            this.ServiceName = XNamespace.Get(base.Description.Namespace).GetName(base.Description.Name);
            this.workflowExtensions.EnsureChannelCache();
            this.WorkflowExtensions.Add(new CorrelationExtension(this.DurableInstancingOptions.ScopeName));
            this.WorkflowExtensions.MakeReadOnly();
            this.IsLoadTransactionRequired = WorkflowServiceInstance.IsLoadTransactionRequired(this);
            if (this.serviceDefinition != null)
            {
                this.ValidateBufferedReceiveProperty();
                this.serviceDefinition.ResetServiceDescription();
            }
        }

        protected override void OnAbort()
        {
            base.OnAbort();
            this.durableInstanceManager.Abort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CloseAsyncResult(this, timeout, callback, state);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new OpenAsyncResult(this, timeout, callback, state);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            TimeoutHelper helper = new TimeoutHelper(timeout);
            base.OnClose(helper.RemainingTime());
            this.durableInstanceManager.Close(helper.RemainingTime());
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
            TimeoutHelper helper = new TimeoutHelper(timeout);
            base.OnOpen(helper.RemainingTime());
            this.durableInstanceManager.Open(helper.RemainingTime());
        }

        private void SetupReceiveContextEnabledAttribute(ServiceEndpoint serviceEndpoint)
        {
            if (BufferedReceiveServiceBehavior.IsWorkflowEndpoint(serviceEndpoint))
            {
                foreach (OperationDescription description in serviceEndpoint.Contract.Operations)
                {
                    ReceiveContextEnabledAttribute attribute = description.Behaviors.Find<ReceiveContextEnabledAttribute>();
                    if (attribute == null)
                    {
                        ReceiveContextEnabledAttribute item = new ReceiveContextEnabledAttribute {
                            ManualControl = true
                        };
                        description.Behaviors.Add(item);
                    }
                    else
                    {
                        attribute.ManualControl = true;
                    }
                }
            }
        }

        private void ValidateBufferedReceiveProperty()
        {
            if (base.Description.Behaviors.Contains(bufferedReceiveServiceBehaviorType) && !this.serviceDefinition.AllowBufferedReceive)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.BufferedReceiveBehaviorUsedWithoutProperty));
            }
        }

        public System.Activities.Activity Activity
        {
            get
            {
                return this.activity;
            }
        }

        internal System.ServiceModel.Activities.Dispatcher.DurableInstanceManager DurableInstanceManager
        {
            get
            {
                return this.durableInstanceManager;
            }
        }

        public System.ServiceModel.Activities.DurableInstancingOptions DurableInstancingOptions
        {
            get
            {
                return this.durableInstanceManager.DurableInstancingOptions;
            }
        }

        internal TimeSpan FilterResumeTimeout { get; set; }

        internal TimeSpan IdleTimeToPersist
        {
            get
            {
                return this.idleTimeToPersist;
            }
            set
            {
                this.idleTimeToPersist = value;
            }
        }

        internal TimeSpan IdleTimeToUnload
        {
            get
            {
                return this.idleTimeToUnload;
            }
            set
            {
                this.idleTimeToUnload = value;
            }
        }

        internal bool IsLoadTransactionRequired { get; private set; }

        internal TimeSpan PersistTimeout { get; set; }

        internal XName ServiceName { get; set; }

        internal TimeSpan TrackTimeout { get; set; }

        internal WorkflowUnhandledExceptionAction UnhandledExceptionAction
        {
            get
            {
                return this.unhandledExceptionAction;
            }
            set
            {
                this.unhandledExceptionAction = value;
            }
        }

        public WorkflowInstanceExtensionManager WorkflowExtensions
        {
            get
            {
                return this.workflowExtensions;
            }
        }

        private class CloseAsyncResult : AsyncResult
        {
            private static AsyncResult.AsyncCompletion handleDurableInstanceManagerEndClose = new AsyncResult.AsyncCompletion(WorkflowServiceHost.CloseAsyncResult.HandleDurableInstanceManagerEndClose);
            private static AsyncResult.AsyncCompletion handleEndHostClose = new AsyncResult.AsyncCompletion(WorkflowServiceHost.CloseAsyncResult.HandleEndHostClose);
            private WorkflowServiceHost host;
            private TimeoutHelper timeoutHelper;

            public CloseAsyncResult(WorkflowServiceHost host, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.host = host;
                if (this.CloseHost())
                {
                    base.Complete(true);
                }
            }

            private bool CloseDurableInstanceManager()
            {
                IAsyncResult result = this.host.durableInstanceManager.BeginClose(this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleDurableInstanceManagerEndClose), this);
                return base.SyncContinue(result);
            }

            private bool CloseHost()
            {
                IAsyncResult result = this.host.BeginHostClose(this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleEndHostClose), this);
                return base.SyncContinue(result);
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<WorkflowServiceHost.CloseAsyncResult>(result);
            }

            private static bool HandleDurableInstanceManagerEndClose(IAsyncResult result)
            {
                WorkflowServiceHost.CloseAsyncResult asyncState = (WorkflowServiceHost.CloseAsyncResult) result.AsyncState;
                asyncState.host.durableInstanceManager.EndClose(result);
                return true;
            }

            private static bool HandleEndHostClose(IAsyncResult result)
            {
                WorkflowServiceHost.CloseAsyncResult asyncState = (WorkflowServiceHost.CloseAsyncResult) result.AsyncState;
                asyncState.host.EndHostClose(result);
                return asyncState.CloseDurableInstanceManager();
            }
        }

        private class OpenAsyncResult : AsyncResult
        {
            private static AsyncResult.AsyncCompletion handleDurableInstanceManagerEndOpen = new AsyncResult.AsyncCompletion(WorkflowServiceHost.OpenAsyncResult.HandleDurableInstanceManagerEndOpen);
            private static AsyncResult.AsyncCompletion handleEndHostOpen = new AsyncResult.AsyncCompletion(WorkflowServiceHost.OpenAsyncResult.HandleEndHostOpen);
            private WorkflowServiceHost host;
            private TimeoutHelper timeoutHelper;

            public OpenAsyncResult(WorkflowServiceHost host, TimeSpan timeout, AsyncCallback callback, object state) : base(callback, state)
            {
                this.timeoutHelper = new TimeoutHelper(timeout);
                this.host = host;
                if (this.HostOpen())
                {
                    base.Complete(true);
                }
            }

            public static void End(IAsyncResult result)
            {
                AsyncResult.End<WorkflowServiceHost.OpenAsyncResult>(result);
            }

            private static bool HandleDurableInstanceManagerEndOpen(IAsyncResult result)
            {
                WorkflowServiceHost.OpenAsyncResult asyncState = (WorkflowServiceHost.OpenAsyncResult) result.AsyncState;
                asyncState.host.durableInstanceManager.EndOpen(result);
                return true;
            }

            private static bool HandleEndHostOpen(IAsyncResult result)
            {
                WorkflowServiceHost.OpenAsyncResult asyncState = (WorkflowServiceHost.OpenAsyncResult) result.AsyncState;
                asyncState.host.EndHostOpen(result);
                return asyncState.OpenDurableInstanceManager();
            }

            private bool HostOpen()
            {
                IAsyncResult result = this.host.BeginHostOpen(this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleEndHostOpen), this);
                return base.SyncContinue(result);
            }

            private bool OpenDurableInstanceManager()
            {
                IAsyncResult result = this.host.durableInstanceManager.BeginOpen(this.timeoutHelper.RemainingTime(), base.PrepareAsyncCompletion(handleDurableInstanceManagerEndOpen), this);
                return base.SyncContinue(result);
            }
        }

        private class WorkflowServiceHostExtensions : WorkflowInstanceExtensionManager
        {
            private bool hasChannelCache;

            public override void Add<T>(Func<T> extensionCreationFunction) where T: class
            {
                if (TypeHelper.AreTypesCompatible(typeof(T), typeof(SendMessageChannelCache)))
                {
                    this.hasChannelCache = true;
                }
                base.Add<T>(extensionCreationFunction);
            }

            public override void Add(object singletonExtension)
            {
                if (singletonExtension is SendMessageChannelCache)
                {
                    this.hasChannelCache = true;
                }
                base.Add(singletonExtension);
            }

            public void EnsureChannelCache()
            {
                if (!this.hasChannelCache)
                {
                    this.Add(new SendMessageChannelCache());
                    this.hasChannelCache = true;
                }
            }
        }
    }
}

