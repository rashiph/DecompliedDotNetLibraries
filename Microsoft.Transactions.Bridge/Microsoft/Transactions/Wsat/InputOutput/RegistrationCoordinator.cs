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
    using System.ServiceModel.Channels;
    using System.ServiceModel.Transactions;

    internal class RegistrationCoordinator : IRegistrationCoordinator
    {
        private ProtocolState state;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RegistrationCoordinator(ProtocolState state)
        {
            this.state = state;
        }

        public void Register(Message message, Microsoft.Transactions.Wsat.Messaging.RequestAsyncResult result)
        {
            Microsoft.Transactions.Wsat.Messaging.Register register = new Microsoft.Transactions.Wsat.Messaging.Register(message, this.state.ProtocolVersion);
            EndpointAddress participantProtocolService = register.ParticipantProtocolService;
            WsatRegistrationHeader header = WsatRegistrationHeader.ReadFrom(message);
            if (header == null)
            {
                if (DebugTrace.Warning)
                {
                    DebugTrace.Trace(TraceLevel.Warning, "Rejecting Register message with no registration header");
                }
                this.SendFault(result, this.state.Faults.InvalidParameters);
                return;
            }
            switch (register.Protocol)
            {
                case ControlProtocol.Completion:
                {
                    CompletionEnlistment completion = this.state.Lookup.FindEnlistment(header.TransactionId) as CompletionEnlistment;
                    if (completion != null)
                    {
                        CompletionParticipantProxy proxy = this.state.TryCreateCompletionParticipantProxy(participantProtocolService);
                        if (proxy == null)
                        {
                            if (DebugTrace.Warning)
                            {
                                DebugTrace.Trace(TraceLevel.Warning, "Rejecting Register message for completion on no completion enlistment");
                            }
                            this.SendFault(result, this.state.Faults.InvalidParameters);
                            return;
                        }
                        try
                        {
                            completion.StateMachine.Enqueue(new MsgRegisterCompletionEvent(completion, ref register, result, proxy));
                            return;
                        }
                        finally
                        {
                            proxy.Release();
                        }
                        break;
                    }
                    if (DebugTrace.Warning)
                    {
                        DebugTrace.Trace(TraceLevel.Warning, "Rejecting uncorrelated Register message for completion");
                    }
                    this.SendFault(result, this.state.Faults.UnknownCompletionEnlistment);
                    return;
                }
                case ControlProtocol.Volatile2PC:
                case ControlProtocol.Durable2PC:
                    break;

                default:
                    goto Label_0222;
            }
            if (!this.state.TransactionManager.Settings.NetworkOutboundAccess)
            {
                if (DebugTrace.Warning)
                {
                    DebugTrace.Trace(TraceLevel.Warning, "Rejecting Register message because outbound transactions are disabled");
                }
                this.SendFault(result, this.state.Faults.ParticipantRegistrationNetAccessDisabled);
                return;
            }
            if (register.Loopback == this.state.ProcessId)
            {
                if (DebugTrace.Warning)
                {
                    DebugTrace.Trace(TraceLevel.Warning, "Rejecting recursive Register message from self");
                }
                this.SendFault(result, this.state.Faults.ParticipantRegistrationLoopback);
                return;
            }
            TwoPhaseCommitParticipantProxy proxy2 = this.state.TryCreateTwoPhaseCommitParticipantProxy(participantProtocolService);
            if (proxy2 == null)
            {
                if (DebugTrace.Warning)
                {
                    DebugTrace.Trace(TraceLevel.Warning, "Rejecting Register message because 2PC proxy could not be created");
                }
                this.SendFault(result, this.state.Faults.InvalidParameters);
                return;
            }
            try
            {
                ParticipantEnlistment participant = new ParticipantEnlistment(this.state, header, register.Protocol, proxy2);
                this.state.TransactionManagerSend.Register(participant, new MsgRegisterEvent(participant, ref register, result));
                return;
            }
            finally
            {
                proxy2.Release();
            }
        Label_0222:
            Microsoft.Transactions.Bridge.DiagnosticUtility.FailFast("Registration protocol should have been validated");
        }

        public void SendFault(Microsoft.Transactions.Wsat.Messaging.RequestAsyncResult result, Fault fault)
        {
            if (DebugTrace.Warning)
            {
                DebugTrace.Trace(TraceLevel.Warning, "Sending {0} fault to registration participant", fault.Code.Name);
            }
            this.state.Perf.FaultsSentCountPerInterval.Increment();
            RegistrationProxy.SendFaultResponse(result, fault);
        }

        public void SendRegisterResponse(Microsoft.Transactions.Wsat.Protocol.TransactionEnlistment enlistment, Microsoft.Transactions.Wsat.Messaging.RequestAsyncResult result, ControlProtocol protocol, EndpointAddress coordinatorService)
        {
            RegisterResponse response = new RegisterResponse(this.state.ProtocolVersion) {
                CoordinatorProtocolService = coordinatorService
            };
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, enlistment.EnlistmentId, "Sending RegisterResponse for {0}", protocol);
            }
            RegistrationProxy.SendRegisterResponse(result, ref response);
        }
    }
}

