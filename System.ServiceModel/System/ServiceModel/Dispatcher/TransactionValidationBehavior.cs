namespace System.ServiceModel.Dispatcher
{
    using System;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    internal class TransactionValidationBehavior : IEndpointBehavior, IServiceBehavior
    {
        private static TransactionValidationBehavior instance;

        private TransactionValidationBehavior()
        {
        }

        private void EnsureNoOneWayTransactions(ServiceEndpoint endpoint)
        {
            CustomBinding binding = new CustomBinding(endpoint.Binding);
            if (binding.Elements.Find<TransactionFlowBindingElement>() != null)
            {
                for (int i = 0; i < endpoint.Contract.Operations.Count; i++)
                {
                    OperationDescription description = endpoint.Contract.Operations[i];
                    if (description.IsOneWay)
                    {
                        TransactionFlowOption transactions;
                        TransactionFlowAttribute attribute = description.Behaviors.Find<TransactionFlowAttribute>();
                        if (attribute != null)
                        {
                            transactions = attribute.Transactions;
                        }
                        else
                        {
                            transactions = TransactionFlowOption.NotAllowed;
                        }
                        if (TransactionFlowOptionHelper.AllowedOrRequired(transactions))
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxOneWayAndTransactionsIncompatible", new object[] { endpoint.Contract.Name, description.Name })));
                        }
                    }
                }
            }
        }

        private OperationDescription GetAutoCompleteFalseOperation(ServiceEndpoint endpoint)
        {
            foreach (OperationDescription description in endpoint.Contract.Operations)
            {
                if (!this.IsAutoComplete(description))
                {
                    return description;
                }
            }
            return null;
        }

        private bool HasTransactedOperations(System.ServiceModel.Description.ServiceDescription service)
        {
            for (int i = 0; i < service.Endpoints.Count; i++)
            {
                if (this.HasTransactedOperations(service.Endpoints[i]))
                {
                    return true;
                }
            }
            return false;
        }

        private bool HasTransactedOperations(ServiceEndpoint endpoint)
        {
            for (int i = 0; i < endpoint.Contract.Operations.Count; i++)
            {
                OperationDescription description = endpoint.Contract.Operations[i];
                OperationBehaviorAttribute attribute = description.Behaviors.Find<OperationBehaviorAttribute>();
                if ((attribute != null) && attribute.TransactionScopeRequired)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsAutoComplete(OperationDescription operation)
        {
            OperationBehaviorAttribute attribute = operation.Behaviors.Find<OperationBehaviorAttribute>();
            if (attribute != null)
            {
                return attribute.TransactionAutoComplete;
            }
            return true;
        }

        private bool IsSingleThreaded(System.ServiceModel.Description.ServiceDescription service)
        {
            ServiceBehaviorAttribute attribute = service.Behaviors.Find<ServiceBehaviorAttribute>();
            if (attribute != null)
            {
                return (attribute.ConcurrencyMode == ConcurrencyMode.Single);
            }
            return true;
        }

        private bool RequiresSessions(ServiceEndpoint endpoint)
        {
            return (endpoint.Contract.SessionMode == SessionMode.Required);
        }

        void IEndpointBehavior.AddBindingParameters(ServiceEndpoint serviceEndpoint, BindingParameterCollection bindingParameters)
        {
        }

        void IEndpointBehavior.ApplyClientBehavior(ServiceEndpoint serviceEndpoint, ClientRuntime behavior)
        {
        }

        void IEndpointBehavior.ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
        {
        }

        void IEndpointBehavior.Validate(ServiceEndpoint serviceEndpoint)
        {
            if (serviceEndpoint == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("serviceEndpoint");
            }
            this.ValidateTransactionFlowRequired("ChannelHasAtLeastOneOperationWithTransactionFlowEnabled", serviceEndpoint.Contract.Name, serviceEndpoint);
            this.EnsureNoOneWayTransactions(serviceEndpoint);
            this.ValidateNoMSMQandTransactionFlow(serviceEndpoint);
            this.ValidateCallbackBehaviorAttributeWithNoScopeRequired(serviceEndpoint);
            OperationDescription autoCompleteFalseOperation = this.GetAutoCompleteFalseOperation(serviceEndpoint);
            if (autoCompleteFalseOperation != null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxTransactionAutoCompleteFalseOnCallbackContract", new object[] { autoCompleteFalseOperation.Name, serviceEndpoint.Contract.Name })));
            }
        }

        void IServiceBehavior.AddBindingParameters(System.ServiceModel.Description.ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(System.ServiceModel.Description.ServiceDescription service, ServiceHostBase serviceHostBase)
        {
        }

        void IServiceBehavior.Validate(System.ServiceModel.Description.ServiceDescription service, ServiceHostBase serviceHostBase)
        {
            if (service == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("service");
            }
            this.ValidateNotConcurrentWhenReleaseServiceInstanceOnTxComplete(service);
            bool singleThreaded = this.IsSingleThreaded(service);
            for (int i = 0; i < service.Endpoints.Count; i++)
            {
                ServiceEndpoint endpoint = service.Endpoints[i];
                this.ValidateTransactionFlowRequired("ServiceHasAtLeastOneOperationWithTransactionFlowEnabled", service.Name, endpoint);
                this.EnsureNoOneWayTransactions(endpoint);
                this.ValidateNoMSMQandTransactionFlow(endpoint);
                ContractDescription contract = endpoint.Contract;
                for (int j = 0; j < contract.Operations.Count; j++)
                {
                    OperationDescription operation = contract.Operations[j];
                    this.ValidateScopeRequiredAndAutoComplete(operation, singleThreaded, contract.Name);
                }
                this.ValidateAutoCompleteFalseRequirements(service, endpoint);
            }
            this.ValidateServiceBehaviorAttributeWithNoScopeRequired(service);
            this.ValidateTransactionAutoCompleteOnSessionCloseHasSession(service);
        }

        private void ValidateAutoCompleteFalseRequirements(System.ServiceModel.Description.ServiceDescription service, ServiceEndpoint endpoint)
        {
            OperationDescription autoCompleteFalseOperation = this.GetAutoCompleteFalseOperation(endpoint);
            if (autoCompleteFalseOperation != null)
            {
                ServiceBehaviorAttribute attribute = service.Behaviors.Find<ServiceBehaviorAttribute>();
                if ((attribute != null) && (attribute.InstanceContextMode != InstanceContextMode.PerSession))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxTransactionAutoCompleteFalseAndInstanceContextMode", new object[] { endpoint.Contract.Name, autoCompleteFalseOperation.Name })));
                }
                if (!autoCompleteFalseOperation.IsInsideTransactedReceiveScope && !this.RequiresSessions(endpoint))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxTransactionAutoCompleteFalseAndSupportsSession", new object[] { endpoint.Contract.Name, autoCompleteFalseOperation.Name })));
                }
            }
        }

        private void ValidateCallbackBehaviorAttributeWithNoScopeRequired(ServiceEndpoint endpoint)
        {
            if (!this.HasTransactedOperations(endpoint))
            {
                CallbackBehaviorAttribute attribute = endpoint.Behaviors.Find<CallbackBehaviorAttribute>();
                if (attribute != null)
                {
                    if (attribute.TransactionTimeoutSet)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxTransactionTransactionTimeoutNeedsScope", new object[] { endpoint.Contract.Name })));
                    }
                    if (attribute.IsolationLevelSet)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxTransactionIsolationLevelNeedsScope", new object[] { endpoint.Contract.Name })));
                    }
                }
            }
        }

        private void ValidateNoMSMQandTransactionFlow(ServiceEndpoint endpoint)
        {
            BindingElementCollection elements = endpoint.Binding.CreateBindingElements();
            if ((elements.Find<TransactionFlowBindingElement>() != null) && (elements.Find<MsmqTransportBindingElement>() != null))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxTransactionFlowAndMSMQ", new object[] { endpoint.Address.Uri.AbsoluteUri })));
            }
        }

        private void ValidateNotConcurrentWhenReleaseServiceInstanceOnTxComplete(System.ServiceModel.Description.ServiceDescription service)
        {
            ServiceBehaviorAttribute attribute = service.Behaviors.Find<ServiceBehaviorAttribute>();
            if (((attribute != null) && this.HasTransactedOperations(service)) && (attribute.ReleaseServiceInstanceOnTransactionComplete && !this.IsSingleThreaded(service)))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxTransactionNonConcurrentOrReleaseServiceInstanceOnTxComplete", new object[] { service.Name })));
            }
        }

        private void ValidateScopeRequiredAndAutoComplete(OperationDescription operation, bool singleThreaded, string contractName)
        {
            OperationBehaviorAttribute attribute = operation.Behaviors.Find<OperationBehaviorAttribute>();
            if (((attribute != null) && !singleThreaded) && !attribute.TransactionAutoComplete)
            {
                string name = "SFxTransactionNonConcurrentOrAutoComplete2";
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString(name, new object[] { contractName, operation.Name })));
            }
        }

        private void ValidateServiceBehaviorAttributeWithNoScopeRequired(System.ServiceModel.Description.ServiceDescription service)
        {
            if (!this.HasTransactedOperations(service))
            {
                ServiceBehaviorAttribute attribute = service.Behaviors.Find<ServiceBehaviorAttribute>();
                if (attribute != null)
                {
                    if (attribute.TransactionTimeoutSet)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxTransactionTransactionTimeoutNeedsScope", new object[] { service.Name })));
                    }
                    if (attribute.IsolationLevelSet)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxTransactionIsolationLevelNeedsScope", new object[] { service.Name })));
                    }
                    if (attribute.ReleaseServiceInstanceOnTransactionCompleteSet)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxTransactionReleaseServiceInstanceOnTransactionCompleteNeedsScope", new object[] { service.Name })));
                    }
                    if (attribute.TransactionAutoCompleteOnSessionCloseSet)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxTransactionTransactionAutoCompleteOnSessionCloseNeedsScope", new object[] { service.Name })));
                    }
                }
            }
        }

        private void ValidateTransactionAutoCompleteOnSessionCloseHasSession(System.ServiceModel.Description.ServiceDescription service)
        {
            ServiceBehaviorAttribute attribute = service.Behaviors.Find<ServiceBehaviorAttribute>();
            if (attribute != null)
            {
                InstanceContextMode instanceContextMode = attribute.InstanceContextMode;
                if (attribute.TransactionAutoCompleteOnSessionClose && (instanceContextMode != InstanceContextMode.PerSession))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("SFxTransactionAutoCompleteOnSessionCloseNoSession", new object[] { service.Name })));
                }
            }
        }

        private void ValidateTransactionFlowRequired(string resource, string name, ServiceEndpoint endpoint)
        {
            bool flag = false;
            for (int i = 0; i < endpoint.Contract.Operations.Count; i++)
            {
                OperationDescription description = endpoint.Contract.Operations[i];
                TransactionFlowAttribute attribute = description.Behaviors.Find<TransactionFlowAttribute>();
                if ((attribute != null) && (attribute.Transactions == TransactionFlowOption.Mandatory))
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
            {
                CustomBinding binding = new CustomBinding(endpoint.Binding);
                TransactionFlowBindingElement element = binding.Elements.Find<TransactionFlowBindingElement>();
                if ((element == null) || !element.Transactions)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, System.ServiceModel.SR.GetString(resource), new object[] { name, binding.Name })));
                }
            }
        }

        internal static TransactionValidationBehavior Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TransactionValidationBehavior();
                }
                return instance;
            }
        }
    }
}

