namespace System.Activities
{
    using System;
    using System.Activities.Runtime;

    public sealed class AsyncCodeActivityContext : CodeActivityContext
    {
        private AsyncOperationContext asyncContext;

        internal AsyncCodeActivityContext(AsyncOperationContext asyncContext, System.Activities.ActivityInstance instance, ActivityExecutor executor) : base(instance, executor)
        {
            this.asyncContext = asyncContext;
        }

        public void MarkCanceled()
        {
            base.ThrowIfDisposed();
            if (!base.CurrentInstance.IsCancellationRequested && !this.asyncContext.IsAborting)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.MarkCanceledOnlyCallableIfCancelRequested));
            }
            base.CurrentInstance.MarkCanceled();
        }

        public bool IsCancellationRequested
        {
            get
            {
                base.ThrowIfDisposed();
                return base.CurrentInstance.IsCancellationRequested;
            }
        }

        public object UserState
        {
            get
            {
                base.ThrowIfDisposed();
                return this.asyncContext.UserState;
            }
            set
            {
                base.ThrowIfDisposed();
                this.asyncContext.UserState = value;
            }
        }
    }
}

