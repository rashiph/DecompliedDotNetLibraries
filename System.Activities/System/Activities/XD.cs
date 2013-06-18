namespace System.Activities
{
    using System;

    internal static class XD
    {
        public static class ActivityInstance
        {
            public const string ActivityReferences = "activityReferences";
            public const string BlockingBookmarkCount = "blockingBookmarkCount";
            public const string Bookmarks = "bookmarks";
            public const string Children = "children";
            public const string FaultBookmark = "faultBookmark";
            public const string Name = "ActivityInstance";
            public const string Owner = "owner";
            public const string PropertyManager = "propertyManager";
            public const string WaitingForTransactionContext = "waitingForTransactionContext";
        }

        public static class Executor
        {
            public const string ActivityInstanceMap = "activities";
            public const string BookmarkManager = "bookmarkMgr";
            public const string BookmarkScopeManager = "bookmarkScopeManager";
            public const string CompletionException = "completionException";
            public const string ExecutionState = "state";
            public const string ExtensionParticipantObjects = "extensionParticipantObjects";
            public const string IsolationBlockWaiters = "isolationBlockWaiters";
            public const string LastInstanceId = "lastInstanceId";
            public const string MainRootCompleteBookmark = "mainRootCompleteBookmark";
            public const string MappableObjectManager = "mappableObjectManager";
            public const string Name = "Executor";
            public const string NextTrackingRecordNumber = "nextTrackingRecordNumber";
            public const string PersistenceWaiters = "persistenceWaiters";
            public const string RootEnvironment = "rootEnvironment";
            public const string RootInstance = "rootInstance";
            public const string SchedulerMember = "scheduler";
            public const string SecondaryRootInstances = "secondaryRootInstances";
            public const string ShouldRaiseMainBodyComplete = "shouldRaiseMainBodyComplete";
            public const string TransactionContextWaiters = "transactionContextWaiters";
            public const string WorkflowOutputs = "workflowOutputs";
        }

        public static class Runtime
        {
            public const string ActivityInstanceMap = "InstanceMap";
            public const string BookmarkManager = "BookmarkManager";
            public const string Namespace = "http://schemas.datacontract.org/2010/02/System.Activities";
            public const string Scheduler = "Scheduler";
        }

        public static class WorkflowApplication
        {
            public const string InstanceState = "WFApplication";
        }
    }
}

