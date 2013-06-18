namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.Runtime;

    internal class AtomicTransactionStrings11 : AtomicTransactionStrings
    {
        private static AtomicTransactionStrings instance = new AtomicTransactionStrings11();

        public override string AbortedAction
        {
            get
            {
                return "http://docs.oasis-open.org/ws-tx/wsat/2006/06/Aborted";
            }
        }

        public override string CommitAction
        {
            get
            {
                return "http://docs.oasis-open.org/ws-tx/wsat/2006/06/Commit";
            }
        }

        public override string CommittedAction
        {
            get
            {
                return "http://docs.oasis-open.org/ws-tx/wsat/2006/06/Committed";
            }
        }

        public override string CompletionUri
        {
            get
            {
                return "http://docs.oasis-open.org/ws-tx/wsat/2006/06/Completion";
            }
        }

        public override string Durable2PCUri
        {
            get
            {
                return "http://docs.oasis-open.org/ws-tx/wsat/2006/06/Durable2PC";
            }
        }

        public override string FaultAction
        {
            get
            {
                return "http://docs.oasis-open.org/ws-tx/wsat/2006/06/fault";
            }
        }

        public static AtomicTransactionStrings Instance
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return instance;
            }
        }

        public override string Namespace
        {
            get
            {
                return "http://docs.oasis-open.org/ws-tx/wsat/2006/06";
            }
        }

        public override string PrepareAction
        {
            get
            {
                return "http://docs.oasis-open.org/ws-tx/wsat/2006/06/Prepare";
            }
        }

        public override string PreparedAction
        {
            get
            {
                return "http://docs.oasis-open.org/ws-tx/wsat/2006/06/Prepared";
            }
        }

        public override string ReadOnlyAction
        {
            get
            {
                return "http://docs.oasis-open.org/ws-tx/wsat/2006/06/ReadOnly";
            }
        }

        public override string ReplayAction
        {
            get
            {
                return "http://docs.oasis-open.org/ws-tx/wsat/2006/06/Replay";
            }
        }

        public override string RollbackAction
        {
            get
            {
                return "http://docs.oasis-open.org/ws-tx/wsat/2006/06/Rollback";
            }
        }

        public override string Volatile2PCUri
        {
            get
            {
                return "http://docs.oasis-open.org/ws-tx/wsat/2006/06/Volatile2PC";
            }
        }
    }
}

