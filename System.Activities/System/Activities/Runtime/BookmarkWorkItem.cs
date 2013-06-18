namespace System.Activities.Runtime
{
    using System;
    using System.Activities;
    using System.Runtime;
    using System.Runtime.Serialization;

    [DataContract]
    internal class BookmarkWorkItem : ActivityExecutionWorkItem
    {
        [DataMember]
        private Bookmark bookmark;
        [DataMember]
        private BookmarkCallbackWrapper callbackWrapper;
        [DataMember(EmitDefaultValue=false)]
        private object state;

        protected BookmarkWorkItem(BookmarkCallbackWrapper callbackWrapper, Bookmark bookmark, object value) : base(callbackWrapper.ActivityInstance)
        {
            this.callbackWrapper = callbackWrapper;
            this.bookmark = bookmark;
            this.state = value;
        }

        public BookmarkWorkItem(ActivityExecutor executor, bool isExternal, BookmarkCallbackWrapper callbackWrapper, Bookmark bookmark, object value) : this(callbackWrapper, bookmark, value)
        {
            if (isExternal)
            {
                executor.EnterNoPersist();
                base.ExitNoPersistRequired = true;
            }
        }

        public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
        {
            NativeActivityContext context = executor.NativeActivityContextPool.Acquire();
            try
            {
                context.Initialize(base.ActivityInstance, executor, bookmarkManager);
                this.callbackWrapper.Invoke(context, this.bookmark, this.state);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }
                base.ExceptionToPropagate = exception;
            }
            finally
            {
                context.Dispose();
                executor.NativeActivityContextPool.Release(context);
            }
            return true;
        }

        public override void TraceCompleted()
        {
            if (TD.CompleteBookmarkWorkItemIsEnabled())
            {
                TD.CompleteBookmarkWorkItem(base.ActivityInstance.Activity.GetType().ToString(), base.ActivityInstance.Activity.DisplayName, base.ActivityInstance.Id, ActivityUtilities.GetTraceString(this.bookmark), ActivityUtilities.GetTraceString(this.bookmark.Scope));
            }
        }

        public override void TraceScheduled()
        {
            if (TD.ScheduleBookmarkWorkItemIsEnabled())
            {
                TD.ScheduleBookmarkWorkItem(base.ActivityInstance.Activity.GetType().ToString(), base.ActivityInstance.Activity.DisplayName, base.ActivityInstance.Id, ActivityUtilities.GetTraceString(this.bookmark), ActivityUtilities.GetTraceString(this.bookmark.Scope));
            }
        }

        public override void TraceStarting()
        {
            if (TD.StartBookmarkWorkItemIsEnabled())
            {
                TD.StartBookmarkWorkItem(base.ActivityInstance.Activity.GetType().ToString(), base.ActivityInstance.Activity.DisplayName, base.ActivityInstance.Id, ActivityUtilities.GetTraceString(this.bookmark), ActivityUtilities.GetTraceString(this.bookmark.Scope));
            }
        }
    }
}

