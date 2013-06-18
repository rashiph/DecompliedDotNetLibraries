namespace System.ServiceModel.Activities
{
    using System;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;

    internal class WorkflowGetInstanceContext
    {
        public bool CanCreateInstance { get; set; }

        public object[] Inputs { get; set; }

        public System.ServiceModel.OperationContext OperationContext { get; set; }

        public System.ServiceModel.Activities.WorkflowCreationContext WorkflowCreationContext { get; set; }

        public System.ServiceModel.Activities.WorkflowHostingEndpoint WorkflowHostingEndpoint { get; set; }

        public System.ServiceModel.Activities.WorkflowHostingResponseContext WorkflowHostingResponseContext { get; set; }
    }
}

