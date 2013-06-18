namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    internal class AsyncOperationContext
    {
        private ActivityExecutor executor;
        private bool hasCanceled;
        private bool hasCompleted;
        private static AsyncCallback onResumeAsyncCodeActivityBookmark;
        private System.Activities.ActivityInstance owningActivityInstance;

        internal AsyncOperationContext(ActivityExecutor executor, System.Activities.ActivityInstance owningActivityInstance)
        {
            this.executor = executor;
            this.owningActivityInstance = owningActivityInstance;
        }

        internal void CancelOperation()
        {
            if (this.ShouldCancel())
            {
                this.executor.CompleteOperation(this.owningActivityInstance);
            }
            this.hasCanceled = true;
        }

        internal void CompleteAsyncCodeActivity(CompleteData completeData)
        {
            if (this.ShouldComplete())
            {
                if (onResumeAsyncCodeActivityBookmark == null)
                {
                    onResumeAsyncCodeActivityBookmark = Fx.ThunkCallback(new AsyncCallback(AsyncOperationContext.OnResumeAsyncCodeActivityBookmark));
                }
                try
                {
                    IAsyncResult result = this.executor.BeginResumeBookmark(Bookmark.AsyncOperationCompletionBookmark, completeData, TimeSpan.MaxValue, onResumeAsyncCodeActivityBookmark, this.executor);
                    if (result.CompletedSynchronously)
                    {
                        this.executor.EndResumeBookmark(result);
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    this.executor.AbortWorkflowInstance(exception);
                }
            }
        }

        public void CompleteOperation()
        {
            if (this.ShouldComplete())
            {
                this.executor.CompleteOperation(this.owningActivityInstance);
                this.hasCompleted = true;
            }
        }

        private static void OnResumeAsyncCodeActivityBookmark(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                ActivityExecutor asyncState = (ActivityExecutor) result.AsyncState;
                try
                {
                    asyncState.EndResumeBookmark(result);
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }
                    asyncState.AbortWorkflowInstance(exception);
                }
            }
        }

        private bool ShouldCancel()
        {
            return this.IsStillActive;
        }

        private bool ShouldComplete()
        {
            if (this.hasCanceled)
            {
                return false;
            }
            if (this.hasCompleted)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.OperationAlreadyCompleted));
            }
            return true;
        }

        public bool HasCalledAsyncCodeActivityCancel { get; set; }

        public bool IsAborting { get; set; }

        internal bool IsStillActive
        {
            get
            {
                return (!this.hasCanceled && !this.hasCompleted);
            }
        }

        public object UserState { get; set; }

        internal abstract class CompleteData
        {
            private AsyncOperationContext context;
            private bool isCancel;

            protected CompleteData(AsyncOperationContext context, bool isCancel)
            {
                this.context = context;
                this.isCancel = isCancel;
            }

            public void CompleteOperation()
            {
                if (this.ShouldCallExecutor())
                {
                    this.OnCallExecutor();
                    if (!this.isCancel)
                    {
                        this.context.hasCompleted = true;
                    }
                }
                if (this.isCancel)
                {
                    this.context.hasCanceled = true;
                }
            }

            protected abstract void OnCallExecutor();
            private bool ShouldCallExecutor()
            {
                if (this.isCancel)
                {
                    return this.context.ShouldCancel();
                }
                return this.context.ShouldComplete();
            }

            protected AsyncOperationContext AsyncContext
            {
                get
                {
                    return this.context;
                }
            }

            protected ActivityExecutor Executor
            {
                get
                {
                    return this.context.executor;
                }
            }

            public System.Activities.ActivityInstance Instance
            {
                get
                {
                    return this.context.owningActivityInstance;
                }
            }
        }
    }
}

