namespace System.ServiceModel
{
    using System;

    internal static class XD2
    {
        public static class ContextHeader
        {
            public const string DurableInstanceContext = "InstanceId";
            public const string IsNewInstance = "IsNewInstance";
            public const string MissingContextHeader = "MissingContext";
            public const string Namespace = "http://schemas.microsoft.com/ws/2006/05/context";
        }

        public static class WorkflowControlServiceFaults
        {
            public const string InstanceAborted = "InstanceAborted";
            public const string InstanceCompleted = "InstanceCompleted";
            public const string InstanceLockedUnderTransaction = "InstanceLockedUnderTransaction";
            public const string InstanceNotFound = "InstanceNotFound";
            public const string InstanceNotSuspended = "InstanceNotSuspended";
            public const string InstanceSuspended = "InstanceSuspended";
            public const string InstanceTerminated = "InstanceTerminated";
            public const string InstanceUnloaded = "InstanceUnloaded";
            public const string OperationNotAvailable = "OperationNotAvailable";
        }

        public static class WorkflowInstanceManagementService
        {
            public const string Abandon = "Abandon";
            public const string Cancel = "Cancel";
            public const string ContractName = "IWorkflowInstanceManagement";
            public const string Run = "Run";
            public const string Suspend = "Suspend";
            public const string Terminate = "Terminate";
            public const string TransactedCancel = "TransactedCancel";
            public const string TransactedRun = "TransactedRun";
            public const string TransactedSuspend = "TransactedSuspend";
            public const string TransactedTerminate = "TransactedTerminate";
            public const string TransactedUnsuspend = "TransactedUnsuspend";
            public const string Unsuspend = "Unsuspend";
        }

        public static class WorkflowServices
        {
            public const string Namespace = "http://schemas.datacontract.org/2008/10/WorkflowServices";
        }
    }
}

