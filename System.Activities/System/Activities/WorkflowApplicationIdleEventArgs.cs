namespace System.Activities
{
    using System;
    using System.Collections.ObjectModel;

    public class WorkflowApplicationIdleEventArgs : WorkflowApplicationEventArgs
    {
        private ReadOnlyCollection<BookmarkInfo> bookmarks;

        internal WorkflowApplicationIdleEventArgs(System.Activities.WorkflowApplication application) : base(application)
        {
        }

        public ReadOnlyCollection<BookmarkInfo> Bookmarks
        {
            get
            {
                if (this.bookmarks == null)
                {
                    this.bookmarks = base.Owner.GetBookmarksForIdle();
                }
                return this.bookmarks;
            }
        }
    }
}

