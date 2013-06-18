namespace System.ServiceModel.Channels
{
    using System;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    internal class PeerOperationSelectorBehavior : IContractBehavior
    {
        private IPeerNodeMessageHandling messageHandler;

        internal PeerOperationSelectorBehavior(IPeerNodeMessageHandling messageHandler)
        {
            this.messageHandler = messageHandler;
        }

        void IContractBehavior.AddBindingParameters(ContractDescription description, ServiceEndpoint endpoint, BindingParameterCollection parameters)
        {
        }

        void IContractBehavior.ApplyClientBehavior(ContractDescription description, ServiceEndpoint endpoint, ClientRuntime proxy)
        {
            proxy.OperationSelector = new OperationSelectorBehavior.MethodInfoOperationSelector(description, MessageDirection.Input);
            proxy.CallbackDispatchRuntime.OperationSelector = new OperationSelector(this.messageHandler);
        }

        void IContractBehavior.ApplyDispatchBehavior(ContractDescription description, ServiceEndpoint endpoint, DispatchRuntime dispatch)
        {
            dispatch.OperationSelector = new OperationSelector(this.messageHandler);
            if (dispatch.ClientRuntime != null)
            {
                dispatch.ClientRuntime.OperationSelector = new OperationSelectorBehavior.MethodInfoOperationSelector(description, MessageDirection.Output);
            }
        }

        void IContractBehavior.Validate(ContractDescription description, ServiceEndpoint endpoint)
        {
        }
    }
}

