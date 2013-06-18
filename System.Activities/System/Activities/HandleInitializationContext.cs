namespace System.Activities
{
    using System;
    using System.Activities.Runtime;

    public sealed class HandleInitializationContext
    {
        private ActivityExecutor executor;
        private bool isDiposed;
        private System.Activities.ActivityInstance scope;

        internal HandleInitializationContext(ActivityExecutor executor, System.Activities.ActivityInstance scope)
        {
            this.executor = executor;
            this.scope = scope;
        }

        public THandle CreateAndInitializeHandle<THandle>() where THandle: Handle
        {
            this.ThrowIfDisposed();
            THandle handleToAdd = Activator.CreateInstance<THandle>();
            handleToAdd.Initialize(this);
            if (this.scope != null)
            {
                this.scope.Environment.AddHandle(handleToAdd);
                return handleToAdd;
            }
            this.executor.AddHandle(handleToAdd);
            return handleToAdd;
        }

        internal object CreateAndInitializeHandle(Type handleType)
        {
            object obj2 = Activator.CreateInstance(handleType);
            ((Handle) obj2).Initialize(this);
            if (this.scope != null)
            {
                this.scope.Environment.AddHandle((Handle) obj2);
                return obj2;
            }
            this.executor.AddHandle((Handle) obj2);
            return obj2;
        }

        internal BookmarkScope CreateAndRegisterBookmarkScope()
        {
            return this.executor.BookmarkScopeManager.CreateAndRegisterScope(Guid.Empty);
        }

        internal void Dispose()
        {
            this.isDiposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (this.isDiposed)
            {
                throw FxTrace.Exception.AsError(new ObjectDisposedException(System.Activities.SR.HandleInitializationContextDisposed));
            }
        }

        public void UninitializeHandle(Handle handle)
        {
            this.ThrowIfDisposed();
            handle.Uninitialize(this);
        }

        internal void UnregisterBookmarkScope(BookmarkScope bookmarkScope)
        {
            this.executor.BookmarkScopeManager.UnregisterScope(bookmarkScope);
        }

        internal ActivityExecutor Executor
        {
            get
            {
                return this.executor;
            }
        }

        internal System.Activities.ActivityInstance OwningActivityInstance
        {
            get
            {
                return this.scope;
            }
        }
    }
}

