namespace System.Transactions
{
    using System;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal sealed class SR
    {
        internal const string ArgumentWrongType = "ArgumentWrongType";
        internal const string BadAsyncResult = "BadAsyncResult";
        internal const string BadResourceManagerId = "BadResourceManagerId";
        internal const string CannotAddToClosedDocument = "CannotAddToClosedDocument";
        internal const string CannotGetPrepareInfo = "CannotGetPrepareInfo";
        internal const string CannotGetTransactionIdentifier = "CannotGetTransactionIdentifier";
        internal const string CannotPromoteSnapshot = "CannotPromoteSnapshot";
        internal const string CannotSetCurrent = "CannotSetCurrent";
        internal const string CannotSupportNodeNameSpecification = "CannotSupportNodeNameSpecification";
        internal const string ConfigDistributedTransactionManagerName = "ConfigDistributedTransactionManagerName";
        internal const string ConfigInvalidConfigurationValue = "ConfigInvalidConfigurationValue";
        internal const string ConfigInvalidTimeSpanValue = "ConfigInvalidTimeSpanValue";
        internal const string ConfigNull = "ConfigNull";
        internal const string ConfigurationSectionNotFound = "ConfigurationSectionNotFound";
        internal const string CurrentDelegateSet = "CurrentDelegateSet";
        internal const string DisposeScope = "DisposeScope";
        internal const string DistributedTransactionManager = "DistributedTransactionManager";
        internal const string DocumentAlreadyClosed = "DocumentAlreadyClosed";
        internal const string DtcTransactionManagerUnavailable = "DtcTransactionManagerUnavailable";
        internal const string DuplicateRecoveryComplete = "DuplicateRecoveryComplete";
        internal const string EnlistmentStateException = "EnlistmentStateException";
        internal const string EsNotSupported = "EsNotSupported";
        internal const string EventLogEventIdValue = "EventLogEventIdValue";
        internal const string EventLogExceptionValue = "EventLogExceptionValue";
        internal const string EventLogSourceValue = "EventLogSourceValue";
        internal const string EventLogTraceValue = "EventLogTraceValue";
        internal const string EventLogValue = "EventLogValue";
        internal const string FailedToCreateTraceSource = "FailedToCreateTraceSource";
        internal const string FailedToInitializeTraceSource = "FailedToInitializeTraceSource";
        internal const string FailedToTraceEvent = "FailedToTraceEvent";
        internal const string InternalError = "InternalError";
        internal const string InvalidArgument = "InvalidArgument";
        internal const string InvalidRecoveryInformation = "InvalidRecoveryInformation";
        internal const string InvalidScopeThread = "InvalidScopeThread";
        private static System.Transactions.SR loader;
        internal const string NamedActivity = "NamedActivity";
        internal const string NetworkTransactionsDisabled = "NetworkTransactionsDisabled";
        internal const string OletxEnlistmentUnexpectedTransactionStatus = "OletxEnlistmentUnexpectedTransactionStatus";
        internal const string OletxTooManyEnlistments = "OletxTooManyEnlistments";
        internal const string OnlySupportedOnWinNT = "OnlySupportedOnWinNT";
        internal const string OperationInvalidOnAnEmptyDocument = "OperationInvalidOnAnEmptyDocument";
        internal const string PrepareInfo = "PrepareInfo";
        internal const string PromotedReturnedInvalidValue = "PromotedReturnedInvalidValue";
        internal const string PromotedTransactionExists = "PromotedTransactionExists";
        internal const string PromotionFailed = "PromotionFailed";
        internal const string ProxyCannotSupportMultipleNodeNames = "ProxyCannotSupportMultipleNodeNames";
        internal const string ReenlistAfterRecoveryComplete = "ReenlistAfterRecoveryComplete";
        internal const string ResourceManagerIdDoesNotMatchRecoveryInformation = "ResourceManagerIdDoesNotMatchRecoveryInformation";
        private ResourceManager resources;
        internal const string TextNodeAlreadyPopulated = "TextNodeAlreadyPopulated";
        internal const string ThrowingException = "ThrowingException";
        internal const string TooLate = "TooLate";
        internal const string TraceActivityIdSet = "TraceActivityIdSet";
        internal const string TraceCloneCreated = "TraceCloneCreated";
        internal const string TraceCodeAppDomainUnloading = "TraceCodeAppDomainUnloading";
        internal const string TraceConfiguredDefaultTimeoutAdjusted = "TraceConfiguredDefaultTimeoutAdjusted";
        internal const string TraceDependentCloneComplete = "TraceDependentCloneComplete";
        internal const string TraceDependentCloneCreated = "TraceDependentCloneCreated";
        internal const string TraceEnlistment = "TraceEnlistment";
        internal const string TraceEnlistmentCallbackNegative = "TraceEnlistmentCallbackNegative";
        internal const string TraceEnlistmentCallbackPositive = "TraceEnlistmentCallbackPositive";
        internal const string TraceEnlistmentNotificationCall = "TraceEnlistmentNotificationCall";
        internal const string TraceExceptionConsumed = "TraceExceptionConsumed";
        internal const string TraceFailure = "TraceFailure";
        internal const string TraceInternalError = "TraceInternalError";
        internal const string TraceInvalidOperationException = "TraceInvalidOperationException";
        internal const string TraceMethodEntered = "TraceMethodEntered";
        internal const string TraceMethodExited = "TraceMethodExited";
        internal const string TraceNewActivityIdIssued = "TraceNewActivityIdIssued";
        internal const string TraceRecoveryComplete = "TraceRecoveryComplete";
        internal const string TraceReenlist = "TraceReenlist";
        internal const string TraceSourceBase = "TraceSourceBase";
        internal const string TraceSourceLtm = "TraceSourceLtm";
        internal const string TraceSourceOletx = "TraceSourceOletx";
        internal const string TraceTransactionAborted = "TraceTransactionAborted";
        internal const string TraceTransactionCommitCalled = "TraceTransactionCommitCalled";
        internal const string TraceTransactionCommitted = "TraceTransactionCommitted";
        internal const string TraceTransactionCreated = "TraceTransactionCreated";
        internal const string TraceTransactionDeserialized = "TraceTransactionDeserialized";
        internal const string TraceTransactionException = "TraceTransactionException";
        internal const string TraceTransactionInDoubt = "TraceTransactionInDoubt";
        internal const string TraceTransactionManagerCreated = "TraceTransactionManagerCreated";
        internal const string TraceTransactionPromoted = "TraceTransactionPromoted";
        internal const string TraceTransactionRollbackCalled = "TraceTransactionRollbackCalled";
        internal const string TraceTransactionScopeCreated = "TraceTransactionScopeCreated";
        internal const string TraceTransactionScopeCurrentTransactionChanged = "TraceTransactionScopeCurrentTransactionChanged";
        internal const string TraceTransactionScopeDisposed = "TraceTransactionScopeDisposed";
        internal const string TraceTransactionScopeIncomplete = "TraceTransactionScopeIncomplete";
        internal const string TraceTransactionScopeNestedIncorrectly = "TraceTransactionScopeNestedIncorrectly";
        internal const string TraceTransactionScopeTimeout = "TraceTransactionScopeTimeout";
        internal const string TraceTransactionSerialized = "TraceTransactionSerialized";
        internal const string TraceTransactionTimeout = "TraceTransactionTimeout";
        internal const string TraceUnhandledException = "TraceUnhandledException";
        internal const string TracingException = "TracingException";
        internal const string TransactionAborted = "TransactionAborted";
        internal const string TransactionAlreadyCompleted = "TransactionAlreadyCompleted";
        internal const string TransactionAlreadyOver = "TransactionAlreadyOver";
        internal const string TransactionIndoubt = "TransactionIndoubt";
        internal const string TransactionManagerCommunicationException = "TransactionManagerCommunicationException";
        internal const string TransactionScopeComplete = "TransactionScopeComplete";
        internal const string TransactionScopeIncorrectCurrent = "TransactionScopeIncorrectCurrent";
        internal const string TransactionScopeInvalidNesting = "TransactionScopeInvalidNesting";
        internal const string TransactionScopeIsolationLevelDifferentFromTransaction = "TransactionScopeIsolationLevelDifferentFromTransaction";
        internal const string TransactionScopeTimerObjectInvalid = "TransactionScopeTimerObjectInvalid";
        internal const string TransactionStateException = "TransactionStateException";
        internal const string UnableToDeserializeTransaction = "UnableToDeserializeTransaction";
        internal const string UnableToDeserializeTransactionInternalError = "UnableToDeserializeTransactionInternalError";
        internal const string UnableToGetNotificationShimFactory = "UnableToGetNotificationShimFactory";
        internal const string UnexpectedFailureOfThreadPool = "UnexpectedFailureOfThreadPool";
        internal const string UnexpectedTimerFailure = "UnexpectedTimerFailure";
        internal const string UnexpectedTransactionManagerConfigurationValue = "UnexpectedTransactionManagerConfigurationValue";
        internal const string UnhandledException = "UnhandledException";
        internal const string UnrecognizedRecoveryInformation = "UnrecognizedRecoveryInformation";
        internal const string VolEnlistNoRecoveryInfo = "VolEnlistNoRecoveryInfo";

        internal SR()
        {
            this.resources = new ResourceManager("Resources", base.GetType().Assembly);
        }

        private static System.Transactions.SR GetLoader()
        {
            if (loader == null)
            {
                System.Transactions.SR sr = new System.Transactions.SR();
                Interlocked.CompareExchange<System.Transactions.SR>(ref loader, sr, null);
            }
            return loader;
        }

        public static object GetObject(string name)
        {
            System.Transactions.SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetObject(name, Culture);
        }

        public static string GetString(string name)
        {
            System.Transactions.SR loader = GetLoader();
            if (loader == null)
            {
                return null;
            }
            return loader.resources.GetString(name, Culture);
        }

        public static string GetString(string name, params object[] args)
        {
            System.Transactions.SR loader = GetLoader();
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
                string str = args[i] as string;
                if ((str != null) && (str.Length > 0x400))
                {
                    args[i] = str.Substring(0, 0x3fd) + "...";
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

