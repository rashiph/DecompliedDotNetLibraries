namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.Runtime;
    using System.Runtime.Serialization;

    public abstract class AsyncCodeActivity : Activity, IAsyncCodeActivity
    {
        private static AsyncCallback onExecuteComplete;

        protected AsyncCodeActivity()
        {
        }

        protected abstract IAsyncResult BeginExecute(AsyncCodeActivityContext context, AsyncCallback callback, object state);
        protected sealed override void CacheMetadata(ActivityMetadata metadata)
        {
            throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.WrongCacheMetadataForCodeActivity));
        }

        protected virtual void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.SetArgumentsCollection(Activity.ReflectedInformation.GetArguments(this), metadata.CreateEmptyBindings);
        }

        protected virtual void Cancel(AsyncCodeActivityContext context)
        {
        }

        internal static void CompleteAsynchronousExecution(IAsyncResult result)
        {
            if (!result.CompletedSynchronously)
            {
                AsyncOperationContext asyncState = result.AsyncState as AsyncOperationContext;
                if (asyncState != null)
                {
                    asyncState.CompleteAsyncCodeActivity(new CompleteAsyncCodeActivityData(asyncState, result));
                }
            }
        }

        protected abstract void EndExecute(AsyncCodeActivityContext context, IAsyncResult result);
        internal sealed override void InternalAbort(System.Activities.ActivityInstance instance, ActivityExecutor executor, Exception terminationReason)
        {
            AsyncOperationContext context;
            if (executor.TryGetPendingOperation(instance, out context))
            {
                try
                {
                    if (!context.HasCalledAsyncCodeActivityCancel)
                    {
                        context.IsAborting = true;
                        this.InternalCancel(instance, executor, null);
                    }
                }
                finally
                {
                    if (context.IsStillActive)
                    {
                        context.CancelOperation();
                    }
                }
            }
        }

        internal sealed override void InternalCancel(System.Activities.ActivityInstance instance, ActivityExecutor executor, BookmarkManager bookmarkManager)
        {
            AsyncOperationContext context;
            if (executor.TryGetPendingOperation(instance, out context))
            {
                using (AsyncCodeActivityContext context2 = new AsyncCodeActivityContext(context, instance, executor))
                {
                    context.HasCalledAsyncCodeActivityCancel = true;
                    this.Cancel(context2);
                }
            }
        }

        internal sealed override void InternalExecute(System.Activities.ActivityInstance instance, ActivityExecutor executor, BookmarkManager bookmarkManager)
        {
            AsyncOperationContext asyncContext = executor.SetupAsyncOperationBlock(instance);
            instance.IncrementBusyCount();
            AsyncCodeActivityContext context = new AsyncCodeActivityContext(asyncContext, instance, executor);
            bool flag = false;
            try
            {
                IAsyncResult result = this.BeginExecute(context, OnExecuteComplete, asyncContext);
                if (result == null)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.BeginExecuteMustNotReturnANullAsyncResult));
                }
                if (!object.ReferenceEquals(result.AsyncState, asyncContext))
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.BeginExecuteMustUseProvidedStateAsAsyncResultState));
                }
                if (result.CompletedSynchronously)
                {
                    this.EndExecute(context, result);
                    asyncContext.CompleteOperation();
                }
                flag = true;
            }
            finally
            {
                context.Dispose();
                if (!flag)
                {
                    asyncContext.CancelOperation();
                }
            }
        }

        internal sealed override void OnInternalCacheMetadata(bool createEmptyBindings)
        {
            CodeActivityMetadata metadata = new CodeActivityMetadata(this, base.GetParentEnvironment(), createEmptyBindings);
            this.CacheMetadata(metadata);
            metadata.Dispose();
        }

        void IAsyncCodeActivity.FinishExecution(AsyncCodeActivityContext context, IAsyncResult result)
        {
            this.EndExecute(context, result);
        }

        [IgnoreDataMember]
        protected sealed override Func<Activity> Implementation
        {
            get
            {
                return null;
            }
            set
            {
                if (value != null)
                {
                    throw FxTrace.Exception.AsError(new NotSupportedException());
                }
            }
        }

        internal override bool InternalCanInduceIdle
        {
            get
            {
                return true;
            }
        }

        internal static AsyncCallback OnExecuteComplete
        {
            get
            {
                if (onExecuteComplete == null)
                {
                    onExecuteComplete = Fx.ThunkCallback(new AsyncCallback(AsyncCodeActivity.CompleteAsynchronousExecution));
                }
                return onExecuteComplete;
            }
        }

        private class CompleteAsyncCodeActivityData : AsyncOperationContext.CompleteData
        {
            private IAsyncResult result;

            public CompleteAsyncCodeActivityData(AsyncOperationContext context, IAsyncResult result) : base(context, false)
            {
                this.result = result;
            }

            protected override void OnCallExecutor()
            {
                base.Executor.CompleteOperation(new CompleteAsyncCodeActivityWorkItem(base.AsyncContext, base.Instance, this.result));
            }

            private class CompleteAsyncCodeActivityWorkItem : ActivityExecutionWorkItem
            {
                private AsyncOperationContext asyncContext;
                private IAsyncResult result;

                public CompleteAsyncCodeActivityWorkItem(AsyncOperationContext asyncContext, System.Activities.ActivityInstance instance, IAsyncResult result) : base(instance)
                {
                    this.result = result;
                    this.asyncContext = asyncContext;
                    base.ExitNoPersistRequired = true;
                }

                public override bool Execute(ActivityExecutor executor, BookmarkManager bookmarkManager)
                {
                    AsyncCodeActivityContext context = null;
                    try
                    {
                        context = new AsyncCodeActivityContext(this.asyncContext, base.ActivityInstance, executor);
                        ((IAsyncCodeActivity) base.ActivityInstance.Activity).FinishExecution(context, this.result);
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
                        if (context != null)
                        {
                            context.Dispose();
                        }
                    }
                    return true;
                }

                public override void TraceCompleted()
                {
                    if (TD.CompleteBookmarkWorkItemIsEnabled())
                    {
                        TD.CompleteBookmarkWorkItem(base.ActivityInstance.Activity.GetType().ToString(), base.ActivityInstance.Activity.DisplayName, base.ActivityInstance.Id, ActivityUtilities.GetTraceString(Bookmark.AsyncOperationCompletionBookmark), ActivityUtilities.GetTraceString(Bookmark.AsyncOperationCompletionBookmark.Scope));
                    }
                }

                public override void TraceScheduled()
                {
                    if (TD.ScheduleBookmarkWorkItemIsEnabled())
                    {
                        TD.ScheduleBookmarkWorkItem(base.ActivityInstance.Activity.GetType().ToString(), base.ActivityInstance.Activity.DisplayName, base.ActivityInstance.Id, ActivityUtilities.GetTraceString(Bookmark.AsyncOperationCompletionBookmark), ActivityUtilities.GetTraceString(Bookmark.AsyncOperationCompletionBookmark.Scope));
                    }
                }

                public override void TraceStarting()
                {
                    if (TD.StartBookmarkWorkItemIsEnabled())
                    {
                        TD.StartBookmarkWorkItem(base.ActivityInstance.Activity.GetType().ToString(), base.ActivityInstance.Activity.DisplayName, base.ActivityInstance.Id, ActivityUtilities.GetTraceString(Bookmark.AsyncOperationCompletionBookmark), ActivityUtilities.GetTraceString(Bookmark.AsyncOperationCompletionBookmark.Scope));
                    }
                }
            }
        }
    }
}

