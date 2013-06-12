namespace System.Web
{
    using System;

    internal sealed class NotificationQueueItem
    {
        internal readonly FileAction Action;
        internal readonly FileChangeEventHandler Callback;
        internal readonly string Filename;

        internal NotificationQueueItem(FileChangeEventHandler callback, FileAction action, string filename)
        {
            this.Callback = callback;
            this.Action = action;
            this.Filename = filename;
        }
    }
}

