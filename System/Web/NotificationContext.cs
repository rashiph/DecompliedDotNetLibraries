namespace System.Web
{
    using System;

    internal class NotificationContext
    {
        internal HttpAsyncResult AsyncResult;
        internal int CurrentModuleEventIndex;
        internal int CurrentModuleIndex;
        internal RequestNotification CurrentNotification;
        internal int CurrentNotificationFlags;
        internal Exception Error;
        internal bool IsPostNotification;
        internal bool IsReEntry;
        internal bool PendingAsyncCompletion;
        internal bool RequestCompleted;

        internal NotificationContext(int flags, bool isReEntry)
        {
            this.CurrentNotificationFlags = flags;
            this.IsReEntry = isReEntry;
        }
    }
}

