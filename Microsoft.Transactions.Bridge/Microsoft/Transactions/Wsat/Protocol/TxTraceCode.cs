namespace Microsoft.Transactions.Wsat.Protocol
{
    using System;

    internal static class TxTraceCode
    {
        public const int CommitMessageRetry = 0xb0021;
        public const int CoordinatorRecovered = 0xb0025;
        public const int CoordinatorRecovered11 = 0xb002f;
        public const int CoordinatorStateMachineFinished = 0xb0028;
        public const int CreateTransactionFailure = 0xb0001;
        public const int DurableParticipantReplayWhilePreparing = 0xb0005;
        public const int EnlistmentIdentityCheckFailed = 0xb0026;
        public const int EnlistTransaction = 0xb000b;
        public const int EnlistTransactionFailure = 0xb0002;
        public const int ParticipantRecovered = 0xb0024;
        public const int ParticipantRecovered11 = 0xb002e;
        public const int ParticipantStateMachineFinished = 0xb0027;
        public const int PreparedMessageRetry = 0xb0022;
        public const int PrepareMessageRetry = 0xb0020;
        public const int ProtocolInitialized = 0xb000e;
        public const int ProtocolStarted = 0xb000f;
        public const int RecoveredCoordinatorInvalidMetadata = 0xb0009;
        public const int RecoveredCoordinatorInvalidMetadata11 = 0xb0030;
        public const int RecoveredParticipantInvalidMetadata = 0xb000a;
        public const int RecoveredParticipantInvalidMetadata11 = 0xb0031;
        public const int RegisterCoordinator = 0xb000c;
        public const int RegisterCoordinator11 = 0xb002c;
        public const int RegisterParticipant = 0xb000d;
        public const int RegisterParticipant11 = 0xb002d;
        public const int RegisterParticipantFailure = 0xb0003;
        public const int RegisterParticipantFailure11 = 0xb002b;
        public const int RegistrationCoordinatorFailed = 0xb0007;
        public const int RegistrationCoordinatorFaulted = 0xb0006;
        public const int RegistrationCoordinatorResponseInvalidMetadata = 0xb0008;
        public const int RegistrationCoordinatorResponseInvalidMetadata11 = 0xb0032;
        public const int ReplayMessageRetry = 0xb0023;
        public const int TransactionBridge = 0xb0000;
        public const int VolatileOutcomeTimeout = 0xb0004;
        public const int VolatileParticipantInDoubt = 0xb0029;
        public const int VolatileParticipantInDoubt11 = 0xb002a;
    }
}

