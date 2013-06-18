namespace Microsoft.Transactions.Wsat.StateMachines
{
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;
    using System.ServiceModel;

    internal class MsgRegisterCompletionEvent : CompletionEvent
    {
        private CompletionParticipantProxy proxy;
        private Register register;
        private RequestAsyncResult result;

        public MsgRegisterCompletionEvent(CompletionEnlistment completion, ref Register register, RequestAsyncResult result, CompletionParticipantProxy proxy) : base(completion)
        {
            this.register = register;
            proxy.AddRef();
            this.proxy = proxy;
            this.result = result;
        }

        public override void Execute(StateMachine stateMachine)
        {
            try
            {
                if (DebugTrace.Info)
                {
                    base.state.DebugTraceSink.OnEvent(this);
                }
                stateMachine.State.OnEvent(this);
            }
            finally
            {
                this.proxy.Release();
            }
        }

        public EndpointAddress ParticipantService
        {
            get
            {
                return this.register.ParticipantProtocolService;
            }
        }

        public CompletionParticipantProxy Proxy
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.proxy;
            }
        }

        public RequestAsyncResult Result
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.result;
            }
        }
    }
}

