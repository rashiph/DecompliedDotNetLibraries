namespace System.ServiceModel
{
    using System;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ReceiveContextEnabledAttribute : Attribute, IOperationBehavior
    {
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
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("operationDescription");
            }
            if (dispatchOperation == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("dispatchOperation");
            }
            if (this.ManualControl)
            {
                dispatchOperation.ReceiveContextAcknowledgementMode = ReceiveContextAcknowledgementMode.ManualAcknowledgement;
            }
            else
            {
                dispatchOperation.ReceiveContextAcknowledgementMode = ReceiveContextAcknowledgementMode.AutoAcknowledgeOnRPCComplete;
            }
        }

        public void Validate(OperationDescription operationDescription)
        {
        }

        public bool ManualControl { get; set; }
    }
}

