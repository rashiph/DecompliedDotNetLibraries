namespace Microsoft.Transactions.Wsat.Messaging
{
    using System;
    using System.Runtime;

    internal class AtomicTransactionStrings10 : AtomicTransactionStrings
    {
        private static AtomicTransactionStrings instance = new AtomicTransactionStrings10();

        public override string AbortedAction
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2004/10/wsat/Aborted";
            }
        }

        public override string CommitAction
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2004/10/wsat/Commit";
            }
        }

        public override string CommittedAction
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2004/10/wsat/Committed";
            }
        }

        public override string CompletionUri
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2004/10/wsat/Completion";
            }
        }

        public override string Durable2PCUri
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2004/10/wsat/Durable2PC";
            }
        }

        public override string FaultAction
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2004/10/wsat/fault";
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
                return "http://schemas.xmlsoap.org/ws/2004/10/wsat";
            }
        }

        public override string PrepareAction
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2004/10/wsat/Prepare";
            }
        }

        public override string PreparedAction
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2004/10/wsat/Prepared";
            }
        }

        public override string ReadOnlyAction
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2004/10/wsat/ReadOnly";
            }
        }

        public override string ReplayAction
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2004/10/wsat/Replay";
            }
        }

        public override string RollbackAction
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2004/10/wsat/Rollback";
            }
        }

        public override string Volatile2PCUri
        {
            get
            {
                return "http://schemas.xmlsoap.org/ws/2004/10/wsat/Volatile2PC";
            }
        }
    }
}

