namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;

    internal static class PerformanceCounterStrings
    {
        internal static class MSDTC_BRIDGE
        {
            internal const string AverageParticipantCommitResponseTime = "Average participant commit response time";
            internal const string AverageParticipantCommitResponseTimeBase = "Average participant commit response time Base";
            internal const string AverageParticipantPrepareResponseTime = "Average participant prepare response time";
            internal const string AverageParticipantPrepareResponseTimeBase = "Average participant prepare response time Base";
            internal const string CommitRetryCountPerInterval = "Commit retry count/sec";
            internal const string FaultsReceivedCountPerInterval = "Faults received count/sec";
            internal const string FaultsSentCountPerInterval = "Faults sent count/sec";
            internal const string MessageSendFailureCountPerInterval = "Message send failures/sec";
            internal const string PreparedRetryCountPerInterval = "Prepared retry count/sec";
            internal const string PrepareRetryCountPerInterval = "Prepare retry count/sec";
            internal const string ReplayRetryCountPerInterval = "Replay retry count/sec";
            internal const string TransactionBridgeV1PerfCounters = "MSDTC Bridge 4.0.0.0";
        }
    }
}

