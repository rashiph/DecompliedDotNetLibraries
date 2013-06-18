namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;
    using System.Runtime;

    internal class PerformanceCounterHolder
    {
        private PerformanceCounterWrapper averageParticipantCommitResponseTime = new PerformanceCounterWrapper("Average participant commit response time");
        private PerformanceCounterWrapper averageParticipantCommitResponseTimeBase = new PerformanceCounterWrapper("Average participant commit response time Base");
        private PerformanceCounterWrapper averageParticipantPrepareResponseTime = new PerformanceCounterWrapper("Average participant prepare response time");
        private PerformanceCounterWrapper averageParticipantPrepareResponseTimeBase = new PerformanceCounterWrapper("Average participant prepare response time Base");
        private PerformanceCounterWrapper commitRetryCountPerInterval = new PerformanceCounterWrapper("Commit retry count/sec");
        private PerformanceCounterWrapper faultsReceivedCountPerInterval = new PerformanceCounterWrapper("Faults received count/sec");
        private PerformanceCounterWrapper faultsSentCountPerInterval = new PerformanceCounterWrapper("Faults sent count/sec");
        private PerformanceCounterWrapper messageSendFailureCountPerInterval = new PerformanceCounterWrapper("Message send failures/sec");
        private PerformanceCounterWrapper preparedRetryCountPerInterval = new PerformanceCounterWrapper("Prepared retry count/sec");
        private PerformanceCounterWrapper prepareRetryCountPerInterval = new PerformanceCounterWrapper("Prepare retry count/sec");
        private PerformanceCounterWrapper replayRetryCountPerInterval = new PerformanceCounterWrapper("Replay retry count/sec");

        public PerformanceCounterWrapper AverageParticipantCommitResponseTime
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.averageParticipantCommitResponseTime;
            }
        }

        public PerformanceCounterWrapper AverageParticipantCommitResponseTimeBase
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.averageParticipantCommitResponseTimeBase;
            }
        }

        public PerformanceCounterWrapper AverageParticipantPrepareResponseTime
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.averageParticipantPrepareResponseTime;
            }
        }

        public PerformanceCounterWrapper AverageParticipantPrepareResponseTimeBase
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.averageParticipantPrepareResponseTimeBase;
            }
        }

        public PerformanceCounterWrapper CommitRetryCountPerInterval
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.commitRetryCountPerInterval;
            }
        }

        public PerformanceCounterWrapper FaultsReceivedCountPerInterval
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.faultsReceivedCountPerInterval;
            }
        }

        public PerformanceCounterWrapper FaultsSentCountPerInterval
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.faultsSentCountPerInterval;
            }
        }

        public PerformanceCounterWrapper MessageSendFailureCountPerInterval
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.messageSendFailureCountPerInterval;
            }
        }

        public PerformanceCounterWrapper PreparedRetryCountPerInterval
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.preparedRetryCountPerInterval;
            }
        }

        public PerformanceCounterWrapper PrepareRetryCountPerInterval
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.prepareRetryCountPerInterval;
            }
        }

        public PerformanceCounterWrapper ReplayRetryCountPerInterval
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.replayRetryCountPerInterval;
            }
        }
    }
}

