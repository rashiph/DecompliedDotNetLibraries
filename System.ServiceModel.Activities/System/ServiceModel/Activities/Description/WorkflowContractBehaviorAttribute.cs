namespace System.ServiceModel.Activities.Description
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    [AttributeUsage(AttributeTargets.Interface, AllowMultiple=false)]
    public sealed class WorkflowContractBehaviorAttribute : Attribute, IContractBehavior
    {
        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
        }

        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime)
        {
            if (dispatchRuntime.ChannelDispatcher.Host.Description.Behaviors.Contains(typeof(WorkflowServiceBehavior)))
            {
                foreach (OperationDescription description in contractDescription.Operations)
                {
                    if (description.Behaviors.Find<ControlOperationBehavior>() == null)
                    {
                        description.Behaviors.Add(new ControlOperationBehavior(true));
                    }
                }
            }
        }

        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {
        }
    }
}

