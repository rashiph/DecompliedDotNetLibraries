namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.Runtime.Serialization;

    public abstract class NativeActivity : Activity
    {
        protected NativeActivity()
        {
        }

        protected virtual void Abort(NativeActivityAbortContext context)
        {
        }

        protected sealed override void CacheMetadata(ActivityMetadata metadata)
        {
            throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.WrongCacheMetadataForNativeActivity));
        }

        protected virtual void CacheMetadata(NativeActivityMetadata metadata)
        {
            Activity.ReflectedInformation information = new Activity.ReflectedInformation(this);
            base.SetArgumentsCollection(information.GetArguments(), metadata.CreateEmptyBindings);
            base.SetChildrenCollection(information.GetChildren());
            base.SetDelegatesCollection(information.GetDelegates());
            base.SetVariablesCollection(information.GetVariables());
        }

        protected virtual void Cancel(NativeActivityContext context)
        {
            if (!context.IsCancellationRequested)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.DefaultCancelationRequiresCancelHasBeenRequested));
            }
            context.Cancel();
        }

        protected abstract void Execute(NativeActivityContext context);
        internal override void InternalAbort(System.Activities.ActivityInstance instance, ActivityExecutor executor, Exception terminationReason)
        {
            using (NativeActivityAbortContext context = new NativeActivityAbortContext(instance, executor, terminationReason))
            {
                this.Abort(context);
            }
        }

        internal override void InternalCancel(System.Activities.ActivityInstance instance, ActivityExecutor executor, BookmarkManager bookmarkManager)
        {
            NativeActivityContext context = executor.NativeActivityContextPool.Acquire();
            try
            {
                context.Initialize(instance, executor, bookmarkManager);
                this.Cancel(context);
            }
            finally
            {
                context.Dispose();
                executor.NativeActivityContextPool.Release(context);
            }
        }

        internal override void InternalExecute(System.Activities.ActivityInstance instance, ActivityExecutor executor, BookmarkManager bookmarkManager)
        {
            NativeActivityContext context = executor.NativeActivityContextPool.Acquire();
            try
            {
                context.Initialize(instance, executor, bookmarkManager);
                this.Execute(context);
            }
            finally
            {
                context.Dispose();
                executor.NativeActivityContextPool.Release(context);
            }
        }

        internal sealed override void OnInternalCacheMetadata(bool createEmptyBindings)
        {
            NativeActivityMetadata metadata = new NativeActivityMetadata(this, base.GetParentEnvironment(), createEmptyBindings);
            this.CacheMetadata(metadata);
            metadata.Dispose();
        }

        protected virtual bool CanInduceIdle
        {
            get
            {
                return false;
            }
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
                return this.CanInduceIdle;
            }
        }
    }
}

