namespace System.ServiceModel.Activities.Description
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Activities;
    using System.ServiceModel.Activities.Dispatcher;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    internal class ControlOperationBehavior : IOperationBehavior
    {
        private bool isWrappedMode;

        public ControlOperationBehavior(bool isWrappedMode)
        {
            this.isWrappedMode = isWrappedMode;
        }

        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            ServiceHostBase host = dispatchOperation.Parent.ChannelDispatcher.Host;
            if (!(host is WorkflowServiceHost))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.ServiceModel.Activities.SR.WorkflowBehaviorWithNonWorkflowHost(typeof(ControlOperationBehavior).Name)));
            }
            ServiceEndpoint endpoint = null;
            foreach (ServiceEndpoint endpoint2 in host.Description.Endpoints)
            {
                if (endpoint2.Id == dispatchOperation.Parent.EndpointDispatcher.Id)
                {
                    endpoint = endpoint2;
                    break;
                }
            }
            if (this.isWrappedMode)
            {
                CorrelationKeyCalculator correlationKeyCalculator = null;
                if (endpoint != null)
                {
                    CorrelationQueryBehavior behavior = endpoint.Behaviors.Find<CorrelationQueryBehavior>();
                    if (behavior != null)
                    {
                        correlationKeyCalculator = behavior.GetKeyCalculator();
                    }
                }
                dispatchOperation.Invoker = new ControlOperationInvoker(operationDescription, endpoint, correlationKeyCalculator, dispatchOperation.Invoker, host);
            }
            else
            {
                dispatchOperation.Invoker = new ControlOperationInvoker(operationDescription, endpoint, null, host);
            }
        }

        public void Validate(OperationDescription operationDescription)
        {
        }
    }
}

