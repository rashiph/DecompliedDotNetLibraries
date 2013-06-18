namespace Microsoft.Transactions.Wsat.Recovery
{
    using System;
    using System.Runtime;
    using System.ServiceModel;

    internal class LogEntry
    {
        private EndpointAddress endpoint;
        private Guid localEnlistmentId;
        private Guid localTransactionId;
        private string remoteTransactionId;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public LogEntry(string remoteTransactionId, Guid localTransactionId, Guid localEnlistmentId) : this(remoteTransactionId, localTransactionId, localEnlistmentId, null)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public LogEntry(string remoteTransactionId, Guid localTransactionId, Guid localEnlistmentId, EndpointAddress endpoint)
        {
            this.remoteTransactionId = remoteTransactionId;
            this.localTransactionId = localTransactionId;
            this.localEnlistmentId = localEnlistmentId;
            this.endpoint = endpoint;
        }

        public EndpointAddress Endpoint
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.endpoint;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.endpoint = value;
            }
        }

        public Guid LocalEnlistmentId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.localEnlistmentId;
            }
        }

        public Guid LocalTransactionId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.localTransactionId;
            }
        }

        public string RemoteTransactionId
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.remoteTransactionId;
            }
        }
    }
}

