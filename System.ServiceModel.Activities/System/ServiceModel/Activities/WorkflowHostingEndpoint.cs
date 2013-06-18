namespace System.ServiceModel.Activities
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Activities.Description;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    public abstract class WorkflowHostingEndpoint : ServiceEndpoint
    {
        private Collection<CorrelationQuery> correlationQueries;

        protected WorkflowHostingEndpoint(System.Type contractType) : this(contractType, null, null)
        {
        }

        protected WorkflowHostingEndpoint(System.Type contractType, Binding binding, EndpointAddress address) : base(ContractDescription.GetContract(contractType), binding, address)
        {
            base.IsSystemEndpoint = true;
            base.Contract.Behaviors.Add(new ServiceMetadataContractBehavior(false));
            base.Contract.Behaviors.Add(new WorkflowHostingContractBehavior());
            this.correlationQueries = new Collection<CorrelationQuery>();
            base.Behaviors.Add(new CorrelationQueryBehavior(this.correlationQueries));
            foreach (OperationDescription description in base.Contract.Operations)
            {
                TransactionFlowAttribute attribute = description.Behaviors.Find<TransactionFlowAttribute>();
                if ((attribute != null) && (attribute.Transactions != TransactionFlowOption.NotAllowed))
                {
                    description.Behaviors.Find<OperationBehaviorAttribute>().TransactionScopeRequired = true;
                }
            }
        }

        internal static FaultException CreateDispatchFaultException()
        {
            FaultCode subCode = new FaultCode("InternalServiceFault", "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/dispatcher");
            return new FaultException(MessageFault.CreateFault(FaultCode.CreateReceiverFaultCode(subCode), new FaultReason(new FaultReasonText(System.ServiceModel.Activities.SR.InternalServerError, CultureInfo.CurrentCulture))), "http://schemas.microsoft.com/net/2005/12/windowscommunicationfoundation/dispatcher/fault");
        }

        protected internal virtual WorkflowCreationContext OnGetCreationContext(object[] inputs, OperationContext operationContext, Guid instanceId, WorkflowHostingResponseContext responseContext)
        {
            return null;
        }

        protected internal virtual Guid OnGetInstanceId(object[] inputs, OperationContext operationContext)
        {
            return Guid.Empty;
        }

        protected internal virtual Bookmark OnResolveBookmark(object[] inputs, OperationContext operationContext, WorkflowHostingResponseContext responseContext, out object value)
        {
            value = null;
            return null;
        }

        public Collection<CorrelationQuery> CorrelationQueries
        {
            get
            {
                return this.correlationQueries;
            }
        }

        private class WorkflowHostingContractBehavior : IContractBehavior
        {
            public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
            {
            }

            public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime)
            {
            }

            public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime)
            {
                foreach (OperationDescription description in contractDescription.Operations)
                {
                    if (description.Behaviors.Find<WorkflowHostingOperationBehavior>() == null)
                    {
                        description.Behaviors.Add(new WorkflowHostingOperationBehavior((WorkflowHostingEndpoint) endpoint));
                    }
                }
            }

            public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
            {
            }

            private class WorkflowHostingOperationBehavior : WorkflowOperationBehavior
            {
                private WorkflowHostingEndpoint hostingEndpoint;

                public WorkflowHostingOperationBehavior(WorkflowHostingEndpoint hostingEndpoint) : base(true)
                {
                    this.hostingEndpoint = hostingEndpoint;
                }

                protected internal override Bookmark OnResolveBookmark(WorkflowOperationContext context, out BookmarkScope bookmarkScope, out object value)
                {
                    CorrelationMessageProperty property;
                    if (CorrelationMessageProperty.TryGet(context.OperationContext.IncomingMessageProperties, out property))
                    {
                        bookmarkScope = new BookmarkScope(property.CorrelationKey.Value);
                    }
                    else
                    {
                        bookmarkScope = null;
                    }
                    WorkflowHostingResponseContext responseContext = new WorkflowHostingResponseContext(context);
                    Bookmark bookmark = this.hostingEndpoint.OnResolveBookmark(context.Inputs, context.OperationContext, responseContext, out value);
                    if (bookmark == null)
                    {
                        throw System.ServiceModel.Activities.FxTrace.Exception.AsError(WorkflowHostingEndpoint.CreateDispatchFaultException());
                    }
                    return bookmark;
                }
            }
        }
    }
}

