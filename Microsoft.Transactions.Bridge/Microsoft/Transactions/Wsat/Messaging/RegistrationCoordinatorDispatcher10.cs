namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [ServiceBehavior(InstanceContextMode=InstanceContextMode.Single, ConcurrencyMode=ConcurrencyMode.Multiple)]
    internal class RegistrationCoordinatorDispatcher10 : IWSRegistrationCoordinator10, IWSRegistrationCoordinator
    {
        private RegistrationCoordinatorDispatcher registrationCoordinatorDispatcher;

        public RegistrationCoordinatorDispatcher10(CoordinationService service, IRegistrationCoordinator dispatch)
        {
            ProtocolVersionHelper.AssertProtocolVersion10(service.ProtocolVersion, typeof(RegistrationCoordinatorDispatcher10), "constr");
            this.registrationCoordinatorDispatcher = new RegistrationCoordinatorDispatcher(service, dispatch);
        }

        public IAsyncResult BeginRegister(Message message, AsyncCallback callback, object state)
        {
            return this.registrationCoordinatorDispatcher.BeginRegister(message, callback, state);
        }

        public Message EndRegister(IAsyncResult ar)
        {
            return this.registrationCoordinatorDispatcher.EndRegister(ar);
        }

        public System.Type ContractType
        {
            get
            {
                return typeof(IWSRegistrationCoordinator10);
            }
        }
    }
}

