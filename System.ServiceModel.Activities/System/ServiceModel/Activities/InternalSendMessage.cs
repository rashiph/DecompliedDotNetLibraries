namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.Activities.Statements;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.Collections;
    using System.Runtime.CompilerServices;
    using System.Runtime.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Activities.Description;
    using System.ServiceModel.Activities.Dispatcher;
    using System.ServiceModel.Activities.Tracking;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.Transactions;
    using System.Xml.Linq;

    internal class InternalSendMessage : NativeActivity
    {
        private ContractDescription cachedContract;
        private AddressHeaderCollection cachedEndpointHeaderCollection;
        private FactoryCacheKey cachedFactoryCacheKey;
        private ServiceEndpoint cachedServiceEndpoint;
        private WaitOnChannelCorrelation channelCorrelationCompletionWaiter;
        private bool configVerified;
        private Collection<CorrelationInitializer> correlationInitializers;
        private ICollection<System.ServiceModel.CorrelationQuery> correlationQueries;
        private bool isConfigSettingsSecure;
        private KeyValuePair<ObjectCacheItem<ChannelFactoryReference>, SendMessageChannelCache> lastUsedFactoryCacheItem;
        private MessageVersion messageVersion;
        private Variable<NoPersistHandle> noPersistHandle;
        private FaultCallback onSendFailure;
        private OpenChannelAndSendMessage openChannelAndSendMessage;
        private OpenChannelFactory openChannelFactory;
        private Activity persist;
        private Collection<System.ServiceModel.CorrelationQuery> replyCorrelationQueries;
        private static string runtimeTransactionHandlePropertyName = typeof(RuntimeTransactionHandle).FullName;
        private Variable<VolatileSendMessageInstance> sendMessageInstance;

        public InternalSendMessage()
        {
            this.TokenImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Identification;
            this.sendMessageInstance = new Variable<VolatileSendMessageInstance>();
            WaitOnChannelCorrelation correlation = new WaitOnChannelCorrelation {
                Instance = this.sendMessageInstance
            };
            this.channelCorrelationCompletionWaiter = correlation;
            this.noPersistHandle = new Variable<NoPersistHandle>();
            OpenChannelFactory factory = new OpenChannelFactory {
                Instance = this.sendMessageInstance
            };
            this.openChannelFactory = factory;
            OpenChannelAndSendMessage message = new OpenChannelAndSendMessage {
                Instance = this.sendMessageInstance,
                InternalSendMessage = this
            };
            this.openChannelAndSendMessage = message;
        }

        protected override void Abort(NativeActivityAbortContext context)
        {
            VolatileSendMessageInstance instance = this.sendMessageInstance.Get(context);
            if (instance != null)
            {
                this.CleanupResources(instance.Instance);
            }
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (this.ShouldPersistBeforeSend)
            {
                if (this.persist == null)
                {
                    this.persist = new Persist();
                }
                metadata.AddImplementationChild(this.persist);
            }
            RuntimeArgument argument = new RuntimeArgument("EndpointAddress", System.ServiceModel.Activities.Constants.UriType, ArgumentDirection.In);
            metadata.Bind(this.EndpointAddress, argument);
            metadata.AddArgument(argument);
            RuntimeArgument argument2 = new RuntimeArgument("CorrelatesWith", System.ServiceModel.Activities.Constants.CorrelationHandleType, ArgumentDirection.In);
            metadata.Bind(this.CorrelatesWith, argument2);
            metadata.AddArgument(argument2);
            if (this.correlationInitializers != null)
            {
                int num = 0;
                foreach (CorrelationInitializer initializer in this.correlationInitializers)
                {
                    if (initializer.CorrelationHandle != null)
                    {
                        RuntimeArgument argument3 = new RuntimeArgument("Parameter" + num, initializer.CorrelationHandle.ArgumentType, initializer.CorrelationHandle.Direction, true);
                        metadata.Bind(initializer.CorrelationHandle, argument3);
                        metadata.AddArgument(argument3);
                        num++;
                    }
                }
            }
            RuntimeArgument argument4 = new RuntimeArgument("RequestMessage", System.ServiceModel.Activities.Constants.MessageType, ArgumentDirection.In);
            metadata.Bind(this.Message, argument4);
            metadata.AddArgument(argument4);
            if (this.MessageOut != null)
            {
                RuntimeArgument argument5 = new RuntimeArgument("MessageReference", System.ServiceModel.Activities.Constants.MessageType, ArgumentDirection.Out);
                metadata.Bind(this.MessageOut, argument5);
                metadata.AddArgument(argument5);
            }
            metadata.AddImplementationVariable(this.sendMessageInstance);
            metadata.AddImplementationVariable(this.noPersistHandle);
            metadata.AddImplementationChild(this.channelCorrelationCompletionWaiter);
            metadata.AddImplementationChild(this.openChannelFactory);
            metadata.AddImplementationChild(this.openChannelAndSendMessage);
            metadata.AddDefaultExtensionProvider<SendMessageChannelCache>(SendMessageChannelCache.DefaultExtensionProvider);
        }

        protected override void Cancel(NativeActivityContext context)
        {
        }

        private void CleanupResources(SendMessageInstance instance)
        {
            if (instance != null)
            {
                instance.Dispose();
            }
        }

        private void CompleteCorrelationCallback(NativeActivityContext context, Bookmark bookmark, object value)
        {
            SendMessageInstance sendMessageInstance = this.GetSendMessageInstance(context);
            if (sendMessageInstance.CorrelationSynchronizer.NotifyWorkflowCorrelationProcessingComplete())
            {
                this.FinalizeSendMessageCore(sendMessageInstance);
            }
        }

        private System.ServiceModel.EndpointAddress CreateEndpointAddress(NativeActivityContext context)
        {
            ServiceEndpoint cachedServiceEndpoint = this.GetCachedServiceEndpoint();
            Uri uri = (this.EndpointAddress != null) ? this.EndpointAddress.Get(context) : null;
            if ((cachedServiceEndpoint != null) && (cachedServiceEndpoint.Address != null))
            {
                if (uri != null)
                {
                    EndpointAddressBuilder builder = new EndpointAddressBuilder(cachedServiceEndpoint.Address) {
                        Uri = uri
                    };
                    return builder.ToEndpointAddress();
                }
                return cachedServiceEndpoint.Address;
            }
            if (this.Endpoint == null)
            {
                return null;
            }
            if (uri != null)
            {
                return new System.ServiceModel.EndpointAddress(uri, this.Endpoint.Identity, this.GetCachedEndpointHeaders());
            }
            return this.Endpoint.GetAddress();
        }

        private System.ServiceModel.EndpointAddress CreateEndpointAddressFromCallback(System.ServiceModel.EndpointAddress CallbackAddress)
        {
            EndpointIdentity identity = null;
            AddressHeaderCollection cachedEndpointHeaders = null;
            if (this.Endpoint != null)
            {
                identity = this.Endpoint.Identity;
                cachedEndpointHeaders = this.GetCachedEndpointHeaders();
            }
            else
            {
                ServiceEndpoint cachedServiceEndpoint = this.GetCachedServiceEndpoint();
                if (cachedServiceEndpoint.Address != null)
                {
                    identity = cachedServiceEndpoint.Address.Identity;
                    cachedEndpointHeaders = cachedServiceEndpoint.Address.Headers;
                }
            }
            if ((identity != null) || (cachedEndpointHeaders != null))
            {
                return new System.ServiceModel.EndpointAddress(CallbackAddress.Uri, identity, cachedEndpointHeaders);
            }
            return CallbackAddress;
        }

        private ServiceEndpoint CreateServiceEndpoint()
        {
            ContractDescription contract = null;
            bool flag = false;
            if (this.cachedContract == null)
            {
                contract = this.GetContractDescription();
                flag = true;
            }
            else
            {
                contract = this.cachedContract;
            }
            ServiceEndpoint serviceEndpoint = new ServiceEndpoint(contract);
            if (this.Endpoint != null)
            {
                serviceEndpoint.Binding = this.Endpoint.Binding;
                if (this.Endpoint.AddressUri != null)
                {
                    serviceEndpoint.Address = new System.ServiceModel.EndpointAddress(this.Endpoint.AddressUri, this.Endpoint.Identity, this.GetCachedEndpointHeaders());
                }
            }
            else
            {
                if (this.ServiceContractName != null)
                {
                    serviceEndpoint.Contract.ConfigurationName = this.ServiceContractName.LocalName;
                }
                this.InitializeEndpoint(ref serviceEndpoint, this.EndpointConfigurationName ?? string.Empty);
            }
            if (flag)
            {
                this.EnsureTransactionFlowOnContract(ref serviceEndpoint);
                this.cachedContract = serviceEndpoint.Contract;
            }
            this.EnsureCorrelationQueryBehavior(serviceEndpoint);
            return serviceEndpoint;
        }

        private static void EnsureCorrelationBehaviorScopeName(ActivityContext context, CorrelationQueryBehavior correlationBehavior)
        {
            if (correlationBehavior.ScopeName == null)
            {
                CorrelationExtension extension = context.GetExtension<CorrelationExtension>();
                if (extension != null)
                {
                    correlationBehavior.ScopeName = extension.ScopeName;
                }
            }
        }

        private void EnsureCorrelationQueryBehavior(ServiceEndpoint serviceEndpoint)
        {
            CorrelationQueryBehavior item = serviceEndpoint.Behaviors.Find<CorrelationQueryBehavior>();
            if ((item == null) && ((CorrelationQueryBehavior.BindingHasDefaultQueries(serviceEndpoint.Binding) || (this.CorrelationQuery != null)) || (this.ReplyCorrelationQueries.Count > 0)))
            {
                item = new CorrelationQueryBehavior(new Collection<System.ServiceModel.CorrelationQuery>());
                serviceEndpoint.Behaviors.Add(item);
            }
            if (item != null)
            {
                if ((this.CorrelationQuery != null) && !item.CorrelationQueries.Contains(this.CorrelationQuery))
                {
                    item.CorrelationQueries.Add(this.CorrelationQuery);
                }
                foreach (System.ServiceModel.CorrelationQuery query in this.ReplyCorrelationQueries)
                {
                    if (!item.CorrelationQueries.Contains(query))
                    {
                        item.CorrelationQueries.Add(query);
                    }
                    else if (System.ServiceModel.Activities.TD.DuplicateCorrelationQueryIsEnabled())
                    {
                        System.ServiceModel.Activities.TD.DuplicateCorrelationQuery(query.Where.ToString());
                    }
                }
                this.correlationQueries = item.CorrelationQueries;
            }
        }

        private void EnsureTransactionFlowOnContract(ref ServiceEndpoint serviceEndpoint)
        {
            if (!this.IsOneWay)
            {
                TransactionFlowBindingElement element = serviceEndpoint.Binding.CreateBindingElements().Find<TransactionFlowBindingElement>();
                if ((element != null) && element.Transactions)
                {
                    ContractInferenceHelper.EnsureTransactionFlowOnContract(ref serviceEndpoint, this.ServiceContractName, this.OperationName, this.Action, this.Parent.ProtectionLevel);
                }
            }
        }

        protected override void Execute(NativeActivityContext context)
        {
            this.noPersistHandle.Get(context).Enter(context);
            SendMessageInstance instance = new SendMessageInstance(this, context);
            this.SetSendMessageInstance(context, instance);
            if (instance.RequestContext != null)
            {
                this.ExecuteClientRequest(context, instance);
            }
            else
            {
                this.ExecuteServerResponse(context, instance);
            }
        }

        private void ExecuteClientRequest(NativeActivityContext context, SendMessageInstance instance)
        {
            ChannelCacheSettings channelSettings;
            instance.CacheExtension = context.GetExtension<SendMessageChannelCache>();
            this.Parent.InitializeChannelCacheEnabledSetting(instance.CacheExtension);
            if (instance.CorrelatesWith != null)
            {
                if (instance.CorrelatesWith.CallbackContext != null)
                {
                    instance.CorrelationCallbackContext = instance.CorrelatesWith.CallbackContext;
                    instance.EndpointAddress = this.CreateEndpointAddressFromCallback(instance.CorrelationCallbackContext.ListenAddress.ToEndpointAddress());
                }
                if (instance.CorrelatesWith.Context != null)
                {
                    instance.CorrelationContext = instance.CorrelatesWith.Context;
                }
            }
            instance.RequestOrReply = this.Message.Get(context);
            if (instance.EndpointAddress == null)
            {
                instance.EndpointAddress = this.CreateEndpointAddress(context);
            }
            if (instance.EndpointAddress == null)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new ValidationException(System.ServiceModel.Activities.SR.EndpointAddressNotSetInEndpoint(this.OperationName)));
            }
            string endpointConfigurationName = (this.Endpoint != null) ? null : this.EndpointConfigurationName;
            this.ProcessSendMessageTrace(context, instance, true);
            ObjectCache<FactoryCacheKey, ChannelFactoryReference> factoryCache = null;
            ObjectCacheItem<ChannelFactoryReference> cacheItem = null;
            if (this.cachedFactoryCacheKey == null)
            {
                ServiceEndpoint cachedServiceEndpoint = this.GetCachedServiceEndpoint();
                this.cachedFactoryCacheKey = new FactoryCacheKey(this.Endpoint, endpointConfigurationName, this.IsOneWay, this.TokenImpersonationLevel, cachedServiceEndpoint.Contract, this.correlationQueries);
            }
            if (instance.CacheExtension.AllowUnsafeCaching || this.IsEndpointSettingsSafeForCache())
            {
                factoryCache = instance.CacheExtension.GetFactoryCache();
                channelSettings = instance.CacheExtension.ChannelSettings;
                KeyValuePair<ObjectCacheItem<ChannelFactoryReference>, SendMessageChannelCache> lastUsedFactoryCacheItem = this.lastUsedFactoryCacheItem;
                if (object.ReferenceEquals(lastUsedFactoryCacheItem.Value, instance.CacheExtension))
                {
                    if ((lastUsedFactoryCacheItem.Key != null) && lastUsedFactoryCacheItem.Key.TryAddReference())
                    {
                        cacheItem = lastUsedFactoryCacheItem.Key;
                    }
                    else
                    {
                        this.lastUsedFactoryCacheItem = new KeyValuePair<ObjectCacheItem<ChannelFactoryReference>, SendMessageChannelCache>(null, null);
                    }
                }
                if (cacheItem == null)
                {
                    cacheItem = factoryCache.Take(this.cachedFactoryCacheKey);
                }
            }
            else
            {
                channelSettings = ChannelCacheSettings.EmptyCacheSettings;
            }
            ChannelFactoryReference newFactoryReference = null;
            if (cacheItem == null)
            {
                ServiceEndpoint targetEndpoint = this.CreateServiceEndpoint();
                newFactoryReference = new ChannelFactoryReference(this.cachedFactoryCacheKey, targetEndpoint, channelSettings);
            }
            instance.SetupFactoryReference(cacheItem, newFactoryReference, factoryCache);
            if (this.onSendFailure == null)
            {
                this.onSendFailure = new FaultCallback(this.OnSendFailure);
            }
            if (instance.FactoryReference.NeedsOpen)
            {
                context.ScheduleActivity(this.openChannelFactory, new CompletionCallback(this.OnChannelFactoryOpened), this.onSendFailure);
            }
            else
            {
                this.OnChannelFactoryOpenedCore(context, instance);
            }
        }

        private void ExecuteServerResponse(NativeActivityContext context, SendMessageInstance instance)
        {
            Func<string> dataProvider = null;
            instance.OperationContext = instance.ResponseContext.WorkflowOperationContext.OperationContext;
            instance.ProcessMessagePropertyCallbacks();
            this.ProcessSendMessageTrace(context, instance, false);
            CorrelationQueryBehavior correlationBehavior = null;
            foreach (CorrelationQueryBehavior behavior2 in instance.OperationContext.Channel.Extensions.FindAll<CorrelationQueryBehavior>())
            {
                if (behavior2.ServiceContractName == this.ServiceContractName)
                {
                    correlationBehavior = behavior2;
                    break;
                }
            }
            instance.RequestOrReply = this.Message.Get(context);
            if (correlationBehavior != null)
            {
                EnsureCorrelationBehaviorScopeName(context, correlationBehavior);
                instance.RegisterCorrelationBehavior(correlationBehavior);
                if (instance.CorrelationKeyCalculator != null)
                {
                    if ((correlationBehavior.SendNames != null) && (correlationBehavior.SendNames.Count > 0))
                    {
                        if ((correlationBehavior.SendNames.Count == 1) && correlationBehavior.SendNames.Contains(ContextExchangeCorrelationHelper.CorrelationName))
                        {
                            ContextMessageProperty contextMessageProperty = null;
                            if (ContextMessageProperty.TryGet(instance.OperationContext.OutgoingMessageProperties, out contextMessageProperty))
                            {
                                if (dataProvider == null)
                                {
                                    dataProvider = () => ContextExchangeCorrelationHelper.GetContextCorrelationData(instance.OperationContext);
                                }
                                CorrelationDataMessageProperty.AddData(instance.RequestOrReply, ContextExchangeCorrelationHelper.CorrelationName, dataProvider);
                            }
                            this.InitializeCorrelations(context, instance);
                        }
                        else
                        {
                            instance.OperationContext.OutgoingMessageProperties.Add(CorrelationCallbackMessageProperty.Name, new MessageCorrelationCallbackMessageProperty(correlationBehavior.SendNames ?? ((ICollection<string>) new string[0]), instance));
                            instance.CorrelationSynchronizer = new CorrelationSynchronizer();
                        }
                    }
                    else
                    {
                        this.InitializeCorrelations(context, instance);
                    }
                }
            }
            if (instance.ResponseContext.Exception != null)
            {
                try
                {
                    instance.ResponseContext.WorkflowOperationContext.SendFault(instance.ResponseContext.Exception);
                    goto Label_0286;
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    instance.ResponseContext.Exception = exception;
                    goto Label_0286;
                }
            }
            try
            {
                instance.ResponseContext.WorkflowOperationContext.SendReply(instance.RequestOrReply);
            }
            catch (Exception exception2)
            {
                if (Fx.IsFatal(exception2))
                {
                    throw;
                }
                instance.ResponseContext.Exception = exception2;
            }
        Label_0286:
            if (TraceUtility.ActivityTracing && (instance.AmbientActivityId != Trace.CorrelationManager.ActivityId))
            {
                if (System.ServiceModel.Activities.TD.StopSignpostEventIsEnabled())
                {
                    Dictionary<string, string> dictionary = new Dictionary<string, string>(3);
                    dictionary.Add("ActivityName", base.DisplayName);
                    dictionary.Add("ActivityType", "MessagingActivityExecution");
                    dictionary.Add("ActivityInstanceId", context.ActivityInstanceId);
                    System.ServiceModel.Activities.TD.StopSignpostEvent(new DictionaryTraceRecord(dictionary));
                }
                System.ServiceModel.Activities.FxTrace.Trace.SetAndTraceTransfer(instance.AmbientActivityId, true);
                instance.AmbientActivityId = Guid.Empty;
            }
            if (instance.CorrelationSynchronizer == null)
            {
                context.SetValue<System.ServiceModel.Channels.Message>(this.Message, null);
                context.SetValue<System.ServiceModel.Channels.Message>(this.MessageOut, null);
                if (this.ShouldPersistBeforeSend)
                {
                    this.noPersistHandle.Get(context).Exit(context);
                    context.ScheduleActivity(this.persist, new CompletionCallback(this.OnPersistCompleted));
                }
                else
                {
                    this.FinalizeSendMessageCore(instance);
                }
            }
            else
            {
                if (instance.CorrelationSynchronizer.IsChannelWorkComplete)
                {
                    this.OnChannelCorrelationCompleteCore(context, instance);
                }
                else
                {
                    context.ScheduleActivity(this.channelCorrelationCompletionWaiter, new CompletionCallback(this.OnChannelCorrelationComplete), null);
                }
                if (instance.CorrelationSynchronizer.NotifySendComplete())
                {
                    this.FinalizeSendMessageCore(instance);
                }
            }
        }

        private void FinalizeSendMessageCore(SendMessageInstance instance)
        {
            Exception completionException = instance.GetCompletionException();
            if (completionException != null)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(completionException);
            }
        }

        private AddressHeaderCollection GetCachedEndpointHeaders()
        {
            if (this.cachedEndpointHeaderCollection == null)
            {
                this.cachedEndpointHeaderCollection = new AddressHeaderCollection(this.Endpoint.Headers);
            }
            return this.cachedEndpointHeaderCollection;
        }

        private ServiceEndpoint GetCachedServiceEndpoint()
        {
            if (this.cachedServiceEndpoint == null)
            {
                this.cachedServiceEndpoint = this.CreateServiceEndpoint();
            }
            return this.cachedServiceEndpoint;
        }

        private ContractDescription GetContractDescription()
        {
            ContractDescription description;
            if (!this.Parent.ChannelCacheEnabled || this.Parent.OperationUsesMessageContract)
            {
                if (this.Parent.OperationDescription == null)
                {
                    this.Parent.OperationDescription = ContractInferenceHelper.CreateOneWayOperationDescription(this.Parent);
                }
                description = ContractInferenceHelper.CreateContractFromOperation(this.ServiceContractName, this.Parent.OperationDescription);
            }
            else if (this.IsOneWay)
            {
                description = ContractInferenceHelper.CreateOutputChannelContractDescription(this.ServiceContractName, this.Parent.ProtectionLevel);
            }
            else
            {
                description = ContractInferenceHelper.CreateRequestChannelContractDescription(this.ServiceContractName, this.Parent.ProtectionLevel);
            }
            if (this.ServiceContractName != null)
            {
                description.ConfigurationName = this.ServiceContractName.LocalName;
            }
            return description;
        }

        internal MessageVersion GetMessageVersion()
        {
            if (this.messageVersion == null)
            {
                ServiceEndpoint cachedServiceEndpoint = this.GetCachedServiceEndpoint();
                this.messageVersion = ((cachedServiceEndpoint != null) && (cachedServiceEndpoint.Binding != null)) ? cachedServiceEndpoint.Binding.MessageVersion : null;
            }
            return this.messageVersion;
        }

        private SendMessageInstance GetSendMessageInstance(ActivityContext context)
        {
            return this.sendMessageInstance.Get(context).Instance;
        }

        private System.ServiceModel.Channels.Message InitializeCorrelations(NativeActivityContext context, SendMessageInstance instance)
        {
            if (instance.CorrelationKeyCalculator != null)
            {
                instance.RequestOrReply = MessagingActivityHelper.InitializeCorrelationHandles(context, instance.ContextBasedCorrelationHandle, instance.AmbientHandle, this.correlationInitializers, instance.CorrelationKeyCalculator, instance.RequestOrReply);
            }
            if (instance.RequestContext != null)
            {
                CorrelationHandle explicitChannelCorrelationHandle = instance.GetExplicitChannelCorrelationHandle(context, this.correlationInitializers);
                if (explicitChannelCorrelationHandle != null)
                {
                    if (!explicitChannelCorrelationHandle.TryRegisterRequestContext(context, instance.RequestContext))
                    {
                        throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.TryRegisterRequestContextFailed));
                    }
                }
                else if (!this.IsOneWay && !instance.AmbientHandle.TryRegisterRequestContext(context, instance.RequestContext))
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.TryRegisterRequestContextFailed));
                }
            }
            return instance.RequestOrReply;
        }

        private void InitializeEndpoint(ref ServiceEndpoint serviceEndpoint, string configurationName)
        {
            ServiceEndpoint endpoint = null;
            if (configurationName != null)
            {
                endpoint = ConfigLoader.LookupEndpoint(configurationName, null, serviceEndpoint.Contract);
            }
            if (endpoint != null)
            {
                serviceEndpoint = endpoint;
            }
            else if (!serviceEndpoint.IsFullyConfigured)
            {
                new ConfigLoader().LoadChannelBehaviors(serviceEndpoint, configurationName);
            }
        }

        private bool IsEndpointSettingsSafeForCache()
        {
            if (!this.configVerified)
            {
                this.isConfigSettingsSecure = this.Endpoint != null;
                this.configVerified = true;
            }
            return this.isConfigSettingsSecure;
        }

        private void OnChannelCorrelationComplete(NativeActivityContext context, System.Activities.ActivityInstance completedInstance)
        {
            SendMessageInstance sendMessageInstance = this.GetSendMessageInstance(context);
            this.OnChannelCorrelationCompleteCore(context, sendMessageInstance);
        }

        private void OnChannelCorrelationCompleteCore(NativeActivityContext context, SendMessageInstance instance)
        {
            System.ServiceModel.Channels.Message message = this.InitializeCorrelations(context, instance);
            instance.CorrelationSynchronizer.NotifyMessageUpdatedByWorkflow(message);
            context.SetValue<System.ServiceModel.Channels.Message>(this.Message, null);
            context.SetValue<System.ServiceModel.Channels.Message>(this.MessageOut, null);
            if (this.ShouldPersistBeforeSend && (instance.RequestContext == null))
            {
                this.noPersistHandle.Get(context).Exit(context);
                context.ScheduleActivity(this.persist, new CompletionCallback(this.OnPersistCompleted));
            }
            else
            {
                Bookmark bookmark = context.CreateBookmark(new BookmarkCallback(this.CompleteCorrelationCallback), BookmarkOptions.NonBlocking);
                context.ResumeBookmark(bookmark, null);
            }
        }

        private void OnChannelFactoryOpened(NativeActivityContext context, System.Activities.ActivityInstance completedInstance)
        {
            SendMessageInstance sendMessageInstance = this.GetSendMessageInstance(context);
            this.OnChannelFactoryOpenedCore(context, sendMessageInstance);
        }

        private void OnChannelFactoryOpenedCore(NativeActivityContext context, SendMessageInstance instance)
        {
            instance.PopulateClientChannel();
            IContextChannel clientSendChannel = instance.ClientSendChannel as IContextChannel;
            instance.OperationContext = (clientSendChannel == null) ? null : new OperationContext(clientSendChannel);
            CorrelationQueryBehavior correlationQueryBehavior = instance.FactoryReference.CorrelationQueryBehavior;
            if (correlationQueryBehavior != null)
            {
                EnsureCorrelationBehaviorScopeName(context, correlationQueryBehavior);
                instance.RegisterCorrelationBehavior(correlationQueryBehavior);
            }
            instance.ProcessMessagePropertyCallbacks();
            ContextMessageProperty property = null;
            if ((instance.CorrelationCallbackContext != null) && (instance.CorrelationContext != null))
            {
                if (!MessagingActivityHelper.CompareContextEquality(instance.CorrelationCallbackContext.Context, instance.CorrelationContext.Context))
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.ContextMismatchInContextAndCallBackContext));
                }
                property = new ContextMessageProperty(instance.CorrelationCallbackContext.Context);
            }
            else if (instance.CorrelationCallbackContext != null)
            {
                property = new ContextMessageProperty(instance.CorrelationCallbackContext.Context);
            }
            else if (instance.CorrelationContext != null)
            {
                property = new ContextMessageProperty(instance.CorrelationContext.Context);
            }
            if (property != null)
            {
                property.AddOrReplaceInMessage(instance.RequestOrReply);
            }
            CorrelationHandle handle = (instance.ContextBasedCorrelationHandle != null) ? instance.ContextBasedCorrelationHandle : instance.AmbientHandle;
            if ((handle != null) && ((handle.Scope == null) || !handle.Scope.IsInitialized))
            {
                string str = Guid.NewGuid().ToString();
                Dictionary<string, string> dictionary2 = new Dictionary<string, string>(1);
                dictionary2.Add("instanceId", str);
                Dictionary<string, string> dictionary = dictionary2;
                new CallbackContextMessageProperty(dictionary).AddOrReplaceInMessage(instance.RequestOrReply);
            }
            if (instance.CorrelationSendNames != null)
            {
                instance.RequestOrReply.Properties.Add(CorrelationCallbackMessageProperty.Name, new MessageCorrelationCallbackMessageProperty(instance.CorrelationSendNames, instance));
                instance.CorrelationSynchronizer = new CorrelationSynchronizer();
            }
            else
            {
                this.InitializeCorrelations(context, instance);
            }
            if (instance.CorrelationSynchronizer != null)
            {
                context.ScheduleActivity(this.channelCorrelationCompletionWaiter, new CompletionCallback(this.OnChannelCorrelationComplete), this.onSendFailure);
            }
            context.ScheduleActivity(this.openChannelAndSendMessage, new CompletionCallback(this.OnClientSendComplete), this.onSendFailure);
        }

        private void OnClientSendComplete(NativeActivityContext context, System.Activities.ActivityInstance completedInstance)
        {
            SendMessageInstance sendMessageInstance = this.GetSendMessageInstance(context);
            if ((sendMessageInstance.CorrelationSynchronizer == null) || sendMessageInstance.CorrelationSynchronizer.NotifySendComplete())
            {
                this.FinalizeSendMessageCore(sendMessageInstance);
            }
        }

        private void OnPersistCompleted(NativeActivityContext context, System.Activities.ActivityInstance completedInstance)
        {
            this.noPersistHandle.Get(context).Enter(context);
            SendMessageInstance sendMessageInstance = this.GetSendMessageInstance(context);
            if ((sendMessageInstance != null) && ((sendMessageInstance.CorrelationSynchronizer == null) || sendMessageInstance.CorrelationSynchronizer.NotifyWorkflowCorrelationProcessingComplete()))
            {
                this.FinalizeSendMessageCore(sendMessageInstance);
            }
        }

        private void OnSendFailure(NativeActivityFaultContext context, Exception propagatedException, System.Activities.ActivityInstance propagatedFrom)
        {
            throw System.ServiceModel.Activities.FxTrace.Exception.AsError(propagatedException);
        }

        private void ProcessSendMessageTrace(NativeActivityContext context, SendMessageInstance instance, bool isClient)
        {
            if (TraceUtility.MessageFlowTracing)
            {
                try
                {
                    if (TraceUtility.ActivityTracing)
                    {
                        instance.AmbientActivityId = Trace.CorrelationManager.ActivityId;
                    }
                    if (isClient)
                    {
                        instance.E2EActivityId = Trace.CorrelationManager.ActivityId;
                        if (instance.E2EActivityId == Guid.Empty)
                        {
                            instance.E2EActivityId = Guid.NewGuid();
                        }
                        if (context.WorkflowInstanceId != instance.E2EActivityId)
                        {
                            DiagnosticTrace.ActivityId = context.WorkflowInstanceId;
                            System.ServiceModel.Activities.FxTrace.Trace.SetAndTraceTransfer(instance.E2EActivityId, true);
                        }
                    }
                    else
                    {
                        DiagnosticTrace.ActivityId = context.WorkflowInstanceId;
                        instance.E2EActivityId = instance.ResponseContext.WorkflowOperationContext.E2EActivityId;
                    }
                    SendMessageRecord record = new SendMessageRecord("MessageCorrelationSendRecord") {
                        E2EActivityId = instance.E2EActivityId
                    };
                    context.Track(record);
                    if (TraceUtility.ActivityTracing && System.ServiceModel.Activities.TD.StartSignpostEventIsEnabled())
                    {
                        Dictionary<string, string> dictionary = new Dictionary<string, string>(3);
                        dictionary.Add("ActivityName", base.DisplayName);
                        dictionary.Add("ActivityType", "MessagingActivityExecution");
                        dictionary.Add("ActivityInstanceId", context.ActivityInstanceId);
                        System.ServiceModel.Activities.TD.StartSignpostEvent(new DictionaryTraceRecord(dictionary));
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    System.ServiceModel.Activities.FxTrace.Exception.AsInformation(exception);
                }
            }
        }

        private void SetSendMessageInstance(NativeActivityContext context, SendMessageInstance instance)
        {
            VolatileSendMessageInstance instance2 = new VolatileSendMessageInstance {
                Instance = instance
            };
            this.sendMessageInstance.Set(context, instance2);
        }

        public string Action { get; set; }

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        public InArgument<CorrelationHandle> CorrelatesWith { get; set; }

        public Collection<CorrelationInitializer> CorrelationInitializers
        {
            get
            {
                if (this.correlationInitializers == null)
                {
                    this.correlationInitializers = new Collection<CorrelationInitializer>();
                }
                return this.correlationInitializers;
            }
        }

        public System.ServiceModel.CorrelationQuery CorrelationQuery { get; set; }

        public System.ServiceModel.Endpoint Endpoint { get; set; }

        public InArgument<Uri> EndpointAddress { get; set; }

        public string EndpointConfigurationName { get; set; }

        public bool IsOneWay { get; set; }

        internal bool IsSendReply { get; set; }

        public InArgument<System.ServiceModel.Channels.Message> Message { get; set; }

        internal OutArgument<System.ServiceModel.Channels.Message> MessageOut { get; set; }

        public string OperationName { get; set; }

        internal Send Parent { get; set; }

        internal ICollection<System.ServiceModel.CorrelationQuery> ReplyCorrelationQueries
        {
            get
            {
                if (this.replyCorrelationQueries == null)
                {
                    this.replyCorrelationQueries = new Collection<System.ServiceModel.CorrelationQuery>();
                }
                return this.replyCorrelationQueries;
            }
        }

        public XName ServiceContractName { get; set; }

        internal bool ShouldPersistBeforeSend { get; set; }

        public System.Security.Principal.TokenImpersonationLevel TokenImpersonationLevel { get; set; }

        internal sealed class ChannelFactoryReference : IDisposable
        {
            private ObjectCache<EndpointAddress, Pool<IChannel>> channelCache;
            private ChannelFactory channelFactory;
            private System.ServiceModel.Activities.Description.CorrelationQueryBehavior correlationQueryBehavior;
            private Func<Pool<IChannel>> createChannelCacheItem;
            private static Action<Pool<IChannel>> disposeChannelPool = new Action<Pool<IChannel>>(InternalSendMessage.ChannelFactoryReference.DisposeChannelPool);
            private readonly InternalSendMessage.FactoryCacheKey factoryKey;
            private static AsyncCallback onDisposeCommunicationObject = Fx.ThunkCallback(new AsyncCallback(InternalSendMessage.ChannelFactoryReference.OnDisposeCommunicationObject));
            private readonly ServiceEndpoint targetEndpoint;

            public ChannelFactoryReference(InternalSendMessage.FactoryCacheKey factoryKey, ServiceEndpoint targetEndpoint, ChannelCacheSettings channelCacheSettings)
            {
                Func<Pool<IChannel>> func = null;
                this.factoryKey = factoryKey;
                this.targetEndpoint = targetEndpoint;
                if (factoryKey.IsOperationContractOneWay)
                {
                    this.channelFactory = new ChannelFactory<IOutputChannel>(targetEndpoint);
                }
                else
                {
                    this.channelFactory = new ChannelFactory<IRequestChannel>(targetEndpoint);
                }
                this.channelFactory.UseActiveAutoClose = true;
                this.channelFactory.Credentials.Windows.AllowedImpersonationLevel = factoryKey.TokenImpersonationLevel;
                ObjectCacheSettings settings = new ObjectCacheSettings {
                    CacheLimit = channelCacheSettings.MaxItemsInCache,
                    IdleTimeout = channelCacheSettings.IdleTimeout,
                    LeaseTimeout = channelCacheSettings.LeaseTimeout
                };
                ObjectCache<EndpointAddress, Pool<IChannel>> cache = new ObjectCache<EndpointAddress, Pool<IChannel>>(settings) {
                    DisposeItemCallback = disposeChannelPool
                };
                this.channelCache = cache;
                if (func == null)
                {
                    func = () => new Pool<IChannel>(channelCacheSettings.MaxItemsInCache);
                }
                this.createChannelCacheItem = func;
            }

            public IAsyncResult BeginOpen(AsyncCallback callback, object state)
            {
                return this.channelFactory.BeginOpen(callback, state);
            }

            public void Dispose()
            {
                DisposeCommunicationObject(this.channelFactory);
            }

            private static void DisposeChannelPool(Pool<IChannel> channelPool)
            {
                IChannel channel;
                while ((channel = channelPool.Take()) != null)
                {
                    DisposeCommunicationObject(channel);
                }
            }

            private static void DisposeCommunicationObject(ICommunicationObject communicationObject)
            {
                bool flag = false;
                try
                {
                    if (communicationObject.State == CommunicationState.Opened)
                    {
                        IAsyncResult result = communicationObject.BeginClose(ServiceDefaults.CloseTimeout, onDisposeCommunicationObject, communicationObject);
                        if (result.CompletedSynchronously)
                        {
                            communicationObject.EndClose(result);
                        }
                        flag = true;
                    }
                }
                catch (CommunicationException)
                {
                }
                catch (TimeoutException)
                {
                }
                finally
                {
                    if (!flag)
                    {
                        communicationObject.Abort();
                    }
                }
            }

            public ObjectCacheItem<InternalSendMessage.ChannelFactoryReference> EndOpen(IAsyncResult result, ObjectCache<InternalSendMessage.FactoryCacheKey, InternalSendMessage.ChannelFactoryReference> factoryCache)
            {
                this.channelFactory.EndOpen(result);
                ObjectCacheItem<InternalSendMessage.ChannelFactoryReference> item = null;
                if (factoryCache != null)
                {
                    item = factoryCache.Add(this.factoryKey, this);
                }
                return item;
            }

            private static void OnDisposeCommunicationObject(IAsyncResult result)
            {
                if (!result.CompletedSynchronously)
                {
                    ICommunicationObject asyncState = (ICommunicationObject) result.AsyncState;
                    bool flag = false;
                    try
                    {
                        asyncState.EndClose(result);
                        flag = true;
                    }
                    catch (CommunicationException)
                    {
                    }
                    catch (TimeoutException)
                    {
                    }
                    finally
                    {
                        if (!flag)
                        {
                            asyncState.Abort();
                        }
                    }
                }
            }

            public void ReturnChannel(IChannel channel, ObjectCacheItem<Pool<IChannel>> channelPool)
            {
                bool flag = channel.State != CommunicationState.Opened;
                if (!flag)
                {
                    lock (channelPool.Value)
                    {
                        flag = !channelPool.Value.Return(channel);
                    }
                }
                if (flag)
                {
                    DisposeCommunicationObject(channel);
                }
                channelPool.ReleaseReference();
            }

            public IChannel TakeChannel(EndpointAddress endpointAddress, out ObjectCacheItem<Pool<IChannel>> channelPool)
            {
                IChannel serviceChannel;
                channelPool = this.channelCache.Take(endpointAddress, this.createChannelCacheItem);
                lock (channelPool.Value)
                {
                    serviceChannel = channelPool.Value.Take();
                }
                ServiceChannel channel2 = serviceChannel as ServiceChannel;
                if ((serviceChannel != null) && ((serviceChannel.State != CommunicationState.Opened) || ((channel2 != null) && (channel2.Binder.Channel.State != CommunicationState.Opened))))
                {
                    serviceChannel.Abort();
                    serviceChannel = null;
                }
                if (serviceChannel == null)
                {
                    Uri via = null;
                    if ((this.targetEndpoint.Address != null) && (this.targetEndpoint.Address.Uri != this.targetEndpoint.ListenUri))
                    {
                        via = this.targetEndpoint.ListenUri;
                    }
                    if (this.factoryKey.IsOperationContractOneWay)
                    {
                        serviceChannel = ((ChannelFactory<IOutputChannel>) this.channelFactory).CreateChannel(endpointAddress, via);
                    }
                    else
                    {
                        serviceChannel = ((ChannelFactory<IRequestChannel>) this.channelFactory).CreateChannel(endpointAddress, via);
                    }
                }
                if (!(serviceChannel is ServiceChannel))
                {
                    serviceChannel = ServiceChannelFactory.GetServiceChannel(serviceChannel);
                }
                return serviceChannel;
            }

            public System.ServiceModel.Activities.Description.CorrelationQueryBehavior CorrelationQueryBehavior
            {
                get
                {
                    if (this.correlationQueryBehavior == null)
                    {
                        this.correlationQueryBehavior = this.targetEndpoint.Behaviors.Find<System.ServiceModel.Activities.Description.CorrelationQueryBehavior>();
                    }
                    return this.correlationQueryBehavior;
                }
            }

            public bool NeedsOpen
            {
                get
                {
                    return (this.channelFactory.State == CommunicationState.Created);
                }
            }
        }

        private class CorrelationSynchronizer
        {
            private Completion completion;
            private Action onRequestSetByChannel;
            private Action<Message> onWorkflowCorrelationProcessingComplete;
            private object thisLock = new object();

            public void NotifyMessageUpdatedByWorkflow(Message message)
            {
                this.UpdatedMessage = message;
            }

            public void NotifyRequestSetByChannel(Action<Message> onWorkflowCorrelationProcessingComplete)
            {
                Action onRequestSetByChannel = null;
                lock (this.thisLock)
                {
                    this.IsChannelWorkComplete = true;
                    this.onWorkflowCorrelationProcessingComplete = onWorkflowCorrelationProcessingComplete;
                    onRequestSetByChannel = this.onRequestSetByChannel;
                }
                if (onRequestSetByChannel != null)
                {
                    onRequestSetByChannel();
                }
            }

            public bool NotifySendComplete()
            {
                lock (this.thisLock)
                {
                    if (this.completion == Completion.CorrelationComplete)
                    {
                        return true;
                    }
                    this.completion = Completion.SendComplete;
                }
                return false;
            }

            public bool NotifyWorkflowCorrelationProcessingComplete()
            {
                bool flag = false;
                lock (this.thisLock)
                {
                    if (this.completion == Completion.SendComplete)
                    {
                        flag = true;
                    }
                    else
                    {
                        this.completion = Completion.CorrelationComplete;
                    }
                }
                this.onWorkflowCorrelationProcessingComplete(this.UpdatedMessage);
                return flag;
            }

            public bool SetWorkflowNotificationCallback(Action onRequestSetByChannel)
            {
                bool isChannelWorkComplete = false;
                lock (this.thisLock)
                {
                    isChannelWorkComplete = this.IsChannelWorkComplete;
                    this.onRequestSetByChannel = onRequestSetByChannel;
                }
                return isChannelWorkComplete;
            }

            public bool IsChannelWorkComplete { get; private set; }

            public Message UpdatedMessage { get; private set; }

            private enum Completion
            {
                None,
                SendComplete,
                CorrelationComplete
            }
        }

        internal class FactoryCacheKey : IEquatable<InternalSendMessage.FactoryCacheKey>
        {
            private ContractDescription contract;
            private Collection<CorrelationQuery> correlationQueries;
            private Endpoint endpoint;
            private string endpointConfigurationName;
            private bool isOperationContractOneWay;
            private System.Security.Principal.TokenImpersonationLevel tokenImpersonationLevel;

            public FactoryCacheKey(Endpoint endpoint, string endpointConfigurationName, bool isOperationOneway, System.Security.Principal.TokenImpersonationLevel tokenImpersonationLevel, ContractDescription contractDescription, ICollection<CorrelationQuery> correlationQueries)
            {
                this.endpoint = endpoint;
                this.endpointConfigurationName = endpointConfigurationName;
                this.isOperationContractOneWay = isOperationOneway;
                this.tokenImpersonationLevel = tokenImpersonationLevel;
                this.contract = contractDescription;
                if (correlationQueries != null)
                {
                    this.correlationQueries = new Collection<CorrelationQuery>();
                    foreach (CorrelationQuery query in correlationQueries)
                    {
                        this.correlationQueries.Add(query);
                    }
                }
            }

            public bool Equals(InternalSendMessage.FactoryCacheKey other)
            {
                if (!object.ReferenceEquals(this, other))
                {
                    if (other == null)
                    {
                        return false;
                    }
                    if (((this.endpoint == null) && (other.endpoint != null)) || ((other.endpoint == null) && (this.endpoint != null)))
                    {
                        return false;
                    }
                    if (this.endpoint != null)
                    {
                        if (!object.ReferenceEquals(this.endpoint, other.endpoint) && (this.endpoint.Binding != other.endpoint.Binding))
                        {
                            return false;
                        }
                    }
                    else if (this.endpointConfigurationName != other.endpointConfigurationName)
                    {
                        return false;
                    }
                    if (this.TokenImpersonationLevel != other.TokenImpersonationLevel)
                    {
                        return false;
                    }
                    if (this.IsOperationContractOneWay != other.IsOperationContractOneWay)
                    {
                        return false;
                    }
                    if (!ContractDescriptionComparerHelper.IsContractDescriptionEquivalent(this.contract, other.contract))
                    {
                        return false;
                    }
                    if (!ContractDescriptionComparerHelper.EqualsUnordered<CorrelationQuery>(this.correlationQueries, other.correlationQueries))
                    {
                        return false;
                    }
                }
                return true;
            }

            public override int GetHashCode()
            {
                int num = 0;
                if ((this.contract != null) && (this.contract.Name != null))
                {
                    num ^= this.contract.Name.GetHashCode();
                }
                if ((this.endpoint != null) && (this.endpoint.Binding != null))
                {
                    num ^= this.endpoint.Binding.GetHashCode();
                }
                return num;
            }

            public bool IsOperationContractOneWay
            {
                get
                {
                    return this.isOperationContractOneWay;
                }
            }

            public System.Security.Principal.TokenImpersonationLevel TokenImpersonationLevel
            {
                get
                {
                    return this.tokenImpersonationLevel;
                }
            }

            private static class ContractDescriptionComparerHelper
            {
                private static bool EqualsOrdered<T>(IList<T> left, IList<T> right, Func<T, T, bool> equals)
                {
                    if (left == null)
                    {
                        if (right != null)
                        {
                            return (right.Count == 0);
                        }
                        return true;
                    }
                    if (right == null)
                    {
                        return (left.Count == 0);
                    }
                    if (left.Count != right.Count)
                    {
                        return false;
                    }
                    for (int i = 0; i < left.Count; i++)
                    {
                        if (!equals(left[i], right[i]))
                        {
                            return false;
                        }
                    }
                    return true;
                }

                public static bool EqualsUnordered<T>(Collection<T> left, Collection<T> right) where T: class
                {
                    return EqualsUnordered<T>(left, right, (t1, t2) => t1 == t2);
                }

                private static bool EqualsUnordered<T>(Collection<T> left, Collection<T> right, Func<T, T, bool> equals)
                {
                    if (left == null)
                    {
                        if (right != null)
                        {
                            return (right.Count == 0);
                        }
                        return true;
                    }
                    if (right == null)
                    {
                        return (left.Count == 0);
                    }
                    return (((left.Count == right.Count) && left.All<T>(leftItem => right.Any<T>(rightItem => equals(leftItem, rightItem)))) && right.All<T>(rightItem => left.Any<T>(leftItem => equals(leftItem, rightItem))));
                }

                public static bool IsContractDescriptionEquivalent(ContractDescription c1, ContractDescription c2)
                {
                    if (c1 == c2)
                    {
                        return true;
                    }
                    if ((c1.ContractType == null) || (c2.ContractType == null))
                    {
                        return false;
                    }
                    if (((((c1 == null) || (c2 == null)) || (!(c1.Name == c2.Name) || !(c1.Namespace == c2.Namespace))) || ((!(c1.ConfigurationName == c2.ConfigurationName) || (c1.ProtectionLevel != c2.ProtectionLevel)) || ((c1.SessionMode != c2.SessionMode) || !(c1.ContractType == c2.ContractType)))) || (c1.Behaviors.Count != c2.Behaviors.Count))
                    {
                        return false;
                    }
                    return EqualsUnordered<OperationDescription>(c1.Operations, c2.Operations, (o1, o2) => IsOperationDescriptionEquivalent(o1, o2));
                }

                private static bool IsMessageDescriptionEquivalent(MessageDescription m1, MessageDescription m2)
                {
                    return ((m1 == m2) || ((m1.Action == m2.Action) && (m1.Direction == m2.Direction)));
                }

                private static bool IsOperationDescriptionEquivalent(OperationDescription o1, OperationDescription o2)
                {
                    if (o1 == o2)
                    {
                        return true;
                    }
                    if ((!(o1.Name == o2.Name) || (o1.ProtectionLevel != o2.ProtectionLevel)) || ((o1.IsOneWay != o2.IsOneWay) || !IsTransactionBehaviorEquivalent(o1, o2)))
                    {
                        return false;
                    }
                    return EqualsOrdered<MessageDescription>(o1.Messages, o2.Messages, (m1, m2) => IsMessageDescriptionEquivalent(m1, m2));
                }

                private static bool IsTransactionBehaviorEquivalent(OperationDescription o1, OperationDescription o2)
                {
                    if (((o1 != null) && (o2 != null)) || (o1 != o2))
                    {
                        if (o1.Behaviors.Count != o2.Behaviors.Count)
                        {
                            return false;
                        }
                        TransactionFlowAttribute attribute = o1.Behaviors.Find<TransactionFlowAttribute>();
                        TransactionFlowAttribute attribute2 = o2.Behaviors.Find<TransactionFlowAttribute>();
                        if (((attribute == null) && (attribute2 != null)) || ((attribute2 == null) && (attribute != null)))
                        {
                            return false;
                        }
                        if ((attribute != null) && (attribute.Transactions != attribute2.Transactions))
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
        }

        private class MessageCorrelationCallbackMessageProperty : CorrelationCallbackMessageProperty
        {
            protected MessageCorrelationCallbackMessageProperty(InternalSendMessage.MessageCorrelationCallbackMessageProperty callback) : base(callback)
            {
                this.Instance = callback.Instance;
            }

            public MessageCorrelationCallbackMessageProperty(ICollection<string> neededData, InternalSendMessage.SendMessageInstance instance) : base(neededData)
            {
                this.Instance = instance;
            }

            public override IMessageProperty CreateCopy()
            {
                return new InternalSendMessage.MessageCorrelationCallbackMessageProperty(this);
            }

            protected override IAsyncResult OnBeginFinalizeCorrelation(Message message, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new FinalizeCorrelationAsyncResult(this, message, callback, state);
            }

            protected override Message OnEndFinalizeCorrelation(IAsyncResult result)
            {
                return FinalizeCorrelationAsyncResult.End(result);
            }

            protected override Message OnFinalizeCorrelation(Message message, TimeSpan timeout)
            {
                return this.OnEndFinalizeCorrelation(this.OnBeginFinalizeCorrelation(message, timeout, null, null));
            }

            public InternalSendMessage.SendMessageInstance Instance { get; private set; }

            private class FinalizeCorrelationAsyncResult : AsyncResult
            {
                private Completion completion;
                private Message message;
                private object thisLock;

                public FinalizeCorrelationAsyncResult(InternalSendMessage.MessageCorrelationCallbackMessageProperty property, Message message, AsyncCallback callback, object state) : base(callback, state)
                {
                    bool flag = false;
                    if (property.Instance.IsCorrelationInitialized)
                    {
                        this.message = message;
                        flag = true;
                    }
                    else
                    {
                        property.Instance.IsCorrelationInitialized = true;
                        this.thisLock = new object();
                        property.Instance.RequestOrReply = message;
                        property.Instance.CorrelationSynchronizer.NotifyRequestSetByChannel(new Action<Message>(this.OnWorkflowCorrelationProcessingComplete));
                        flag = false;
                        lock (this.thisLock)
                        {
                            if (this.completion == Completion.WorkflowCorrelationProcessingComplete)
                            {
                                flag = true;
                            }
                            else
                            {
                                this.completion = Completion.ConstructorComplete;
                            }
                        }
                    }
                    if (flag)
                    {
                        base.Complete(true);
                    }
                }

                public static Message End(IAsyncResult result)
                {
                    return AsyncResult.End<InternalSendMessage.MessageCorrelationCallbackMessageProperty.FinalizeCorrelationAsyncResult>(result).message;
                }

                private void OnWorkflowCorrelationProcessingComplete(Message updatedMessage)
                {
                    this.message = updatedMessage;
                    bool flag = false;
                    lock (this.thisLock)
                    {
                        if (this.completion == Completion.ConstructorComplete)
                        {
                            flag = true;
                        }
                        else
                        {
                            this.completion = Completion.WorkflowCorrelationProcessingComplete;
                        }
                    }
                    if (flag)
                    {
                        base.Complete(false);
                    }
                }

                private enum Completion
                {
                    None,
                    ConstructorComplete,
                    WorkflowCorrelationProcessingComplete
                }
            }
        }

        private class OpenChannelAndSendMessage : AsyncCodeActivity
        {
            protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
            {
                System.ServiceModel.Activities.InternalSendMessage.VolatileSendMessageInstance instance = this.Instance.Get(context);
                Transaction currentTransactionContext = null;
                RuntimeTransactionHandle property = context.GetProperty<RuntimeTransactionHandle>();
                if (property != null)
                {
                    currentTransactionContext = property.GetCurrentTransaction(context);
                }
                return new OpenChannelAndSendMessageAsyncResult(this.InternalSendMessage, instance.Instance, currentTransactionContext, callback, state);
            }

            protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
            {
                OpenChannelAndSendMessageAsyncResult.End(result);
            }

            public InArgument<System.ServiceModel.Activities.InternalSendMessage.VolatileSendMessageInstance> Instance { get; set; }

            public System.ServiceModel.Activities.InternalSendMessage InternalSendMessage { get; set; }

            private class OpenChannelAndSendMessageAsyncResult : AsyncResult
            {
                private Guid ambientActivityId;
                private IChannel channel;
                private Transaction currentTransactionContext;
                private DependentTransaction dependentClone;
                private InternalSendMessage.SendMessageInstance instance;
                private InternalSendMessage internalSendMessage;
                private static AsyncResult.AsyncCompletion onChannelOpened = new AsyncResult.AsyncCompletion(InternalSendMessage.OpenChannelAndSendMessage.OpenChannelAndSendMessageAsyncResult.OnChannelOpened);
                private static AsyncCallback onChannelReceiveReplyCompleted = Fx.ThunkCallback(new AsyncCallback(InternalSendMessage.OpenChannelAndSendMessage.OpenChannelAndSendMessageAsyncResult.OnChannelReceiveReplyComplete));
                private static AsyncResult.AsyncCompletion onChannelSendComplete = new AsyncResult.AsyncCompletion(InternalSendMessage.OpenChannelAndSendMessage.OpenChannelAndSendMessageAsyncResult.OnChannelSendComplete);

                public OpenChannelAndSendMessageAsyncResult(InternalSendMessage internalSendMessage, InternalSendMessage.SendMessageInstance instance, Transaction currentTransactionContext, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.internalSendMessage = internalSendMessage;
                    this.instance = instance;
                    this.channel = this.instance.ClientSendChannel;
                    this.currentTransactionContext = currentTransactionContext;
                    bool flag = false;
                    if (this.channel.State == CommunicationState.Created)
                    {
                        IContextManager property = this.channel.GetProperty<IContextManager>();
                        if (property != null)
                        {
                            property.Enabled = false;
                        }
                        IAsyncResult result = this.channel.BeginOpen(base.PrepareAsyncCompletion(onChannelOpened), this);
                        if (result.CompletedSynchronously)
                        {
                            flag = OnChannelOpened(result);
                        }
                    }
                    else
                    {
                        flag = this.BeginSendMessage();
                    }
                    if (flag)
                    {
                        base.Complete(true);
                    }
                }

                private bool BeginSendMessage()
                {
                    IAsyncResult result = null;
                    bool flag = false;
                    OperationContext current = OperationContext.Current;
                    bool flag2 = !this.internalSendMessage.IsOneWay;
                    try
                    {
                        OperationContext.Current = this.instance.OperationContext;
                        if (TraceUtility.MessageFlowTracingOnly)
                        {
                            DiagnosticTrace.ActivityId = this.instance.E2EActivityId;
                        }
                        using (base.PrepareTransactionalCall(this.currentTransactionContext))
                        {
                            if (flag2)
                            {
                                if (this.currentTransactionContext != null)
                                {
                                    this.dependentClone = this.currentTransactionContext.DependentClone(DependentCloneOption.BlockCommitUntilComplete);
                                }
                                this.instance.RequestContext.EnsureAsyncWaitHandle();
                                result = ((IRequestChannel) this.channel).BeginRequest(this.instance.RequestOrReply, onChannelReceiveReplyCompleted, this);
                                if (result.CompletedSynchronously)
                                {
                                    Message reply = ((IRequestChannel) this.channel).EndRequest(result);
                                    this.instance.RequestContext.ReceiveReply(this.instance.OperationContext, reply);
                                }
                            }
                            else
                            {
                                result = ((IOutputChannel) this.channel).BeginSend(this.instance.RequestOrReply, base.PrepareAsyncCompletion(onChannelSendComplete), this);
                                if (result.CompletedSynchronously)
                                {
                                    ((IOutputChannel) this.channel).EndSend(result);
                                }
                            }
                            flag = true;
                        }
                    }
                    finally
                    {
                        OperationContext.Current = current;
                        if (!flag)
                        {
                            if (this.dependentClone != null)
                            {
                                this.dependentClone.Complete();
                                this.dependentClone = null;
                            }
                            this.channel.Abort();
                        }
                        if ((result != null) && result.CompletedSynchronously)
                        {
                            if (this.dependentClone != null)
                            {
                                this.dependentClone.Complete();
                                this.dependentClone = null;
                            }
                            this.internalSendMessage.CleanupResources(this.instance);
                        }
                    }
                    return (flag2 || base.SyncContinue(result));
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<InternalSendMessage.OpenChannelAndSendMessage.OpenChannelAndSendMessageAsyncResult>(result);
                }

                private static bool OnChannelOpened(IAsyncResult result)
                {
                    InternalSendMessage.OpenChannelAndSendMessage.OpenChannelAndSendMessageAsyncResult asyncState = (InternalSendMessage.OpenChannelAndSendMessage.OpenChannelAndSendMessageAsyncResult) result.AsyncState;
                    asyncState.channel.EndOpen(result);
                    return asyncState.BeginSendMessage();
                }

                private static void OnChannelReceiveReplyComplete(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        InternalSendMessage.OpenChannelAndSendMessage.OpenChannelAndSendMessageAsyncResult asyncState = (InternalSendMessage.OpenChannelAndSendMessage.OpenChannelAndSendMessageAsyncResult) result.AsyncState;
                        OperationContext current = OperationContext.Current;
                        Message reply = null;
                        bool flag = false;
                        try
                        {
                            OperationContext.Current = asyncState.instance.OperationContext;
                            asyncState.TraceActivityData();
                            System.Transactions.TransactionScope scope = Fx.CreateTransactionScope(asyncState.currentTransactionContext);
                            try
                            {
                                reply = ((IRequestChannel) asyncState.channel).EndRequest(result);
                                asyncState.instance.RequestContext.ReceiveAsyncReply(asyncState.instance.OperationContext, reply, null);
                                flag = true;
                            }
                            finally
                            {
                                Fx.CompleteTransactionScope(ref scope);
                            }
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                            asyncState.instance.RequestContext.ReceiveAsyncReply(asyncState.instance.OperationContext, null, exception);
                        }
                        finally
                        {
                            if (asyncState.dependentClone != null)
                            {
                                asyncState.dependentClone.Complete();
                                asyncState.dependentClone = null;
                            }
                            OperationContext.Current = current;
                            if (!flag)
                            {
                                asyncState.channel.Abort();
                            }
                            asyncState.internalSendMessage.CleanupResources(asyncState.instance);
                        }
                    }
                }

                private static bool OnChannelSendComplete(IAsyncResult result)
                {
                    if (!result.CompletedSynchronously)
                    {
                        InternalSendMessage.OpenChannelAndSendMessage.OpenChannelAndSendMessageAsyncResult asyncState = (InternalSendMessage.OpenChannelAndSendMessage.OpenChannelAndSendMessageAsyncResult) result.AsyncState;
                        OperationContext current = OperationContext.Current;
                        try
                        {
                            OperationContext.Current = asyncState.instance.OperationContext;
                            asyncState.TraceActivityData();
                            System.Transactions.TransactionScope scope = Fx.CreateTransactionScope(asyncState.currentTransactionContext);
                            try
                            {
                                ((IOutputChannel) asyncState.channel).EndSend(result);
                            }
                            finally
                            {
                                Fx.CompleteTransactionScope(ref scope);
                            }
                        }
                        catch (Exception exception)
                        {
                            if (Fx.IsFatal(exception))
                            {
                                throw;
                            }
                            asyncState.instance.RequestContext.Exception = exception;
                        }
                        finally
                        {
                            OperationContext.Current = current;
                            asyncState.internalSendMessage.CleanupResources(asyncState.instance);
                        }
                    }
                    return true;
                }

                private void TraceActivityData()
                {
                    if (TraceUtility.ActivityTracing)
                    {
                        if (System.ServiceModel.Activities.TD.StopSignpostEventIsEnabled())
                        {
                            Dictionary<string, string> dictionary = new Dictionary<string, string>(3);
                            dictionary.Add("ActivityName", this.instance.Activity.DisplayName);
                            dictionary.Add("ActivityType", "MessagingActivityExecution");
                            dictionary.Add("ActivityInstanceId", this.instance.ActivityInstanceId);
                            System.ServiceModel.Activities.TD.StopSignpostEvent(new DictionaryTraceRecord(dictionary));
                        }
                        System.ServiceModel.Activities.FxTrace.Trace.SetAndTraceTransfer(this.ambientActivityId, true);
                        this.ambientActivityId = Guid.Empty;
                    }
                }
            }
        }

        private class OpenChannelFactory : AsyncCodeActivity
        {
            protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
            {
                return new OpenChannelFactoryAsyncResult(this.Instance.Get(context).Instance, callback, state);
            }

            protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
            {
                OpenChannelFactoryAsyncResult.End(result);
            }

            public InArgument<InternalSendMessage.VolatileSendMessageInstance> Instance { get; set; }

            private class OpenChannelFactoryAsyncResult : AsyncResult
            {
                private static AsyncResult.AsyncCompletion channelFactoryOpenCompletion = new AsyncResult.AsyncCompletion(InternalSendMessage.OpenChannelFactory.OpenChannelFactoryAsyncResult.ChannelFactoryOpenCompletion);
                private InternalSendMessage.SendMessageInstance instance;

                public OpenChannelFactoryAsyncResult(InternalSendMessage.SendMessageInstance instance, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.instance = instance;
                    bool flag = false;
                    if (this.instance.FactoryReference.NeedsOpen)
                    {
                        IAsyncResult result = this.instance.FactoryReference.BeginOpen(base.PrepareAsyncCompletion(channelFactoryOpenCompletion), this);
                        if (result.CompletedSynchronously)
                        {
                            flag = this.OnNewChannelFactoryOpened(result);
                        }
                    }
                    else
                    {
                        flag = true;
                    }
                    if (flag)
                    {
                        base.Complete(true);
                    }
                }

                private static bool ChannelFactoryOpenCompletion(IAsyncResult result)
                {
                    InternalSendMessage.OpenChannelFactory.OpenChannelFactoryAsyncResult asyncState = (InternalSendMessage.OpenChannelFactory.OpenChannelFactoryAsyncResult) result.AsyncState;
                    return asyncState.OnNewChannelFactoryOpened(result);
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<InternalSendMessage.OpenChannelFactory.OpenChannelFactoryAsyncResult>(result);
                }

                private bool OnNewChannelFactoryOpened(IAsyncResult result)
                {
                    ObjectCacheItem<InternalSendMessage.ChannelFactoryReference> newCacheItem = this.instance.FactoryReference.EndOpen(result, this.instance.FactoryCache);
                    this.instance.RegisterNewCacheItem(newCacheItem);
                    return true;
                }
            }
        }

        private class SendMessageInstance
        {
            private ObjectCacheItem<InternalSendMessage.ChannelFactoryReference> cacheItem;
            private ObjectCacheItem<Pool<IChannel>> clientChannelPool;
            private CorrelationHandle explicitChannelCorrelationHandle;
            private ObjectCache<InternalSendMessage.FactoryCacheKey, InternalSendMessage.ChannelFactoryReference> factoryCache;
            private InternalSendMessage.ChannelFactoryReference factoryReference;
            private bool isUsingCacheFromExtension;
            private readonly InternalSendMessage parent;
            private IList<ISendMessageCallback> sendMessageCallbacks;

            public SendMessageInstance(InternalSendMessage parent, NativeActivityContext context)
            {
                this.parent = parent;
                CorrelationHandle ambientHandle = (parent.CorrelatesWith == null) ? null : parent.CorrelatesWith.Get(context);
                if ((ambientHandle != null) && !ambientHandle.IsInitalized())
                {
                    throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new ValidationException(System.ServiceModel.Activities.SR.SendWithUninitializedCorrelatesWith(this.parent.OperationName ?? string.Empty)));
                }
                if (ambientHandle == null)
                {
                    this.AmbientHandle = context.Properties.Find(CorrelationHandle.StaticExecutionPropertyName) as CorrelationHandle;
                    ambientHandle = this.AmbientHandle;
                }
                this.CorrelatesWith = ambientHandle;
                if (!parent.IsSendReply)
                {
                    CorrelationHandle explicitChannelCorrelationHandle = this.GetExplicitChannelCorrelationHandle(context, parent.correlationInitializers);
                    if (parent.IsOneWay)
                    {
                        if (explicitChannelCorrelationHandle != null)
                        {
                            throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.RequestReplyHandleShouldNotBePresentForOneWay));
                        }
                    }
                    else if ((explicitChannelCorrelationHandle == null) && (this.AmbientHandle == null))
                    {
                        this.AmbientHandle = context.Properties.Find(CorrelationHandle.StaticExecutionPropertyName) as CorrelationHandle;
                        if (this.AmbientHandle == null)
                        {
                            throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.SendMessageNeedsToPairWithReceiveMessageForTwoWayContract(parent.OperationName ?? string.Empty)));
                        }
                    }
                    this.RequestContext = new CorrelationRequestContext();
                    this.ContextBasedCorrelationHandle = CorrelationHandle.GetExplicitCallbackCorrelation(context, parent.correlationInitializers);
                    this.isUsingCacheFromExtension = true;
                }
                else
                {
                    CorrelationResponseContext context2;
                    if ((ambientHandle == null) || !ambientHandle.TryAcquireResponseContext(context, out context2))
                    {
                        throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.CorrelatedContextRequiredForAnonymousSend));
                    }
                    this.ResponseContext = context2;
                    this.ContextBasedCorrelationHandle = CorrelationHandle.GetExplicitContextCorrelation(context, parent.correlationInitializers);
                }
                this.sendMessageCallbacks = MessagingActivityHelper.GetCallbacks<ISendMessageCallback>(context.Properties);
                if (TraceUtility.MessageFlowTracing)
                {
                    this.ActivityInstanceId = context.ActivityInstanceId;
                }
            }

            public void Dispose()
            {
                if (this.ClientSendChannel != null)
                {
                    this.FactoryReference.ReturnChannel(this.ClientSendChannel, this.clientChannelPool);
                    this.ClientSendChannel = null;
                    this.clientChannelPool = null;
                }
                if (this.cacheItem != null)
                {
                    this.cacheItem.ReleaseReference();
                    if (this.isUsingCacheFromExtension)
                    {
                        this.parent.lastUsedFactoryCacheItem = new KeyValuePair<ObjectCacheItem<InternalSendMessage.ChannelFactoryReference>, SendMessageChannelCache>(this.cacheItem, this.CacheExtension);
                    }
                    this.cacheItem = null;
                }
            }

            public Exception GetCompletionException()
            {
                if (this.RequestContext != null)
                {
                    return this.RequestContext.Exception;
                }
                return this.ResponseContext.Exception;
            }

            public CorrelationHandle GetExplicitChannelCorrelationHandle(NativeActivityContext context, Collection<CorrelationInitializer> additionalCorrelations)
            {
                if (this.explicitChannelCorrelationHandle == null)
                {
                    this.explicitChannelCorrelationHandle = CorrelationHandle.GetExplicitChannelCorrelation(context, additionalCorrelations);
                }
                return this.explicitChannelCorrelationHandle;
            }

            public void PopulateClientChannel()
            {
                this.ClientSendChannel = this.FactoryReference.TakeChannel(this.EndpointAddress, out this.clientChannelPool);
            }

            public void ProcessMessagePropertyCallbacks()
            {
                if (this.sendMessageCallbacks != null)
                {
                    foreach (ISendMessageCallback callback in this.sendMessageCallbacks)
                    {
                        callback.OnSendMessage(this.OperationContext);
                    }
                }
            }

            public void RegisterCorrelationBehavior(CorrelationQueryBehavior correlationBehavior)
            {
                if (correlationBehavior.ScopeName != null)
                {
                    System.ServiceModel.Activities.Dispatcher.CorrelationKeyCalculator keyCalculator = correlationBehavior.GetKeyCalculator();
                    if (keyCalculator != null)
                    {
                        this.CorrelationKeyCalculator = keyCalculator;
                        if (this.RequestContext != null)
                        {
                            this.RequestContext.CorrelationKeyCalculator = keyCalculator;
                            if ((correlationBehavior.SendNames != null) && (correlationBehavior.SendNames.Count > 0))
                            {
                                this.CorrelationSendNames = correlationBehavior.SendNames;
                            }
                        }
                    }
                }
            }

            public void RegisterNewCacheItem(ObjectCacheItem<InternalSendMessage.ChannelFactoryReference> newCacheItem)
            {
                this.cacheItem = newCacheItem;
            }

            public void SetupFactoryReference(ObjectCacheItem<InternalSendMessage.ChannelFactoryReference> cacheItem, InternalSendMessage.ChannelFactoryReference newFactoryReference, ObjectCache<InternalSendMessage.FactoryCacheKey, InternalSendMessage.ChannelFactoryReference> factoryCache)
            {
                this.factoryCache = factoryCache;
                if (this.factoryCache == null)
                {
                    this.isUsingCacheFromExtension = false;
                }
                if (cacheItem != null)
                {
                    this.cacheItem = cacheItem;
                    this.factoryReference = cacheItem.Value;
                }
                else
                {
                    this.factoryReference = newFactoryReference;
                }
            }

            public InternalSendMessage Activity
            {
                get
                {
                    return this.parent;
                }
            }

            public string ActivityInstanceId { get; private set; }

            public Guid AmbientActivityId { get; set; }

            public CorrelationHandle AmbientHandle { get; private set; }

            public SendMessageChannelCache CacheExtension { get; set; }

            public IChannel ClientSendChannel { get; private set; }

            public CorrelationHandle ContextBasedCorrelationHandle { get; private set; }

            public CorrelationHandle CorrelatesWith { get; private set; }

            public System.ServiceModel.Activities.CorrelationCallbackContext CorrelationCallbackContext { get; set; }

            public System.ServiceModel.Activities.CorrelationContext CorrelationContext { get; set; }

            public System.ServiceModel.Activities.Dispatcher.CorrelationKeyCalculator CorrelationKeyCalculator { get; private set; }

            public ICollection<string> CorrelationSendNames { get; private set; }

            public System.ServiceModel.Activities.InternalSendMessage.CorrelationSynchronizer CorrelationSynchronizer { get; set; }

            public Guid E2EActivityId { get; set; }

            public System.ServiceModel.EndpointAddress EndpointAddress { get; set; }

            public ObjectCache<InternalSendMessage.FactoryCacheKey, InternalSendMessage.ChannelFactoryReference> FactoryCache
            {
                get
                {
                    return this.factoryCache;
                }
            }

            public InternalSendMessage.ChannelFactoryReference FactoryReference
            {
                get
                {
                    return this.factoryReference;
                }
            }

            public bool IsCorrelationInitialized { get; set; }

            public System.ServiceModel.OperationContext OperationContext { get; set; }

            public CorrelationRequestContext RequestContext { get; private set; }

            public Message RequestOrReply { get; set; }

            public CorrelationResponseContext ResponseContext { get; private set; }
        }

        [DataContract]
        private class VolatileSendMessageInstance
        {
            public InternalSendMessage.SendMessageInstance Instance { get; set; }
        }

        private class WaitOnChannelCorrelation : AsyncCodeActivity
        {
            protected override IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state)
            {
                return new WaitOnChannelCorrelationAsyncResult(this.Instance.Get(context).Instance.CorrelationSynchronizer, callback, state);
            }

            protected override void EndExecute(AsyncCodeActivityContext context, IAsyncResult result)
            {
                WaitOnChannelCorrelationAsyncResult.End(result);
            }

            public InArgument<InternalSendMessage.VolatileSendMessageInstance> Instance { get; set; }

            private class WaitOnChannelCorrelationAsyncResult : AsyncResult
            {
                private InternalSendMessage.CorrelationSynchronizer synchronizer;

                public WaitOnChannelCorrelationAsyncResult(InternalSendMessage.CorrelationSynchronizer synchronizer, AsyncCallback callback, object state) : base(callback, state)
                {
                    this.synchronizer = synchronizer;
                    if (synchronizer.IsChannelWorkComplete)
                    {
                        base.Complete(true);
                    }
                    else if (synchronizer.SetWorkflowNotificationCallback(new Action(this.OnChannelCorrelationComplete)))
                    {
                        base.Complete(true);
                    }
                }

                public static void End(IAsyncResult result)
                {
                    AsyncResult.End<InternalSendMessage.WaitOnChannelCorrelation.WaitOnChannelCorrelationAsyncResult>(result);
                }

                private void OnChannelCorrelationComplete()
                {
                    base.Complete(false);
                }
            }
        }
    }
}

