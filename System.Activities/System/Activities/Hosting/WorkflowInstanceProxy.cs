namespace System.Activities.Hosting
{
    using System;
    using System.Activities;
    using System.Runtime;

    public sealed class WorkflowInstanceProxy
    {
        private WorkflowInstance instance;

        internal WorkflowInstanceProxy(WorkflowInstance instance)
        {
            this.instance = instance;
        }

        public IAsyncResult BeginResumeBookmark(Bookmark bookmark, object value, AsyncCallback callback, object state)
        {
            return this.BeginResumeBookmark(bookmark, value, TimeSpan.MaxValue, callback, state);
        }

        public IAsyncResult BeginResumeBookmark(Bookmark bookmark, object value, TimeSpan timeout, AsyncCallback callback, object state)
        {
            TimeoutHelper.ThrowIfNegativeArgument(timeout);
            return this.instance.OnBeginResumeBookmark(bookmark, value, timeout, callback, state);
        }

        public BookmarkResumptionResult EndResumeBookmark(IAsyncResult result)
        {
            return this.instance.OnEndResumeBookmark(result);
        }

        public Guid Id
        {
            get
            {
                return this.instance.Id;
            }
        }

        public Activity WorkflowDefinition
        {
            get
            {
                return this.instance.WorkflowDefinition;
            }
        }
    }
}

