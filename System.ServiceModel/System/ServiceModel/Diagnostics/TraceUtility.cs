namespace System.ServiceModel.Diagnostics
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.Diagnostics;
    using System.Security;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Diagnostics.Application;
    using System.ServiceModel.Dispatcher;
    using System.Threading;

    internal static class TraceUtility
    {
        private const string ActivityIdKey = "ActivityId";
        private static bool activityTracing;
        private static Func<Action<AsyncCallback, IAsyncResult>> asyncCallbackGenerator;
        private const string AsyncOperationActivityKey = "AsyncOperationActivity";
        private const string AsyncOperationStartTimeKey = "AsyncOperationStartTime";
        public const string E2EActivityId = "E2EActivityId";
        private static bool messageFlowTracing;
        private static bool messageFlowTracingOnly;
        private static long messageNumber = 0L;
        private static bool shouldPropagateActivity;
        private static bool shouldPropagateActivityGlobal;
        public const string TraceApplicationReference = "TraceApplicationReference";
        private static SortedList<int, string> traceCodes;

        static TraceUtility()
        {
            SortedList<int, string> list = new SortedList<int, string>(0x17d);
            list.Add(0x10001, "WmiPut");
            list.Add(0x20001, "AppDomainUnload");
            list.Add(0x20002, "EventLog");
            list.Add(0x20003, "ThrowingException");
            list.Add(0x20004, "TraceHandledException");
            list.Add(0x20005, "UnhandledException");
            list.Add(0x20006, "FailedToAddAnActivityIdHeader");
            list.Add(0x20007, "FailedToReadAnActivityIdHeader");
            list.Add(0x20008, "FilterNotMatchedNodeQuotaExceeded");
            list.Add(0x20009, "MessageCountLimitExceeded");
            list.Add(0x2000a, "DiagnosticsFailedMessageTrace");
            list.Add(0x2000b, "MessageNotLoggedQuotaExceeded");
            list.Add(0x2000c, "TraceTruncatedQuotaExceeded");
            list.Add(0x2000d, "ActivityBoundary");
            list.Add(0x30007, "");
            list.Add(0x40001, "ConnectionAbandoned");
            list.Add(0x40002, "ConnectionPoolCloseException");
            list.Add(0x40003, "ConnectionPoolIdleTimeoutReached");
            list.Add(0x40004, "ConnectionPoolLeaseTimeoutReached");
            list.Add(0x40005, "ConnectionPoolMaxOutboundConnectionsPerEndpointQuotaReached");
            list.Add(0x40006, "ServerMaxPooledConnectionsQuotaReached");
            list.Add(0x40007, "EndpointListenerClose");
            list.Add(0x40008, "EndpointListenerOpen");
            list.Add(0x40009, "HttpResponseReceived");
            list.Add(0x4000a, "HttpChannelConcurrentReceiveQuotaReached");
            list.Add(0x4000b, "HttpChannelMessageReceiveFailed");
            list.Add(0x4000c, "HttpChannelUnexpectedResponse");
            list.Add(0x4000d, "HttpChannelRequestAborted");
            list.Add(0x4000e, "HttpChannelResponseAborted");
            list.Add(0x4000f, "HttpsClientCertificateInvalid");
            list.Add(0x40010, "HttpsClientCertificateNotPresent");
            list.Add(0x40011, "NamedPipeChannelMessageReceiveFailed");
            list.Add(0x40012, "NamedPipeChannelMessageReceived");
            list.Add(0x40013, "MessageReceived");
            list.Add(0x40014, "MessageSent");
            list.Add(0x40015, "RequestChannelReplyReceived");
            list.Add(0x40016, "TcpChannelMessageReceiveFailed");
            list.Add(0x40017, "TcpChannelMessageReceived");
            list.Add(0x40018, "ConnectToIPEndpoint");
            list.Add(0x40019, "SocketConnectionCreate");
            list.Add(0x4001a, "SocketConnectionClose");
            list.Add(0x4001b, "SocketConnectionAbort");
            list.Add(0x4001c, "SocketConnectionAbortClose");
            list.Add(0x4001d, "PipeConnectionAbort");
            list.Add(0x4001e, "RequestContextAbort");
            list.Add(0x4001f, "ChannelCreated");
            list.Add(0x40020, "ChannelDisposed");
            list.Add(0x40021, "ListenerCreated");
            list.Add(0x40022, "ListenerDisposed");
            list.Add(0x40023, "PrematureDatagramEof");
            list.Add(0x40024, "MaxPendingConnectionsReached");
            list.Add(0x40025, "MaxAcceptedChannelsReached");
            list.Add(0x40026, "ChannelConnectionDropped");
            list.Add(0x40027, "HttpAuthFailed");
            list.Add(0x40028, "NoExistingTransportManager");
            list.Add(0x40029, "IncompatibleExistingTransportManager");
            list.Add(0x4002a, "InitiatingNamedPipeConnection");
            list.Add(0x4002b, "InitiatingTcpConnection");
            list.Add(0x4002c, "OpenedListener");
            list.Add(0x4002d, "SslClientCertMissing");
            list.Add(0x4002e, "StreamSecurityUpgradeAccepted");
            list.Add(0x4002f, "TcpConnectError");
            list.Add(0x40030, "FailedAcceptFromPool");
            list.Add(0x40031, "FailedPipeConnect");
            list.Add(0x40032, "SystemTimeResolution");
            list.Add(0x40033, "PeerNeighborCloseFailed");
            list.Add(0x40034, "PeerNeighborClosingFailed");
            list.Add(0x40035, "PeerNeighborNotAccepted");
            list.Add(0x40036, "PeerNeighborNotFound");
            list.Add(0x40037, "PeerNeighborOpenFailed");
            list.Add(0x40038, "PeerNeighborStateChanged");
            list.Add(0x40039, "PeerNeighborStateChangeFailed");
            list.Add(0x4003a, "PeerNeighborMessageReceived");
            list.Add(0x4003b, "PeerNeighborManagerOffline");
            list.Add(0x4003c, "PeerNeighborManagerOnline");
            list.Add(0x4003d, "PeerChannelMessageReceived");
            list.Add(0x4003e, "PeerChannelMessageSent");
            list.Add(0x4003f, "PeerNodeAddressChanged");
            list.Add(0x40040, "PeerNodeOpening");
            list.Add(0x40041, "PeerNodeOpened");
            list.Add(0x40042, "PeerNodeOpenFailed");
            list.Add(0x40043, "PeerNodeClosing");
            list.Add(0x40044, "PeerNodeClosed");
            list.Add(0x40045, "PeerFloodedMessageReceived");
            list.Add(0x40046, "PeerFloodedMessageNotPropagated");
            list.Add(0x40047, "PeerFloodedMessageNotMatched");
            list.Add(0x40048, "PnrpRegisteredAddresses");
            list.Add(0x40049, "PnrpUnregisteredAddresses");
            list.Add(0x4004a, "PnrpResolvedAddresses");
            list.Add(0x4004b, "PnrpResolveException");
            list.Add(0x4004c, "PeerReceiveMessageAuthenticationFailure");
            list.Add(0x4004d, "PeerNodeAuthenticationFailure");
            list.Add(0x4004e, "PeerNodeAuthenticationTimeout");
            list.Add(0x4004f, "PeerFlooderReceiveMessageQuotaExceeded");
            list.Add(0x40050, "PeerServiceOpened");
            list.Add(0x40051, "PeerMaintainerActivity");
            list.Add(0x40052, "MsmqCannotPeekOnQueue");
            list.Add(0x40053, "MsmqCannotReadQueues");
            list.Add(0x40054, "MsmqDatagramSent");
            list.Add(0x40055, "MsmqDatagramReceived");
            list.Add(0x40056, "MsmqDetected");
            list.Add(0x40057, "MsmqEnteredBatch");
            list.Add(0x40058, "MsmqExpectedException");
            list.Add(0x40059, "MsmqFoundBaseAddress");
            list.Add(0x4005a, "MsmqLeftBatch");
            list.Add(0x4005b, "MsmqMatchedApplicationFound");
            list.Add(0x4005c, "MsmqMessageDropped");
            list.Add(0x4005d, "MsmqMessageLockedUnderTheTransaction");
            list.Add(0x4005e, "MsmqMessageRejected");
            list.Add(0x4005f, "MsmqMoveOrDeleteAttemptFailed");
            list.Add(0x40060, "MsmqPoisonMessageMovedPoison");
            list.Add(0x40061, "MsmqPoisonMessageMovedRetry");
            list.Add(0x40062, "MsmqPoisonMessageRejected");
            list.Add(0x40063, "MsmqPoolFull");
            list.Add(0x40064, "MsmqPotentiallyPoisonMessageDetected");
            list.Add(0x40065, "MsmqQueueClosed");
            list.Add(0x40066, "MsmqQueueOpened");
            list.Add(0x40067, "MsmqQueueTransactionalStatusUnknown");
            list.Add(0x40068, "MsmqScanStarted");
            list.Add(0x40069, "MsmqSessiongramReceived");
            list.Add(0x4006a, "MsmqSessiongramSent");
            list.Add(0x4006b, "MsmqStartingApplication");
            list.Add(0x4006c, "MsmqStartingService");
            list.Add(0x4006d, "MsmqUnexpectedAcknowledgment");
            list.Add(0x50001, "ComIntegrationServiceHostStartingService");
            list.Add(0x50002, "ComIntegrationServiceHostStartedService");
            list.Add(0x50003, "ComIntegrationServiceHostCreatedServiceContract");
            list.Add(0x50004, "ComIntegrationServiceHostStartedServiceDetails");
            list.Add(0x50005, "ComIntegrationServiceHostCreatedServiceEndpoint");
            list.Add(0x50006, "ComIntegrationServiceHostStoppingService");
            list.Add(0x50007, "ComIntegrationServiceHostStoppedService");
            list.Add(0x50008, "ComIntegrationDllHostInitializerStarting");
            list.Add(0x50009, "ComIntegrationDllHostInitializerAddingHost");
            list.Add(0x5000a, "ComIntegrationDllHostInitializerStarted");
            list.Add(0x5000b, "ComIntegrationDllHostInitializerStopping");
            list.Add(0x5000c, "ComIntegrationDllHostInitializerStopped");
            list.Add(0x5000d, "ComIntegrationTLBImportStarting");
            list.Add(0x5000e, "ComIntegrationTLBImportFromAssembly");
            list.Add(0x5000f, "ComIntegrationTLBImportFromTypelib");
            list.Add(0x50010, "ComIntegrationTLBImportConverterEvent");
            list.Add(0x50011, "ComIntegrationTLBImportFinished");
            list.Add(0x50012, "ComIntegrationInstanceCreationRequest");
            list.Add(0x50013, "ComIntegrationInstanceCreationSuccess");
            list.Add(0x50014, "ComIntegrationInstanceReleased");
            list.Add(0x50015, "ComIntegrationEnteringActivity");
            list.Add(0x50016, "ComIntegrationExecutingCall");
            list.Add(0x50017, "ComIntegrationLeftActivity");
            list.Add(0x50018, "ComIntegrationInvokingMethod");
            list.Add(0x50019, "ComIntegrationInvokedMethod");
            list.Add(0x5001a, "ComIntegrationInvokingMethodNewTransaction");
            list.Add(0x5001b, "ComIntegrationInvokingMethodContextTransaction");
            list.Add(0x5001c, "ComIntegrationServiceMonikerParsed");
            list.Add(0x5001d, "ComIntegrationWsdlChannelBuilderLoaded");
            list.Add(0x5001e, "ComIntegrationTypedChannelBuilderLoaded");
            list.Add(0x5001f, "ComIntegrationChannelCreated");
            list.Add(0x50020, "ComIntegrationDispatchMethod");
            list.Add(0x50021, "ComIntegrationTxProxyTxCommitted");
            list.Add(0x50022, "ComIntegrationTxProxyTxAbortedByContext");
            list.Add(0x50023, "ComIntegrationTxProxyTxAbortedByTM");
            list.Add(0x50024, "ComIntegrationMexMonikerMetadataExchangeComplete");
            list.Add(0x50025, "ComIntegrationMexChannelBuilderLoaded");
            list.Add(0x70000, "Security");
            list.Add(0x70001, "SecurityIdentityVerificationSuccess");
            list.Add(0x70002, "SecurityIdentityVerificationFailure");
            list.Add(0x70003, "SecurityIdentityDeterminationSuccess");
            list.Add(0x70004, "SecurityIdentityDeterminationFailure");
            list.Add(0x70005, "SecurityIdentityHostNameNormalizationFailure");
            list.Add(0x70006, "SecurityImpersonationSuccess");
            list.Add(0x70007, "SecurityImpersonationFailure");
            list.Add(0x70008, "SecurityNegotiationProcessingFailure");
            list.Add(0x70009, "IssuanceTokenProviderRemovedCachedToken");
            list.Add(0x7000a, "IssuanceTokenProviderUsingCachedToken");
            list.Add(0x7000b, "IssuanceTokenProviderBeginSecurityNegotiation");
            list.Add(0x7000c, "IssuanceTokenProviderEndSecurityNegotiation");
            list.Add(0x7000d, "IssuanceTokenProviderRedirectApplied");
            list.Add(0x7000e, "IssuanceTokenProviderServiceTokenCacheFull");
            list.Add(0x7000f, "NegotiationTokenProviderAttached");
            list.Add(0x70020, "SpnegoClientNegotiationCompleted");
            list.Add(0x70021, "SpnegoServiceNegotiationCompleted");
            list.Add(0x70022, "SpnegoClientNegotiation");
            list.Add(0x70023, "SpnegoServiceNegotiation");
            list.Add(0x70024, "NegotiationAuthenticatorAttached");
            list.Add(0x70025, "ServiceSecurityNegotiationCompleted");
            list.Add(0x70026, "SecurityContextTokenCacheFull");
            list.Add(0x70027, "ExportSecurityChannelBindingEntry");
            list.Add(0x70028, "ExportSecurityChannelBindingExit");
            list.Add(0x70029, "ImportSecurityChannelBindingEntry");
            list.Add(0x7002a, "ImportSecurityChannelBindingExit");
            list.Add(0x7002b, "SecurityTokenProviderOpened");
            list.Add(0x7002c, "SecurityTokenProviderClosed");
            list.Add(0x7002d, "SecurityTokenAuthenticatorOpened");
            list.Add(0x7002e, "SecurityTokenAuthenticatorClosed");
            list.Add(0x7002f, "SecurityBindingOutgoingMessageSecured");
            list.Add(0x70030, "SecurityBindingIncomingMessageVerified");
            list.Add(0x70031, "SecurityBindingSecureOutgoingMessageFailure");
            list.Add(0x70032, "SecurityBindingVerifyIncomingMessageFailure");
            list.Add(0x70033, "SecuritySpnToSidMappingFailure");
            list.Add(0x70034, "SecuritySessionRedirectApplied");
            list.Add(0x70035, "SecurityClientSessionCloseSent");
            list.Add(0x70036, "SecurityClientSessionCloseResponseSent");
            list.Add(0x70037, "SecurityClientSessionCloseMessageReceived");
            list.Add(0x70038, "SecuritySessionKeyRenewalFaultReceived");
            list.Add(0x70039, "SecuritySessionAbortedFaultReceived");
            list.Add(0x7003a, "SecuritySessionClosedResponseReceived");
            list.Add(0x7003b, "SecurityClientSessionPreviousKeyDiscarded");
            list.Add(0x7003c, "SecurityClientSessionKeyRenewed");
            list.Add(0x7003d, "SecurityPendingServerSessionAdded");
            list.Add(0x7003e, "SecurityPendingServerSessionClosed");
            list.Add(0x7003f, "SecurityPendingServerSessionActivated");
            list.Add(0x70040, "SecurityActiveServerSessionRemoved");
            list.Add(0x70041, "SecurityNewServerSessionKeyIssued");
            list.Add(0x70042, "SecurityInactiveSessionFaulted");
            list.Add(0x70043, "SecurityServerSessionKeyUpdated");
            list.Add(0x70044, "SecurityServerSessionCloseReceived");
            list.Add(0x70045, "SecurityServerSessionRenewalFaultSent");
            list.Add(0x70046, "SecurityServerSessionAbortedFaultSent");
            list.Add(0x70047, "SecuritySessionCloseResponseSent");
            list.Add(0x70048, "SecuritySessionServerCloseSent");
            list.Add(0x70049, "SecurityServerSessionCloseResponseReceived");
            list.Add(0x7004a, "SecuritySessionRenewFaultSendFailure");
            list.Add(0x7004b, "SecuritySessionAbortedFaultSendFailure");
            list.Add(0x7004c, "SecuritySessionClosedResponseSendFailure");
            list.Add(0x7004d, "SecuritySessionServerCloseSendFailure");
            list.Add(0x7004e, "SecuritySessionRequestorStartOperation");
            list.Add(0x7004f, "SecuritySessionRequestorOperationSuccess");
            list.Add(0x70050, "SecuritySessionRequestorOperationFailure");
            list.Add(0x70051, "SecuritySessionResponderOperationFailure");
            list.Add(0x70052, "SecuritySessionDemuxFailure");
            list.Add(0x70053, "SecurityAuditWrittenSuccess");
            list.Add(0x70054, "SecurityAuditWrittenFailure");
            list.Add(0x80001, "AsyncCallbackThrewException");
            list.Add(0x80002, "CommunicationObjectAborted");
            list.Add(0x80003, "CommunicationObjectAbortFailed");
            list.Add(0x80004, "CommunicationObjectCloseFailed");
            list.Add(0x80005, "CommunicationObjectOpenFailed");
            list.Add(0x80006, "CommunicationObjectClosing");
            list.Add(0x80007, "CommunicationObjectClosed");
            list.Add(0x80008, "CommunicationObjectCreated");
            list.Add(0x80009, "CommunicationObjectDisposing");
            list.Add(0x8000a, "CommunicationObjectFaultReason");
            list.Add(0x8000b, "CommunicationObjectFaulted");
            list.Add(0x8000c, "CommunicationObjectOpening");
            list.Add(0x8000d, "CommunicationObjectOpened");
            list.Add(0x8000e, "DidNotUnderstandMessageHeader");
            list.Add(0x8000f, "UnderstoodMessageHeader");
            list.Add(0x80010, "MessageClosed");
            list.Add(0x80011, "MessageClosedAgain");
            list.Add(0x80012, "MessageCopied");
            list.Add(0x80013, "MessageRead");
            list.Add(0x80014, "MessageWritten");
            list.Add(0x80015, "BeginExecuteMethod");
            list.Add(0x80016, "ConfigurationIsReadOnly");
            list.Add(0x80017, "ConfiguredExtensionTypeNotFound");
            list.Add(0x80018, "EvaluationContextNotFound");
            list.Add(0x80019, "EndExecuteMethod");
            list.Add(0x8001a, "ExtensionCollectionDoesNotExist");
            list.Add(0x8001b, "ExtensionCollectionNameNotFound");
            list.Add(0x8001c, "ExtensionCollectionIsEmpty");
            list.Add(0x8001d, "ExtensionElementAlreadyExistsInCollection");
            list.Add(0x8001e, "ElementTypeDoesntMatchConfiguredType");
            list.Add(0x8001f, "ErrorInvokingUserCode");
            list.Add(0x80020, "GetBehaviorElement");
            list.Add(0x80021, "GetCommonBehaviors");
            list.Add(0x80022, "GetConfiguredBinding");
            list.Add(0x80023, "GetChannelEndpointElement");
            list.Add(0x80024, "GetConfigurationSection");
            list.Add(0x80025, "GetDefaultConfiguredBinding");
            list.Add(0x80026, "GetServiceElement");
            list.Add(0x80027, "MessageProcessingPaused");
            list.Add(0x80028, "ManualFlowThrottleLimitReached");
            list.Add(0x80029, "OverridingDuplicateConfigurationKey");
            list.Add(0x8002a, "RemoveBehavior");
            list.Add(0x8002b, "ServiceChannelLifetime");
            list.Add(0x8002c, "ServiceHostCreation");
            list.Add(0x8002d, "ServiceHostBaseAddresses");
            list.Add(0x8002e, "ServiceHostTimeoutOnClose");
            list.Add(0x8002f, "ServiceHostFaulted");
            list.Add(0x80030, "ServiceHostErrorOnReleasePerformanceCounter");
            list.Add(0x80031, "ServiceThrottleLimitReached");
            list.Add(0x80032, "ServiceOperationMissingReply");
            list.Add(0x80033, "ServiceOperationMissingReplyContext");
            list.Add(0x80034, "ServiceOperationExceptionOnReply");
            list.Add(0x80035, "SkipBehavior");
            list.Add(0x80036, "TransportListen");
            list.Add(0x80037, "UnhandledAction");
            list.Add(0x80038, "PerformanceCounterFailedToLoad");
            list.Add(0x80039, "PerformanceCountersFailed");
            list.Add(0x8003a, "PerformanceCountersFailedDuringUpdate");
            list.Add(0x8003b, "PerformanceCountersFailedForService");
            list.Add(0x8003c, "PerformanceCountersFailedOnRelease");
            list.Add(0x8003d, "WsmexNonCriticalWsdlExportError");
            list.Add(0x8003e, "WsmexNonCriticalWsdlImportError");
            list.Add(0x8003f, "FailedToOpenIncomingChannel");
            list.Add(0x80040, "UnhandledExceptionInUserOperation");
            list.Add(0x80041, "DroppedAMessage");
            list.Add(0x80042, "CannotBeImportedInCurrentFormat");
            list.Add(0x80043, "GetConfiguredEndpoint");
            list.Add(0x80044, "GetDefaultConfiguredEndpoint");
            list.Add(0x80045, "ExtensionTypeNotFound");
            list.Add(0x80046, "DefaultEndpointsAdded");
            list.Add(0x8005b, "MetadataExchangeClientSendRequest");
            list.Add(0x8005c, "MetadataExchangeClientReceiveReply");
            list.Add(0x8005d, "WarnHelpPageEnabledNoBaseAddress");
            list.Add(0xa0001, "PortSharingClosed");
            list.Add(0xa0002, "PortSharingDuplicatedPipe");
            list.Add(0xa0003, "PortSharingDupHandleGranted");
            list.Add(0xa0004, "PortSharingDuplicatedSocket");
            list.Add(0xa0005, "PortSharingListening");
            list.Add(0xa000e, "SharedManagerServiceEndpointNotExist");
            list.Add(0xe0001, "TxSourceTxScopeRequiredIsTransactedTransport");
            list.Add(0xe0002, "TxSourceTxScopeRequiredIsTransactionFlow");
            list.Add(0xe0003, "TxSourceTxScopeRequiredIsAttachedTransaction");
            list.Add(0xe0004, "TxSourceTxScopeRequiredIsCreateNewTransaction");
            list.Add(0xe0005, "TxCompletionStatusCompletedForAutocomplete");
            list.Add(0xe0006, "TxCompletionStatusCompletedForError");
            list.Add(0xe0007, "TxCompletionStatusCompletedForSetComplete");
            list.Add(0xe0008, "TxCompletionStatusCompletedForTACOSC");
            list.Add(0xe0009, "TxCompletionStatusCompletedForAsyncAbort");
            list.Add(0xe000a, "TxCompletionStatusRemainsAttached");
            list.Add(0xe000b, "TxCompletionStatusAbortedOnSessionClose");
            list.Add(0xe000c, "TxReleaseServiceInstanceOnCompletion");
            list.Add(0xe000d, "TxAsyncAbort");
            list.Add(0xe000e, "TxFailedToNegotiateOleTx");
            list.Add(0xe000f, "TxSourceTxScopeRequiredUsingExistingTransaction");
            list.Add(0xf0000, "ActivatingMessageReceived");
            list.Add(0xf0001, "InstanceContextBoundToDurableInstance");
            list.Add(0xf0002, "InstanceContextDetachedFromDurableInstance");
            list.Add(0xf0003, "ContextChannelFactoryChannelCreated");
            list.Add(0xf0004, "ContextChannelListenerChannelAccepted");
            list.Add(0xf0005, "ContextProtocolContextAddedToMessage");
            list.Add(0xf0006, "ContextProtocolContextRetrievedFromMessage");
            list.Add(0xf0007, "DICPInstanceContextCached");
            list.Add(0xf0008, "DICPInstanceContextRemovedFromCache");
            list.Add(0xf0009, "ServiceDurableInstanceDeleted");
            list.Add(0xf000a, "ServiceDurableInstanceDisposed");
            list.Add(0xf000b, "ServiceDurableInstanceLoaded");
            list.Add(0xf000c, "ServiceDurableInstanceSaved");
            list.Add(0xf000d, "SqlPersistenceProviderSQLCallStart");
            list.Add(0xf000e, "SqlPersistenceProviderSQLCallEnd");
            list.Add(0xf000f, "SqlPersistenceProviderOpenParameters");
            list.Add(0xf0010, "SyncContextSchedulerServiceTimerCancelled");
            list.Add(0xf0011, "SyncContextSchedulerServiceTimerCreated");
            list.Add(0xf0012, "WorkflowDurableInstanceLoaded");
            list.Add(0xf0013, "WorkflowDurableInstanceAborted");
            list.Add(0xf0014, "WorkflowDurableInstanceActivated");
            list.Add(0xf0015, "WorkflowOperationInvokerItemQueued");
            list.Add(0xf0016, "WorkflowRequestContextReplySent");
            list.Add(0xf0017, "WorkflowRequestContextFaultSent");
            list.Add(0xf0018, "WorkflowServiceHostCreated");
            list.Add(0xf0019, "SyndicationReadFeedBegin");
            list.Add(0xf001a, "SyndicationReadFeedEnd");
            list.Add(0xf001b, "SyndicationReadItemBegin");
            list.Add(0xf001c, "SyndicationReadItemEnd");
            list.Add(0xf001d, "SyndicationWriteFeedBegin");
            list.Add(0xf001e, "SyndicationWriteFeedEnd");
            list.Add(0xf001f, "SyndicationWriteItemBegin");
            list.Add(0xf0020, "SyndicationWriteItemEnd");
            list.Add(0xf0021, "SyndicationProtocolElementIgnoredOnRead");
            list.Add(0xf0022, "SyndicationProtocolElementIgnoredOnWrite");
            list.Add(0xf0023, "SyndicationProtocolElementInvalid");
            list.Add(0xf0024, "WebUnknownQueryParameterIgnored");
            list.Add(0xf0025, "WebRequestMatchesOperation");
            list.Add(0xf0026, "WebRequestDoesNotMatchOperations");
            list.Add(0xf0027, "WebRequestRedirect");
            list.Add(0xf0028, "SyndicationReadServiceDocumentBegin");
            list.Add(0xf0029, "SyndicationReadServiceDocumentEnd");
            list.Add(0xf002a, "SyndicationReadCategoriesDocumentBegin");
            list.Add(0xf002b, "SyndicationReadCategoriesDocumentEnd");
            list.Add(0xf002c, "SyndicationWriteServiceDocumentBegin");
            list.Add(0xf002d, "SyndicationWriteServiceDocumentEnd");
            list.Add(0xf002e, "SyndicationWriteCategoriesDocumentBegin");
            list.Add(0xf002f, "SyndicationWriteCategoriesDocumentEnd");
            list.Add(0xf0030, "AutomaticFormatSelectedOperationDefault");
            list.Add(0xf0031, "AutomaticFormatSelectedRequestBased");
            list.Add(0xf0032, "RequestFormatSelectedFromContentTypeMapper");
            list.Add(0xf0033, "RequestFormatSelectedByEncoderDefaults");
            list.Add(0xf0034, "AddingResponseToOutputCache");
            list.Add(0xf0035, "AddingAuthenticatedResponseToOutputCache");
            list.Add(0xf0037, "JsonpCallbackNameSet");
            traceCodes = list;
            SetEtwProviderId();
            SetEndToEndTracingFlags();
            if (DiagnosticUtility.DiagnosticTrace != null)
            {
                System.ServiceModel.Diagnostics.DiagnosticTraceSource traceSource = (System.ServiceModel.Diagnostics.DiagnosticTraceSource) DiagnosticUtility.DiagnosticTrace.TraceSource;
                shouldPropagateActivity = traceSource.PropagateActivity || shouldPropagateActivityGlobal;
            }
        }

        internal static void AddActivityHeader(Message message)
        {
            try
            {
                new ActivityIdHeader(ExtractActivityId(message)).AddTo(message);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                TraceEvent(TraceEventType.Error, 0x20006, System.ServiceModel.SR.GetString("TraceCodeFailedToAddAnActivityIdHeader"), exception, message);
            }
        }

        internal static void AddAmbientActivityToMessage(Message message)
        {
            try
            {
                new ActivityIdHeader(System.ServiceModel.Diagnostics.DiagnosticTrace.ActivityId).AddTo(message);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                TraceEvent(TraceEventType.Error, 0x20006, System.ServiceModel.SR.GetString("TraceCodeFailedToAddAnActivityIdHeader"), exception, message);
            }
        }

        private static Action<AsyncCallback, IAsyncResult> CallbackGenerator()
        {
            if (DiagnosticUtility.ShouldUseActivity)
            {
                Action<AsyncCallback, IAsyncResult> action = null;
                ServiceModelActivity callbackActivity = ServiceModelActivity.Current;
                if (callbackActivity != null)
                {
                    if (action == null)
                    {
                        action = delegate (AsyncCallback callback, IAsyncResult result) {
                            using (ServiceModelActivity.BoundOperation(callbackActivity))
                            {
                                callback(result);
                            }
                        };
                    }
                    return action;
                }
            }
            return null;
        }

        internal static void CopyActivity(Message source, Message destination)
        {
            if (DiagnosticUtility.ShouldUseActivity)
            {
                SetActivity(destination, ExtractActivity(source));
            }
        }

        public static InputQueue<T> CreateInputQueue<T>() where T: class
        {
            if (asyncCallbackGenerator == null)
            {
                asyncCallbackGenerator = new Func<Action<AsyncCallback, IAsyncResult>>(TraceUtility.CallbackGenerator);
            }
            return new InputQueue<T>(asyncCallbackGenerator) { DisposeItemCallback = delegate (T value) {
                if (value is ICommunicationObject)
                {
                    ((ICommunicationObject) value).Abort();
                }
            } };
        }

        internal static string CreateSourceString(object source)
        {
            return (source.GetType().ToString() + "/" + source.GetHashCode().ToString(CultureInfo.CurrentCulture));
        }

        internal static ServiceModelActivity ExtractActivity(Message message)
        {
            ServiceModelActivity activity = null;
            object obj2;
            if ((DiagnosticUtility.ShouldUseActivity || ShouldPropagateActivityGlobal) && (((message != null) && (message.State != MessageState.Closed)) && message.Properties.TryGetValue("ActivityId", out obj2)))
            {
                activity = obj2 as ServiceModelActivity;
            }
            return activity;
        }

        internal static Guid ExtractActivityId(Message message)
        {
            if (MessageFlowTracingOnly)
            {
                return ActivityIdHeader.ExtractActivityId(message);
            }
            ServiceModelActivity activity = ExtractActivity(message);
            if (activity != null)
            {
                return activity.Id;
            }
            return Guid.Empty;
        }

        internal static ServiceModelActivity ExtractAndRemoveActivity(Message message)
        {
            ServiceModelActivity activity = ExtractActivity(message);
            if (activity != null)
            {
                message.Properties["ActivityId"] = false;
            }
            return activity;
        }

        internal static object ExtractAsyncOperationContextActivity()
        {
            object obj2 = null;
            if ((OperationContext.Current != null) && OperationContext.Current.OutgoingMessageProperties.TryGetValue("AsyncOperationActivity", out obj2))
            {
                OperationContext.Current.OutgoingMessageProperties.Remove("AsyncOperationActivity");
            }
            return obj2;
        }

        internal static long ExtractAsyncOperationStartTime()
        {
            object obj2 = null;
            if ((OperationContext.Current != null) && OperationContext.Current.OutgoingMessageProperties.TryGetValue("AsyncOperationStartTime", out obj2))
            {
                OperationContext.Current.OutgoingMessageProperties.Remove("AsyncOperationStartTime");
                return (long) obj2;
            }
            return 0L;
        }

        private static string GenerateMsdnTraceCode(int traceCode)
        {
            int num = traceCode & ((int) 0xffff0000L);
            string traceSource = null;
            switch (num)
            {
                case 0x30000:
                    traceSource = "System.Runtime.Serialization";
                    break;

                case 0x40000:
                    traceSource = "System.ServiceModel.Channels";
                    break;

                case 0x10000:
                    traceSource = "System.ServiceModel.Administration";
                    break;

                case 0x20000:
                    traceSource = "System.ServiceModel.Diagnostics";
                    break;

                case 0x50000:
                    traceSource = "System.ServiceModel.ComIntegration";
                    break;

                case 0x70000:
                    traceSource = "System.ServiceModel.Security";
                    break;

                case 0x80000:
                case 0xe0000:
                    traceSource = "System.ServiceModel";
                    break;

                case 0xa0000:
                    traceSource = "System.ServiceModel.PortSharing";
                    break;

                default:
                    traceSource = string.Empty;
                    break;
            }
            return System.ServiceModel.Diagnostics.DiagnosticTrace.GenerateMsdnTraceCode(traceSource, traceCodes[traceCode]);
        }

        internal static string GetAnnotation(OperationContext context)
        {
            object annotationFromHost;
            if (((context != null) && (context.IncomingMessage != null)) && (MessageState.Closed != context.IncomingMessage.State))
            {
                if (!context.IncomingMessageProperties.TryGetValue("TraceApplicationReference", out annotationFromHost))
                {
                    annotationFromHost = AspNetEnvironment.Current.GetAnnotationFromHost(context.Host);
                    context.IncomingMessageProperties.Add("TraceApplicationReference", annotationFromHost);
                }
            }
            else
            {
                annotationFromHost = AspNetEnvironment.Current.GetAnnotationFromHost(null);
            }
            return (string) annotationFromHost;
        }

        internal static string GetCallerInfo(OperationContext context)
        {
            object obj2;
            if (((context != null) && (context.IncomingMessageProperties != null)) && context.IncomingMessageProperties.TryGetValue(RemoteEndpointMessageProperty.Name, out obj2))
            {
                RemoteEndpointMessageProperty property = obj2 as RemoteEndpointMessageProperty;
                if (property != null)
                {
                    return string.Format(CultureInfo.InvariantCulture, "{0}:{1}", new object[] { property.Address, property.Port });
                }
            }
            return "null";
        }

        internal static Guid GetReceivedActivityId(OperationContext operationContext)
        {
            object obj2;
            if (!operationContext.IncomingMessageProperties.TryGetValue("E2EActivityId", out obj2))
            {
                return ExtractActivityId(operationContext.IncomingMessage);
            }
            return (Guid) obj2;
        }

        internal static long GetUtcBasedDurationForTrace(long startTicks)
        {
            if (startTicks > 0L)
            {
                TimeSpan span = new TimeSpan(DateTime.UtcNow.Ticks - startTicks);
                return (long) span.TotalMilliseconds;
            }
            return 0L;
        }

        internal static void MessageFlowAtMessageReceived(Message message, OperationContext context, bool createNewActivityId)
        {
            if (MessageFlowTracing)
            {
                Guid guid;
                Guid guid2;
                bool flag = ActivityIdHeader.ExtractActivityAndCorrelationId(message, out guid, out guid2);
                if (MessageFlowTracingOnly)
                {
                    if (createNewActivityId)
                    {
                        if (!flag)
                        {
                            guid = Guid.NewGuid();
                            flag = true;
                        }
                        System.ServiceModel.Diagnostics.DiagnosticTrace.ActivityId = Guid.Empty;
                    }
                    if (flag)
                    {
                        System.ServiceModel.Diagnostics.Application.FxTrace.Trace.SetAndTraceTransfer(guid, !createNewActivityId);
                        message.Properties["E2EActivityId"] = Trace.CorrelationManager.ActivityId;
                    }
                }
                if (System.ServiceModel.Diagnostics.Application.TD.MessageReceivedFromTransportIsEnabled())
                {
                    if (context == null)
                    {
                        context = OperationContext.Current;
                    }
                    System.ServiceModel.Diagnostics.Application.TD.MessageReceivedFromTransport(guid2, GetAnnotation(context));
                }
            }
        }

        internal static void MessageFlowAtMessageSent(Message message)
        {
            if (MessageFlowTracing)
            {
                Guid guid;
                Guid guid2;
                bool flag = ActivityIdHeader.ExtractActivityAndCorrelationId(message, out guid, out guid2);
                if ((MessageFlowTracingOnly && flag) && (guid != System.ServiceModel.Diagnostics.DiagnosticTrace.ActivityId))
                {
                    System.ServiceModel.Diagnostics.DiagnosticTrace.ActivityId = guid;
                }
                if (System.ServiceModel.Diagnostics.Application.TD.MessageSentToTransportIsEnabled())
                {
                    System.ServiceModel.Diagnostics.Application.TD.MessageSentToTransport(guid2);
                }
            }
        }

        internal static void ProcessIncomingMessage(Message message)
        {
            ServiceModelActivity current = ServiceModelActivity.Current;
            if ((current != null) && DiagnosticUtility.ShouldUseActivity)
            {
                ServiceModelActivity activity = ExtractActivity(message);
                if ((activity != null) && (activity.Id != current.Id))
                {
                    using (ServiceModelActivity.BoundOperation(activity))
                    {
                        if (System.ServiceModel.Diagnostics.Application.FxTrace.Trace != null)
                        {
                            System.ServiceModel.Diagnostics.Application.FxTrace.Trace.TraceTransfer(current.Id);
                        }
                    }
                }
                SetActivity(message, current);
            }
            MessageFlowAtMessageReceived(message, null, true);
            if (MessageLogger.LogMessagesAtServiceLevel)
            {
                MessageLogger.LogMessage(ref message, MessageLoggingSource.LastChance | MessageLoggingSource.ServiceLevelReceiveReply);
            }
        }

        internal static void ProcessOutgoingMessage(Message message)
        {
            ServiceModelActivity current = ServiceModelActivity.Current;
            if (DiagnosticUtility.ShouldUseActivity)
            {
                SetActivity(message, current);
            }
            if (PropagateUserActivity || ShouldPropagateActivity)
            {
                AddAmbientActivityToMessage(message);
            }
            MessageFlowAtMessageSent(message);
            if (MessageLogger.LogMessagesAtServiceLevel)
            {
                MessageLogger.LogMessage(ref message, MessageLoggingSource.LastChance | MessageLoggingSource.ServiceLevelSendRequest);
            }
        }

        public static long RetrieveMessageNumber()
        {
            return Interlocked.Increment(ref messageNumber);
        }

        internal static void SetActivity(Message message, ServiceModelActivity activity)
        {
            if ((DiagnosticUtility.ShouldUseActivity && (message != null)) && (message.State != MessageState.Closed))
            {
                message.Properties["ActivityId"] = activity;
            }
        }

        internal static void SetActivityId(MessageProperties properties)
        {
            Guid guid;
            if ((properties != null) && properties.TryGetValue<Guid>("E2EActivityId", out guid))
            {
                System.ServiceModel.Diagnostics.DiagnosticTrace.ActivityId = guid;
            }
        }

        [SecuritySafeCritical]
        private static void SetEndToEndTracingFlags()
        {
            EndToEndTracingElement endToEndTracing = DiagnosticSection.UnsafeGetSection().EndToEndTracing;
            shouldPropagateActivityGlobal = endToEndTracing.PropagateActivity;
            shouldPropagateActivity = shouldPropagateActivityGlobal || shouldPropagateActivity;
            DiagnosticUtility.ShouldUseActivity = DiagnosticUtility.ShouldUseActivity || endToEndTracing.ActivityTracing;
            activityTracing = DiagnosticUtility.ShouldUseActivity;
            messageFlowTracing = endToEndTracing.MessageFlowTracing || activityTracing;
            messageFlowTracingOnly = endToEndTracing.MessageFlowTracing && !endToEndTracing.ActivityTracing;
            DiagnosticUtility.TracingEnabled = DiagnosticUtility.TracingEnabled || activityTracing;
        }

        [SecuritySafeCritical]
        internal static void SetEtwProviderId()
        {
            DiagnosticSection section = DiagnosticSection.UnsafeGetSectionNoTrace();
            Guid empty = Guid.Empty;
            if (PartialTrustHelpers.HasEtwPermissions() || section.IsEtwProviderIdFromConfigFile())
            {
                empty = Fx.CreateGuid(section.EtwProviderId);
            }
            System.Runtime.Diagnostics.DiagnosticTrace.DefaultEtwProviderId = empty;
        }

        internal static ArgumentException ThrowHelperArgument(string paramName, string message, Message msg)
        {
            return (ArgumentException) ThrowHelperError(new ArgumentException(message, paramName), msg);
        }

        internal static ArgumentNullException ThrowHelperArgumentNull(string paramName, Message message)
        {
            return (ArgumentNullException) ThrowHelperError(new ArgumentNullException(paramName), message);
        }

        internal static Exception ThrowHelperError(Exception exception, Message message)
        {
            Guid activityId = ExtractActivityId(message);
            if (DiagnosticUtility.ShouldTraceError)
            {
                DiagnosticUtility.DiagnosticTrace.TraceEvent(TraceEventType.Error, 0x20003, GenerateMsdnTraceCode(0x20003), TraceSR.GetString("ThrowingException"), null, exception, activityId, null);
            }
            return exception;
        }

        internal static Exception ThrowHelperError(Exception exception, Guid activityId, object source)
        {
            if (DiagnosticUtility.ShouldTraceError)
            {
                DiagnosticUtility.DiagnosticTrace.TraceEvent(TraceEventType.Error, 0x20003, GenerateMsdnTraceCode(0x20003), TraceSR.GetString("ThrowingException"), null, exception, activityId, source);
            }
            return exception;
        }

        internal static Exception ThrowHelperWarning(Exception exception, Message message)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                Guid activityId = ExtractActivityId(message);
                DiagnosticUtility.DiagnosticTrace.TraceEvent(TraceEventType.Warning, 0x20003, GenerateMsdnTraceCode(0x20003), TraceSR.GetString("ThrowingException"), null, exception, activityId, null);
            }
            return exception;
        }

        internal static void TraceDroppedMessage(Message message, EndpointDispatcher dispatcher)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                EndpointAddress endpointAddress = null;
                if (dispatcher != null)
                {
                    endpointAddress = dispatcher.EndpointAddress;
                }
                TraceEvent(TraceEventType.Information, 0x80041, System.ServiceModel.SR.GetString("TraceCodeDroppedAMessage"), (TraceRecord) new MessageDroppedTraceRecord(message, endpointAddress));
            }
        }

        internal static void TraceEvent(TraceEventType severity, int traceCode, string traceDescription)
        {
            TraceEvent(severity, traceCode, traceDescription, null, traceDescription, null);
        }

        internal static void TraceEvent(TraceEventType severity, int traceCode, string traceDescription, object source)
        {
            TraceEvent(severity, traceCode, traceDescription, null, source, null);
        }

        internal static void TraceEvent(TraceEventType severity, int traceCode, string traceDescription, TraceRecord extendedData)
        {
            TraceEvent(severity, traceCode, traceDescription, extendedData, null, null);
        }

        internal static void TraceEvent(TraceEventType severity, int traceCode, string traceDescription, Message message)
        {
            if (message == null)
            {
                TraceEvent(severity, traceCode, traceDescription, null, (Exception) null);
            }
            else
            {
                TraceEvent(severity, traceCode, traceDescription, message, message);
            }
        }

        internal static void TraceEvent(TraceEventType severity, int traceCode, string traceDescription, Exception exception, Message message)
        {
            Guid activityId = ExtractActivityId(message);
            if (DiagnosticUtility.ShouldTrace(severity))
            {
                DiagnosticUtility.DiagnosticTrace.TraceEvent(severity, traceCode, GenerateMsdnTraceCode(traceCode), traceDescription, new MessageTraceRecord(message), exception, activityId, null);
            }
        }

        internal static void TraceEvent(TraceEventType severity, int traceCode, string traceDescription, object source, Exception exception)
        {
            TraceEvent(severity, traceCode, traceDescription, null, source, exception);
        }

        internal static void TraceEvent(TraceEventType severity, int traceCode, string traceDescription, object source, Message message)
        {
            Guid activityId = ExtractActivityId(message);
            if (DiagnosticUtility.ShouldTrace(severity))
            {
                DiagnosticUtility.DiagnosticTrace.TraceEvent(severity, traceCode, GenerateMsdnTraceCode(traceCode), traceDescription, new MessageTraceRecord(message), null, activityId, message);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void TraceEvent(TraceEventType severity, int traceCode, string traceDescription, TraceRecord extendedData, object source, Exception exception)
        {
            if (DiagnosticUtility.ShouldTrace(severity))
            {
                DiagnosticUtility.DiagnosticTrace.TraceEvent(severity, traceCode, GenerateMsdnTraceCode(traceCode), traceDescription, extendedData, exception, source);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void TraceEvent(TraceEventType severity, int traceCode, string traceDescription, TraceRecord extendedData, object source, Exception exception, Guid activityId)
        {
            if (DiagnosticUtility.ShouldTrace(severity))
            {
                DiagnosticUtility.DiagnosticTrace.TraceEvent(severity, traceCode, GenerateMsdnTraceCode(traceCode), traceDescription, extendedData, exception, activityId, source);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void TraceEvent(TraceEventType severity, int traceCode, string traceDescription, TraceRecord extendedData, object source, Exception exception, Message message)
        {
            Guid activityId = ExtractActivityId(message);
            if (DiagnosticUtility.ShouldTrace(severity))
            {
                DiagnosticUtility.DiagnosticTrace.TraceEvent(severity, traceCode, GenerateMsdnTraceCode(traceCode), traceDescription, extendedData, exception, activityId, source);
            }
        }

        internal static void TraceEventNoCheck(TraceEventType severity, int traceCode, string traceDescription, TraceRecord extendedData, object source, Exception exception)
        {
            DiagnosticUtility.DiagnosticTrace.TraceEvent(severity, traceCode, GenerateMsdnTraceCode(traceCode), traceDescription, extendedData, exception, source);
        }

        internal static void TraceEventNoCheck(TraceEventType severity, int traceCode, string traceDescription, TraceRecord extendedData, object source, Exception exception, Guid activityId)
        {
            DiagnosticUtility.DiagnosticTrace.TraceEvent(severity, traceCode, GenerateMsdnTraceCode(traceCode), traceDescription, extendedData, exception, activityId, source);
        }

        internal static void TraceHttpConnectionInformation(string localEndpoint, string remoteEndpoint, object source)
        {
            if (DiagnosticUtility.ShouldTraceInformation)
            {
                Dictionary<string, string> dictionary2 = new Dictionary<string, string>(2);
                dictionary2.Add("LocalEndpoint", localEndpoint);
                dictionary2.Add("RemoteEndpoint", remoteEndpoint);
                Dictionary<string, string> dictionary = dictionary2;
                TraceEvent(TraceEventType.Information, 0x40018, System.ServiceModel.SR.GetString("TraceCodeConnectToIPEndpoint"), new DictionaryTraceRecord(dictionary), source, null);
            }
        }

        internal static void TraceUserCodeException(Exception e, MethodInfo method)
        {
            if (DiagnosticUtility.ShouldTraceWarning)
            {
                StringTraceRecord trace = new StringTraceRecord("Comment", System.ServiceModel.SR.GetString("SFxUserCodeThrewException", new object[] { method.DeclaringType.FullName, method.Name }));
                DiagnosticUtility.DiagnosticTrace.TraceEvent(TraceEventType.Warning, 0x80040, GenerateMsdnTraceCode(0x80040), System.ServiceModel.SR.GetString("TraceCodeUnhandledExceptionInUserOperation", new object[] { method.DeclaringType.FullName, method.Name }), trace, e, null);
            }
        }

        internal static void TransferFromTransport(Message message)
        {
            if ((message != null) && DiagnosticUtility.ShouldUseActivity)
            {
                Guid empty = Guid.Empty;
                if (ShouldPropagateActivity)
                {
                    empty = ActivityIdHeader.ExtractActivityId(message);
                }
                if (empty == Guid.Empty)
                {
                    empty = Guid.NewGuid();
                }
                ServiceModelActivity current = null;
                bool flag = true;
                if (ServiceModelActivity.Current != null)
                {
                    if ((ServiceModelActivity.Current.Id == empty) || (ServiceModelActivity.Current.ActivityType == ActivityType.ProcessAction))
                    {
                        current = ServiceModelActivity.Current;
                        flag = false;
                    }
                    else if ((ServiceModelActivity.Current.PreviousActivity != null) && (ServiceModelActivity.Current.PreviousActivity.Id == empty))
                    {
                        current = ServiceModelActivity.Current.PreviousActivity;
                        flag = false;
                    }
                }
                if (current == null)
                {
                    current = ServiceModelActivity.CreateActivity(empty);
                }
                if (DiagnosticUtility.ShouldUseActivity && flag)
                {
                    if (System.ServiceModel.Diagnostics.Application.FxTrace.Trace != null)
                    {
                        System.ServiceModel.Diagnostics.Application.FxTrace.Trace.TraceTransfer(empty);
                    }
                    ServiceModelActivity.Start(current, System.ServiceModel.SR.GetString("ActivityProcessAction", new object[] { message.Headers.Action }), ActivityType.ProcessAction);
                }
                message.Properties["ActivityId"] = current;
            }
        }

        internal static void UpdateAsyncOperationContextWithActivity(object activity)
        {
            if ((OperationContext.Current != null) && (activity != null))
            {
                OperationContext.Current.OutgoingMessageProperties["AsyncOperationActivity"] = activity;
            }
        }

        internal static void UpdateAsyncOperationContextWithStartTime(long startTime)
        {
            if (OperationContext.Current != null)
            {
                OperationContext.Current.OutgoingMessageProperties["AsyncOperationStartTime"] = startTime;
            }
        }

        internal static AsyncCallback WrapExecuteUserCodeAsyncCallback(AsyncCallback callback)
        {
            if (DiagnosticUtility.ShouldUseActivity && (callback != null))
            {
                return new ExecuteUserCodeAsync(callback).Callback;
            }
            return callback;
        }

        internal static bool ActivityTracing
        {
            get
            {
                return activityTracing;
            }
        }

        internal static bool MessageFlowTracing
        {
            get
            {
                return messageFlowTracing;
            }
        }

        internal static bool MessageFlowTracingOnly
        {
            get
            {
                return messageFlowTracingOnly;
            }
        }

        public static bool PropagateUserActivity
        {
            get
            {
                return (ShouldPropagateActivity && PropagateUserActivityCore);
            }
        }

        private static bool PropagateUserActivityCore
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get
            {
                return (!DiagnosticUtility.TracingEnabled && (System.ServiceModel.Diagnostics.DiagnosticTrace.ActivityId != Guid.Empty));
            }
        }

        internal static bool ShouldPropagateActivity
        {
            get
            {
                return shouldPropagateActivity;
            }
        }

        internal static bool ShouldPropagateActivityGlobal
        {
            get
            {
                return shouldPropagateActivityGlobal;
            }
        }

        private sealed class ExecuteUserCodeAsync
        {
            private AsyncCallback callback;

            public ExecuteUserCodeAsync(AsyncCallback callback)
            {
                this.callback = callback;
            }

            private void ExecuteUserCode(IAsyncResult result)
            {
                using (ServiceModelActivity activity = ServiceModelActivity.CreateBoundedActivity())
                {
                    ServiceModelActivity.Start(activity, System.ServiceModel.SR.GetString("ActivityCallback"), ActivityType.ExecuteUserCode);
                    this.callback(result);
                }
            }

            public AsyncCallback Callback
            {
                get
                {
                    return Fx.ThunkCallback(new AsyncCallback(this.ExecuteUserCode));
                }
            }
        }

        internal class TracingAsyncCallbackState
        {
            private Guid activityId;
            private object innerState;

            internal TracingAsyncCallbackState(object innerState)
            {
                this.innerState = innerState;
                this.activityId = System.ServiceModel.Diagnostics.DiagnosticTrace.ActivityId;
            }

            internal Guid ActivityId
            {
                get
                {
                    return this.activityId;
                }
            }

            internal object InnerState
            {
                get
                {
                    return this.innerState;
                }
            }
        }
    }
}

