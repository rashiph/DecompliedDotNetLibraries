namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [ServiceBehavior(InstanceContextMode=InstanceContextMode.Single, ConcurrencyMode=ConcurrencyMode.Multiple)]
    internal class ActivationCoordinatorDispatcher11 : IWSActivationCoordinator11, IWSActivationCoordinator
    {
        private ActivationCoordinatorDispatcher activationCoordinatorDispatcher;

        public ActivationCoordinatorDispatcher11(CoordinationService service, IActivationCoordinator dispatch)
        {
            ProtocolVersionHelper.AssertProtocolVersion11(service.ProtocolVersion, typeof(ActivationCoordinatorDispatcher11), "constr");
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
                return typeof(IWSActivationCoordinator11);
            }
        }
    }
}

