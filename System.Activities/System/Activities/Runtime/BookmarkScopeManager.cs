namespace System.Activities.Runtime
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Runtime.DurableInstancing;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [DataContract]
    internal class BookmarkScopeManager
    {
        private Dictionary<BookmarkScope, BookmarkManager> bookmarkManagers;
        [DataMember]
        private BookmarkScope defaultScope;
        private List<InstanceKey> keysToAssociate;
        private List<InstanceKey> keysToDisassociate;
        [DataMember]
        private long nextTemporaryId = 1L;
        private List<BookmarkScope> uninitializedScopes;

        public BookmarkScopeManager()
        {
            this.defaultScope = this.CreateAndRegisterScope(Guid.Empty);
        }

        public BookmarkScope CreateAndRegisterScope(Guid scopeId)
        {
            return this.CreateAndRegisterScope(scopeId, null);
        }

        internal BookmarkScope CreateAndRegisterScope(Guid scopeId, BookmarkScopeHandle scopeHandle)
        {
            if (this.bookmarkManagers == null)
            {
                this.bookmarkManagers = new Dictionary<BookmarkScope, BookmarkManager>();
            }
            BookmarkScope key = null;
            if (scopeId == Guid.Empty)
            {
                key = new BookmarkScope(this.GetNextTemporaryId());
                this.bookmarkManagers.Add(key, new BookmarkManager(key, scopeHandle));
                if (TD.CreateBookmarkScopeIsEnabled())
                {
                    TD.CreateBookmarkScope(ActivityUtilities.GetTraceString(key));
                }
                if (this.uninitializedScopes == null)
                {
                    this.uninitializedScopes = new List<BookmarkScope>();
                }
                this.uninitializedScopes.Add(key);
                return key;
            }
            foreach (BookmarkScope scope2 in this.bookmarkManagers.Keys)
            {
                if (scope2.Id.Equals(scopeId))
                {
                    key = scope2;
                    break;
                }
            }
            if (key == null)
            {
                key = new BookmarkScope(scopeId);
                this.bookmarkManagers.Add(key, new BookmarkManager(key, scopeHandle));
                if (TD.CreateBookmarkScopeIsEnabled())
                {
                    TD.CreateBookmarkScope(string.Format(CultureInfo.InvariantCulture, "Id: {0}", new object[] { ActivityUtilities.GetTraceString(key) }));
                }
            }
            this.CreateAssociatedKey(key);
            return key;
        }

        private void CreateAssociatedKey(BookmarkScope newScope)
        {
            if (this.keysToAssociate == null)
            {
                this.keysToAssociate = new List<InstanceKey>(2);
            }
            this.keysToAssociate.Add(new InstanceKey(newScope.Id));
        }

        public Bookmark CreateBookmark(string name, BookmarkScope scope, BookmarkCallback callback, System.Activities.ActivityInstance owningInstance, BookmarkOptions options)
        {
            BookmarkManager manager = null;
            BookmarkScope key = scope;
            if (scope.IsDefault)
            {
                key = this.defaultScope;
            }
            if (!this.bookmarkManagers.TryGetValue(key, out manager))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.RegisteredBookmarkScopeRequired));
            }
            return manager.CreateBookmark(name, callback, owningInstance, options);
        }

        public ReadOnlyCollection<BookmarkInfo> GetBookmarks(BookmarkScope scope)
        {
            BookmarkManager manager = null;
            BookmarkScope key = scope;
            if (scope.IsDefault)
            {
                key = this.defaultScope;
            }
            if (this.bookmarkManagers.TryGetValue(key, out manager) && !manager.HasBookmarks)
            {
                manager = null;
            }
            if (manager != null)
            {
                List<BookmarkInfo> bookmarks = new List<BookmarkInfo>();
                manager.PopulateBookmarkInfo(bookmarks);
                return new ReadOnlyCollection<BookmarkInfo>(bookmarks);
            }
            return null;
        }

        public ICollection<InstanceKey> GetKeysToAssociate()
        {
            if ((this.keysToAssociate == null) || (this.keysToAssociate.Count == 0))
            {
                return null;
            }
            ICollection<InstanceKey> keysToAssociate = this.keysToAssociate;
            this.keysToAssociate = null;
            return keysToAssociate;
        }

        public ICollection<InstanceKey> GetKeysToDisassociate()
        {
            if ((this.keysToDisassociate == null) || (this.keysToDisassociate.Count == 0))
            {
                return null;
            }
            ICollection<InstanceKey> keysToDisassociate = this.keysToDisassociate;
            this.keysToDisassociate = null;
            return keysToDisassociate;
        }

        private long GetNextTemporaryId()
        {
            long nextTemporaryId = this.nextTemporaryId;
            this.nextTemporaryId += 1L;
            return nextTemporaryId;
        }

        public BookmarkScope InitializeBookmarkScopeWithoutKeyAssociation(BookmarkScope scope, Guid id)
        {
            BookmarkScope item = scope;
            if (scope.IsDefault)
            {
                item = this.defaultScope;
            }
            if ((this.uninitializedScopes == null) || !this.uninitializedScopes.Contains(item))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.BookmarkScopeNotRegisteredForInitialize));
            }
            if (this.bookmarkManagers.ContainsKey(new BookmarkScope(id)))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.BookmarkScopeWithIdAlreadyExists(id)));
            }
            BookmarkManager manager = this.bookmarkManagers[item];
            this.bookmarkManagers.Remove(item);
            this.uninitializedScopes.Remove(item);
            long temporaryId = item.TemporaryId;
            item.Id = id;
            this.bookmarkManagers.Add(item, manager);
            if (TD.BookmarkScopeInitializedIsEnabled())
            {
                TD.BookmarkScopeInitialized(temporaryId.ToString(CultureInfo.InvariantCulture), item.Id.ToString());
            }
            return item;
        }

        public void InitializeScope(BookmarkScope scope, Guid id)
        {
            BookmarkScope newScope = this.InitializeBookmarkScopeWithoutKeyAssociation(scope, id);
            this.CreateAssociatedKey(newScope);
        }

        public bool IsExclusiveScopeUnstable(Bookmark bookmark)
        {
            if (bookmark.ExclusiveHandles != null)
            {
                for (int i = 0; i < bookmark.ExclusiveHandles.Count; i++)
                {
                    ExclusiveHandle handle = bookmark.ExclusiveHandles[i];
                    if (((handle.ImportantBookmarks != null) && handle.ImportantBookmarks.Contains(bookmark)) && ((handle.UnimportantBookmarks != null) && (handle.UnimportantBookmarks.Count != 0)))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private bool IsStable(BookmarkScope scope, bool nonScopedBookmarksExist)
        {
            if (nonScopedBookmarksExist)
            {
                return false;
            }
            if (this.bookmarkManagers != null)
            {
                foreach (KeyValuePair<BookmarkScope, BookmarkManager> pair in this.bookmarkManagers)
                {
                    if (!pair.Key.Equals(scope) && pair.Value.HasBookmarks)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public void PopulateBookmarkInfo(ref List<BookmarkInfo> bookmarks)
        {
            foreach (BookmarkManager manager in this.bookmarkManagers.Values)
            {
                if (manager.HasBookmarks)
                {
                    if (bookmarks == null)
                    {
                        bookmarks = new List<BookmarkInfo>();
                    }
                    manager.PopulateBookmarkInfo(bookmarks);
                }
            }
        }

        private void PurgeBookmark(Bookmark bookmark, BookmarkManager nonScopedBookmarkManager)
        {
            BookmarkManager manager = null;
            if (bookmark.Scope != null)
            {
                BookmarkScope scope = bookmark.Scope;
                bool isDefault = bookmark.Scope.IsDefault;
                manager = this.bookmarkManagers[bookmark.Scope];
            }
            else
            {
                manager = nonScopedBookmarkManager;
            }
            manager.PurgeSingleBookmark(bookmark);
        }

        public void PurgeBookmarks(BookmarkManager nonScopedBookmarkManager, Bookmark singleBookmark, IList<Bookmark> multipleBookmarks)
        {
            if (singleBookmark != null)
            {
                this.PurgeBookmark(singleBookmark, nonScopedBookmarkManager);
            }
            else
            {
                for (int i = 0; i < multipleBookmarks.Count; i++)
                {
                    Bookmark bookmark = multipleBookmarks[i];
                    this.PurgeBookmark(bookmark, nonScopedBookmarkManager);
                }
            }
        }

        public bool RemoveBookmark(Bookmark bookmark, BookmarkScope scope, System.Activities.ActivityInstance instanceAttemptingRemove)
        {
            BookmarkManager manager;
            BookmarkScope key = scope;
            if (scope.IsDefault)
            {
                key = this.defaultScope;
            }
            return (this.bookmarkManagers.TryGetValue(key, out manager) && manager.Remove(bookmark, instanceAttemptingRemove));
        }

        public BookmarkResumptionResult TryGenerateWorkItem(ActivityExecutor executor, ref Bookmark bookmark, BookmarkScope scope, object value, System.Activities.ActivityInstance isolationInstance, bool nonScopedBookmarksExist, out ActivityExecutionWorkItem workItem)
        {
            BookmarkManager manager = null;
            Bookmark bookmark3;
            BookmarkCallbackWrapper wrapper2;
            BookmarkResumptionResult result3;
            workItem = null;
            BookmarkScope key = scope;
            if (scope.IsDefault)
            {
                key = this.defaultScope;
            }
            this.bookmarkManagers.TryGetValue(key, out manager);
            if (manager == null)
            {
                BookmarkResumptionResult notFound = BookmarkResumptionResult.NotFound;
                if (this.uninitializedScopes != null)
                {
                    for (int i = 0; i < this.uninitializedScopes.Count; i++)
                    {
                        Bookmark bookmark2;
                        BookmarkCallbackWrapper wrapper;
                        BookmarkResumptionResult notReady;
                        BookmarkScope scope3 = this.uninitializedScopes[i];
                        if (!this.bookmarkManagers[scope3].TryGetBookmarkFromInternalList(bookmark, out bookmark2, out wrapper))
                        {
                            notReady = BookmarkResumptionResult.NotFound;
                        }
                        else if (this.IsExclusiveScopeUnstable(bookmark2))
                        {
                            notReady = BookmarkResumptionResult.NotReady;
                        }
                        else
                        {
                            notReady = this.bookmarkManagers[scope3].TryGenerateWorkItem(executor, true, ref bookmark, value, isolationInstance, out workItem);
                        }
                        switch (notReady)
                        {
                            case BookmarkResumptionResult.Success:
                                this.InitializeBookmarkScopeWithoutKeyAssociation(scope3, scope.Id);
                                return BookmarkResumptionResult.Success;

                            case BookmarkResumptionResult.NotReady:
                                notFound = BookmarkResumptionResult.NotReady;
                                break;

                            default:
                                if ((notFound == BookmarkResumptionResult.NotFound) && !this.IsStable(scope3, nonScopedBookmarksExist))
                                {
                                    notFound = BookmarkResumptionResult.NotReady;
                                }
                                break;
                        }
                    }
                }
                return notFound;
            }
            if (!manager.TryGetBookmarkFromInternalList(bookmark, out bookmark3, out wrapper2))
            {
                result3 = BookmarkResumptionResult.NotFound;
            }
            else if (this.IsExclusiveScopeUnstable(bookmark3))
            {
                result3 = BookmarkResumptionResult.NotReady;
            }
            else
            {
                result3 = manager.TryGenerateWorkItem(executor, true, ref bookmark, value, isolationInstance, out workItem);
            }
            if ((result3 == BookmarkResumptionResult.NotFound) && !this.IsStable(key, nonScopedBookmarksExist))
            {
                result3 = BookmarkResumptionResult.NotReady;
            }
            return result3;
        }

        public void UnregisterScope(BookmarkScope scope)
        {
            if ((this.bookmarkManagers == null) || !this.bookmarkManagers.ContainsKey(scope))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.BookmarkScopeNotRegisteredForUnregister));
            }
            if (this.bookmarkManagers[scope].HasBookmarks)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.BookmarkScopeHasBookmarks));
            }
            this.bookmarkManagers.Remove(scope);
            if (!scope.IsInitialized)
            {
                this.uninitializedScopes.Remove(scope);
            }
            else
            {
                if (this.keysToDisassociate == null)
                {
                    this.keysToDisassociate = new List<InstanceKey>(2);
                }
                this.keysToDisassociate.Add(new InstanceKey(scope.Id));
            }
        }

        public BookmarkScope Default
        {
            get
            {
                return this.defaultScope;
            }
        }

        public bool HasKeysToUpdate
        {
            get
            {
                return (((this.keysToAssociate != null) && (this.keysToAssociate.Count > 0)) || ((this.keysToDisassociate != null) && (this.keysToDisassociate.Count > 0)));
            }
        }

        [DataMember(EmitDefaultValue=false)]
        private Dictionary<BookmarkScope, BookmarkManager> SerializedBookmarkManagers
        {
            get
            {
                return this.bookmarkManagers;
            }
            set
            {
                this.bookmarkManagers = value;
            }
        }

        [DataMember(EmitDefaultValue=false)]
        private List<BookmarkScope> SerializedUninitializedScopes
        {
            get
            {
                if ((this.uninitializedScopes != null) && (this.uninitializedScopes.Count != 0))
                {
                    return this.uninitializedScopes;
                }
                return null;
            }
            set
            {
                this.uninitializedScopes = value;
            }
        }
    }
}

