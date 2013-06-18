namespace Microsoft.Transactions.Wsat.Protocol
{
    using Microsoft.Transactions.Bridge;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Diagnostics;

    internal static class TxTraceUtility
    {
        private static Dictionary<int, string> traceCodes;

        static TxTraceUtility()
        {
            Dictionary<int, string> dictionary = new Dictionary<int, string>(50);
            dictionary.Add(0xb0001, "CreateTransactionFailure");
            dictionary.Add(0xb0002, "EnlistTransactionFailure");
            dictionary.Add(0xb0003, "RegisterParticipantFailure");
            dictionary.Add(0xb0004, "VolatileOutcomeTimeout");
            dictionary.Add(0xb0005, "DurableParticipantReplayWhilePreparing");
            dictionary.Add(0xb0006, "RegistrationCoordinatorFaulted");
            dictionary.Add(0xb0007, "RegistrationCoordinatorFailed");
            dictionary.Add(0xb0008, "RegistrationCoordinatorResponseInvalidMetadata");
            dictionary.Add(0xb0009, "RecoveredCoordinatorInvalidMetadata");
            dictionary.Add(0xb000a, "RecoveredParticipantInvalidMetadata");
            dictionary.Add(0xb000b, "EnlistTransaction");
            dictionary.Add(0xb000c, "RegisterCoordinator");
            dictionary.Add(0xb000d, "RegisterParticipant");
            dictionary.Add(0xb000e, "ProtocolInitialized");
            dictionary.Add(0xb000f, "ProtocolStarted");
            dictionary.Add(0xb0020, "PrepareMessageRetry");
            dictionary.Add(0xb0021, "CommitMessageRetry");
            dictionary.Add(0xb0022, "PreparedMessageRetry");
            dictionary.Add(0xb0023, "ReplayMessageRetry");
            dictionary.Add(0xb0024, "ParticipantRecovered");
            dictionary.Add(0xb0025, "CoordinatorRecovered");
            dictionary.Add(0xb0026, "EnlistmentIdentityCheckFailed");
            dictionary.Add(0xb0027, "ParticipantStateMachineFinished");
            dictionary.Add(0xb0028, "CoordinatorStateMachineFinished");
            dictionary.Add(0xb0029, "VolatileParticipantInDoubt");
            dictionary.Add(0xb002a, "VolatileParticipantInDoubt11");
            dictionary.Add(0xb002b, "RegisterParticipantFailure11");
            dictionary.Add(0xb002c, "RegisterCoordinator11");
            dictionary.Add(0xb002d, "RegisterParticipant11");
            dictionary.Add(0xb002e, "ParticipantRecovered11");
            dictionary.Add(0xb002f, "CoordinatorRecovered11");
            dictionary.Add(0xb0030, "RecoveredCoordinatorInvalidMetadata11");
            dictionary.Add(0xb0031, "RecoveredParticipantInvalidMetadata11");
            dictionary.Add(0xb0032, "RegistrationCoordinatorResponseInvalidMetadata11");
            traceCodes = dictionary;
        }

        internal static void Trace(TraceEventType severity, int traceCode, string traceDescription)
        {
            Trace(severity, traceCode, traceDescription, null, null, Guid.Empty, null);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void Trace(TraceEventType severity, int traceCode, string traceDescription, TraceRecord extendedData, object source, Guid activityId, Exception exception)
        {
            if (DiagnosticUtility.ShouldTrace(severity))
            {
                string msdnTraceCode = System.ServiceModel.Diagnostics.DiagnosticTrace.GenerateMsdnTraceCode("Microsoft.Transactions.TransactionBridge", traceCodes[traceCode]);
                DiagnosticUtility.DiagnosticTrace.TraceEvent(severity, traceCode, msdnTraceCode, traceDescription, extendedData, exception, activityId, source);
            }
        }
    }
}

