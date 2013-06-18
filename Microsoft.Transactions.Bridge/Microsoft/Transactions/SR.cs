namespace Microsoft.Transactions
{
    using System;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class SR
    {
        internal const string AlreadyRegisteredReason = "AlreadyRegisteredReason";
        internal const string AsyncResultAlreadyEnded = "AsyncResultAlreadyEnded";
        internal const string CannotCreateContextReason = "CannotCreateContextReason";
        internal const string CannotRegisterParticipant = "CannotRegisterParticipant";
        internal const string ClusterEnumFailed = "ClusterEnumFailed";
        internal const string ClusterOpenEnumFailed = "ClusterOpenEnumFailed";
        internal const string ClusterRegOpenKeyFailed = "ClusterRegOpenKeyFailed";
        internal const string ClusterRegQueryValueFailed = "ClusterRegQueryValueFailed";
        internal const string ClusterRegQueryValueInvalidResults = "ClusterRegQueryValueInvalidResults";
        internal const string ClusterResourceControlFailed = "ClusterResourceControlFailed";
        internal const string ClusterResourceControlInvalidResults = "ClusterResourceControlInvalidResults";
        internal const string ClusterResourceNotFound = "ClusterResourceNotFound";
        internal const string CommitMessageRetry = "CommitMessageRetry";
        internal const string ConfigurationManagerGetSectionFailed = "ConfigurationManagerGetSectionFailed";
        internal const string ContextRefusedReason = "ContextRefusedReason";
        internal const string CoordinatorRecovered = "CoordinatorRecovered";
        internal const string CoordinatorRegistrationFailedReason = "CoordinatorRegistrationFailedReason";
        internal const string CoordinatorRegistrationFaultedReason = "CoordinatorRegistrationFaultedReason";
        internal const string CoordinatorRegistrationFaultedUnknownReason = "CoordinatorRegistrationFaultedUnknownReason";
        internal const string CoordinatorStateMachineFinished = "CoordinatorStateMachineFinished";
        internal const string CouldNotLoadTM = "CouldNotLoadTM";
        internal const string CouldNotQueueStartUserWorkItem = "CouldNotQueueStartUserWorkItem";
        internal const string CreateTransactionFailure = "CreateTransactionFailure";
        internal const string DeserializationDataCorrupt = "DeserializationDataCorrupt";
        internal const string DeserializationLogEntryTooBig = "DeserializationLogEntryTooBig";
        internal const string DisabledReason = "DisabledReason";
        internal const string DurableParticipantReplayWhilePreparing = "DurableParticipantReplayWhilePreparing";
        internal const string EndpointReferenceSerializationFailed = "EndpointReferenceSerializationFailed";
        internal const string EnlistmentIdentityCheckFailed = "EnlistmentIdentityCheckFailed";
        internal const string EnlistTransaction = "EnlistTransaction";
        internal const string EnlistTransactionFailure = "EnlistTransactionFailure";
        internal const string FailedToCreateChannelFactory = "FailedToCreateChannelFactory";
        internal const string FailedToOpenChannelFactory = "FailedToOpenChannelFactory";
        internal const string GetClusterResourceKeyFailed = "GetClusterResourceKeyFailed";
        internal const string GetClusterResourceNetworkNameFailed = "GetClusterResourceNetworkNameFailed";
        internal const string InconsistentInternalStateReason = "InconsistentInternalStateReason";
        internal const string InvalidAsyncResult = "InvalidAsyncResult";
        internal const string InvalidCoordinationContext = "InvalidCoordinationContext";
        internal const string InvalidEnlistmentHeader = "InvalidEnlistmentHeader";
        internal const string InvalidMessageAction = "InvalidMessageAction";
        internal const string InvalidMessageBody = "InvalidMessageBody";
        internal const string InvalidParametersReason = "InvalidParametersReason";
        internal const string InvalidPolicyReason = "InvalidPolicyReason";
        internal const string InvalidProtocolReason = "InvalidProtocolReason";
        internal const string InvalidSchemeWithTrustIdentity = "InvalidSchemeWithTrustIdentity";
        internal const string InvalidStateReason = "InvalidStateReason";
        internal const string InvalidTrustIdentity = "InvalidTrustIdentity";
        internal const string InvalidTrustIdentityType = "InvalidTrustIdentityType";
        internal const string IssuedTokenIdentifierMismatch = "IssuedTokenIdentifierMismatch";
        internal const string ListenerCannotBeStarted = "ListenerCannotBeStarted";
        private static Microsoft.Transactions.SR loader;
        internal const string OpenClusterFailed = "OpenClusterFailed";
        internal const string OpenClusterResourceFailed = "OpenClusterResourceFailed";
        internal const string ParticipantRecovered = "ParticipantRecovered";
        internal const string ParticipantStateMachineFinished = "ParticipantStateMachineFinished";
        internal const string PerformanceCounterSchema = "PerformanceCounterSchema";
        internal const string PplCreateSubordinateEnlistmentFailed = "PplCreateSubordinateEnlistmentFailed";
        internal const string PplCreateSuperiorEnlistmentFailed = "PplCreateSuperiorEnlistmentFailed";
        internal const string PplCreateTransactionFailed = "PplCreateTransactionFailed";
        internal const string PreparedMessageRetry = "PreparedMessageRetry";
        internal const string PrepareMessageRetry = "PrepareMessageRetry";
        internal const string ProtocolInfoInvalidBasePath = "ProtocolInfoInvalidBasePath";
        internal const string ProtocolInfoInvalidFlags = "ProtocolInfoInvalidFlags";
        internal const string ProtocolInfoInvalidHostName = "ProtocolInfoInvalidHostName";
        internal const string ProtocolInfoInvalidHttpsPort = "ProtocolInfoInvalidHttpsPort";
        internal const string ProtocolInfoInvalidMaxTimeout = "ProtocolInfoInvalidMaxTimeout";
        internal const string ProtocolInfoInvalidNodeName = "ProtocolInfoInvalidNodeName";
        internal const string ProtocolInfoUnsupportedVersion = "ProtocolInfoUnsupportedVersion";
        internal const string ProtocolInitialized = "ProtocolInitialized";
        internal const string ProtocolServiceRecordSchema = "ProtocolServiceRecordSchema";
        internal const string ProtocolStarted = "ProtocolStarted";
        internal const string ProtocolTypeWrongSignature = "ProtocolTypeWrongSignature";
        internal const string ProxyCreationFailed = "ProxyCreationFailed";
        internal const string ReasonWithEnlistmentRecordSchema = "ReasonWithEnlistmentRecordSchema";
        internal const string ReasonWithTransactionIdRecordSchema = "ReasonWithTransactionIdRecordSchema";
        internal const string RecoveredCoordinatorInvalidMetadata = "RecoveredCoordinatorInvalidMetadata";
        internal const string RecoveredParticipantInvalidMetadata = "RecoveredParticipantInvalidMetadata";
        internal const string RecoveryLogEntryRecordSchema = "RecoveryLogEntryRecordSchema";
        internal const string RegisterCompletionFailureDuplicate = "RegisterCompletionFailureDuplicate";
        internal const string RegisterCoordinator = "RegisterCoordinator";
        internal const string RegisterFailureInvalidState = "RegisterFailureInvalidState";
        internal const string RegisterParticipant = "RegisterParticipant";
        internal const string RegisterParticipantFailure = "RegisterParticipantFailure";
        internal const string RegistrationCoordinatorFailed = "RegistrationCoordinatorFailed";
        internal const string RegistrationCoordinatorFaulted = "RegistrationCoordinatorFaulted";
        internal const string RegistrationCoordinatorResponseInvalidMetadata = "RegistrationCoordinatorResponseInvalidMetadata";
        internal const string RegistryKeyGetValueFailed = "RegistryKeyGetValueFailed";
        internal const string RegistryKeyOpenSubKeyFailed = "RegistryKeyOpenSubKeyFailed";
        internal const string ReplayMessageRetry = "ReplayMessageRetry";
        internal const string ReplyServerCredentialMismatch = "ReplyServerCredentialMismatch";
        internal const string ReplyServerIdentityAccessDenied = "ReplyServerIdentityAccessDenied";
        internal const string RequestReplyFault = "RequestReplyFault";
        private ResourceManager resources;
        internal const string SerializationLogEntryTooBig = "SerializationLogEntryTooBig";
        internal const string SupportingTokenSignatureExpected = "SupportingTokenSignatureExpected";
        internal const string TooManyEnlistmentsReason = "TooManyEnlistmentsReason";
        internal const string TransactionManagerTypeWrongSignature = "TransactionManagerTypeWrongSignature";
        internal const string UnexpectedStateMachineEventRecordSchema = "UnexpectedStateMachineEventRecordSchema";
        internal const string UnhandledStateMachineExceptionRecordSchema = "UnhandledStateMachineExceptionRecordSchema";
        internal const string UnknownTransactionReason = "UnknownTransactionReason";
        internal const string VolatileOutcomeTimeout = "VolatileOutcomeTimeout";
        internal const string VolatileParticipantInDoubt = "VolatileParticipantInDoubt";

        internal SR()
        {
            this.resources = new ResourceManager("Microsoft.Transactions.Bridge", base.GetType().Assembly);
        }

        private static Microsoft.Transactions.SR GetLoader()
        {
            if (loader == null)
            {
                Microsoft.Transactions.SR sr = new Microsoft.Transactions.SR();
                Interlocked.CompareExchange<Microsoft.Transactions.SR>(ref loader, sr, null);
            }
            return loader;
        }

        public static object GetObject(string name)
        {
            Microsoft.Transactions.SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetObject(name, Culture);
        }

        public static string GetString(string name)
        {
            Microsoft.Transactions.SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetString(name, Culture);
        }

        public static string GetString(string name, params object[] args)
        {
            Microsoft.Transactions.SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            string format = loader.resources.GetString(name, Culture);
            if ((args == null) || (args.Length <= 0))
            {
                return format;
            }
            for (int i = 0; i < args.Length; i++)
            {
                string str2 = args[i] as string;
                if ((str2 != null) && (str2.Length > 0x400))
                {
                    args[i] = str2.Substring(0, 0x3fd) + "...";
                }
            }
            return string.Format(CultureInfo.CurrentCulture, format, args);
        }

        public static string GetString(string name, out bool usedFallback)
        {
            usedFallback = false;
            return GetString(name);
        }

        private static CultureInfo Culture
        {
            get
            {
                return null;
            }
        }

        public static ResourceManager Resources
        {
            get
            {
                return GetLoader().resources;
            }
        }
    }
}

