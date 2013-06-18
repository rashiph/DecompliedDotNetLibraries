namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal class CoordinatorInitializing : InactiveState
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CoordinatorInitializing(ProtocolState state) : base(state)
        {
        }

        public override void OnEvent(MsgEnlistTransactionEvent e)
        {
            CoordinatorEnlistment coordinator = e.Coordinator;
            if (!base.state.TransactionManager.Settings.NetworkInboundAccess)
            {
                base.ForwardEnlistmentEventToSubordinate(e);
                coordinator.StateMachine.ChangeState(base.state.States.CoordinatorInitializationFailed);
            }
            else
            {
                CoordinationContext currentContext = e.Body.CurrentContext;
                RegistrationProxy proxy = base.state.TryCreateRegistrationProxy(currentContext.RegistrationService);
                if (proxy == null)
                {
                    coordinator.ContextManager.Fault = base.state.Faults.RegistrationProxyFailed;
                    coordinator.StateMachine.ChangeState(base.state.States.CoordinatorInitializationFailed);
                }
                else
                {
                    try
                    {
                        coordinator.SetRegistrationProxy(proxy);
                        EnlistmentOptions options = coordinator.CreateEnlistmentOptions(currentContext.Expires, currentContext.ExpiresPresent, currentContext.IsolationLevel, currentContext.IsolationFlags, currentContext.Description);
                        base.state.TransactionManagerSend.EnlistTransaction(coordinator, options, e);
                        e.StateMachine.ChangeState(base.state.States.CoordinatorEnlisting);
                    }
                    finally
                    {
                        proxy.Release();
                    }
                }
            }
        }
    }
}

