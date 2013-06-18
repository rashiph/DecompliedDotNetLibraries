namespace System.ServiceModel.Activities
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Activities.Description;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;

    public class WorkflowControlEndpoint : ServiceEndpoint
    {
        private static Uri defaultBaseUri;
        private static object workflowContractDescriptionLock = new object();
        private static ContractDescription workflowControlServiceContract;

        public WorkflowControlEndpoint() : this(GetDefaultBinding(), new EndpointAddress(new Uri(DefaultBaseUri, new Uri(Guid.NewGuid().ToString(), UriKind.Relative)), new AddressHeader[0]))
        {
        }

        public WorkflowControlEndpoint(Binding binding, EndpointAddress address) : base(WorkflowControlServiceContract, binding, address)
        {
            base.IsSystemEndpoint = true;
        }

        private static void ApplyOperationBehaviors(ContractDescription contractDescription)
        {
            foreach (OperationDescription description in contractDescription.Operations)
            {
                switch (description.Name)
                {
                    case "Abandon":
                    case "Cancel":
                    case "Run":
                    case "Suspend":
                    case "Terminate":
                    case "Unsuspend":
                        EnsureDispatch(description);
                        break;

                    case "TransactedCancel":
                    case "TransactedRun":
                    case "TransactedSuspend":
                    case "TransactedTerminate":
                    case "TransactedUnsuspend":
                        EnsureDispatch(description);
                        EnsureTransactedInvoke(description);
                        break;
                }
            }
        }

        private static void EnsureDispatch(OperationDescription operationDescription)
        {
            operationDescription.Behaviors.Add(new ControlOperationBehavior(false));
        }

        private static void EnsureTransactedInvoke(OperationDescription operationDescription)
        {
            operationDescription.Behaviors.Find<OperationBehaviorAttribute>().TransactionScopeRequired = true;
        }

        private static Binding GetDefaultBinding()
        {
            return new NetNamedPipeBinding(NetNamedPipeSecurityMode.None) { TransactionFlow = true };
        }

        private static Uri DefaultBaseUri
        {
            get
            {
                if (defaultBaseUri == null)
                {
                    defaultBaseUri = new Uri(string.Format(CultureInfo.InvariantCulture, "net.pipe://localhost/workflowControlServiceEndpoint/{0}/{1}", new object[] { Process.GetCurrentProcess().Id, AppDomain.CurrentDomain.Id }));
                }
                return defaultBaseUri;
            }
        }

        internal static ContractDescription WorkflowControlServiceContract
        {
            get
            {
                if (workflowControlServiceContract == null)
                {
                    lock (workflowContractDescriptionLock)
                    {
                        if (workflowControlServiceContract == null)
                        {
                            ContractDescription contract = ContractDescription.GetContract(typeof(IWorkflowInstanceManagement));
                            contract.Behaviors.Add(new ServiceMetadataContractBehavior(true));
                            ApplyOperationBehaviors(contract);
                            workflowControlServiceContract = contract;
                        }
                    }
                }
                return workflowControlServiceContract;
            }
        }
    }
}

