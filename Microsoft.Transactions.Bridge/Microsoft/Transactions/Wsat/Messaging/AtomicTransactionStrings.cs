namespace Microsoft.Transactions.Wsat.Messaging
{
    using Microsoft.Transactions.Wsat.Protocol;
    using System;
    using System.Runtime;

    internal abstract class AtomicTransactionStrings
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected AtomicTransactionStrings()
        {
        }

        public static AtomicTransactionStrings Version(ProtocolVersion protocolVersion)
        {
            ProtocolVersionHelper.AssertProtocolVersion(protocolVersion, typeof(AtomicTransactionStrings), "V");
            switch (protocolVersion)
            {
                case ProtocolVersion.Version10:
                    return AtomicTransactionStrings10.Instance;

                case ProtocolVersion.Version11:
                    return AtomicTransactionStrings11.Instance;
            }
            return null;
        }

        public string Aborted
        {
            get
            {
                return "Aborted";
            }
        }

        public abstract string AbortedAction { get; }

        public string Commit
        {
            get
            {
                return "Commit";
            }
        }

        public abstract string CommitAction { get; }

        public string Committed
        {
            get
            {
                return "Committed";
            }
        }

        public abstract string CommittedAction { get; }

        public string CompletionCoordinatorPortType
        {
            get
            {
                return "CompletionCoordinatorPortType";
            }
        }

        public string CompletionParticipantPortType
        {
            get
            {
                return "CompletionParticipantPortType";
            }
        }

        public abstract string CompletionUri { get; }

        public string CoordinatorPortType
        {
            get
            {
                return "CoordinatorPortType";
            }
        }

        public abstract string Durable2PCUri { get; }

        public abstract string FaultAction { get; }

        public string InconsistentInternalState
        {
            get
            {
                return "InconsistentInternalState";
            }
        }

        public abstract string Namespace { get; }

        public string ParticipantPortType
        {
            get
            {
                return "ParticipantPortType";
            }
        }

        public string Prefix
        {
            get
            {
                return "wsat";
            }
        }

        public string Prepare
        {
            get
            {
                return "Prepare";
            }
        }

        public abstract string PrepareAction { get; }

        public string Prepared
        {
            get
            {
                return "Prepared";
            }
        }

        public abstract string PreparedAction { get; }

        public string ReadOnly
        {
            get
            {
                return "ReadOnly";
            }
        }

        public abstract string ReadOnlyAction { get; }

        public string Replay
        {
            get
            {
                return "Replay";
            }
        }

        public abstract string ReplayAction { get; }

        public string Rollback
        {
            get
            {
                return "Rollback";
            }
        }

        public abstract string RollbackAction { get; }

        public string UnknownTransaction
        {
            get
            {
                return "UnknownTransaction";
            }
        }

        public abstract string Volatile2PCUri { get; }
    }
}

