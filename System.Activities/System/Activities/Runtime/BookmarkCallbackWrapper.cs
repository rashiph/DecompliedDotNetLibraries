namespace System.Activities.Runtime
{
    using System;
    using System.Activities;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;

    [DataContract]
    internal class BookmarkCallbackWrapper : CallbackWrapper
    {
        private static Type[] bookmarkCallbackParameters = new Type[] { typeof(NativeActivityContext), typeof(System.Activities.Bookmark), typeof(object) };
        private static Type bookmarkCallbackType = typeof(BookmarkCallback);

        public BookmarkCallbackWrapper(BookmarkCallback callback, System.Activities.ActivityInstance owningInstance) : this(callback, owningInstance, BookmarkOptions.None)
        {
        }

        public BookmarkCallbackWrapper(BookmarkCallback callback, System.Activities.ActivityInstance owningInstance, BookmarkOptions bookmarkOptions) : base(callback, owningInstance)
        {
            this.Options = bookmarkOptions;
        }

        public ActivityExecutionWorkItem CreateWorkItem(ActivityExecutor executor, bool isExternal, System.Activities.Bookmark bookmark, object value)
        {
            if (base.IsCallbackNull)
            {
                return executor.CreateEmptyWorkItem(base.ActivityInstance);
            }
            return new BookmarkWorkItem(executor, isExternal, this, bookmark, value);
        }

        public void Invoke(NativeActivityContext context, System.Activities.Bookmark bookmark, object value)
        {
            base.EnsureCallback(bookmarkCallbackType, bookmarkCallbackParameters);
            BookmarkCallback callback = (BookmarkCallback) base.Callback;
            callback(context, bookmark, value);
        }

        [DataMember(EmitDefaultValue=false)]
        public System.Activities.Bookmark Bookmark { get; set; }

        [DataMember(EmitDefaultValue=false)]
        public BookmarkOptions Options { get; private set; }
    }
}

