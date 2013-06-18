namespace Microsoft.Transactions.Wsat.InputOutput
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using Microsoft.Transactions.Wsat.StateMachines;
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;

    internal class RegistrationParticipant
    {
        private AsyncCallback sendDurableRegisterComplete;
        private AsyncCallback sendVolatileRegisterComplete;
        private ProtocolState state;

        public RegistrationParticipant(ProtocolState state)
        {
            this.state = state;
            this.sendDurableRegisterComplete = Fx.ThunkCallback(new AsyncCallback(this.OnSendDurableRegisterComplete));
            this.sendVolatileRegisterComplete = Fx.ThunkCallback(new AsyncCallback(this.OnSendVolatileRegisterComplete));
        }

        private void OnSendDurableRegisterComplete(IAsyncResult ar)
        {
            if (!ar.CompletedSynchronously)
            {
                this.OnSendRegisterComplete((CoordinatorEnlistment) ar.AsyncState, ControlProtocol.Durable2PC, ar);
            }
        }

        private void OnSendRegisterComplete(CoordinatorEnlistment coordinator, ControlProtocol protocol, IAsyncResult ar)
        {
            SynchronizationEvent event2;
            EndpointAddress to = null;
            try
            {
                RegisterResponse response = coordinator.RegistrationProxy.EndSendRegister(ar);
                to = response.CoordinatorProtocolService;
                TwoPhaseCommitCoordinatorProxy proxy = this.state.TryCreateTwoPhaseCommitCoordinatorProxy(to);
                if (proxy == null)
                {
                    if (RegistrationCoordinatorResponseInvalidMetadataRecord.ShouldTrace)
                    {
                        RegistrationCoordinatorResponseInvalidMetadataRecord.Trace(coordinator.EnlistmentId, coordinator.SuperiorContext, protocol, to, null, this.state.ProtocolVersion);
                    }
                    event2 = new MsgRegistrationCoordinatorSendFailureEvent(coordinator);
                }
                else
                {
                    try
                    {
                        if (protocol == ControlProtocol.Durable2PC)
                        {
                            event2 = new MsgRegisterDurableResponseEvent(coordinator, response, proxy);
                        }
                        else
                        {
                            VolatileCoordinatorEnlistment asyncState = (VolatileCoordinatorEnlistment) ar.AsyncState;
                            event2 = new MsgRegisterVolatileResponseEvent(asyncState, response, proxy);
                        }
                    }
                    finally
                    {
                        proxy.Release();
                    }
                }
            }
            catch (WsatFaultException exception)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception, TraceEventType.Warning);
                event2 = new MsgRegistrationCoordinatorFaultEvent(coordinator, protocol, exception.Fault);
                if (RegistrationCoordinatorFaultedRecord.ShouldTrace)
                {
                    RegistrationCoordinatorFaultedRecord.Trace(coordinator.EnlistmentId, coordinator.SuperiorContext, protocol, exception.Fault);
                }
            }
            catch (WsatMessagingException exception2)
            {
                Microsoft.Transactions.Bridge.DiagnosticUtility.ExceptionUtility.TraceHandledException(exception2, TraceEventType.Warning);
                this.state.Perf.MessageSendFailureCountPerInterval.Increment();
                if (RegistrationCoordinatorFailedRecord.ShouldTrace)
                {
                    RegistrationCoordinatorFailedRecord.Trace(coordinator.EnlistmentId, coordinator.SuperiorContext, protocol, exception2);
                }
                DebugTrace.TraceSendFailure(coordinator.EnlistmentId, exception2);
                event2 = new MsgRegistrationCoordinatorSendFailureEvent(coordinator);
            }
            coordinator.StateMachine.Enqueue(event2);
        }

        private void OnSendVolatileRegisterComplete(IAsyncResult ar)
        {
            if (!ar.CompletedSynchronously)
            {
                VolatileCoordinatorEnlistment asyncState = (VolatileCoordinatorEnlistment) ar.AsyncState;
                this.OnSendRegisterComplete(asyncState.Coordinator, ControlProtocol.Volatile2PC, ar);
            }
        }

        public void SendDurableRegister(CoordinatorEnlistment coordinator)
        {
            this.SendRegister(coordinator, ControlProtocol.Durable2PC, coordinator.ParticipantService, this.sendDurableRegisterComplete, coordinator);
        }

        private void SendRegister(CoordinatorEnlistment coordinator, ControlProtocol protocol, EndpointAddress protocolService, AsyncCallback callback, object callbackState)
        {
            Register register = new Register(this.state.ProtocolVersion) {
                Protocol = protocol,
                Loopback = this.state.ProcessId,
                ParticipantProtocolService = protocolService,
                SupportingToken = coordinator.SuperiorIssuedToken
            };
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, coordinator.EnlistmentId, "Sending Register for {0} to {1}", protocol, Ports.TryGetAddress(coordinator.RegistrationProxy));
            }
            IAsyncResult ar = coordinator.RegistrationProxy.BeginSendRegister(ref register, callback, callbackState);
            if (ar.CompletedSynchronously)
            {
                this.OnSendRegisterComplete(coordinator, protocol, ar);
            }
        }

        public void SendVolatileRegister(VolatileCoordinatorEnlistment volatileCoordinator)
        {
            this.SendRegister(volatileCoordinator.Coordinator, ControlProtocol.Volatile2PC, volatileCoordinator.ParticipantService, this.sendVolatileRegisterComplete, volatileCoordinator);
        }
    }
}

