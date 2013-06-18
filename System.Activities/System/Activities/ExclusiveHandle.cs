namespace System.Activities
{
    using System;
    using System.Activities.Runtime;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.Serialization;

    [DataContract]
    public class ExclusiveHandle : Handle
    {
        [DataMember(EmitDefaultValue=false)]
        private List<BookmarkScopeHandle> bookmarkScopes;
        [DataMember(EmitDefaultValue=false)]
        private bool bookmarkScopesListIsDefault;
        [DataMember(EmitDefaultValue=false)]
        private ActivityExecutor executor;
        [DataMember(EmitDefaultValue=false)]
        private ExclusiveHandleBookmarkList importantBookmarks;
        [DataMember]
        private System.Activities.ActivityInstance owningInstance;
        private ReadOnlyCollection<BookmarkScopeHandle> readOnlyBookmarkScopeCollection;
        [DataMember(EmitDefaultValue=false)]
        private ExclusiveHandleBookmarkList unimportantBookmarks;

        public ExclusiveHandle()
        {
            base.CanBeRemovedWithExecutingChildren = true;
        }

        internal void AddToImportantBookmarks(Bookmark bookmark)
        {
            if (this.ImportantBookmarks == null)
            {
                this.ImportantBookmarks = new ExclusiveHandleBookmarkList();
            }
            this.ImportantBookmarks.Add(bookmark);
            if (bookmark.ExclusiveHandles == null)
            {
                bookmark.ExclusiveHandles = new ExclusiveHandleList();
            }
            bookmark.ExclusiveHandles.Add(this);
        }

        internal void AddToUnimportantBookmarks(Bookmark bookmark)
        {
            if (this.UnimportantBookmarks == null)
            {
                this.UnimportantBookmarks = new ExclusiveHandleBookmarkList();
            }
            this.UnimportantBookmarks.Add(bookmark);
            if (bookmark.ExclusiveHandles == null)
            {
                bookmark.ExclusiveHandles = new ExclusiveHandleList();
            }
            bookmark.ExclusiveHandles.Add(this);
        }

        protected override void OnInitialize(HandleInitializationContext context)
        {
            this.owningInstance = context.OwningActivityInstance;
            this.executor = context.Executor;
            this.PerformDefaultRegistration();
        }

        private void PerformDefaultRegistration()
        {
            if (this.bookmarkScopes == null)
            {
                this.bookmarkScopes = new List<BookmarkScopeHandle>();
            }
            this.bookmarkScopes.Add(BookmarkScopeHandle.Default);
            LocationEnvironment environment = this.owningInstance.Environment;
            if (environment != null)
            {
                for (environment = environment.Parent; environment != null; environment = environment.Parent)
                {
                    if (environment.HasHandles)
                    {
                        List<Handle> list = environment.Handles;
                        if (list != null)
                        {
                            int count = list.Count;
                            for (int i = 0; i < count; i++)
                            {
                                BookmarkScopeHandle item = list[i] as BookmarkScopeHandle;
                                if (item != null)
                                {
                                    this.bookmarkScopes.Add(item);
                                }
                            }
                        }
                    }
                }
            }
            List<Handle> handles = this.executor.Handles;
            if (handles != null)
            {
                int num3 = handles.Count;
                for (int j = 0; j < num3; j++)
                {
                    BookmarkScopeHandle handle2 = handles[j] as BookmarkScopeHandle;
                    if (handle2 != null)
                    {
                        this.bookmarkScopes.Add(handle2);
                    }
                }
            }
            this.bookmarkScopesListIsDefault = true;
        }

        public void RegisterBookmarkScope(NativeActivityContext context, BookmarkScopeHandle bookmarkScopeHandle)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }
            context.ThrowIfDisposed();
            if (bookmarkScopeHandle == null)
            {
                throw FxTrace.Exception.ArgumentNull("bookmarkScopeHandle");
            }
            if (((this.ImportantBookmarks != null) && (this.ImportantBookmarks.Count != 0)) || ((this.UnimportantBookmarks != null) && (this.UnimportantBookmarks.Count != 0)))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.ExclusiveHandleRegisterBookmarkScopeFailed));
            }
            if (this.bookmarkScopesListIsDefault)
            {
                this.bookmarkScopesListIsDefault = false;
                this.bookmarkScopes.Clear();
            }
            this.bookmarkScopes.Add(bookmarkScopeHandle);
            this.readOnlyBookmarkScopeCollection = null;
        }

        public void Reinitialize(NativeActivityContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }
            context.ThrowIfDisposed();
            if (((this.ImportantBookmarks != null) && (this.ImportantBookmarks.Count != 0)) || ((this.UnimportantBookmarks != null) && (this.UnimportantBookmarks.Count != 0)))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.ExclusiveHandleReinitializeFailed));
            }
            this.bookmarkScopes.Clear();
            this.readOnlyBookmarkScopeCollection = null;
            this.PerformDefaultRegistration();
        }

        internal void RemoveBookmark(Bookmark bookmark)
        {
            if ((this.ImportantBookmarks != null) && this.ImportantBookmarks.Contains(bookmark))
            {
                this.ImportantBookmarks.Remove(bookmark);
            }
            else if ((this.UnimportantBookmarks != null) && this.UnimportantBookmarks.Contains(bookmark))
            {
                this.UnimportantBookmarks.Remove(bookmark);
            }
        }

        internal ExclusiveHandleBookmarkList ImportantBookmarks
        {
            get
            {
                return this.importantBookmarks;
            }
            set
            {
                this.importantBookmarks = value;
            }
        }

        public ReadOnlyCollection<BookmarkScopeHandle> RegisteredBookmarkScopes
        {
            get
            {
                if (this.bookmarkScopes == null)
                {
                    return new ReadOnlyCollection<BookmarkScopeHandle>(new List<BookmarkScopeHandle>());
                }
                if (this.readOnlyBookmarkScopeCollection == null)
                {
                    this.readOnlyBookmarkScopeCollection = new ReadOnlyCollection<BookmarkScopeHandle>(this.bookmarkScopes);
                }
                return this.readOnlyBookmarkScopeCollection;
            }
        }

        internal ExclusiveHandleBookmarkList UnimportantBookmarks
        {
            get
            {
                return this.unimportantBookmarks;
            }
            set
            {
                this.unimportantBookmarks = value;
            }
        }

        [DataContract]
        internal class ExclusiveHandleBookmarkList
        {
            [DataMember]
            private List<Bookmark> bookmarks = new List<Bookmark>();

            public void Add(Bookmark bookmark)
            {
                this.bookmarks.Add(bookmark);
            }

            public bool Contains(Bookmark bookmark)
            {
                for (int i = 0; i < this.bookmarks.Count; i++)
                {
                    if (object.ReferenceEquals(this.bookmarks[i], bookmark))
                    {
                        return true;
                    }
                }
                return false;
            }

            public void Remove(Bookmark bookmark)
            {
                for (int i = 0; i < this.bookmarks.Count; i++)
                {
                    if (object.ReferenceEquals(this.bookmarks[i], bookmark))
                    {
                        this.bookmarks.RemoveAt(i);
                        return;
                    }
                }
            }

            public int Count
            {
                get
                {
                    return this.bookmarks.Count;
                }
            }
        }
    }
}

