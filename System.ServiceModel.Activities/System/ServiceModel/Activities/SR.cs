namespace System.ServiceModel.Activities
{
    using System;
    using System.Globalization;
    using System.Resources;

    internal class SR
    {
        private static CultureInfo resourceCulture;
        private static System.Resources.ResourceManager resourceManager;

        private SR()
        {
        }

        internal static string AbortInstanceOnTransactionFailureDoesNotMatch(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("AbortInstanceOnTransactionFailureDoesNotMatch", Culture), new object[] { param0, param1 });
        }

        internal static string ArgumentCannotHaveNullOrVoidType(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("ArgumentCannotHaveNullOrVoidType", Culture), new object[] { param0, param1 });
        }

        internal static string BufferedReceiveRequiresReceiveContext(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("BufferedReceiveRequiresReceiveContext", Culture), new object[] { param0 });
        }

        internal static string BusyCountTraceFormatString(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("BusyCountTraceFormatString", Culture), new object[] { param0 });
        }

        internal static string CannotNestTransactedReceiveScopeWhenAmbientHandleIsSuppressed(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("CannotNestTransactedReceiveScopeWhenAmbientHandleIsSuppressed", Culture), new object[] { param0 });
        }

        internal static string ConflictingValueName(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ConflictingValueName", Culture), new object[] { param0 });
        }

        internal static string ConnectionStringNameWrong(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ConnectionStringNameWrong", Culture), new object[] { param0 });
        }

        internal static string ContractNotFoundInAddServiceEndpoint(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("ContractNotFoundInAddServiceEndpoint", Culture), new object[] { param0, param1 });
        }

        internal static string CorrelationHandleInUse(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("CorrelationHandleInUse", Culture), new object[] { param0, param1 });
        }

        internal static string DuplicateInstanceKeyExists(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("DuplicateInstanceKeyExists", Culture), new object[] { param0 });
        }

        internal static string EndpointAddressNotSetInEndpoint(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("EndpointAddressNotSetInEndpoint", Culture), new object[] { param0 });
        }

        internal static string EndpointIncorrectlySet(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("EndpointIncorrectlySet", Culture), new object[] { param0, param1 });
        }

        internal static string EndpointNotSet(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("EndpointNotSet", Culture), new object[] { param0, param1 });
        }

        internal static string FailedToLoadBindingInControlEndpoint(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("FailedToLoadBindingInControlEndpoint", Culture), new object[] { param0, param1, param2 });
        }

        internal static string InitializeCorrelationRequiresWorkflowServiceHost(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("InitializeCorrelationRequiresWorkflowServiceHost", Culture), new object[] { param0 });
        }

        internal static string InstanceLockedUnderTransaction(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("InstanceLockedUnderTransaction", Culture), new object[] { param0, param1 });
        }

        internal static string InstanceSuspended(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("InstanceSuspended", Culture), new object[] { param0, param1 });
        }

        internal static string MissingBindingInEndpoint(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("MissingBindingInEndpoint", Culture), new object[] { param0, param1 });
        }

        internal static string MissingOperationName(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("MissingOperationName", Culture), new object[] { param0 });
        }

        internal static string MissingServiceContractName(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("MissingServiceContractName", Culture), new object[] { param0, param1 });
        }

        internal static string MissingUriInEndpoint(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("MissingUriInEndpoint", Culture), new object[] { param0, param1 });
        }

        internal static string NullCorrelationHandleInInitializeCorrelation(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("NullCorrelationHandleInInitializeCorrelation", Culture), new object[] { param0 });
        }

        internal static string OperationHasSerializerBehavior(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("OperationHasSerializerBehavior", Culture), new object[] { param0, param1, param2 });
        }

        internal static string OperationNotAvailable(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("OperationNotAvailable", Culture), new object[] { param0, param1 });
        }

        internal static string QueryCorrelationInitializerWithEmptyMessageQuerySet(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("QueryCorrelationInitializerWithEmptyMessageQuerySet", Culture), new object[] { param0, param1 });
        }

        internal static string ReceiveAndReceiveParametersHaveSameName(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ReceiveAndReceiveParametersHaveSameName", Culture), new object[] { param0 });
        }

        internal static string ReceiveMessageNeedsToPairWithSendMessageForTwoWayContract(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ReceiveMessageNeedsToPairWithSendMessageForTwoWayContract", Culture), new object[] { param0 });
        }

        internal static string ReceivePairedWithSendReplyAndSendReplyParameters(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ReceivePairedWithSendReplyAndSendReplyParameters", Culture), new object[] { param0 });
        }

        internal static string ReceiveParametersContentDoesNotSupportMessage(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("ReceiveParametersContentDoesNotSupportMessage", Culture), new object[] { param0, param1 });
        }

        internal static string ReceiveReplyRequestCannotBeNull(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ReceiveReplyRequestCannotBeNull", Culture), new object[] { param0 });
        }

        internal static string RelativeUriRequiresBinding(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("RelativeUriRequiresBinding", Culture), new object[] { param0, param1, param2 });
        }

        internal static string RelativeUriRequiresHost(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("RelativeUriRequiresHost", Culture), new object[] { param0, param1, param2 });
        }

        internal static string ReplyShouldNotIncludeRequestReplyHandle(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("ReplyShouldNotIncludeRequestReplyHandle", Culture), new object[] { param0, param1 });
        }

        internal static string SendMessageNeedsToPairWithReceiveMessageForTwoWayContract(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("SendMessageNeedsToPairWithReceiveMessageForTwoWayContract", Culture), new object[] { param0 });
        }

        internal static string SendParametersContentDoesNotSupportMessage(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("SendParametersContentDoesNotSupportMessage", Culture), new object[] { param0, param1 });
        }

        internal static string SendReplyRequestCannotBeNull(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("SendReplyRequestCannotBeNull", Culture), new object[] { param0 });
        }

        internal static string SendWithUninitializedCorrelatesWith(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("SendWithUninitializedCorrelatesWith", Culture), new object[] { param0 });
        }

        internal static string ServiceInstanceTerminated(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ServiceInstanceTerminated", Culture), new object[] { param0 });
        }

        internal static string ServiceInstanceUnloaded(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ServiceInstanceUnloaded", Culture), new object[] { param0 });
        }

        internal static string ServiceMetadataBehaviorNotFoundForServiceMetadataEndpoint(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ServiceMetadataBehaviorNotFoundForServiceMetadataEndpoint", Culture), new object[] { param0 });
        }

        internal static string TimeoutOnOperation(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("TimeoutOnOperation", Culture), new object[] { param0 });
        }

        internal static string TransactedReceiveScopeMustHaveValidReceive(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("TransactedReceiveScopeMustHaveValidReceive", Culture), new object[] { param0 });
        }

        internal static string TransactedReceiveScopeRequiresReceive(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("TransactedReceiveScopeRequiresReceive", Culture), new object[] { param0 });
        }

        internal static string TwoReceiveParametersWithSameNameButDifferentParameterCount(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("TwoReceiveParametersWithSameNameButDifferentParameterCount", Culture), new object[] { param0 });
        }

        internal static string TwoReceiveParametersWithSameNameButDifferentParameterName(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("TwoReceiveParametersWithSameNameButDifferentParameterName", Culture), new object[] { param0 });
        }

        internal static string TwoReceiveParametersWithSameNameButDifferentParameterType(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("TwoReceiveParametersWithSameNameButDifferentParameterType", Culture), new object[] { param0 });
        }

        internal static string TwoReceivesWithSameNameButDifferentAction(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("TwoReceivesWithSameNameButDifferentAction", Culture), new object[] { param0 });
        }

        internal static string TwoReceivesWithSameNameButDifferentIsOneWay(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("TwoReceivesWithSameNameButDifferentIsOneWay", Culture), new object[] { param0 });
        }

        internal static string TwoReceivesWithSameNameButDifferentTxProperties(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("TwoReceivesWithSameNameButDifferentTxProperties", Culture), new object[] { param0 });
        }

        internal static string TwoReceivesWithSameNameButDifferentValueType(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("TwoReceivesWithSameNameButDifferentValueType", Culture), new object[] { param0 });
        }

        internal static string TwoSendRepliesWithSameNameButDifferentAction(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("TwoSendRepliesWithSameNameButDifferentAction", Culture), new object[] { param0 });
        }

        internal static string TwoSendRepliesWithSameNameButDifferentValueType(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("TwoSendRepliesWithSameNameButDifferentValueType", Culture), new object[] { param0 });
        }

        internal static string TwoSendReplyParametersWithSameNameButDifferentParameterCount(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("TwoSendReplyParametersWithSameNameButDifferentParameterCount", Culture), new object[] { param0 });
        }

        internal static string TwoSendReplyParametersWithSameNameButDifferentParameterName(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("TwoSendReplyParametersWithSameNameButDifferentParameterName", Culture), new object[] { param0 });
        }

        internal static string TwoSendReplyParametersWithSameNameButDifferentParameterType(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("TwoSendReplyParametersWithSameNameButDifferentParameterType", Culture), new object[] { param0 });
        }

        internal static string ValueArgumentTypeNotDerivedFromValueType(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("ValueArgumentTypeNotDerivedFromValueType", Culture), new object[] { param0, param1 });
        }

        internal static string ValueCannotBeNegative(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ValueCannotBeNegative", Culture), new object[] { param0 });
        }

        internal static string ValueCannotBeNull(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("ValueCannotBeNull", Culture), new object[] { param0, param1 });
        }

        internal static string ValueTooLarge(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ValueTooLarge", Culture), new object[] { param0 });
        }

        internal static string WindowsGroupNotFound(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("WindowsGroupNotFound", Culture), new object[] { param0 });
        }

        internal static string WorkflowBehaviorWithNonWorkflowHost(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("WorkflowBehaviorWithNonWorkflowHost", Culture), new object[] { param0 });
        }

        internal static string WorkflowInstanceAborted(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("WorkflowInstanceAborted", Culture), new object[] { param0 });
        }

        internal static string WorkflowInstanceCompleted(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("WorkflowInstanceCompleted", Culture), new object[] { param0 });
        }

        internal static string WorkflowInstanceNotFoundInStore(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("WorkflowInstanceNotFoundInStore", Culture), new object[] { param0 });
        }

        internal static string WorkflowInstanceTerminated(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("WorkflowInstanceTerminated", Culture), new object[] { param0 });
        }

        internal static string WorkflowInstanceUnloaded(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("WorkflowInstanceUnloaded", Culture), new object[] { param0 });
        }

        internal static string BufferedReceiveBehaviorMultipleUse
        {
            get
            {
                return ResourceManager.GetString("BufferedReceiveBehaviorMultipleUse", Culture);
            }
        }

        internal static string BufferedReceiveBehaviorUsedWithoutProperty
        {
            get
            {
                return ResourceManager.GetString("BufferedReceiveBehaviorUsedWithoutProperty", Culture);
            }
        }

        internal static string CacheSettingsLocked
        {
            get
            {
                return ResourceManager.GetString("CacheSettingsLocked", Culture);
            }
        }

        internal static string CannotCreateMessageFault
        {
            get
            {
                return ResourceManager.GetString("CannotCreateMessageFault", Culture);
            }
        }

        internal static string CannotSpecifyBothConnectionStringAndName
        {
            get
            {
                return ResourceManager.GetString("CannotSpecifyBothConnectionStringAndName", Culture);
            }
        }

        internal static string CannotUseAddServiceEndpointOverloadForWorkflowServices
        {
            get
            {
                return ResourceManager.GetString("CannotUseAddServiceEndpointOverloadForWorkflowServices", Culture);
            }
        }

        internal static string CompensableActivityInsideTransactedReceiveScope
        {
            get
            {
                return ResourceManager.GetString("CompensableActivityInsideTransactedReceiveScope", Culture);
            }
        }

        internal static string ContextMismatchInContextAndCallBackContext
        {
            get
            {
                return ResourceManager.GetString("ContextMismatchInContextAndCallBackContext", Culture);
            }
        }

        internal static string ContractInferenceValidationForTransactionFlowBehavior
        {
            get
            {
                return ResourceManager.GetString("ContractInferenceValidationForTransactionFlowBehavior", Culture);
            }
        }

        internal static string CorrelatedContextRequiredForAnonymousSend
        {
            get
            {
                return ResourceManager.GetString("CorrelatedContextRequiredForAnonymousSend", Culture);
            }
        }

        internal static string CorrelationResponseContextShouldNotBeNull
        {
            get
            {
                return ResourceManager.GetString("CorrelationResponseContextShouldNotBeNull", Culture);
            }
        }

        internal static CultureInfo Culture
        {
            get
            {
                return resourceCulture;
            }
            set
            {
                resourceCulture = value;
            }
        }

        internal static string CurrentOperationCannotCreateInstance
        {
            get
            {
                return ResourceManager.GetString("CurrentOperationCannotCreateInstance", Culture);
            }
        }

        internal static string DanglingReceive
        {
            get
            {
                return ResourceManager.GetString("DanglingReceive", Culture);
            }
        }

        internal static string DefaultAbortReason
        {
            get
            {
                return ResourceManager.GetString("DefaultAbortReason", Culture);
            }
        }

        internal static string DefaultCreateOnlyReason
        {
            get
            {
                return ResourceManager.GetString("DefaultCreateOnlyReason", Culture);
            }
        }

        internal static string DefaultSuspendReason
        {
            get
            {
                return ResourceManager.GetString("DefaultSuspendReason", Culture);
            }
        }

        internal static string DefaultTerminationReason
        {
            get
            {
                return ResourceManager.GetString("DefaultTerminationReason", Culture);
            }
        }

        internal static string DifferentContractsSameConfigName
        {
            get
            {
                return ResourceManager.GetString("DifferentContractsSameConfigName", Culture);
            }
        }

        internal static string DirectoryAborted
        {
            get
            {
                return ResourceManager.GetString("DirectoryAborted", Culture);
            }
        }

        internal static string DispatchOperationInInvalidState
        {
            get
            {
                return ResourceManager.GetString("DispatchOperationInInvalidState", Culture);
            }
        }

        internal static string EmptyCorrelationQueryResults
        {
            get
            {
                return ResourceManager.GetString("EmptyCorrelationQueryResults", Culture);
            }
        }

        internal static string ErrorTimeToPersistLessThanZero
        {
            get
            {
                return ResourceManager.GetString("ErrorTimeToPersistLessThanZero", Culture);
            }
        }

        internal static string ErrorTimeToUnloadLessThanZero
        {
            get
            {
                return ResourceManager.GetString("ErrorTimeToUnloadLessThanZero", Culture);
            }
        }

        internal static string FailedToGetInstanceIdForControlOperation
        {
            get
            {
                return ResourceManager.GetString("FailedToGetInstanceIdForControlOperation", Culture);
            }
        }

        internal static string FlowedTransactionDifferentFromAmbient
        {
            get
            {
                return ResourceManager.GetString("FlowedTransactionDifferentFromAmbient", Culture);
            }
        }

        internal static string HandleFreedInDirectory
        {
            get
            {
                return ResourceManager.GetString("HandleFreedInDirectory", Culture);
            }
        }

        internal static string InstanceMustNotBeSuspended
        {
            get
            {
                return ResourceManager.GetString("InstanceMustNotBeSuspended", Culture);
            }
        }

        internal static string InternalServerError
        {
            get
            {
                return ResourceManager.GetString("InternalServerError", Culture);
            }
        }

        internal static string InvalidInstanceId
        {
            get
            {
                return ResourceManager.GetString("InvalidInstanceId", Culture);
            }
        }

        internal static string InvalidKey
        {
            get
            {
                return ResourceManager.GetString("InvalidKey", Culture);
            }
        }

        internal static string InvalidServiceImplementation
        {
            get
            {
                return ResourceManager.GetString("InvalidServiceImplementation", Culture);
            }
        }

        internal static string LoadingAborted
        {
            get
            {
                return ResourceManager.GetString("LoadingAborted", Culture);
            }
        }

        internal static string MaxPendingMessagesPerChannelMustBeGreaterThanZero
        {
            get
            {
                return ResourceManager.GetString("MaxPendingMessagesPerChannelMustBeGreaterThanZero", Culture);
            }
        }

        internal static string MissingBodyInWorkflowService
        {
            get
            {
                return ResourceManager.GetString("MissingBodyInWorkflowService", Culture);
            }
        }

        internal static string MissingDisplayNameInRootActivity
        {
            get
            {
                return ResourceManager.GetString("MissingDisplayNameInRootActivity", Culture);
            }
        }

        internal static string MustSpecifyConnectionStringOrName
        {
            get
            {
                return ResourceManager.GetString("MustSpecifyConnectionStringOrName", Culture);
            }
        }

        internal static string NoAdditionalKeysOnInstanceIdLoad
        {
            get
            {
                return ResourceManager.GetString("NoAdditionalKeysOnInstanceIdLoad", Culture);
            }
        }

        internal static string NoRunnableInstances
        {
            get
            {
                return ResourceManager.GetString("NoRunnableInstances", Culture);
            }
        }

        internal static string NotSpecified
        {
            get
            {
                return ResourceManager.GetString("NotSpecified", Culture);
            }
        }

        internal static string NullCorrelationHandleInMultipleQueryCorrelation
        {
            get
            {
                return ResourceManager.GetString("NullCorrelationHandleInMultipleQueryCorrelation", Culture);
            }
        }

        internal static string NullReplyMessageContractMismatch
        {
            get
            {
                return ResourceManager.GetString("NullReplyMessageContractMismatch", Culture);
            }
        }

        internal static string OperationFormatterAndFaultFormatterIncorrectlySet
        {
            get
            {
                return ResourceManager.GetString("OperationFormatterAndFaultFormatterIncorrectlySet", Culture);
            }
        }

        internal static string OperationFormatterAndFaultFormatterNotSet
        {
            get
            {
                return ResourceManager.GetString("OperationFormatterAndFaultFormatterNotSet", Culture);
            }
        }

        internal static string PartialTrustPerformanceCounterNotEnabled
        {
            get
            {
                return ResourceManager.GetString("PartialTrustPerformanceCounterNotEnabled", Culture);
            }
        }

        internal static string PersistenceProviderRequiredToPersist
        {
            get
            {
                return ResourceManager.GetString("PersistenceProviderRequiredToPersist", Culture);
            }
        }

        internal static string PersistenceTooLateToEnlist
        {
            get
            {
                return ResourceManager.GetString("PersistenceTooLateToEnlist", Culture);
            }
        }

        internal static string PersistenceViolationNoCreate
        {
            get
            {
                return ResourceManager.GetString("PersistenceViolationNoCreate", Culture);
            }
        }

        internal static string QueryCorrelationInitializerCannotBeInitialized
        {
            get
            {
                return ResourceManager.GetString("QueryCorrelationInitializerCannotBeInitialized", Culture);
            }
        }

        internal static string ReceiveNotWithinATransactedReceiveScope
        {
            get
            {
                return ResourceManager.GetString("ReceiveNotWithinATransactedReceiveScope", Culture);
            }
        }

        internal static string RequestReplyHandleShouldNotBePresentForOneWay
        {
            get
            {
                return ResourceManager.GetString("RequestReplyHandleShouldNotBePresentForOneWay", Culture);
            }
        }

        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceManager, null))
                {
                    System.Resources.ResourceManager manager = new System.Resources.ResourceManager("System.ServiceModel.Activities.SR", typeof(System.ServiceModel.Activities.SR).Assembly);
                    resourceManager = manager;
                }
                return resourceManager;
            }
        }

        internal static string ResponseContextIsNotNull
        {
            get
            {
                return ResourceManager.GetString("ResponseContextIsNotNull", Culture);
            }
        }

        internal static string SendRepliesHaveSameFaultTypeDifferentAction
        {
            get
            {
                return ResourceManager.GetString("SendRepliesHaveSameFaultTypeDifferentAction", Culture);
            }
        }

        internal static string ServiceHostExtensionAborted
        {
            get
            {
                return ResourceManager.GetString("ServiceHostExtensionAborted", Culture);
            }
        }

        internal static string ServiceHostExtensionImmutable
        {
            get
            {
                return ResourceManager.GetString("ServiceHostExtensionImmutable", Culture);
            }
        }

        internal static string StoreViolationNoInstanceBound
        {
            get
            {
                return ResourceManager.GetString("StoreViolationNoInstanceBound", Culture);
            }
        }

        internal static string TransactionPersistenceTimeout
        {
            get
            {
                return ResourceManager.GetString("TransactionPersistenceTimeout", Culture);
            }
        }

        internal static string TryRegisterRequestContextFailed
        {
            get
            {
                return ResourceManager.GetString("TryRegisterRequestContextFailed", Culture);
            }
        }

        internal static string UnableToOpenAndRegisterStore
        {
            get
            {
                return ResourceManager.GetString("UnableToOpenAndRegisterStore", Culture);
            }
        }

        internal static string UseInstanceStoreInsteadOfPersistenceProvider
        {
            get
            {
                return ResourceManager.GetString("UseInstanceStoreInsteadOfPersistenceProvider", Culture);
            }
        }

        internal static string WorkflowCompletionAsyncResultCannotBeNull
        {
            get
            {
                return ResourceManager.GetString("WorkflowCompletionAsyncResultCannotBeNull", Culture);
            }
        }

        internal static string WorkflowMustBeHosted
        {
            get
            {
                return ResourceManager.GetString("WorkflowMustBeHosted", Culture);
            }
        }
    }
}

