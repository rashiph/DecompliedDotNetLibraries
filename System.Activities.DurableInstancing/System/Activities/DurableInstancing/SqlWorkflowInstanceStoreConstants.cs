namespace System.Activities.DurableInstancing
{
    using System;
    using System.Xml.Linq;

    internal static class SqlWorkflowInstanceStoreConstants
    {
        public static readonly XName BinaryBlockingBookmarksPropertyName = WorkflowNamespace.GetName("Bookmarks");
        public const InstanceCompletionAction DefaultInstanceCompletionAction = InstanceCompletionAction.DeleteAll;
        public const InstanceEncodingOption DefaultInstanceEncodingOption = InstanceEncodingOption.GZip;
        public const InstanceLockedExceptionAction DefaultInstanceLockedExceptionAction = InstanceLockedExceptionAction.NoRetry;
        public const string DefaultSchema = "[System.Activities.DurableInstancing]";
        public const int DefaultStringBuilderCapacity = 0x200;
        public static readonly XNamespace DurableInstancingNamespace = XNamespace.Get("urn:schemas-microsoft-com:System.ServiceModel.Activities.DurableInstancing/SqlWorkflowInstanceStore");
        public const string ExecutingStatusPropertyValue = "Executing";
        public static readonly XName LastUpdatePropertyName = WorkflowNamespace.GetName("LastUpdate");
        public static readonly TimeSpan LockOwnerTimeoutBuffer = TimeSpan.FromSeconds(30.0);
        public static readonly string MachineName = Environment.MachineName;
        public const int MaximumPropertiesPerPromotion = 0x20;
        public const int MaximumStringLengthSupported = 450;
        public static readonly XName PendingTimerExpirationPropertyName = WorkflowNamespace.GetName("TimerExpirationTime");
        public static readonly XName StatusPropertyName = WorkflowNamespace.GetName("Status");
        public static readonly XNamespace WorkflowNamespace = XNamespace.Get("urn:schemas-microsoft-com:System.Activities/4.0/properties");
    }
}

