namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.Runtime.Serialization;

    public abstract class AsyncCodeActivity<TResult> : Activity<TResult>, IAsyncCodeActivity
    {
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

        protected abstract TResult EndExecute(AsyncCodeActivityContext context, IAsyncResult result);
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
                IAsyncResult result = this.BeginExecute(context, AsyncCodeActivity.OnExecuteComplete, asyncContext);
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
                    ((IAsyncCodeActivity) this).FinishExecution(context, result);
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

        internal sealed override void OnInternalCacheMetadataExceptResult(bool createEmptyBindings)
        {
            CodeActivityMetadata metadata = new CodeActivityMetadata(this, base.GetParentEnvironment(), createEmptyBindings);
            this.CacheMetadata(metadata);
            metadata.Dispose();
        }

        void IAsyncCodeActivity.FinishExecution(AsyncCodeActivityContext context, IAsyncResult result)
        {
            TResult local = this.EndExecute(context, result);
            base.Result.Set(context, local);
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
    }
}

