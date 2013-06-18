namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.Runtime.Serialization;

    public abstract class CodeActivity : Activity
    {
        protected CodeActivity()
        {
        }

        protected sealed override void CacheMetadata(ActivityMetadata metadata)
        {
            throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.WrongCacheMetadataForCodeActivity));
        }

        protected virtual void CacheMetadata(CodeActivityMetadata metadata)
        {
            base.SetArgumentsCollection(Activity.ReflectedInformation.GetArguments(this), metadata.CreateEmptyBindings);
        }

        protected abstract void Execute(CodeActivityContext context);
        internal sealed override void InternalAbort(System.Activities.ActivityInstance instance, ActivityExecutor executor, Exception terminationReason)
        {
        }

        internal sealed override void InternalCancel(System.Activities.ActivityInstance instance, ActivityExecutor executor, BookmarkManager bookmarkManager)
        {
        }

        internal sealed override void InternalExecute(System.Activities.ActivityInstance instance, ActivityExecutor executor, BookmarkManager bookmarkManager)
        {
            CodeActivityContext context = executor.CodeActivityContextPool.Acquire();
            try
            {
                context.Initialize(instance, executor);
                this.Execute(context);
            }
            finally
            {
                context.Dispose();
                executor.CodeActivityContextPool.Release(context);
            }
        }

        internal sealed override void OnInternalCacheMetadata(bool createEmptyBindings)
        {
            CodeActivityMetadata metadata = new CodeActivityMetadata(this, base.GetParentEnvironment(), createEmptyBindings);
            this.CacheMetadata(metadata);
            metadata.Dispose();
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
    }
}

