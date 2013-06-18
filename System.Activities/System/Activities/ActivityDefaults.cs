namespace System.Activities
{
    using System;

    internal static class ActivityDefaults
    {
        public static TimeSpan AcquireLockTimeout = TimeSpan.FromSeconds(30.0);
        public static TimeSpan AsyncOperationContextCompleteTimeout = TimeSpan.FromSeconds(30.0);
        public static TimeSpan CloseTimeout = TimeSpan.FromSeconds(30.0);
        public static TimeSpan DeleteTimeout = TimeSpan.FromSeconds(30.0);
        public static TimeSpan InternalSaveTimeout = TimeSpan.MaxValue;
        public static TimeSpan InvokeTimeout = TimeSpan.MaxValue;
        public static TimeSpan LoadTimeout = TimeSpan.FromSeconds(30.0);
        public static TimeSpan OpenTimeout = TimeSpan.FromSeconds(30.0);
        public static TimeSpan ResumeBookmarkTimeout = TimeSpan.FromSeconds(30.0);
        public static TimeSpan SaveTimeout = TimeSpan.FromSeconds(30.0);
        public static TimeSpan TrackingTimeout = TimeSpan.FromSeconds(30.0);
        public static TimeSpan TransactionCompletionTimeout = TimeSpan.FromSeconds(30.0);
    }
}

