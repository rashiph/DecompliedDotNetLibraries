namespace System.ServiceModel.Activities.Description
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.DurableInstancing;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Activities.Dispatcher;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using System.Transactions;

    internal class WorkflowOperationBehavior : IOperationBehavior
    {
        private Bookmark bookmark;

        protected WorkflowOperationBehavior(bool canCreateInstance)
        {
            this.CanCreateInstance = canCreateInstance;
        }

        public WorkflowOperationBehavior(Bookmark bookmark, bool canCreateInstance) : this(canCreateInstance)
        {
            this.bookmark = bookmark;
        }

        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            if (operationDescription == null)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.ArgumentNull("operationDescription");
            }
            if (dispatchOperation == null)
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.ArgumentNull("dispatchOperation");
            }
            if (((dispatchOperation.Parent == null) || (dispatchOperation.Parent.ChannelDispatcher == null)) || (((dispatchOperation.Parent.ChannelDispatcher.Host == null) || (dispatchOperation.Parent.ChannelDispatcher.Host.Description == null)) || (dispatchOperation.Parent.ChannelDispatcher.Host.Description.Behaviors == null)))
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.DispatchOperationInInvalidState));
            }
            ServiceHostBase host = dispatchOperation.Parent.ChannelDispatcher.Host;
            if (!(host is WorkflowServiceHost))
            {
                throw System.ServiceModel.Activities.FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.WorkflowBehaviorWithNonWorkflowHost(typeof(WorkflowOperationBehavior).Name)));
            }
            CorrelationKeyCalculator keyCalculator = null;
            ServiceEndpoint endpoint = null;
            foreach (ServiceEndpoint endpoint2 in host.Description.Endpoints)
            {
                if (endpoint2.Id == dispatchOperation.Parent.EndpointDispatcher.Id)
                {
                    endpoint = endpoint2;
                    break;
                }
            }
            if (endpoint != null)
            {
                CorrelationQueryBehavior behavior = endpoint.Behaviors.Find<CorrelationQueryBehavior>();
                if (behavior != null)
                {
                    keyCalculator = behavior.GetKeyCalculator();
                }
            }
            dispatchOperation.Invoker = new WorkflowOperationInvoker(operationDescription, endpoint, keyCalculator, this, host, dispatchOperation.Invoker);
        }

        protected internal virtual Bookmark OnResolveBookmark(WorkflowOperationContext context, out BookmarkScope bookmarkScope, out object value)
        {
            CorrelationMessageProperty property;
            if (CorrelationMessageProperty.TryGet(context.OperationContext.IncomingMessageProperties, out property))
            {
                bookmarkScope = new BookmarkScope(property.CorrelationKey.Value);
            }
            else
            {
                bookmarkScope = BookmarkScope.Default;
            }
            value = context;
            return this.bookmark;
        }

        public void Validate(OperationDescription operationDescription)
        {
        }

        internal bool CanCreateInstance { get; set; }

        private class WorkflowOperationInvoker : ControlOperationInvoker, IInstanceTransaction
        {
            private WorkflowOperationBehavior behavior;
            private IOperationInvoker innerInvoker;
            private bool isFirstReceiveOfTransactedReceiveScopeTree;
            private bool isHostingEndpoint;
            private bool performanceCountersEnabled;
            private bool propagateActivity;

            public WorkflowOperationInvoker(OperationDescription operationDescription, ServiceEndpoint endpoint, CorrelationKeyCalculator keyCalculator, WorkflowOperationBehavior behavior, ServiceHostBase host, IOperationInvoker innerInvoker) : base(operationDescription, endpoint, keyCalculator, host)
            {
                base.StaticBookmarkName = (behavior.bookmark == null) ? null : behavior.bookmark.Name;
                this.behavior = behavior;
                base.CanCreateInstance = behavior.CanCreateInstance;
                this.performanceCountersEnabled = PerformanceCounters.PerformanceCountersEnabled;
                this.propagateActivity = TraceUtility.ShouldPropagateActivity;
                this.isHostingEndpoint = endpoint is WorkflowHostingEndpoint;
                this.innerInvoker = innerInvoker;
                this.isFirstReceiveOfTransactedReceiveScopeTree = operationDescription.IsFirstReceiveOfTransactedReceiveScopeTree;
            }

            public override object[] AllocateInputs()
            {
                if (this.isHostingEndpoint)
                {
                    return this.innerInvoker.AllocateInputs();
                }
                return new object[1];
            }

            public Transaction GetTransactionForInstance(OperationContext operationContext)
            {
                Transaction transactionForInstance = null;
                if (!this.isFirstReceiveOfTransactedReceiveScopeTree)
                {
                    InstanceKey key;
                    ICollection<InstanceKey> is2;
                    base.GetInstanceKeys(operationContext, out key, out is2);
                    transactionForInstance = base.InstanceManager.PersistenceProviderDirectory.GetTransactionForInstance(key);
                }
                return transactionForInstance;
            }

            protected override IAsyncResult OnBeginServiceOperation(WorkflowServiceInstance workflowInstance, OperationContext operationContext, object[] inputs, Transaction currentTransaction, IInvokeReceivedNotification notification, TimeSpan timeout, AsyncCallback callback, object state)
            {
                return WorkflowOperationContext.BeginProcessRequest(workflowInstance, operationContext, base.OperationName, inputs, this.performanceCountersEnabled, this.propagateActivity, currentTransaction, notification, this.behavior, timeout, callback, state);
            }

            protected override object OnEndServiceOperation(WorkflowServiceInstance durableInstance, out object[] outputs, IAsyncResult result)
            {
                return WorkflowOperationContext.EndProcessRequest(result, out outputs);
            }
        }
    }
}

