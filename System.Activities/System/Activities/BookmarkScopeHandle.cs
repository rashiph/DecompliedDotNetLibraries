namespace System.Activities
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    public sealed class BookmarkScopeHandle : Handle
    {
        [DataMember(EmitDefaultValue=false)]
        private System.Activities.BookmarkScope bookmarkScope;
        private static BookmarkScopeHandle defaultBookmarkScopeHandle = new BookmarkScopeHandle(System.Activities.BookmarkScope.Default);

        public BookmarkScopeHandle()
        {
        }

        internal BookmarkScopeHandle(System.Activities.BookmarkScope bookmarkScope)
        {
            this.bookmarkScope = bookmarkScope;
        }

        public void CreateBookmarkScope(NativeActivityContext context)
        {
            this.ThrowIfContextIsNullOrDisposed(context);
            if (this.bookmarkScope != null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CreateBookmarkScopeFailed));
            }
            base.ThrowIfUninitialized();
            this.bookmarkScope = context.CreateBookmarkScope(Guid.Empty, this);
        }

        public void CreateBookmarkScope(NativeActivityContext context, Guid scopeId)
        {
            this.ThrowIfContextIsNullOrDisposed(context);
            if (this.bookmarkScope != null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CreateBookmarkScopeFailed));
            }
            base.ThrowIfUninitialized();
            this.bookmarkScope = context.CreateBookmarkScope(scopeId, this);
        }

        public void Initialize(NativeActivityContext context, Guid scope)
        {
            this.ThrowIfContextIsNullOrDisposed(context);
            base.ThrowIfUninitialized();
            this.bookmarkScope.Initialize(context, scope);
        }

        private void ThrowIfContextIsNullOrDisposed(NativeActivityContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }
            context.ThrowIfDisposed();
        }

        public System.Activities.BookmarkScope BookmarkScope
        {
            get
            {
                return this.bookmarkScope;
            }
        }

        public static BookmarkScopeHandle Default
        {
            get
            {
                return defaultBookmarkScopeHandle;
            }
        }
    }
}

