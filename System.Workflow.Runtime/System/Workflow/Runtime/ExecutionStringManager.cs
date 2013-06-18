namespace System.Workflow.Runtime
{
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Resources;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    [GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0"), DebuggerNonUserCode, CompilerGenerated]
    internal class ExecutionStringManager
    {
        private static CultureInfo resourceCulture;
        private static System.Resources.ResourceManager resourceMan;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal ExecutionStringManager()
        {
        }

        internal static string AsyncCallbackThrewException
        {
            get
            {
                return ResourceManager.GetString("AsyncCallbackThrewException", resourceCulture);
            }
        }

        internal static string AttemptToBindUnknownProperties
        {
            get
            {
                return ResourceManager.GetString("AttemptToBindUnknownProperties", resourceCulture);
            }
        }

        internal static string CannotAbortBeforeStart
        {
            get
            {
                return ResourceManager.GetString("CannotAbortBeforeStart", resourceCulture);
            }
        }

        internal static string CannotCauseEventInEvent
        {
            get
            {
                return ResourceManager.GetString("CannotCauseEventInEvent", resourceCulture);
            }
        }

        internal static string CannotCreateRootActivity
        {
            get
            {
                return ResourceManager.GetString("CannotCreateRootActivity", resourceCulture);
            }
        }

        internal static string CannotResetIsPrivate
        {
            get
            {
                return ResourceManager.GetString("CannotResetIsPrivate", resourceCulture);
            }
        }

        internal static string CannotStartInstanceTwice
        {
            get
            {
                return ResourceManager.GetString("CannotStartInstanceTwice", resourceCulture);
            }
        }

        internal static string CannotSuspendBeforeStart
        {
            get
            {
                return ResourceManager.GetString("CannotSuspendBeforeStart", resourceCulture);
            }
        }

        internal static string CantAddContainerToItself
        {
            get
            {
                return ResourceManager.GetString("CantAddContainerToItself", resourceCulture);
            }
        }

        internal static string CantAddServiceTwice
        {
            get
            {
                return ResourceManager.GetString("CantAddServiceTwice", resourceCulture);
            }
        }

        internal static string CantBeEmptyGuid
        {
            get
            {
                return ResourceManager.GetString("CantBeEmptyGuid", resourceCulture);
            }
        }

        internal static string CantChangeImmutableContainer
        {
            get
            {
                return ResourceManager.GetString("CantChangeImmutableContainer", resourceCulture);
            }
        }

        internal static string CantChangeNameAfterStart
        {
            get
            {
                return ResourceManager.GetString("CantChangeNameAfterStart", resourceCulture);
            }
        }

        internal static string CantRemoveServiceNotContained
        {
            get
            {
                return ResourceManager.GetString("CantRemoveServiceNotContained", resourceCulture);
            }
        }

        internal static string CompletedScopeNotFound
        {
            get
            {
                return ResourceManager.GetString("CompletedScopeNotFound", resourceCulture);
            }
        }

        internal static string ConfigurationSectionNotFound
        {
            get
            {
                return ResourceManager.GetString("ConfigurationSectionNotFound", resourceCulture);
            }
        }

        internal static string CorrelationAlreadyInitialized
        {
            get
            {
                return ResourceManager.GetString("CorrelationAlreadyInitialized", resourceCulture);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return resourceCulture;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                resourceCulture = value;
            }
        }

        internal static string DuplicateManualScheduleKey
        {
            get
            {
                return ResourceManager.GetString("DuplicateManualScheduleKey", resourceCulture);
            }
        }

        internal static string DynamicUpdateIsNotPending
        {
            get
            {
                return ResourceManager.GetString("DynamicUpdateIsNotPending", resourceCulture);
            }
        }

        internal static string EndCalledTwice
        {
            get
            {
                return ResourceManager.GetString("EndCalledTwice", resourceCulture);
            }
        }

        internal static string Error_InsideAtomicScope
        {
            get
            {
                return ResourceManager.GetString("Error_InsideAtomicScope", resourceCulture);
            }
        }

        internal static string EventQueueException
        {
            get
            {
                return ResourceManager.GetString("EventQueueException", resourceCulture);
            }
        }

        internal static string InstanceIDNotFound
        {
            get
            {
                return ResourceManager.GetString("InstanceIDNotFound", resourceCulture);
            }
        }

        internal static string InstanceNotFound
        {
            get
            {
                return ResourceManager.GetString("InstanceNotFound", resourceCulture);
            }
        }

        internal static string InstanceOperationNotValidinWorkflowThread
        {
            get
            {
                return ResourceManager.GetString("InstanceOperationNotValidinWorkflowThread", resourceCulture);
            }
        }

        internal static string InteropArgumentDirectionMismatch
        {
            get
            {
                return ResourceManager.GetString("InteropArgumentDirectionMismatch", resourceCulture);
            }
        }

        internal static string InteropBodyMustHavePublicDefaultConstructor
        {
            get
            {
                return ResourceManager.GetString("InteropBodyMustHavePublicDefaultConstructor", resourceCulture);
            }
        }

        internal static string InteropBodyNestedPersistOnCloseWithinTransactionScope
        {
            get
            {
                return ResourceManager.GetString("InteropBodyNestedPersistOnCloseWithinTransactionScope", resourceCulture);
            }
        }

        internal static string InteropBodyNestedTransactionScope
        {
            get
            {
                return ResourceManager.GetString("InteropBodyNestedTransactionScope", resourceCulture);
            }
        }

        internal static string InteropBodyNestedViolation
        {
            get
            {
                return ResourceManager.GetString("InteropBodyNestedViolation", resourceCulture);
            }
        }

        internal static string InteropBodyNotSet
        {
            get
            {
                return ResourceManager.GetString("InteropBodyNotSet", resourceCulture);
            }
        }

        internal static string InteropBodyRootLevelViolation
        {
            get
            {
                return ResourceManager.GetString("InteropBodyRootLevelViolation", resourceCulture);
            }
        }

        internal static string InteropCantFindTimerExtension
        {
            get
            {
                return ResourceManager.GetString("InteropCantFindTimerExtension", resourceCulture);
            }
        }

        internal static string InteropExceptionTraceMessage
        {
            get
            {
                return ResourceManager.GetString("InteropExceptionTraceMessage", resourceCulture);
            }
        }

        internal static string InteropInvalidPropertyDescriptor
        {
            get
            {
                return ResourceManager.GetString("InteropInvalidPropertyDescriptor", resourceCulture);
            }
        }

        internal static string InteropNonSupportedBehavior
        {
            get
            {
                return ResourceManager.GetString("InteropNonSupportedBehavior", resourceCulture);
            }
        }

        internal static string InteropTimerIdCantBeEmpty
        {
            get
            {
                return ResourceManager.GetString("InteropTimerIdCantBeEmpty", resourceCulture);
            }
        }

        internal static string InteropWorkflowRuntimeServiceNotSupported
        {
            get
            {
                return ResourceManager.GetString("InteropWorkflowRuntimeServiceNotSupported", resourceCulture);
            }
        }

        internal static string InteropWrongBody
        {
            get
            {
                return ResourceManager.GetString("InteropWrongBody", resourceCulture);
            }
        }

        internal static string InvalidActivityEventReader
        {
            get
            {
                return ResourceManager.GetString("InvalidActivityEventReader", resourceCulture);
            }
        }

        internal static string InvalidActivityName
        {
            get
            {
                return ResourceManager.GetString("InvalidActivityName", resourceCulture);
            }
        }

        internal static string InvalidActivityTrackingRecordParameter
        {
            get
            {
                return ResourceManager.GetString("InvalidActivityTrackingRecordParameter", resourceCulture);
            }
        }

        internal static string InvalidArgumentType
        {
            get
            {
                return ResourceManager.GetString("InvalidArgumentType", resourceCulture);
            }
        }

        internal static string InvalidAsyncResult
        {
            get
            {
                return ResourceManager.GetString("InvalidAsyncResult", resourceCulture);
            }
        }

        internal static string InvalidAttemptToLoad
        {
            get
            {
                return ResourceManager.GetString("InvalidAttemptToLoad", resourceCulture);
            }
        }

        internal static string InvalidAttemptToSuspend
        {
            get
            {
                return ResourceManager.GetString("InvalidAttemptToSuspend", resourceCulture);
            }
        }

        internal static string InvalidCacheItem
        {
            get
            {
                return ResourceManager.GetString("InvalidCacheItem", resourceCulture);
            }
        }

        internal static string InvalidCommandBadConnection
        {
            get
            {
                return ResourceManager.GetString("InvalidCommandBadConnection", resourceCulture);
            }
        }

        internal static string InvalidConnection
        {
            get
            {
                return ResourceManager.GetString("InvalidConnection", resourceCulture);
            }
        }

        internal static string InvalidDbConnection
        {
            get
            {
                return ResourceManager.GetString("InvalidDbConnection", resourceCulture);
            }
        }

        internal static string InvalidDefinitionReader
        {
            get
            {
                return ResourceManager.GetString("InvalidDefinitionReader", resourceCulture);
            }
        }

        internal static string InvalidEnlist
        {
            get
            {
                return ResourceManager.GetString("InvalidEnlist", resourceCulture);
            }
        }

        internal static string InvalidEventSourceName
        {
            get
            {
                return ResourceManager.GetString("InvalidEventSourceName", resourceCulture);
            }
        }

        internal static string InvalidExecutionContext
        {
            get
            {
                return ResourceManager.GetString("InvalidExecutionContext", resourceCulture);
            }
        }

        internal static string InvalidMember
        {
            get
            {
                return ResourceManager.GetString("InvalidMember", resourceCulture);
            }
        }

        internal static string InvalidOpConnectionNotLocal
        {
            get
            {
                return ResourceManager.GetString("InvalidOpConnectionNotLocal", resourceCulture);
            }
        }

        internal static string InvalidOpConnectionReset
        {
            get
            {
                return ResourceManager.GetString("InvalidOpConnectionReset", resourceCulture);
            }
        }

        internal static string InvalidOperationRequest
        {
            get
            {
                return ResourceManager.GetString("InvalidOperationRequest", resourceCulture);
            }
        }

        internal static string InvalidOwnershipTimeoutValue
        {
            get
            {
                return ResourceManager.GetString("InvalidOwnershipTimeoutValue", resourceCulture);
            }
        }

        internal static string InvalidProfileCheckValue
        {
            get
            {
                return ResourceManager.GetString("InvalidProfileCheckValue", resourceCulture);
            }
        }

        internal static string InvalidProfileVersion
        {
            get
            {
                return ResourceManager.GetString("InvalidProfileVersion", resourceCulture);
            }
        }

        internal static string InvalidRevertRequest
        {
            get
            {
                return ResourceManager.GetString("InvalidRevertRequest", resourceCulture);
            }
        }

        internal static string InvalidSenderWorkflowExecutor
        {
            get
            {
                return ResourceManager.GetString("InvalidSenderWorkflowExecutor", resourceCulture);
            }
        }

        internal static string InvalidSqlDataReader
        {
            get
            {
                return ResourceManager.GetString("InvalidSqlDataReader", resourceCulture);
            }
        }

        internal static string InvalidStatus
        {
            get
            {
                return ResourceManager.GetString("InvalidStatus", resourceCulture);
            }
        }

        internal static string InvalidTrackingService
        {
            get
            {
                return ResourceManager.GetString("InvalidTrackingService", resourceCulture);
            }
        }

        internal static string InvalidTransaction
        {
            get
            {
                return ResourceManager.GetString("InvalidTransaction", resourceCulture);
            }
        }

        internal static string InvalidUserEventReader
        {
            get
            {
                return ResourceManager.GetString("InvalidUserEventReader", resourceCulture);
            }
        }

        internal static string InvalidUserTrackingRecordParameter
        {
            get
            {
                return ResourceManager.GetString("InvalidUserTrackingRecordParameter", resourceCulture);
            }
        }

        internal static string InvalidWaitForIdleOnSuspendedWorkflow
        {
            get
            {
                return ResourceManager.GetString("InvalidWaitForIdleOnSuspendedWorkflow", resourceCulture);
            }
        }

        internal static string InvalidWorkflowChangeArgs
        {
            get
            {
                return ResourceManager.GetString("InvalidWorkflowChangeArgs", resourceCulture);
            }
        }

        internal static string InvalidWorkflowChangeEventArgsParameter
        {
            get
            {
                return ResourceManager.GetString("InvalidWorkflowChangeEventArgsParameter", resourceCulture);
            }
        }

        internal static string InvalidWorkflowChangeEventArgsReader
        {
            get
            {
                return ResourceManager.GetString("InvalidWorkflowChangeEventArgsReader", resourceCulture);
            }
        }

        internal static string InvalidWorkflowEvent
        {
            get
            {
                return ResourceManager.GetString("InvalidWorkflowEvent", resourceCulture);
            }
        }

        internal static string InvalidWorkflowInstanceEventReader
        {
            get
            {
                return ResourceManager.GetString("InvalidWorkflowInstanceEventReader", resourceCulture);
            }
        }

        internal static string InvalidWorkflowInstanceInternalId
        {
            get
            {
                return ResourceManager.GetString("InvalidWorkflowInstanceInternalId", resourceCulture);
            }
        }

        internal static string InvalidWorkflowParameterValue
        {
            get
            {
                return ResourceManager.GetString("InvalidWorkflowParameterValue", resourceCulture);
            }
        }

        internal static string InvalidWorkflowRuntimeConfiguration
        {
            get
            {
                return ResourceManager.GetString("InvalidWorkflowRuntimeConfiguration", resourceCulture);
            }
        }

        internal static string InvalidWorkflowTrackingRecordParameter
        {
            get
            {
                return ResourceManager.GetString("InvalidWorkflowTrackingRecordParameter", resourceCulture);
            }
        }

        internal static string InvalidXAML
        {
            get
            {
                return ResourceManager.GetString("InvalidXAML", resourceCulture);
            }
        }

        internal static string ItemAlreadyExist
        {
            get
            {
                return ResourceManager.GetString("ItemAlreadyExist", resourceCulture);
            }
        }

        internal static string ListenerNotInCache
        {
            get
            {
                return ResourceManager.GetString("ListenerNotInCache", resourceCulture);
            }
        }

        internal static string ListenerNotInCacheDisposed
        {
            get
            {
                return ResourceManager.GetString("ListenerNotInCacheDisposed", resourceCulture);
            }
        }

        internal static string LoadContextActivityFailed
        {
            get
            {
                return ResourceManager.GetString("LoadContextActivityFailed", resourceCulture);
            }
        }

        internal static string LoadingIntervalTooLarge
        {
            get
            {
                return ResourceManager.GetString("LoadingIntervalTooLarge", resourceCulture);
            }
        }

        internal static string MetaPropertyDoesNotExist
        {
            get
            {
                return ResourceManager.GetString("MetaPropertyDoesNotExist", resourceCulture);
            }
        }

        internal static string MissingActivityEvents
        {
            get
            {
                return ResourceManager.GetString("MissingActivityEvents", resourceCulture);
            }
        }

        internal static string MissingActivityType
        {
            get
            {
                return ResourceManager.GetString("MissingActivityType", resourceCulture);
            }
        }

        internal static string MissingArgumentType
        {
            get
            {
                return ResourceManager.GetString("MissingArgumentType", resourceCulture);
            }
        }

        internal static string MissingConnectionString
        {
            get
            {
                return ResourceManager.GetString("MissingConnectionString", resourceCulture);
            }
        }

        internal static string MissingMemberName
        {
            get
            {
                return ResourceManager.GetString("MissingMemberName", resourceCulture);
            }
        }

        internal static string MissingParameters
        {
            get
            {
                return ResourceManager.GetString("MissingParameters", resourceCulture);
            }
        }

        internal static string MissingParametersTrack
        {
            get
            {
                return ResourceManager.GetString("MissingParametersTrack", resourceCulture);
            }
        }

        internal static string MissingPersistenceService
        {
            get
            {
                return ResourceManager.GetString("MissingPersistenceService", resourceCulture);
            }
        }

        internal static string MissingPersistenceServiceWithPersistOnClose
        {
            get
            {
                return ResourceManager.GetString("MissingPersistenceServiceWithPersistOnClose", resourceCulture);
            }
        }

        internal static string MissingProfileForService
        {
            get
            {
                return ResourceManager.GetString("MissingProfileForService", resourceCulture);
            }
        }

        internal static string MissingProfileForVersion
        {
            get
            {
                return ResourceManager.GetString("MissingProfileForVersion", resourceCulture);
            }
        }

        internal static string MissingTrackingService
        {
            get
            {
                return ResourceManager.GetString("MissingTrackingService", resourceCulture);
            }
        }

        internal static string MissingWorkflowEvents
        {
            get
            {
                return ResourceManager.GetString("MissingWorkflowEvents", resourceCulture);
            }
        }

        internal static string MoreThanOneRuntime
        {
            get
            {
                return ResourceManager.GetString("MoreThanOneRuntime", resourceCulture);
            }
        }

        internal static string MoreThanOneService
        {
            get
            {
                return ResourceManager.GetString("MoreThanOneService", resourceCulture);
            }
        }

        internal static string MustUseRuntimeThread
        {
            get
            {
                return ResourceManager.GetString("MustUseRuntimeThread", resourceCulture);
            }
        }

        internal static string NoChannels
        {
            get
            {
                return ResourceManager.GetString("NoChannels", resourceCulture);
            }
        }

        internal static string NoMatchingLocation
        {
            get
            {
                return ResourceManager.GetString("NoMatchingLocation", resourceCulture);
            }
        }

        internal static string NoMatchingLocations
        {
            get
            {
                return ResourceManager.GetString("NoMatchingLocations", resourceCulture);
            }
        }

        internal static string NoReaderLock
        {
            get
            {
                return ResourceManager.GetString("NoReaderLock", resourceCulture);
            }
        }

        internal static string NullAmbientTransaction
        {
            get
            {
                return ResourceManager.GetString("NullAmbientTransaction", resourceCulture);
            }
        }

        internal static string NullChannel
        {
            get
            {
                return ResourceManager.GetString("NullChannel", resourceCulture);
            }
        }

        internal static string NullEngine
        {
            get
            {
                return ResourceManager.GetString("NullEngine", resourceCulture);
            }
        }

        internal static string NullParameters
        {
            get
            {
                return ResourceManager.GetString("NullParameters", resourceCulture);
            }
        }

        internal static string NullProfileForChannel
        {
            get
            {
                return ResourceManager.GetString("NullProfileForChannel", resourceCulture);
            }
        }

        internal static string NullTrackingBroker
        {
            get
            {
                return ResourceManager.GetString("NullTrackingBroker", resourceCulture);
            }
        }

        internal static string OwnerActivityMissing
        {
            get
            {
                return ResourceManager.GetString("OwnerActivityMissing", resourceCulture);
            }
        }

        internal static string PerformanceCounterCategory
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterCategory", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesAbortedDescription
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesAbortedDescription", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesAbortedName
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesAbortedName", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesAbortedRateDescription
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesAbortedRateDescription", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesAbortedRateName
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesAbortedRateName", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesCompletedDescription
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesCompletedDescription", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesCompletedName
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesCompletedName", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesCompletedRateDescription
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesCompletedRateDescription", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesCompletedRateName
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesCompletedRateName", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesCreatedDescription
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesCreatedDescription", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesCreatedName
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesCreatedName", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesCreatedRateDescription
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesCreatedRateDescription", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesCreatedRateName
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesCreatedRateName", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesExecutingDescription
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesExecutingDescription", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesExecutingName
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesExecutingName", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesIdleRateDescription
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesIdleRateDescription", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesIdleRateName
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesIdleRateName", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesInMemoryDescription
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesInMemoryDescription", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesInMemoryName
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesInMemoryName", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesLoadedDescription
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesLoadedDescription", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesLoadedName
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesLoadedName", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesLoadedRateDescription
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesLoadedRateDescription", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesLoadedRateName
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesLoadedRateName", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesPersistedDescription
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesPersistedDescription", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesPersistedName
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesPersistedName", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesPersistedRateDescription
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesPersistedRateDescription", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesPersistedRateName
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesPersistedRateName", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesRunnableDescription
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesRunnableDescription", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesRunnableName
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesRunnableName", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesSuspendedDescription
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesSuspendedDescription", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesSuspendedName
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesSuspendedName", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesSuspendedRateDescription
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesSuspendedRateDescription", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesSuspendedRateName
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesSuspendedRateName", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesTerminatedDescription
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesTerminatedDescription", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesTerminatedName
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesTerminatedName", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesTerminatedRateDescription
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesTerminatedRateDescription", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesTerminatedRateName
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesTerminatedRateName", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesUnloadedDescription
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesUnloadedDescription", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesUnloadedName
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesUnloadedName", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesUnloadedRateDescription
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesUnloadedRateDescription", resourceCulture);
            }
        }

        internal static string PerformanceCounterSchedulesUnloadedRateName
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterSchedulesUnloadedRateName", resourceCulture);
            }
        }

        internal static string PerformanceCounterWorkflowsWaitingDescription
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterWorkflowsWaitingDescription", resourceCulture);
            }
        }

        internal static string PerformanceCounterWorkflowsWaitingName
        {
            get
            {
                return ResourceManager.GetString("PerformanceCounterWorkflowsWaitingName", resourceCulture);
            }
        }

        internal static string PersistenceException
        {
            get
            {
                return ResourceManager.GetString("PersistenceException", resourceCulture);
            }
        }

        internal static string ProfileCacheInsertFailure
        {
            get
            {
                return ResourceManager.GetString("ProfileCacheInsertFailure", resourceCulture);
            }
        }

        internal static string ProfileIsNotPrivate
        {
            get
            {
                return ResourceManager.GetString("ProfileIsNotPrivate", resourceCulture);
            }
        }

        internal static string PromotionNotSupported
        {
            get
            {
                return ResourceManager.GetString("PromotionNotSupported", resourceCulture);
            }
        }

        internal static string QueueBusyException
        {
            get
            {
                return ResourceManager.GetString("QueueBusyException", resourceCulture);
            }
        }

        internal static string QueueNotEnabled
        {
            get
            {
                return ResourceManager.GetString("QueueNotEnabled", resourceCulture);
            }
        }

        internal static string QueueNotFound
        {
            get
            {
                return ResourceManager.GetString("QueueNotFound", resourceCulture);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceMan, null))
                {
                    System.Resources.ResourceManager manager = new System.Resources.ResourceManager("System.Workflow.Runtime.ExecutionStringManager", typeof(ExecutionStringManager).Assembly);
                    resourceMan = manager;
                }
                return resourceMan;
            }
        }

        internal static string RTProfileActCacheDupKey
        {
            get
            {
                return ResourceManager.GetString("RTProfileActCacheDupKey", resourceCulture);
            }
        }

        internal static string RTProfileDynamicActCacheIsNull
        {
            get
            {
                return ResourceManager.GetString("RTProfileDynamicActCacheIsNull", resourceCulture);
            }
        }

        internal static string SemanticErrorInvalidNamedParameter
        {
            get
            {
                return ResourceManager.GetString("SemanticErrorInvalidNamedParameter", resourceCulture);
            }
        }

        internal static string ServiceAlreadyStarted
        {
            get
            {
                return ResourceManager.GetString("ServiceAlreadyStarted", resourceCulture);
            }
        }

        internal static string ServiceNotAddedToRuntime
        {
            get
            {
                return ResourceManager.GetString("ServiceNotAddedToRuntime", resourceCulture);
            }
        }

        internal static string ServiceNotStarted
        {
            get
            {
                return ResourceManager.GetString("ServiceNotStarted", resourceCulture);
            }
        }

        internal static string SharedConnectionStringSpecificationConflict
        {
            get
            {
                return ResourceManager.GetString("SharedConnectionStringSpecificationConflict", resourceCulture);
            }
        }

        internal static string SqlTrackingTypeNotFound
        {
            get
            {
                return ResourceManager.GetString("SqlTrackingTypeNotFound", resourceCulture);
            }
        }

        internal static string TerminatedEventLogText
        {
            get
            {
                return ResourceManager.GetString("TerminatedEventLogText", resourceCulture);
            }
        }

        internal static string TrackingDeserializationCloseElementNotFound
        {
            get
            {
                return ResourceManager.GetString("TrackingDeserializationCloseElementNotFound", resourceCulture);
            }
        }

        internal static string TrackingDeserializationInvalidPosition
        {
            get
            {
                return ResourceManager.GetString("TrackingDeserializationInvalidPosition", resourceCulture);
            }
        }

        internal static string TrackingDeserializationInvalidType
        {
            get
            {
                return ResourceManager.GetString("TrackingDeserializationInvalidType", resourceCulture);
            }
        }

        internal static string TrackingDeserializationSchemaError
        {
            get
            {
                return ResourceManager.GetString("TrackingDeserializationSchemaError", resourceCulture);
            }
        }

        internal static string TrackingProfileInvalidMember
        {
            get
            {
                return ResourceManager.GetString("TrackingProfileInvalidMember", resourceCulture);
            }
        }

        internal static string TrackingProfileManagerNotInitialized
        {
            get
            {
                return ResourceManager.GetString("TrackingProfileManagerNotInitialized", resourceCulture);
            }
        }

        internal static string TrackingProfileUpdate
        {
            get
            {
                return ResourceManager.GetString("TrackingProfileUpdate", resourceCulture);
            }
        }

        internal static string TrackingRecord
        {
            get
            {
                return ResourceManager.GetString("TrackingRecord", resourceCulture);
            }
        }

        internal static string TrackingSerializationInvalidExtract
        {
            get
            {
                return ResourceManager.GetString("TrackingSerializationInvalidExtract", resourceCulture);
            }
        }

        internal static string TrackingSerializationNoTrackPoints
        {
            get
            {
                return ResourceManager.GetString("TrackingSerializationNoTrackPoints", resourceCulture);
            }
        }

        internal static string TypeMustHavePublicDefaultConstructor
        {
            get
            {
                return ResourceManager.GetString("TypeMustHavePublicDefaultConstructor", resourceCulture);
            }
        }

        internal static string TypeMustImplementRootActivity
        {
            get
            {
                return ResourceManager.GetString("TypeMustImplementRootActivity", resourceCulture);
            }
        }

        internal static string UnknownActivityActionType
        {
            get
            {
                return ResourceManager.GetString("UnknownActivityActionType", resourceCulture);
            }
        }

        internal static string UnknownConfigurationParameter
        {
            get
            {
                return ResourceManager.GetString("UnknownConfigurationParameter", resourceCulture);
            }
        }

        internal static string UnsupportedSqlProvider
        {
            get
            {
                return ResourceManager.GetString("UnsupportedSqlProvider", resourceCulture);
            }
        }

        internal static string UpdatedProfile
        {
            get
            {
                return ResourceManager.GetString("UpdatedProfile", resourceCulture);
            }
        }

        internal static string WorkBatchNotFound
        {
            get
            {
                return ResourceManager.GetString("WorkBatchNotFound", resourceCulture);
            }
        }

        internal static string WorkflowChange
        {
            get
            {
                return ResourceManager.GetString("WorkflowChange", resourceCulture);
            }
        }

        internal static string WorkflowMarkupDeserializationError
        {
            get
            {
                return ResourceManager.GetString("WorkflowMarkupDeserializationError", resourceCulture);
            }
        }

        internal static string WorkflowNotValid
        {
            get
            {
                return ResourceManager.GetString("WorkflowNotValid", resourceCulture);
            }
        }

        internal static string WorkflowOwnershipException
        {
            get
            {
                return ResourceManager.GetString("WorkflowOwnershipException", resourceCulture);
            }
        }

        internal static string WorkflowRuntimeNotStarted
        {
            get
            {
                return ResourceManager.GetString("WorkflowRuntimeNotStarted", resourceCulture);
            }
        }

        internal static string WorkflowTypeMismatch
        {
            get
            {
                return ResourceManager.GetString("WorkflowTypeMismatch", resourceCulture);
            }
        }

        internal static string WorkflowValidationFailure
        {
            get
            {
                return ResourceManager.GetString("WorkflowValidationFailure", resourceCulture);
            }
        }

        internal static string WorkflowWithIdAlreadyExists
        {
            get
            {
                return ResourceManager.GetString("WorkflowWithIdAlreadyExists", resourceCulture);
            }
        }

        internal static string XomlWorkflowHasClassName
        {
            get
            {
                return ResourceManager.GetString("XomlWorkflowHasClassName", resourceCulture);
            }
        }

        internal static string XomlWorkflowHasCode
        {
            get
            {
                return ResourceManager.GetString("XomlWorkflowHasCode", resourceCulture);
            }
        }
    }
}

