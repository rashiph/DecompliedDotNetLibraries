namespace System.Workflow.Runtime.Tracking
{
    using System;
    using System.Workflow.Runtime;

    public static class TrackingProfileCache
    {
        public static void Clear()
        {
            TrackingProfileManager.ClearCache();
        }
    }
}

