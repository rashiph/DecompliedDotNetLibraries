namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [ServiceBehavior(InstanceContextMode=InstanceContextMode.Single, ConcurrencyMode=ConcurrencyMode.Multiple)]
    internal class ActivationCoordinatorDispatcher10 : IWSActivationCoordinator10, IWSActivationCoordinator
    {
        private ActivationCoordinatorDispatcher activationCoordinatorDispatcher;

        public ActivationCoordinatorDispatcher10(CoordinationService service, IActivationCoordinator dispatch)
        {
            ProtocolVersionHelper.AssertProtocolVersion10(service.ProtocolVersion, typeof(ActivationCoordinatorDispatcher10), "constr");
            this.activationCoordinatorDispatcher = new ActivationCoordinatorDispatcher(service, dispatch);
        }

        public IAsyncResult BeginCreateCoordinationContext(Message message, AsyncCallback callback, object state)
        {
            return this.activationCoordinatorDispatcher.BeginCreateCoordinationContext(message, callback, state);
        }

        public Message EndCreateCoordinationContext(IAsyncResult ar)
        {
            return this.activationCoordinatorDispatcher.EndCreateCoordinationContext(ar);
        }

        public System.Type ContractType
        {
            get
            {
                return typeof(IWSActivationCoordinator10);
            }
        }
    }
}

