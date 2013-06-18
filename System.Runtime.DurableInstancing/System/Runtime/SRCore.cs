namespace System.Runtime
{
    using System;
    using System.Globalization;
    using System.Resources;

    internal class SRCore
    {
        private static CultureInfo resourceCulture;
        private static System.Resources.ResourceManager resourceManager;

        private SRCore()
        {
        }

        internal static string ArgumentNullOrEmpty(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ArgumentNullOrEmpty", Culture), new object[] { param0 });
        }

        internal static string AsyncResultCompletedTwice(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("AsyncResultCompletedTwice", Culture), new object[] { param0 });
        }

        internal static string BufferAllocationFailed(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("BufferAllocationFailed", Culture), new object[] { param0 });
        }

        internal static string BufferedOutputStreamQuotaExceeded(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("BufferedOutputStreamQuotaExceeded", Culture), new object[] { param0 });
        }

        internal static string CannotAcquireLockSpecific(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("CannotAcquireLockSpecific", Culture), new object[] { param0 });
        }

        internal static string CannotAcquireLockSpecificWithOwner(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("CannotAcquireLockSpecificWithOwner", Culture), new object[] { param0, param1 });
        }

        internal static string CannotConvertObject(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("CannotConvertObject", Culture), new object[] { param0, param1 });
        }

        internal static string CouldNotResolveNamespacePrefix(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("CouldNotResolveNamespacePrefix", Culture), new object[] { param0 });
        }

        internal static string EtwAPIMaxStringCountExceeded(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("EtwAPIMaxStringCountExceeded", Culture), new object[] { param0 });
        }

        internal static string EtwMaxNumberArgumentsExceeded(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("EtwMaxNumberArgumentsExceeded", Culture), new object[] { param0 });
        }

        internal static string EtwRegistrationFailed(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("EtwRegistrationFailed", Culture), new object[] { param0 });
        }

        internal static string FailFastMessage(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("FailFastMessage", Culture), new object[] { param0 });
        }

        internal static string GenericInstanceCommand(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("GenericInstanceCommand", Culture), new object[] { param0 });
        }

        internal static string GetParameterTypeMismatch(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("GetParameterTypeMismatch", Culture), new object[] { param0, param1 });
        }

        internal static string IncompatibleArgumentType(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("IncompatibleArgumentType", Culture), new object[] { param0, param1 });
        }

        internal static string IncorrectValueType(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("IncorrectValueType", Culture), new object[] { param0, param1 });
        }

        internal static string InitialMetadataCannotBeDeleted(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("InitialMetadataCannotBeDeleted", Culture), new object[] { param0 });
        }

        internal static string InstanceCollisionSpecific(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("InstanceCollisionSpecific", Culture), new object[] { param0 });
        }

        internal static string InstanceCompleteSpecific(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("InstanceCompleteSpecific", Culture), new object[] { param0 });
        }

        internal static string InstanceHandleConflictSpecific(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("InstanceHandleConflictSpecific", Culture), new object[] { param0 });
        }

        internal static string InstanceLockLostSpecific(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("InstanceLockLostSpecific", Culture), new object[] { param0 });
        }

        internal static string InstanceNotReadySpecific(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("InstanceNotReadySpecific", Culture), new object[] { param0 });
        }

        internal static string InstanceOwnerSpecific(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("InstanceOwnerSpecific", Culture), new object[] { param0 });
        }

        internal static string InvalidAsyncResultImplementation(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("InvalidAsyncResultImplementation", Culture), new object[] { param0 });
        }

        internal static string KeyCollisionSpecific(object param0, object param1, object param2)
        {
            return string.Format(Culture, ResourceManager.GetString("KeyCollisionSpecific", Culture), new object[] { param0, param1, param2 });
        }

        internal static string KeyCollisionSpecificKeyOnly(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("KeyCollisionSpecificKeyOnly", Culture), new object[] { param0 });
        }

        internal static string KeyCompleteSpecific(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("KeyCompleteSpecific", Culture), new object[] { param0 });
        }

        internal static string KeyNotReadySpecific(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("KeyNotReadySpecific", Culture), new object[] { param0 });
        }

        internal static string LockTimeoutExceptionMessage(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("LockTimeoutExceptionMessage", Culture), new object[] { param0 });
        }

        internal static string MetadataCannotContainNullValue(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("MetadataCannotContainNullValue", Culture), new object[] { param0 });
        }

        internal static string NameCollisionOnCollect(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("NameCollisionOnCollect", Culture), new object[] { param0, param1 });
        }

        internal static string NameCollisionOnMap(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("NameCollisionOnMap", Culture), new object[] { param0, param1 });
        }

        internal static string NullAssignedToValueType(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("NullAssignedToValueType", Culture), new object[] { param0 });
        }

        internal static string OutsideInstanceExecutionScope(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("OutsideInstanceExecutionScope", Culture), new object[] { param0 });
        }

        internal static string OutsideTransactionalCommand(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("OutsideTransactionalCommand", Culture), new object[] { param0 });
        }

        internal static string PersistencePipelineAbortThrew(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("PersistencePipelineAbortThrew", Culture), new object[] { param0 });
        }

        internal static string ProviderDoesNotSupportCommand(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ProviderDoesNotSupportCommand", Culture), new object[] { param0 });
        }

        internal static string ShipAssertExceptionMessage(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("ShipAssertExceptionMessage", Culture), new object[] { param0 });
        }

        internal static string TimeoutInputQueueDequeue(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("TimeoutInputQueueDequeue", Culture), new object[] { param0 });
        }

        internal static string TimeoutMustBeNonNegative(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("TimeoutMustBeNonNegative", Culture), new object[] { param0, param1 });
        }

        internal static string TimeoutMustBePositive(object param0, object param1)
        {
            return string.Format(Culture, ResourceManager.GetString("TimeoutMustBePositive", Culture), new object[] { param0, param1 });
        }

        internal static string TimeoutOnOperation(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("TimeoutOnOperation", Culture), new object[] { param0 });
        }

        internal static string WaitForEventsTimedOut(object param0)
        {
            return string.Format(Culture, ResourceManager.GetString("WaitForEventsTimedOut", Culture), new object[] { param0 });
        }

        internal static string ActionItemIsAlreadyScheduled
        {
            get
            {
                return ResourceManager.GetString("ActionItemIsAlreadyScheduled", Culture);
            }
        }

        internal static string AlreadyBoundToInstance
        {
            get
            {
                return ResourceManager.GetString("AlreadyBoundToInstance", Culture);
            }
        }

        internal static string AlreadyBoundToOwner
        {
            get
            {
                return ResourceManager.GetString("AlreadyBoundToOwner", Culture);
            }
        }

        internal static string AsyncCallbackThrewException
        {
            get
            {
                return ResourceManager.GetString("AsyncCallbackThrewException", Culture);
            }
        }

        internal static string AsyncResultAlreadyEnded
        {
            get
            {
                return ResourceManager.GetString("AsyncResultAlreadyEnded", Culture);
            }
        }

        internal static string AsyncTransactionException
        {
            get
            {
                return ResourceManager.GetString("AsyncTransactionException", Culture);
            }
        }

        internal static string BadCopyToArray
        {
            get
            {
                return ResourceManager.GetString("BadCopyToArray", Culture);
            }
        }

        internal static string BindLockRequiresCommandFlag
        {
            get
            {
                return ResourceManager.GetString("BindLockRequiresCommandFlag", Culture);
            }
        }

        internal static string BindReclaimedLockException
        {
            get
            {
                return ResourceManager.GetString("BindReclaimedLockException", Culture);
            }
        }

        internal static string BindReclaimSucceeded
        {
            get
            {
                return ResourceManager.GetString("BindReclaimSucceeded", Culture);
            }
        }

        internal static string BufferIsNotRightSizeForBufferManager
        {
            get
            {
                return ResourceManager.GetString("BufferIsNotRightSizeForBufferManager", Culture);
            }
        }

        internal static string CannotAcquireLockDefault
        {
            get
            {
                return ResourceManager.GetString("CannotAcquireLockDefault", Culture);
            }
        }

        internal static string CannotCompleteWithKeys
        {
            get
            {
                return ResourceManager.GetString("CannotCompleteWithKeys", Culture);
            }
        }

        internal static string CannotCreateContextWithNullId
        {
            get
            {
                return ResourceManager.GetString("CannotCreateContextWithNullId", Culture);
            }
        }

        internal static string CannotInvokeBindingFromNonBinding
        {
            get
            {
                return ResourceManager.GetString("CannotInvokeBindingFromNonBinding", Culture);
            }
        }

        internal static string CannotInvokeTransactionalFromNonTransactional
        {
            get
            {
                return ResourceManager.GetString("CannotInvokeTransactionalFromNonTransactional", Culture);
            }
        }

        internal static string CannotReplaceTransaction
        {
            get
            {
                return ResourceManager.GetString("CannotReplaceTransaction", Culture);
            }
        }

        internal static string CommandExecutionCannotOverlap
        {
            get
            {
                return ResourceManager.GetString("CommandExecutionCannotOverlap", Culture);
            }
        }

        internal static string CompletedMustNotHaveAssociatedKeys
        {
            get
            {
                return ResourceManager.GetString("CompletedMustNotHaveAssociatedKeys", Culture);
            }
        }

        internal static string ContextAlreadyBoundToInstance
        {
            get
            {
                return ResourceManager.GetString("ContextAlreadyBoundToInstance", Culture);
            }
        }

        internal static string ContextAlreadyBoundToLock
        {
            get
            {
                return ResourceManager.GetString("ContextAlreadyBoundToLock", Culture);
            }
        }

        internal static string ContextAlreadyBoundToOwner
        {
            get
            {
                return ResourceManager.GetString("ContextAlreadyBoundToOwner", Culture);
            }
        }

        internal static string ContextMustBeBoundToInstance
        {
            get
            {
                return ResourceManager.GetString("ContextMustBeBoundToInstance", Culture);
            }
        }

        internal static string ContextMustBeBoundToOwner
        {
            get
            {
                return ResourceManager.GetString("ContextMustBeBoundToOwner", Culture);
            }
        }

        internal static string ContextNotFromThisStore
        {
            get
            {
                return ResourceManager.GetString("ContextNotFromThisStore", Culture);
            }
        }

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

        internal static string DictionaryIsReadOnly
        {
            get
            {
                return ResourceManager.GetString("DictionaryIsReadOnly", Culture);
            }
        }

        internal static string DoNotCompleteTryCommandWithPendingReclaim
        {
            get
            {
                return ResourceManager.GetString("DoNotCompleteTryCommandWithPendingReclaim", Culture);
            }
        }

        internal static string ExecuteMustBeNested
        {
            get
            {
                return ResourceManager.GetString("ExecuteMustBeNested", Culture);
            }
        }

        internal static string ExtensionsCannotBeSetByIndex
        {
            get
            {
                return ResourceManager.GetString("ExtensionsCannotBeSetByIndex", Culture);
            }
        }

        internal static string GenericInstanceCommandNull
        {
            get
            {
                return ResourceManager.GetString("GenericInstanceCommandNull", Culture);
            }
        }

        internal static string GuidCannotBeEmpty
        {
            get
            {
                return ResourceManager.GetString("GuidCannotBeEmpty", Culture);
            }
        }

        internal static string HandleFreed
        {
            get
            {
                return ResourceManager.GetString("HandleFreed", Culture);
            }
        }

        internal static string HandleFreedBeforeInitialized
        {
            get
            {
                return ResourceManager.GetString("HandleFreedBeforeInitialized", Culture);
            }
        }

        internal static string InstanceCollisionDefault
        {
            get
            {
                return ResourceManager.GetString("InstanceCollisionDefault", Culture);
            }
        }

        internal static string InstanceCompleteDefault
        {
            get
            {
                return ResourceManager.GetString("InstanceCompleteDefault", Culture);
            }
        }

        internal static string InstanceHandleConflictDefault
        {
            get
            {
                return ResourceManager.GetString("InstanceHandleConflictDefault", Culture);
            }
        }

        internal static string InstanceKeyRequiresValidGuid
        {
            get
            {
                return ResourceManager.GetString("InstanceKeyRequiresValidGuid", Culture);
            }
        }

        internal static string InstanceLockLostDefault
        {
            get
            {
                return ResourceManager.GetString("InstanceLockLostDefault", Culture);
            }
        }

        internal static string InstanceNotReadyDefault
        {
            get
            {
                return ResourceManager.GetString("InstanceNotReadyDefault", Culture);
            }
        }

        internal static string InstanceOperationRequiresInstance
        {
            get
            {
                return ResourceManager.GetString("InstanceOperationRequiresInstance", Culture);
            }
        }

        internal static string InstanceOperationRequiresLock
        {
            get
            {
                return ResourceManager.GetString("InstanceOperationRequiresLock", Culture);
            }
        }

        internal static string InstanceOperationRequiresNotCompleted
        {
            get
            {
                return ResourceManager.GetString("InstanceOperationRequiresNotCompleted", Culture);
            }
        }

        internal static string InstanceOperationRequiresNotUninitialized
        {
            get
            {
                return ResourceManager.GetString("InstanceOperationRequiresNotUninitialized", Culture);
            }
        }

        internal static string InstanceOperationRequiresOwner
        {
            get
            {
                return ResourceManager.GetString("InstanceOperationRequiresOwner", Culture);
            }
        }

        internal static string InstanceOwnerDefault
        {
            get
            {
                return ResourceManager.GetString("InstanceOwnerDefault", Culture);
            }
        }

        internal static string InstanceRequired
        {
            get
            {
                return ResourceManager.GetString("InstanceRequired", Culture);
            }
        }

        internal static string InstanceStoreBoundSameVersionTwice
        {
            get
            {
                return ResourceManager.GetString("InstanceStoreBoundSameVersionTwice", Culture);
            }
        }

        internal static string InvalidAsyncResult
        {
            get
            {
                return ResourceManager.GetString("InvalidAsyncResult", Culture);
            }
        }

        internal static string InvalidAsyncResultImplementationGeneric
        {
            get
            {
                return ResourceManager.GetString("InvalidAsyncResultImplementationGeneric", Culture);
            }
        }

        internal static string InvalidInstanceState
        {
            get
            {
                return ResourceManager.GetString("InvalidInstanceState", Culture);
            }
        }

        internal static string InvalidKeyArgument
        {
            get
            {
                return ResourceManager.GetString("InvalidKeyArgument", Culture);
            }
        }

        internal static string InvalidLockToken
        {
            get
            {
                return ResourceManager.GetString("InvalidLockToken", Culture);
            }
        }

        internal static string InvalidNullAsyncResult
        {
            get
            {
                return ResourceManager.GetString("InvalidNullAsyncResult", Culture);
            }
        }

        internal static string InvalidSemaphoreExit
        {
            get
            {
                return ResourceManager.GetString("InvalidSemaphoreExit", Culture);
            }
        }

        internal static string InvalidStateInAsyncResult
        {
            get
            {
                return ResourceManager.GetString("InvalidStateInAsyncResult", Culture);
            }
        }

        internal static string KeyAlreadyAssociated
        {
            get
            {
                return ResourceManager.GetString("KeyAlreadyAssociated", Culture);
            }
        }

        internal static string KeyAlreadyCompleted
        {
            get
            {
                return ResourceManager.GetString("KeyAlreadyCompleted", Culture);
            }
        }

        internal static string KeyAlreadyUnassociated
        {
            get
            {
                return ResourceManager.GetString("KeyAlreadyUnassociated", Culture);
            }
        }

        internal static string KeyCollisionDefault
        {
            get
            {
                return ResourceManager.GetString("KeyCollisionDefault", Culture);
            }
        }

        internal static string KeyCompleteDefault
        {
            get
            {
                return ResourceManager.GetString("KeyCompleteDefault", Culture);
            }
        }

        internal static string KeyNotAssociated
        {
            get
            {
                return ResourceManager.GetString("KeyNotAssociated", Culture);
            }
        }

        internal static string KeyNotCompleted
        {
            get
            {
                return ResourceManager.GetString("KeyNotCompleted", Culture);
            }
        }

        internal static string KeyNotFoundInDictionary
        {
            get
            {
                return ResourceManager.GetString("KeyNotFoundInDictionary", Culture);
            }
        }

        internal static string KeyNotReadyDefault
        {
            get
            {
                return ResourceManager.GetString("KeyNotReadyDefault", Culture);
            }
        }

        internal static string LoadedWriteOnlyValue
        {
            get
            {
                return ResourceManager.GetString("LoadedWriteOnlyValue", Culture);
            }
        }

        internal static string LoadOpAssociateKeysCannotContainLookupKey
        {
            get
            {
                return ResourceManager.GetString("LoadOpAssociateKeysCannotContainLookupKey", Culture);
            }
        }

        internal static string LoadOpFreeKeyRequiresAcceptUninitialized
        {
            get
            {
                return ResourceManager.GetString("LoadOpFreeKeyRequiresAcceptUninitialized", Culture);
            }
        }

        internal static string LoadOpKeyMustBeValid
        {
            get
            {
                return ResourceManager.GetString("LoadOpKeyMustBeValid", Culture);
            }
        }

        internal static string MayBindLockCommandShouldValidateOwner
        {
            get
            {
                return ResourceManager.GetString("MayBindLockCommandShouldValidateOwner", Culture);
            }
        }

        internal static string MetadataCannotContainNullKey
        {
            get
            {
                return ResourceManager.GetString("MetadataCannotContainNullKey", Culture);
            }
        }

        internal static string MustCancelOldTimer
        {
            get
            {
                return ResourceManager.GetString("MustCancelOldTimer", Culture);
            }
        }

        internal static string MustSetTransactionOnFirstCall
        {
            get
            {
                return ResourceManager.GetString("MustSetTransactionOnFirstCall", Culture);
            }
        }

        internal static string OnCancelRequestedThrew
        {
            get
            {
                return ResourceManager.GetString("OnCancelRequestedThrew", Culture);
            }
        }

        internal static string OnFreeInstanceHandleThrew
        {
            get
            {
                return ResourceManager.GetString("OnFreeInstanceHandleThrew", Culture);
            }
        }

        internal static string OwnerBelongsToWrongStore
        {
            get
            {
                return ResourceManager.GetString("OwnerBelongsToWrongStore", Culture);
            }
        }

        internal static string OwnerRequired
        {
            get
            {
                return ResourceManager.GetString("OwnerRequired", Culture);
            }
        }

        internal static string PersistenceInitializerThrew
        {
            get
            {
                return ResourceManager.GetString("PersistenceInitializerThrew", Culture);
            }
        }

        internal static string ReadNotSupported
        {
            get
            {
                return ResourceManager.GetString("ReadNotSupported", Culture);
            }
        }

        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (object.ReferenceEquals(resourceManager, null))
                {
                    System.Resources.ResourceManager manager = new System.Resources.ResourceManager("System.Runtime.SRCore", typeof(SRCore).Assembly);
                    resourceManager = manager;
                }
                return resourceManager;
            }
        }

        internal static string SeekNotSupported
        {
            get
            {
                return ResourceManager.GetString("SeekNotSupported", Culture);
            }
        }

        internal static string StoreReportedConflictingLockTokens
        {
            get
            {
                return ResourceManager.GetString("StoreReportedConflictingLockTokens", Culture);
            }
        }

        internal static string ThreadNeutralSemaphoreAborted
        {
            get
            {
                return ResourceManager.GetString("ThreadNeutralSemaphoreAborted", Culture);
            }
        }

        internal static string TimedOutWaitingForLockResolution
        {
            get
            {
                return ResourceManager.GetString("TimedOutWaitingForLockResolution", Culture);
            }
        }

        internal static string TransactionInDoubtNonHost
        {
            get
            {
                return ResourceManager.GetString("TransactionInDoubtNonHost", Culture);
            }
        }

        internal static string TransactionRolledBackNonHost
        {
            get
            {
                return ResourceManager.GetString("TransactionRolledBackNonHost", Culture);
            }
        }

        internal static string TryCommandCannotExecuteSubCommandsAndReduce
        {
            get
            {
                return ResourceManager.GetString("TryCommandCannotExecuteSubCommandsAndReduce", Culture);
            }
        }

        internal static string UninitializedCannotHaveData
        {
            get
            {
                return ResourceManager.GetString("UninitializedCannotHaveData", Culture);
            }
        }

        internal static string ValidateUnlockInstance
        {
            get
            {
                return ResourceManager.GetString("ValidateUnlockInstance", Culture);
            }
        }

        internal static string ValueMustBeNonNegative
        {
            get
            {
                return ResourceManager.GetString("ValueMustBeNonNegative", Culture);
            }
        }

        internal static string WaitAlreadyInProgress
        {
            get
            {
                return ResourceManager.GetString("WaitAlreadyInProgress", Culture);
            }
        }
    }
}

