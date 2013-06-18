namespace System.ServiceModel.Description
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.MsmqIntegration;
    using System.ServiceModel.Security;
    using System.Xml;

    internal class DispatcherBuilder
    {
        private static void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection parameters)
        {
            foreach (IContractBehavior behavior in endpoint.Contract.Behaviors)
            {
                behavior.AddBindingParameters(endpoint.Contract, endpoint, parameters);
            }
            foreach (IEndpointBehavior behavior2 in endpoint.Behaviors)
            {
                behavior2.AddBindingParameters(endpoint, parameters);
            }
            foreach (OperationDescription description in endpoint.Contract.Operations)
            {
                foreach (IOperationBehavior behavior3 in description.Behaviors)
                {
                    behavior3.AddBindingParameters(description, parameters);
                }
            }
        }

        private static void AddMsmqIntegrationContractInformation(ServiceEndpoint endpoint)
        {
            MsmqIntegrationBinding binding = endpoint.Binding as MsmqIntegrationBinding;
            if (binding != null)
            {
                System.Type[] typeArray = ProcessDescriptionForMsmqIntegration(endpoint, binding.TargetSerializationTypes);
                binding.TargetSerializationTypes = typeArray;
            }
            else
            {
                CustomBinding binding2 = endpoint.Binding as CustomBinding;
                if (binding2 != null)
                {
                    MsmqIntegrationBindingElement element = binding2.Elements.Find<MsmqIntegrationBindingElement>();
                    if (element != null)
                    {
                        System.Type[] typeArray2 = ProcessDescriptionForMsmqIntegration(endpoint, element.TargetSerializationTypes);
                        element.TargetSerializationTypes = typeArray2;
                    }
                }
            }
        }

        private static void ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime clientRuntime)
        {
            ContractDescription contract = serviceEndpoint.Contract;
            for (int i = 0; i < contract.Behaviors.Count; i++)
            {
                contract.Behaviors[i].ApplyClientBehavior(contract, serviceEndpoint, clientRuntime);
            }
            BindingInformationEndpointBehavior.Instance.ApplyClientBehavior(serviceEndpoint, clientRuntime);
            TransactionContractInformationEndpointBehavior.Instance.ApplyClientBehavior(serviceEndpoint, clientRuntime);
            for (int j = 0; j < serviceEndpoint.Behaviors.Count; j++)
            {
                serviceEndpoint.Behaviors[j].ApplyClientBehavior(serviceEndpoint, clientRuntime);
            }
            BindOperations(contract, clientRuntime, null);
        }

        private static void BindOperations(ContractDescription contract, ClientRuntime proxy, DispatchRuntime dispatch)
        {
            if ((proxy == null) == (dispatch == null))
            {
                throw Fx.AssertAndThrowFatal("DispatcherBuilder.BindOperations: ((proxy == null) != (dispatch == null))");
            }
            MessageDirection direction = (proxy == null) ? MessageDirection.Input : MessageDirection.Output;
            for (int i = 0; i < contract.Operations.Count; i++)
            {
                OperationDescription operationDescription = contract.Operations[i];
                MessageDescription description2 = operationDescription.Messages[0];
                if (description2.Direction != direction)
                {
                    if (proxy == null)
                    {
                        proxy = dispatch.CallbackClientRuntime;
                    }
                    ClientOperation clientOperation = proxy.Operations[operationDescription.Name];
                    for (int j = 0; j < operationDescription.Behaviors.Count; j++)
                    {
                        operationDescription.Behaviors[j].ApplyClientBehavior(operationDescription, clientOperation);
                    }
                }
                else
                {
                    if (dispatch == null)
                    {
                        dispatch = proxy.CallbackDispatchRuntime;
                    }
                    DispatchOperation dispatchOperation = null;
                    if (dispatch.Operations.Contains(operationDescription.Name))
                    {
                        dispatchOperation = dispatch.Operations[operationDescription.Name];
                    }
                    if (((dispatchOperation == null) && (dispatch.UnhandledDispatchOperation != null)) && (dispatch.UnhandledDispatchOperation.Name == operationDescription.Name))
                    {
                        dispatchOperation = dispatch.UnhandledDispatchOperation;
                    }
                    if (dispatchOperation != null)
                    {
                        for (int k = 0; k < operationDescription.Behaviors.Count; k++)
                        {
                            operationDescription.Behaviors[k].ApplyDispatchBehavior(operationDescription, dispatchOperation);
                        }
                    }
                }
            }
        }

        private System.Type BuildChannelListener(StuffPerListenUriInfo stuff, ServiceHostBase serviceHost, Uri listenUri, ListenUriMode listenUriMode, bool supportContextSession, out IChannelListener result)
        {
            Uri uri;
            string str;
            Binding binding = stuff.Endpoints[0].Binding;
            CustomBinding binding2 = new CustomBinding(binding);
            BindingParameterCollection parameters = stuff.Parameters;
            this.GetBaseAndRelativeAddresses(serviceHost, listenUri, binding2.Scheme, out uri, out str);
            InternalDuplexBindingElement internalDuplexBindingElement = null;
            InternalDuplexBindingElement.AddDuplexListenerSupport(binding2, ref internalDuplexBindingElement);
            bool flag = true;
            bool flag2 = true;
            bool flag3 = true;
            bool flag4 = true;
            bool flag5 = true;
            bool flag6 = true;
            string name = null;
            string str3 = null;
            for (int i = 0; i < stuff.Endpoints.Count; i++)
            {
                ContractDescription contract = stuff.Endpoints[i].Contract;
                if (contract.SessionMode == SessionMode.Required)
                {
                    name = contract.Name;
                }
                if (contract.SessionMode == SessionMode.NotAllowed)
                {
                    str3 = contract.Name;
                }
                IList supportedChannelTypes = GetSupportedChannelTypes(contract);
                if (!supportedChannelTypes.Contains(typeof(IReplyChannel)))
                {
                    flag = false;
                }
                if (!supportedChannelTypes.Contains(typeof(IReplySessionChannel)))
                {
                    flag2 = false;
                }
                if (!supportedChannelTypes.Contains(typeof(IInputChannel)))
                {
                    flag3 = false;
                }
                if (!supportedChannelTypes.Contains(typeof(IInputSessionChannel)))
                {
                    flag4 = false;
                }
                if (!supportedChannelTypes.Contains(typeof(IDuplexChannel)))
                {
                    flag5 = false;
                }
                if (!supportedChannelTypes.Contains(typeof(IDuplexSessionChannel)))
                {
                    flag6 = false;
                }
            }
            if ((name != null) && (str3 != null))
            {
                Exception exception = new InvalidOperationException(System.ServiceModel.SR.GetString("SFxCannotRequireBothSessionAndDatagram3", new object[] { str3, name, binding2.Name }));
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
            }
            List<System.Type> requiredChannels = new List<System.Type>();
            if (flag3)
            {
                requiredChannels.Add(typeof(IInputChannel));
            }
            if (flag4)
            {
                requiredChannels.Add(typeof(IInputSessionChannel));
            }
            if (flag)
            {
                requiredChannels.Add(typeof(IReplyChannel));
            }
            if (flag2)
            {
                requiredChannels.Add(typeof(IReplySessionChannel));
            }
            if (flag5)
            {
                requiredChannels.Add(typeof(IDuplexChannel));
            }
            if (flag6)
            {
                requiredChannels.Add(typeof(IDuplexSessionChannel));
            }
            System.Type type = MaybeCreateListener(true, requiredChannels.ToArray(), binding2, parameters, uri, str, listenUriMode, serviceHost.ServiceThrottle, out result, supportContextSession && (name != null));
            if (result != null)
            {
                return type;
            }
            Dictionary<System.Type, byte> dictionary = new Dictionary<System.Type, byte>();
            if (binding2.CanBuildChannelListener<IInputChannel>(new object[0]))
            {
                dictionary.Add(typeof(IInputChannel), 0);
            }
            if (binding2.CanBuildChannelListener<IReplyChannel>(new object[0]))
            {
                dictionary.Add(typeof(IReplyChannel), 0);
            }
            if (binding2.CanBuildChannelListener<IDuplexChannel>(new object[0]))
            {
                dictionary.Add(typeof(IDuplexChannel), 0);
            }
            if (binding2.CanBuildChannelListener<IInputSessionChannel>(new object[0]))
            {
                dictionary.Add(typeof(IInputSessionChannel), 0);
            }
            if (binding2.CanBuildChannelListener<IReplySessionChannel>(new object[0]))
            {
                dictionary.Add(typeof(IReplySessionChannel), 0);
            }
            if (binding2.CanBuildChannelListener<IDuplexSessionChannel>(new object[0]))
            {
                dictionary.Add(typeof(IDuplexSessionChannel), 0);
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(ChannelRequirements.CantCreateListenerException(dictionary.Keys, requiredChannels, binding.Name));
        }

        private static EndpointDispatcher BuildDispatcher(ServiceHostBase service, System.ServiceModel.Description.ServiceDescription serviceDescription, ServiceEndpoint endpoint, ContractDescription contractDescription, EndpointFilterProvider provider)
        {
            if (service == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("service");
            }
            if (serviceDescription == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceDescription");
            }
            if (contractDescription == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("contractDescription");
            }
            EndpointDispatcher dispatcher = new EndpointDispatcher(endpoint.Address, contractDescription.Name, contractDescription.Namespace, endpoint.Id, endpoint.InternalIsSystemEndpoint(serviceDescription));
            DispatchRuntime dispatchRuntime = dispatcher.DispatchRuntime;
            if (contractDescription.CallbackContractType != null)
            {
                dispatchRuntime.CallbackClientRuntime.CallbackClientType = contractDescription.CallbackContractType;
                dispatchRuntime.CallbackClientRuntime.ContractClientType = contractDescription.ContractType;
            }
            for (int i = 0; i < contractDescription.Operations.Count; i++)
            {
                OperationDescription operation = contractDescription.Operations[i];
                if (!operation.IsServerInitiated())
                {
                    BuildDispatchOperation(operation, dispatchRuntime, provider);
                }
                else
                {
                    BuildProxyOperation(operation, dispatchRuntime.CallbackClientRuntime);
                }
            }
            int priority = 0;
            dispatcher.ContractFilter = provider.CreateFilter(out priority);
            dispatcher.FilterPriority = priority;
            return dispatcher;
        }

        private static void BuildDispatchOperation(OperationDescription operation, DispatchRuntime parent, EndpointFilterProvider provider)
        {
            string action = operation.Messages[0].Action;
            DispatchOperation item = null;
            if (operation.IsOneWay)
            {
                item = new DispatchOperation(parent, operation.Name, action);
            }
            else
            {
                string replyAction = operation.Messages[1].Action;
                item = new DispatchOperation(parent, operation.Name, action, replyAction);
            }
            item.HasNoDisposableParameters = operation.HasNoDisposableParameters;
            item.IsTerminating = operation.IsTerminating;
            for (int i = 0; i < operation.Faults.Count; i++)
            {
                FaultDescription description = operation.Faults[i];
                item.FaultContractInfos.Add(new FaultContractInfo(description.Action, description.DetailType, description.ElementName, description.Namespace, operation.KnownTypes));
            }
            item.IsInsideTransactedReceiveScope = operation.IsInsideTransactedReceiveScope;
            if ((provider != null) && operation.IsInitiating)
            {
                provider.InitiatingActions.Add(action);
            }
            if (action != "*")
            {
                parent.Operations.Add(item);
            }
            else
            {
                if (parent.HasMatchAllOperation)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxMultipleContractStarOperations0")));
                }
                parent.UnhandledDispatchOperation = item;
            }
        }

        internal static ClientRuntime BuildProxyBehavior(ServiceEndpoint serviceEndpoint, out BindingParameterCollection parameters)
        {
            parameters = new BindingParameterCollection();
            SecurityContractInformationEndpointBehavior.ClientInstance.AddBindingParameters(serviceEndpoint, parameters);
            AddBindingParameters(serviceEndpoint, parameters);
            ContractDescription contract = serviceEndpoint.Contract;
            ClientRuntime parent = new ClientRuntime(contract.Name, contract.Namespace) {
                ContractClientType = contract.ContractType
            };
            IdentityVerifier property = serviceEndpoint.Binding.GetProperty<IdentityVerifier>(parameters);
            if (property != null)
            {
                parent.IdentityVerifier = property;
            }
            for (int i = 0; i < contract.Operations.Count; i++)
            {
                OperationDescription operation = contract.Operations[i];
                if (!operation.IsServerInitiated())
                {
                    BuildProxyOperation(operation, parent);
                }
                else
                {
                    BuildDispatchOperation(operation, parent.CallbackDispatchRuntime, null);
                }
            }
            ApplyClientBehavior(serviceEndpoint, parent);
            return parent;
        }

        private static void BuildProxyOperation(OperationDescription operation, ClientRuntime parent)
        {
            ClientOperation operation2;
            if (operation.Messages.Count == 1)
            {
                operation2 = new ClientOperation(parent, operation.Name, operation.Messages[0].Action);
            }
            else
            {
                operation2 = new ClientOperation(parent, operation.Name, operation.Messages[0].Action, operation.Messages[1].Action);
            }
            operation2.SyncMethod = operation.SyncMethod;
            operation2.BeginMethod = operation.BeginMethod;
            operation2.EndMethod = operation.EndMethod;
            operation2.IsOneWay = operation.IsOneWay;
            operation2.IsTerminating = operation.IsTerminating;
            operation2.IsInitiating = operation.IsInitiating;
            for (int i = 0; i < operation.Faults.Count; i++)
            {
                FaultDescription description = operation.Faults[i];
                operation2.FaultContractInfos.Add(new FaultContractInfo(description.Action, description.DetailType, description.ElementName, description.Namespace, operation.KnownTypes));
            }
            parent.Operations.Add(operation2);
        }

        private Uri EnsureListenUri(ServiceHostBase serviceHost, ServiceEndpoint endpoint)
        {
            Uri listenUri = endpoint.ListenUri;
            if (listenUri == null)
            {
                listenUri = serviceHost.GetVia(endpoint.Binding.Scheme, ServiceHostBase.EmptyUri);
            }
            if (listenUri == null)
            {
                AspNetEnvironment.Current.ProcessNotMatchedEndpointAddress(listenUri, endpoint.Binding.Name);
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxEndpointNoMatchingScheme", new object[] { endpoint.Binding.Scheme, endpoint.Binding.Name, serviceHost.GetBaseAddressSchemes() })));
            }
            return listenUri;
        }

        private void EnsureRequiredRuntimeProperties(Dictionary<EndpointAddress, Collection<EndpointInfo>> endpointInfosPerEndpointAddress)
        {
            foreach (Collection<EndpointInfo> collection in endpointInfosPerEndpointAddress.Values)
            {
                for (int i = 0; i < collection.Count; i++)
                {
                    if (collection[i].EndpointDispatcher.DispatchRuntime.InstanceContextProvider == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxRequiredRuntimePropertyMissing", new object[] { "InstanceContextProvider" })));
                    }
                }
            }
        }

        private void EnsureThereAreApplicationEndpoints(System.ServiceModel.Description.ServiceDescription description)
        {
            foreach (ServiceEndpoint endpoint in description.Endpoints)
            {
                if (!endpoint.InternalIsSystemEndpoint(description))
                {
                    return;
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ServiceHasZeroAppEndpoints", new object[] { description.ConfigurationName })));
        }

        private void GetBaseAndRelativeAddresses(ServiceHostBase serviceHost, Uri listenUri, string scheme, out Uri listenUriBaseAddress, out string listenUriRelativeAddress)
        {
            listenUriBaseAddress = listenUri;
            listenUriRelativeAddress = string.Empty;
            if (serviceHost.InternalBaseAddresses.Contains(scheme))
            {
                Uri uri = serviceHost.InternalBaseAddresses[scheme];
                if (!uri.AbsoluteUri.EndsWith("/", StringComparison.Ordinal))
                {
                    uri = new Uri(uri.AbsoluteUri + "/");
                }
                string str = uri.ToString();
                string str2 = listenUri.ToString();
                if (str2.StartsWith(str, StringComparison.OrdinalIgnoreCase))
                {
                    listenUriBaseAddress = uri;
                    listenUriRelativeAddress = str2.Substring(str.Length);
                }
            }
        }

        internal static System.Type[] GetSupportedChannelTypes(ContractDescription contractDescription)
        {
            ChannelRequirements requirements;
            if (contractDescription == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("contractDescription"));
            }
            ChannelRequirements.ComputeContractRequirements(contractDescription, out requirements);
            System.Type[] typeArray = ChannelRequirements.ComputeRequiredChannels(ref requirements);
            for (int i = 0; i < typeArray.Length; i++)
            {
                if (typeArray[i] == typeof(IRequestChannel))
                {
                    typeArray[i] = typeof(IReplyChannel);
                }
                else if (typeArray[i] == typeof(IRequestSessionChannel))
                {
                    typeArray[i] = typeof(IReplySessionChannel);
                }
                else if (typeArray[i] == typeof(IOutputChannel))
                {
                    typeArray[i] = typeof(IInputChannel);
                }
                else if (typeArray[i] == typeof(IOutputSessionChannel))
                {
                    typeArray[i] = typeof(IInputSessionChannel);
                }
                else if (!(typeArray[i] == typeof(IDuplexChannel)) && !(typeArray[i] == typeof(IDuplexSessionChannel)))
                {
                    throw Fx.AssertAndThrowFatal("DispatcherBuilder.GetSupportedChannelTypes: Unexpected channel type");
                }
            }
            return typeArray;
        }

        private static bool HaveCommonInitiatingActions(EndpointFilterProvider x, EndpointFilterProvider y, out string commonAction)
        {
            commonAction = null;
            foreach (string str in x.InitiatingActions)
            {
                if (y.InitiatingActions.Contains(str))
                {
                    commonAction = str;
                    return true;
                }
            }
            return false;
        }

        public void InitializeServiceHost(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHost)
        {
            if ((serviceHost.ImplementedContracts != null) && (serviceHost.ImplementedContracts.Count > 0))
            {
                this.EnsureThereAreApplicationEndpoints(description);
            }
            this.ValidateDescription(description, serviceHost);
            AspNetEnvironment.Current.AddHostingBehavior(serviceHost, description);
            description.Behaviors.Find<ServiceBehaviorAttribute>();
            this.InitializeServicePerformanceCounters(serviceHost);
            Dictionary<ListenUriInfo, StuffPerListenUriInfo> dictionary = new Dictionary<ListenUriInfo, StuffPerListenUriInfo>();
            Dictionary<EndpointAddress, Collection<EndpointInfo>> endpointInfosPerEndpointAddress = new Dictionary<EndpointAddress, Collection<EndpointInfo>>();
            for (int i = 0; i < description.Endpoints.Count; i++)
            {
                bool flag = false;
                ServiceEndpoint endpoint = description.Endpoints[i];
                foreach (OperationDescription description2 in endpoint.Contract.Operations)
                {
                    if (description2.Behaviors.Find<ReceiveContextEnabledAttribute>() != null)
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    IReceiveContextSettings property = endpoint.Binding.GetProperty<IReceiveContextSettings>(new BindingParameterCollection());
                    if (property == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxReceiveContextSettingsPropertyMissing", new object[] { endpoint.Contract.Name, typeof(ReceiveContextEnabledAttribute).Name, endpoint.Address.Uri.AbsoluteUri, typeof(IReceiveContextSettings).Name })));
                    }
                    property.Enabled = true;
                }
                ListenUriInfo key = new ListenUriInfo(this.EnsureListenUri(serviceHost, endpoint), endpoint.ListenUriMode);
                if (!dictionary.ContainsKey(key))
                {
                    dictionary.Add(key, new StuffPerListenUriInfo());
                }
                dictionary[key].Endpoints.Add(endpoint);
            }
            foreach (KeyValuePair<ListenUriInfo, StuffPerListenUriInfo> pair in dictionary)
            {
                ThreadSafeMessageFilterTable<EndpointAddress> table;
                IChannelListener listener;
                Uri listenUri = pair.Key.ListenUri;
                ListenUriMode listenUriMode = pair.Key.ListenUriMode;
                BindingParameterCollection bindingParameters = pair.Value.Parameters;
                Binding timeouts = pair.Value.Endpoints[0].Binding;
                EndpointIdentity objB = pair.Value.Endpoints[0].Address.Identity;
                table = new ThreadSafeMessageFilterTable<EndpointAddress> {
                    table
                };
                bool supportContextSession = false;
                foreach (IServiceBehavior behavior in description.Behaviors)
                {
                    if (behavior is IContextSessionProvider)
                    {
                        supportContextSession = true;
                    }
                    behavior.AddBindingParameters(description, serviceHost, pair.Value.Endpoints, bindingParameters);
                }
                for (int k = 0; k < pair.Value.Endpoints.Count; k++)
                {
                    ServiceEndpoint endpoint2 = pair.Value.Endpoints[k];
                    string absoluteUri = listenUri.AbsoluteUri;
                    if (endpoint2.Binding != timeouts)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ABindingInstanceHasAlreadyBeenAssociatedTo1", new object[] { absoluteUri })));
                    }
                    if (!object.Equals(endpoint2.Address.Identity, objB))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxWhenMultipleEndpointsShareAListenUriTheyMustHaveSameIdentity", new object[] { absoluteUri })));
                    }
                    AddMsmqIntegrationContractInformation(endpoint2);
                    SecurityContractInformationEndpointBehavior.ServerInstance.AddBindingParameters(endpoint2, bindingParameters);
                    AddBindingParameters(endpoint2, bindingParameters);
                }
                this.BuildChannelListener(pair.Value, serviceHost, listenUri, listenUriMode, supportContextSession, out listener);
                XmlQualifiedName name = new XmlQualifiedName(timeouts.Name, timeouts.Namespace);
                ChannelDispatcher item = new ChannelDispatcher(listener, name.ToString(), timeouts);
                item.SetEndpointAddressTable(table);
                pair.Value.ChannelDispatcher = item;
                bool flag3 = false;
                int num3 = 0x7fffffff;
                for (int m = 0; m < pair.Value.Endpoints.Count; m++)
                {
                    ServiceEndpoint endpoint3 = pair.Value.Endpoints[m];
                    string text1 = listenUri.AbsoluteUri;
                    EndpointFilterProvider provider = new EndpointFilterProvider(new string[0]);
                    EndpointDispatcher endpointDispatcher = BuildDispatcher(serviceHost, description, endpoint3, endpoint3.Contract, provider);
                    for (int n = 0; n < endpoint3.Contract.Operations.Count; n++)
                    {
                        OperationDescription description3 = endpoint3.Contract.Operations[n];
                        OperationBehaviorAttribute attribute = description3.Behaviors.Find<OperationBehaviorAttribute>();
                        if ((attribute != null) && attribute.TransactionScopeRequired)
                        {
                            flag3 = true;
                            break;
                        }
                    }
                    if (!endpointInfosPerEndpointAddress.ContainsKey(endpoint3.Address))
                    {
                        endpointInfosPerEndpointAddress.Add(endpoint3.Address, new Collection<EndpointInfo>());
                    }
                    endpointInfosPerEndpointAddress[endpoint3.Address].Add(new EndpointInfo(endpoint3, endpointDispatcher, provider));
                    item.Endpoints.Add(endpointDispatcher);
                    TransactedBatchingBehavior behavior2 = endpoint3.Behaviors.Find<TransactedBatchingBehavior>();
                    if (behavior2 == null)
                    {
                        num3 = 0;
                    }
                    else
                    {
                        if (!flag3)
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("MsmqBatchRequiresTransactionScope")));
                        }
                        num3 = Math.Min(num3, behavior2.MaxBatchSize);
                    }
                    if (PerformanceCounters.PerformanceCountersEnabled || PerformanceCounters.MinimalPerformanceCountersEnabled)
                    {
                        PerformanceCounters.AddPerformanceCountersForEndpoint(serviceHost, endpoint3.Contract, endpointDispatcher);
                    }
                }
                if (flag3)
                {
                    foreach (BindingElement element in timeouts.CreateBindingElements())
                    {
                        ITransactedBindingElement element2 = element as ITransactedBindingElement;
                        if ((element2 != null) && element2.TransactedReceiveEnabled)
                        {
                            item.IsTransactedReceive = true;
                            item.MaxTransactedBatchSize = num3;
                            break;
                        }
                    }
                }
                IReceiveContextSettings settings2 = timeouts.GetProperty<IReceiveContextSettings>(new BindingParameterCollection());
                if (settings2 != null)
                {
                    item.ReceiveContextEnabled = settings2.Enabled;
                }
                serviceHost.ChannelDispatchers.Add(item);
            }
            for (int j = 0; j < description.Behaviors.Count; j++)
            {
                description.Behaviors[j].ApplyDispatchBehavior(description, serviceHost);
            }
            foreach (KeyValuePair<ListenUriInfo, StuffPerListenUriInfo> pair2 in dictionary)
            {
                for (int num7 = 0; num7 < pair2.Value.Endpoints.Count; num7++)
                {
                    ServiceEndpoint endpoint4 = pair2.Value.Endpoints[num7];
                    Collection<EndpointInfo> collection = endpointInfosPerEndpointAddress[endpoint4.Address];
                    EndpointInfo info2 = null;
                    foreach (EndpointInfo info3 in collection)
                    {
                        if (info3.Endpoint == endpoint4)
                        {
                            info2 = info3;
                            break;
                        }
                    }
                    EndpointDispatcher dispatcher3 = info2.EndpointDispatcher;
                    for (int num8 = 0; num8 < endpoint4.Contract.Behaviors.Count; num8++)
                    {
                        endpoint4.Contract.Behaviors[num8].ApplyDispatchBehavior(endpoint4.Contract, endpoint4, dispatcher3.DispatchRuntime);
                    }
                    BindingInformationEndpointBehavior.Instance.ApplyDispatchBehavior(endpoint4, dispatcher3);
                    TransactionContractInformationEndpointBehavior.Instance.ApplyDispatchBehavior(endpoint4, dispatcher3);
                    for (int num9 = 0; num9 < endpoint4.Behaviors.Count; num9++)
                    {
                        endpoint4.Behaviors[num9].ApplyDispatchBehavior(endpoint4, dispatcher3);
                    }
                    BindOperations(endpoint4.Contract, null, dispatcher3.DispatchRuntime);
                }
            }
            this.EnsureRequiredRuntimeProperties(endpointInfosPerEndpointAddress);
            foreach (Collection<EndpointInfo> collection2 in endpointInfosPerEndpointAddress.Values)
            {
                if (collection2.Count > 1)
                {
                    for (int num10 = 0; num10 < collection2.Count; num10++)
                    {
                        for (int num11 = num10 + 1; num11 < collection2.Count; num11++)
                        {
                            if (collection2[num10].EndpointDispatcher.ChannelDispatcher == collection2[num11].EndpointDispatcher.ChannelDispatcher)
                            {
                                string str2;
                                EndpointFilterProvider filterProvider = collection2[num10].FilterProvider;
                                EndpointFilterProvider y = collection2[num11].FilterProvider;
                                if (((filterProvider != null) && (y != null)) && HaveCommonInitiatingActions(filterProvider, y, out str2))
                                {
                                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxDuplicateInitiatingActionAtSameVia", new object[] { collection2[num10].Endpoint.ListenUri, str2 })));
                                }
                            }
                        }
                    }
                }
            }
        }

        private void InitializeServicePerformanceCounters(ServiceHostBase serviceHost)
        {
            if (PerformanceCounters.PerformanceCountersEnabled)
            {
                ServicePerformanceCountersBase base2 = PerformanceCountersFactory.CreateServiceCounters(serviceHost);
                if ((base2 != null) && base2.Initialized)
                {
                    serviceHost.Counters = base2;
                }
            }
            else if (PerformanceCounters.MinimalPerformanceCountersEnabled)
            {
                DefaultPerformanceCounters counters = new DefaultPerformanceCounters(serviceHost);
                if (counters.Initialized)
                {
                    serviceHost.DefaultCounters = counters;
                }
            }
        }

        internal static System.Type MaybeCreateListener(bool actuallyCreate, System.Type[] supportedChannels, Binding binding, BindingParameterCollection parameters, Uri listenUriBaseAddress, string listenUriRelativeAddress, ListenUriMode listenUriMode, ServiceThrottle throttle, out IChannelListener result)
        {
            return MaybeCreateListener(actuallyCreate, supportedChannels, binding, parameters, listenUriBaseAddress, listenUriRelativeAddress, listenUriMode, throttle, out result, false);
        }

        private static System.Type MaybeCreateListener(bool actuallyCreate, System.Type[] supportedChannels, Binding binding, BindingParameterCollection parameters, Uri listenUriBaseAddress, string listenUriRelativeAddress, ListenUriMode listenUriMode, ServiceThrottle throttle, out IChannelListener result, bool supportContextSession)
        {
            result = null;
            for (int i = 0; i < supportedChannels.Length; i++)
            {
                System.Type type = supportedChannels[i];
                if ((type == typeof(IInputChannel)) && binding.CanBuildChannelListener<IInputChannel>(parameters))
                {
                    if (actuallyCreate)
                    {
                        result = binding.BuildChannelListener<IInputChannel>(listenUriBaseAddress, listenUriRelativeAddress, listenUriMode, parameters);
                    }
                    return typeof(IInputChannel);
                }
                if ((type == typeof(IReplyChannel)) && binding.CanBuildChannelListener<IReplyChannel>(parameters))
                {
                    if (actuallyCreate)
                    {
                        result = binding.BuildChannelListener<IReplyChannel>(listenUriBaseAddress, listenUriRelativeAddress, listenUriMode, parameters);
                    }
                    return typeof(IReplyChannel);
                }
                if ((type == typeof(IDuplexChannel)) && binding.CanBuildChannelListener<IDuplexChannel>(parameters))
                {
                    if (actuallyCreate)
                    {
                        result = binding.BuildChannelListener<IDuplexChannel>(listenUriBaseAddress, listenUriRelativeAddress, listenUriMode, parameters);
                    }
                    return typeof(IDuplexChannel);
                }
                if ((type == typeof(IInputSessionChannel)) && binding.CanBuildChannelListener<IInputSessionChannel>(parameters))
                {
                    if (actuallyCreate)
                    {
                        result = binding.BuildChannelListener<IInputSessionChannel>(listenUriBaseAddress, listenUriRelativeAddress, listenUriMode, parameters);
                    }
                    return typeof(IInputSessionChannel);
                }
                if ((type == typeof(IReplySessionChannel)) && binding.CanBuildChannelListener<IReplySessionChannel>(parameters))
                {
                    if (actuallyCreate)
                    {
                        result = binding.BuildChannelListener<IReplySessionChannel>(listenUriBaseAddress, listenUriRelativeAddress, listenUriMode, parameters);
                    }
                    return typeof(IReplySessionChannel);
                }
                if ((type == typeof(IDuplexSessionChannel)) && binding.CanBuildChannelListener<IDuplexSessionChannel>(parameters))
                {
                    if (actuallyCreate)
                    {
                        result = binding.BuildChannelListener<IDuplexSessionChannel>(listenUriBaseAddress, listenUriRelativeAddress, listenUriMode, parameters);
                    }
                    return typeof(IDuplexSessionChannel);
                }
            }
            for (int j = 0; j < supportedChannels.Length; j++)
            {
                System.Type type2 = supportedChannels[j];
                if ((type2 == typeof(IInputChannel)) && binding.CanBuildChannelListener<IInputSessionChannel>(parameters))
                {
                    if (actuallyCreate)
                    {
                        IChannelListener<IInputSessionChannel> inner = binding.BuildChannelListener<IInputSessionChannel>(listenUriBaseAddress, listenUriRelativeAddress, listenUriMode, parameters);
                        result = DatagramAdapter.GetInputListener(inner, throttle, binding);
                    }
                    return typeof(IInputSessionChannel);
                }
                if ((type2 == typeof(IReplyChannel)) && binding.CanBuildChannelListener<IReplySessionChannel>(parameters))
                {
                    if (actuallyCreate)
                    {
                        IChannelListener<IReplySessionChannel> listener2 = binding.BuildChannelListener<IReplySessionChannel>(listenUriBaseAddress, listenUriRelativeAddress, listenUriMode, parameters);
                        result = DatagramAdapter.GetReplyListener(listener2, throttle, binding);
                    }
                    return typeof(IReplySessionChannel);
                }
                if ((supportContextSession && (type2 == typeof(IReplySessionChannel))) && (binding.CanBuildChannelListener<IReplyChannel>(parameters) && (binding.GetProperty<IContextSessionProvider>(parameters) != null)))
                {
                    if (actuallyCreate)
                    {
                        result = binding.BuildChannelListener<IReplyChannel>(listenUriBaseAddress, listenUriRelativeAddress, listenUriMode, parameters);
                    }
                    return typeof(IReplyChannel);
                }
            }
            return null;
        }

        private static System.Type[] ProcessDescriptionForMsmqIntegration(ServiceEndpoint endpoint, System.Type[] existingSerializationTypes)
        {
            List<System.Type> list;
            if (existingSerializationTypes == null)
            {
                list = new List<System.Type>();
            }
            else
            {
                list = new List<System.Type>(existingSerializationTypes);
            }
            foreach (OperationDescription description in endpoint.Contract.Operations)
            {
                foreach (System.Type type in description.KnownTypes)
                {
                    if (!list.Contains(type))
                    {
                        list.Add(type);
                    }
                }
                foreach (MessageDescription description2 in description.Messages)
                {
                    description2.Body.WrapperName = (string) (description2.Body.WrapperNamespace = null);
                }
            }
            return list.ToArray();
        }

        private void ValidateDescription(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHost)
        {
            description.EnsureInvariants();
            PartialTrustValidationBehavior.Instance.Validate(description, serviceHost);
            ((IServiceBehavior) PeerValidationBehavior.Instance).Validate(description, serviceHost);
            ((IServiceBehavior) TransactionValidationBehavior.Instance).Validate(description, serviceHost);
            ((IServiceBehavior) MsmqIntegrationValidationBehavior.Instance).Validate(description, serviceHost);
            ((IServiceBehavior) SecurityValidationBehavior.Instance).Validate(description, serviceHost);
            new UniqueContractNameValidationBehavior().Validate(description, serviceHost);
            for (int i = 0; i < description.Behaviors.Count; i++)
            {
                description.Behaviors[i].Validate(description, serviceHost);
            }
            for (int j = 0; j < description.Endpoints.Count; j++)
            {
                ServiceEndpoint endpoint = description.Endpoints[j];
                ContractDescription contract = endpoint.Contract;
                bool flag = false;
                for (int k = 0; k < j; k++)
                {
                    if (description.Endpoints[k].Contract == contract)
                    {
                        flag = true;
                        break;
                    }
                }
                endpoint.ValidateForService(!flag);
            }
        }

        private class BindingInformationEndpointBehavior : IEndpointBehavior
        {
            private static DispatcherBuilder.BindingInformationEndpointBehavior instance;

            public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection parameters)
            {
            }

            public void ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
            {
                behavior.ManualAddressing = this.IsManualAddressing(serviceEndpoint.Binding);
                behavior.EnableFaults = !this.IsMulticast(serviceEndpoint.Binding);
                if (serviceEndpoint.Contract.IsDuplex())
                {
                    behavior.CallbackDispatchRuntime.ChannelDispatcher.MessageVersion = serviceEndpoint.Binding.MessageVersion;
                }
            }

            public void ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
            {
                IBindingRuntimePreferences binding = serviceEndpoint.Binding as IBindingRuntimePreferences;
                if (binding != null)
                {
                    endpointDispatcher.ChannelDispatcher.ReceiveSynchronously = binding.ReceiveSynchronously;
                }
                endpointDispatcher.ChannelDispatcher.ManualAddressing = this.IsManualAddressing(serviceEndpoint.Binding);
                endpointDispatcher.ChannelDispatcher.EnableFaults = !this.IsMulticast(serviceEndpoint.Binding);
                endpointDispatcher.ChannelDispatcher.MessageVersion = serviceEndpoint.Binding.MessageVersion;
            }

            private bool IsManualAddressing(Binding binding)
            {
                TransportBindingElement element = binding.CreateBindingElements().Find<TransportBindingElement>();
                if (element == null)
                {
                    Exception exception = new InvalidOperationException(System.ServiceModel.SR.GetString("SFxBindingMustContainTransport2", new object[] { binding.Name, binding.Namespace }));
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(exception);
                }
                return element.ManualAddressing;
            }

            private bool IsMulticast(Binding binding)
            {
                IBindingMulticastCapabilities property = binding.GetProperty<IBindingMulticastCapabilities>(new BindingParameterCollection());
                return ((property != null) && property.IsMulticast);
            }

            public void Validate(ServiceEndpoint serviceEndpoint)
            {
            }

            public static DispatcherBuilder.BindingInformationEndpointBehavior Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new DispatcherBuilder.BindingInformationEndpointBehavior();
                    }
                    return instance;
                }
            }
        }

        private class EndpointInfo
        {
            private ServiceEndpoint endpoint;
            private System.ServiceModel.Dispatcher.EndpointDispatcher endpointDispatcher;
            private EndpointFilterProvider provider;

            public EndpointInfo(ServiceEndpoint endpoint, System.ServiceModel.Dispatcher.EndpointDispatcher endpointDispatcher, EndpointFilterProvider provider)
            {
                this.endpoint = endpoint;
                this.endpointDispatcher = endpointDispatcher;
                this.provider = provider;
            }

            public ServiceEndpoint Endpoint
            {
                get
                {
                    return this.endpoint;
                }
            }

            public System.ServiceModel.Dispatcher.EndpointDispatcher EndpointDispatcher
            {
                get
                {
                    return this.endpointDispatcher;
                }
            }

            public EndpointFilterProvider FilterProvider
            {
                get
                {
                    return this.provider;
                }
            }
        }

        private class ListenUriInfo
        {
            private Uri listenUri;
            private System.ServiceModel.Description.ListenUriMode listenUriMode;

            public ListenUriInfo(Uri listenUri, System.ServiceModel.Description.ListenUriMode listenUriMode)
            {
                this.listenUri = listenUri;
                this.listenUriMode = listenUriMode;
            }

            public override bool Equals(object other)
            {
                return this.Equals(other as DispatcherBuilder.ListenUriInfo);
            }

            public bool Equals(DispatcherBuilder.ListenUriInfo other)
            {
                if (other == null)
                {
                    return false;
                }
                return (object.ReferenceEquals(this, other) || ((this.listenUriMode == other.listenUriMode) && EndpointAddress.UriEquals(this.listenUri, other.listenUri, true, true)));
            }

            public override int GetHashCode()
            {
                return EndpointAddress.UriGetHashCode(this.listenUri, true);
            }

            public Uri ListenUri
            {
                get
                {
                    return this.listenUri;
                }
            }

            public System.ServiceModel.Description.ListenUriMode ListenUriMode
            {
                get
                {
                    return this.listenUriMode;
                }
            }
        }

        private class SecurityContractInformationEndpointBehavior : IEndpointBehavior
        {
            private static DispatcherBuilder.SecurityContractInformationEndpointBehavior clientInstance;
            private bool isForClient;
            private static DispatcherBuilder.SecurityContractInformationEndpointBehavior serverInstance;

            private SecurityContractInformationEndpointBehavior(bool isForClient)
            {
                this.isForClient = isForClient;
            }

            public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection parameters)
            {
                ISecurityCapabilities bindingElement = null;
                BindingElementCollection elements = endpoint.Binding.CreateBindingElements();
                for (int i = 0; i < elements.Count; i++)
                {
                    if (!(elements[i] is ITransportTokenAssertionProvider))
                    {
                        ISecurityCapabilities individualProperty = elements[i].GetIndividualProperty<ISecurityCapabilities>();
                        if (individualProperty != null)
                        {
                            bindingElement = individualProperty;
                            break;
                        }
                    }
                }
                if (bindingElement != null)
                {
                    ChannelProtectionRequirements item = parameters.Find<ChannelProtectionRequirements>();
                    if (item == null)
                    {
                        item = new ChannelProtectionRequirements();
                        parameters.Add(item);
                    }
                    MessageEncodingBindingElement element = elements.Find<MessageEncodingBindingElement>();
                    if ((element != null) && (element.MessageVersion.Addressing == AddressingVersion.None))
                    {
                        item.Add(ChannelProtectionRequirements.CreateFromContractAndUnionResponseProtectionRequirements(endpoint.Contract, bindingElement, this.isForClient));
                    }
                    else
                    {
                        item.Add(ChannelProtectionRequirements.CreateFromContract(endpoint.Contract, bindingElement, this.isForClient));
                    }
                }
            }

            public void ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
            {
            }

            public void ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
            {
            }

            public void Validate(ServiceEndpoint serviceEndpoint)
            {
            }

            public static DispatcherBuilder.SecurityContractInformationEndpointBehavior ClientInstance
            {
                get
                {
                    if (clientInstance == null)
                    {
                        clientInstance = new DispatcherBuilder.SecurityContractInformationEndpointBehavior(true);
                    }
                    return clientInstance;
                }
            }

            public static DispatcherBuilder.SecurityContractInformationEndpointBehavior ServerInstance
            {
                get
                {
                    if (serverInstance == null)
                    {
                        serverInstance = new DispatcherBuilder.SecurityContractInformationEndpointBehavior(false);
                    }
                    return serverInstance;
                }
            }
        }

        private class StuffPerListenUriInfo
        {
            public System.ServiceModel.Dispatcher.ChannelDispatcher ChannelDispatcher;
            public Collection<ServiceEndpoint> Endpoints = new Collection<ServiceEndpoint>();
            public BindingParameterCollection Parameters = new BindingParameterCollection();
        }

        private class TransactionContractInformationEndpointBehavior : IEndpointBehavior
        {
            private static DispatcherBuilder.TransactionContractInformationEndpointBehavior instance;

            public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection parameters)
            {
            }

            public void ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
            {
                behavior.AddTransactionFlowProperties = UsesTransactionFlowProperties(serviceEndpoint.Binding.CreateBindingElements(), serviceEndpoint.Contract);
            }

            public void ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
            {
                endpointDispatcher.DispatchRuntime.IgnoreTransactionMessageProperty = !UsesTransactionFlowProperties(serviceEndpoint.Binding.CreateBindingElements(), serviceEndpoint.Contract);
            }

            private static bool UsesTransactionFlowProperties(BindingElementCollection bindingElements, ContractDescription contract)
            {
                TransactionFlowBindingElement element = new BindingElementCollection(bindingElements).Find<TransactionFlowBindingElement>();
                if (element == null)
                {
                    return false;
                }
                return element.IsFlowEnabled(contract);
            }

            public void Validate(ServiceEndpoint serviceEndpoint)
            {
            }

            public static DispatcherBuilder.TransactionContractInformationEndpointBehavior Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new DispatcherBuilder.TransactionContractInformationEndpointBehavior();
                    }
                    return instance;
                }
            }
        }
    }
}

