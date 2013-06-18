namespace System.Web.Util
{
    using System;
    using System.Security.Permissions;
    using System.Threading;
    using System.Web;

    public class WorkItem
    {
        private static WaitCallback _onQueueUserWorkItemCompletion = new WaitCallback(WorkItem.OnQueueUserWorkItemCompletion);
        private static bool _useQueueUserWorkItem = true;

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        private static void CallCallbackWithAssert(WorkItemCallback callback)
        {
            callback();
        }

        private static void OnQueueUserWorkItemCompletion(object state)
        {
            WorkItemCallback callback = state as WorkItemCallback;
            if (callback != null)
            {
                CallCallbackWithAssert(callback);
            }
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted=true)]
        public static void Post(WorkItemCallback callback)
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                throw new PlatformNotSupportedException(System.Web.SR.GetString("RequiresNT"));
            }
            PostInternal(callback);
        }

        internal static void PostInternal(WorkItemCallback callback)
        {
            if (_useQueueUserWorkItem)
            {
                ThreadPool.QueueUserWorkItem(_onQueueUserWorkItemCompletion, callback);
            }
            else
            {
                new WrappedWorkItemCallback(callback).Post();
            }
        }
    }
}

