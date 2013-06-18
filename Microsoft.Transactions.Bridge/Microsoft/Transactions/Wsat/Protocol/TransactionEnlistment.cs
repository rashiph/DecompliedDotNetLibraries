namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Bridge;
    using Microsoft.Transactions.Wsat.Messaging;
    using Microsoft.Transactions.Wsat.StateMachines;
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security;
    using System.ServiceModel.Transactions;
    using System.Transactions;

    internal abstract class TransactionEnlistment
    {
        protected Microsoft.Transactions.Bridge.Enlistment enlistment;
        protected Guid enlistmentId;
        protected ProtocolProviderCallback lastCallback;
        protected object lastCallbackState;
        protected TransactionContextManager ourContextManager;
        private bool removeEnlistmentFromLookupTable;
        private int retries;
        protected ProtocolState state;
        protected Microsoft.Transactions.Wsat.StateMachines.StateMachine stateMachine;

        protected TransactionEnlistment(ProtocolState state) : this(state, Guid.NewGuid())
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected TransactionEnlistment(ProtocolState state, Guid enlistmentId)
        {
            this.state = state;
            this.enlistmentId = enlistmentId;
        }

        protected void ActivateTransactionContextManager(TransactionContextManager contextManager)
        {
            TransactionContext context = this.CreateTransactionContext();
            contextManager.StateMachine.Enqueue(new TransactionContextCreatedEvent(contextManager, context));
        }

        protected void AddToLookupTable()
        {
            this.state.Lookup.AddEnlistment(this);
            this.removeEnlistmentFromLookupTable = true;
        }

        private TimeSpan CalculateTimeout(uint expires, bool expiresPresent)
        {
            if (!expiresPresent)
            {
                return this.state.Config.DefaultTimeout;
            }
            if (expires == 0)
            {
                return this.state.Config.MaxTimeout;
            }
            TimeSpan maxTimeout = TimeSpan.FromMilliseconds((double) expires);
            if ((this.state.Config.MaxTimeout != TimeSpan.Zero) && (this.state.Config.MaxTimeout < maxTimeout))
            {
                maxTimeout = this.state.Config.MaxTimeout;
            }
            return maxTimeout;
        }

        public Microsoft.Transactions.Bridge.EnlistmentOptions CreateEnlistmentOptions(uint expires, bool expiresPresent, IsolationLevel isoLevel, IsolationFlags isoFlags, string description)
        {
            Microsoft.Transactions.Bridge.EnlistmentOptions options = new Microsoft.Transactions.Bridge.EnlistmentOptions {
                Expires = this.CalculateTimeout(expires, expiresPresent),
                IsoLevel = isoLevel
            };
            if (options.IsoLevel == IsolationLevel.Unspecified)
            {
                options.IsoLevel = IsolationLevel.Serializable;
            }
            options.IsolationFlags = isoFlags;
            options.Description = description;
            return options;
        }

        private TransactionContext CreateTransactionContext()
        {
            Microsoft.Transactions.Bridge.EnlistmentOptions enlistmentOptions = this.enlistment.EnlistmentOptions;
            string remoteTransactionId = this.enlistment.RemoteTransactionId;
            Guid localTransactionId = this.enlistment.LocalTransactionId;
            CoordinationContext context = new CoordinationContext(this.state.ProtocolVersion) {
                Expires = (enlistmentOptions.Expires == TimeSpan.MaxValue) ? uint.MaxValue : ((uint) ((int) enlistmentOptions.Expires.TotalMilliseconds)),
                Identifier = remoteTransactionId,
                LocalTransactionId = localTransactionId,
                IsolationLevel = enlistmentOptions.IsoLevel,
                IsolationFlags = enlistmentOptions.IsolationFlags,
                Description = enlistmentOptions.Description
            };
            string contextId = CoordinationContext.IsNativeIdentifier(remoteTransactionId, localTransactionId) ? null : remoteTransactionId;
            string sctId = null;
            RequestSecurityTokenResponse issuedToken = null;
            if (this.state.Config.PortConfiguration.SupportingTokensEnabled)
            {
                CoordinationServiceSecurity.CreateIssuedToken(localTransactionId, remoteTransactionId, this.state.ProtocolVersion, out issuedToken, out sctId);
            }
            AddressHeader refParam = new WsatRegistrationHeader(localTransactionId, contextId, sctId);
            context.RegistrationService = this.state.RegistrationCoordinatorListener.CreateEndpointReference(refParam);
            return new TransactionContext(context, issuedToken);
        }

        public void DeliverCallback(Status status)
        {
            this.lastCallback(this.enlistment, status, this.lastCallbackState);
            this.lastCallback = null;
            this.lastCallbackState = null;
        }

        protected void FindAndActivateTransactionContextManager()
        {
            string remoteTransactionId = this.enlistment.RemoteTransactionId;
            TransactionContextManager contextManager = this.state.Lookup.FindTransactionContextManager(remoteTransactionId);
            this.ActivateTransactionContextManager(contextManager);
        }

        public virtual void OnStateMachineComplete()
        {
            if (this.removeEnlistmentFromLookupTable)
            {
                this.state.Lookup.RemoveEnlistment(this);
                this.removeEnlistmentFromLookupTable = false;
            }
            if (this.ourContextManager != null)
            {
                this.ourContextManager.StateMachine.Enqueue(new TransactionContextTransactionDoneEvent(this.ourContextManager));
            }
        }

        public void SetCallback(ProtocolProviderCallback callback, object callbackState)
        {
            this.lastCallback = callback;
            this.lastCallbackState = callbackState;
        }

        public override string ToString()
        {
            return this.enlistmentId.ToString();
        }

        protected void TraceTransferEvent()
        {
            if (DiagnosticUtility.ShouldUseActivity)
            {
                using (Activity.CreateActivity(this.enlistment.LocalTransactionId))
                {
                    DiagnosticUtility.DiagnosticTrace.TraceTransfer(this.enlistmentId);
                }
            }
        }

        protected void VerifyAndTraceEnlistmentOptions()
        {
            Microsoft.Transactions.Bridge.EnlistmentOptions enlistmentOptions = this.enlistment.EnlistmentOptions;
            if (enlistmentOptions == null)
            {
                DiagnosticUtility.FailFast("Need EnlistmentOptions for context");
            }
            if (enlistmentOptions.IsoLevel == IsolationLevel.Unspecified)
            {
                DiagnosticUtility.FailFast("Need IsolationLevel for context");
            }
            if (this.enlistment.LocalTransactionId == Guid.Empty)
            {
                DiagnosticUtility.FailFast("Need LocalTransactionId for context");
            }
            if (string.IsNullOrEmpty(this.enlistment.RemoteTransactionId))
            {
                DiagnosticUtility.FailFast("Need RemoteTransactionId for context");
            }
            if (DebugTrace.Info)
            {
                DebugTrace.TxTrace(TraceLevel.Info, this.enlistmentId, "Local transactionId is {0}", this.enlistment.LocalTransactionId);
                DebugTrace.TxTrace(TraceLevel.Info, this.enlistmentId, "Remote transactionId is {0}", this.enlistment.RemoteTransactionId);
                DebugTrace.TxTrace(TraceLevel.Info, this.enlistmentId, "Transaction timeout is {0} seconds", enlistmentOptions.Expires.TotalSeconds);
                DebugTrace.TxTrace(TraceLevel.Info, this.enlistmentId, "Transaction isolation level is {0}", enlistmentOptions.IsoLevel);
            }
        }

        public TransactionContextManager ContextManager
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.ourContextManager;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.ourContextManager = value;
            }
        }

        public Microsoft.Transactions.Bridge.Enlistment Enlistment
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.enlistment;
            }
        }

        public Guid EnlistmentId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.enlistmentId;
            }
        }

        public int Retries
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.retries;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.retries = value;
            }
        }

        public ProtocolState State
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.state;
            }
        }

        public Microsoft.Transactions.Wsat.StateMachines.StateMachine StateMachine
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.stateMachine;
            }
        }
    }
}

