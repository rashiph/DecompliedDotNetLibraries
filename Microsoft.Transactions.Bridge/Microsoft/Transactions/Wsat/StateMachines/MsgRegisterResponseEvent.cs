namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal abstract class MsgRegisterResponseEvent : SynchronizationEvent
    {
        private TwoPhaseCommitCoordinatorProxy proxy;

        public MsgRegisterResponseEvent(CoordinatorEnlistmentBase coordinator, RegisterResponse response, TwoPhaseCommitCoordinatorProxy proxy) : base(coordinator)
        {
            proxy.AddRef();
            this.proxy = proxy;
        }

        public TwoPhaseCommitCoordinatorProxy Proxy
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.proxy;
            }
        }
    }
}

