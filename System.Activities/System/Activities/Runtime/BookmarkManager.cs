namespace System.Activities.Runtime
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;

    [DataContract(Name="BookmarkManager", Namespace="http://schemas.datacontract.org/2010/02/System.Activities")]
    internal class BookmarkManager
    {
        [DataMember(EmitDefaultValue=false)]
        private Dictionary<Bookmark, BookmarkCallbackWrapper> bookmarks;
        [DataMember]
        private long nextId;
        [DataMember(EmitDefaultValue=false)]
        private BookmarkScope scope;
        [DataMember(EmitDefaultValue=false)]
        private BookmarkScopeHandle scopeHandle;

        public BookmarkManager()
        {
            this.nextId = 1L;
        }

        internal BookmarkManager(BookmarkScope scope, BookmarkScopeHandle scopeHandle) : this()
        {
            this.scope = scope;
            this.scopeHandle = scopeHandle;
        }

        private void AddBookmark(Bookmark bookmark, BookmarkCallback callback, System.Activities.ActivityInstance owningInstance, BookmarkOptions options)
        {
            if (this.bookmarks == null)
            {
                this.bookmarks = new Dictionary<Bookmark, BookmarkCallbackWrapper>(Bookmark.Comparer);
            }
            bookmark.Scope = this.scope;
            BookmarkCallbackWrapper wrapper = new BookmarkCallbackWrapper(callback, owningInstance, options) {
                Bookmark = bookmark
            };
            this.bookmarks.Add(bookmark, wrapper);
            owningInstance.AddBookmark(bookmark, options);
            if (TD.CreateBookmarkIsEnabled())
            {
                TD.CreateBookmark(owningInstance.Activity.GetType().ToString(), owningInstance.Activity.DisplayName, owningInstance.Id, ActivityUtilities.GetTraceString(bookmark), ActivityUtilities.GetTraceString(bookmark.Scope));
            }
        }

        public Bookmark CreateBookmark(BookmarkCallback callback, System.Activities.ActivityInstance owningInstance, BookmarkOptions options)
        {
            Bookmark bookmark = Bookmark.Create(this.GetNextBookmarkId());
            this.AddBookmark(bookmark, callback, owningInstance, options);
            this.UpdateAllExclusiveHandles(bookmark, owningInstance);
            return bookmark;
        }

        public Bookmark CreateBookmark(string name, BookmarkCallback callback, System.Activities.ActivityInstance owningInstance, BookmarkOptions options)
        {
            Bookmark key = new Bookmark(name);
            if ((this.bookmarks != null) && this.bookmarks.ContainsKey(key))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.BookmarkAlreadyExists(name)));
            }
            this.AddBookmark(key, callback, owningInstance, options);
            this.UpdateAllExclusiveHandles(key, owningInstance);
            return key;
        }

        public Bookmark GenerateTempBookmark()
        {
            return Bookmark.Create(this.GetNextBookmarkId());
        }

        private long GetNextBookmarkId()
        {
            if (this.nextId == 0x7fffffffffffffffL)
            {
                throw FxTrace.Exception.AsError(new NotSupportedException(System.Activities.SR.OutOfInternalBookmarks));
            }
            long nextId = this.nextId;
            this.nextId += 1L;
            return nextId;
        }

        public void PopulateBookmarkInfo(List<BookmarkInfo> bookmarks)
        {
            foreach (KeyValuePair<Bookmark, BookmarkCallbackWrapper> pair in this.bookmarks)
            {
                if (pair.Key.IsNamed)
                {
                    bookmarks.Add(pair.Key.GenerateBookmarkInfo(pair.Value));
                }
            }
        }

        public void PurgeBookmarks(Bookmark singleBookmark, IList<Bookmark> multipleBookmarks)
        {
            if (singleBookmark != null)
            {
                this.PurgeSingleBookmark(singleBookmark);
            }
            else
            {
                for (int i = 0; i < multipleBookmarks.Count; i++)
                {
                    Bookmark bookmark = multipleBookmarks[i];
                    this.PurgeSingleBookmark(bookmark);
                }
            }
        }

        internal void PurgeSingleBookmark(Bookmark bookmark)
        {
            this.UpdateExclusiveHandleList(bookmark);
            this.bookmarks.Remove(bookmark);
        }

        public bool Remove(Bookmark bookmark, System.Activities.ActivityInstance instanceAttemptingRemove)
        {
            BookmarkCallbackWrapper wrapper;
            Bookmark bookmark2;
            if (!this.TryGetBookmarkFromInternalList(bookmark, out bookmark2, out wrapper))
            {
                return false;
            }
            if (wrapper.ActivityInstance != instanceAttemptingRemove)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.OnlyBookmarkOwnerCanRemove));
            }
            this.Remove(bookmark2, wrapper);
            return true;
        }

        private void Remove(Bookmark bookmark, BookmarkCallbackWrapper callbackWrapper)
        {
            callbackWrapper.ActivityInstance.RemoveBookmark(bookmark, callbackWrapper.Options);
            this.UpdateExclusiveHandleList(bookmark);
            this.bookmarks.Remove(bookmark);
        }

        public BookmarkResumptionResult TryGenerateWorkItem(ActivityExecutor executor, bool isExternal, ref Bookmark bookmark, object value, System.Activities.ActivityInstance isolationInstance, out ActivityExecutionWorkItem workItem)
        {
            Bookmark internalBookmark = null;
            BookmarkCallbackWrapper callbackWrapper = null;
            if (!this.TryGetBookmarkFromInternalList(bookmark, out internalBookmark, out callbackWrapper))
            {
                workItem = null;
                return BookmarkResumptionResult.NotFound;
            }
            bookmark = internalBookmark;
            if (!ActivityUtilities.IsInScope(callbackWrapper.ActivityInstance, isolationInstance))
            {
                workItem = null;
                return BookmarkResumptionResult.NotReady;
            }
            workItem = callbackWrapper.CreateWorkItem(executor, isExternal, bookmark, value);
            if (!BookmarkOptionsHelper.SupportsMultipleResumes(callbackWrapper.Options))
            {
                this.Remove(bookmark, callbackWrapper);
            }
            return BookmarkResumptionResult.Success;
        }

        public bool TryGetBookmarkFromInternalList(Bookmark bookmark, out Bookmark internalBookmark, out BookmarkCallbackWrapper callbackWrapper)
        {
            BookmarkCallbackWrapper wrapper;
            internalBookmark = null;
            callbackWrapper = null;
            if ((this.bookmarks != null) && this.bookmarks.TryGetValue(bookmark, out wrapper))
            {
                internalBookmark = wrapper.Bookmark;
                callbackWrapper = wrapper;
                return true;
            }
            return false;
        }

        private void UpdateAllExclusiveHandles(Bookmark bookmark, System.Activities.ActivityInstance owningInstance)
        {
            if ((owningInstance.PropertyManager != null) && owningInstance.PropertyManager.HasExclusiveHandlesInScope)
            {
                List<ExclusiveHandle> list = owningInstance.PropertyManager.FindAll<ExclusiveHandle>();
                if (list != null)
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        ExclusiveHandle handle = list[i];
                        if (handle != null)
                        {
                            if (this.scopeHandle != null)
                            {
                                bool flag = false;
                                foreach (BookmarkScopeHandle handle2 in handle.RegisteredBookmarkScopes)
                                {
                                    if (handle2 == this.scopeHandle)
                                    {
                                        handle.AddToImportantBookmarks(bookmark);
                                        flag = true;
                                        break;
                                    }
                                }
                                if (!flag)
                                {
                                    handle.AddToUnimportantBookmarks(bookmark);
                                }
                                continue;
                            }
                            handle.AddToUnimportantBookmarks(bookmark);
                        }
                    }
                }
            }
        }

        private void UpdateExclusiveHandleList(Bookmark bookmark)
        {
            if (bookmark.ExclusiveHandles != null)
            {
                for (int i = 0; i < bookmark.ExclusiveHandles.Count; i++)
                {
                    bookmark.ExclusiveHandles[i].RemoveBookmark(bookmark);
                }
            }
        }

        public bool HasBookmarks
        {
            get
            {
                return ((this.bookmarks != null) && (this.bookmarks.Count > 0));
            }
        }
    }
}

