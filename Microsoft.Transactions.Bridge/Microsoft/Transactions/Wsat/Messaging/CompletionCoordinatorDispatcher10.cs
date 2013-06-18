namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Channels;

    [ServiceBehavior(InstanceContextMode=InstanceContextMode.Single, ConcurrencyMode=ConcurrencyMode.Multiple)]
    internal class CompletionCoordinatorDispatcher10 : IWSCompletionCoordinator10, IWSCompletionCoordinator
    {
        private CompletionCoordinatorDispatcher completionCoordinatorDispatcher;

        public CompletionCoordinatorDispatcher10(CoordinationService service, ICompletionCoordinator dispatch)
        {
            ProtocolVersionHelper.AssertProtocolVersion10(service.ProtocolVersion, typeof(CompletionCoordinatorDispatcher10), "constr");
            this.completionCoordinatorDispatcher = new CompletionCoordinatorDispatcher(service, dispatch);
        }

        public void Commit(Message message)
        {
            this.completionCoordinatorDispatcher.Commit(message);
        }

        public void Rollback(Message message)
        {
            this.completionCoordinatorDispatcher.Rollback(message);
        }

        public void WsaFault(Message message)
        {
            this.completionCoordinatorDispatcher.WsaFault(message);
        }

        public void WsatFault(Message message)
        {
            this.completionCoordinatorDispatcher.WsatFault(message);
        }

        public void WscoorFault(Message message)
        {
            this.completionCoordinatorDispatcher.WscoorFault(message);
        }

        public System.Type ContractType
        {
            get
            {
                return typeof(IWSCompletionCoordinator10);
            }
        }
    }
}

