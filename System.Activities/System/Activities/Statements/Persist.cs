namespace System.Activities.Statements
{
    using System;
    using System.Activities;

    public sealed class Persist : NativeActivity
    {
        private static BookmarkCallback onPersistCompleteCallback;

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (context.IsInNoPersistScope)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CannotPersistInsideNoPersist));
            }
            if (onPersistCompleteCallback == null)
            {
                onPersistCompleteCallback = new BookmarkCallback(Persist.OnPersistComplete);
            }
            context.RequestPersist(onPersistCompleteCallback);
        }

        private static void OnPersistComplete(NativeActivityContext context, Bookmark bookmark, object value)
        {
        }

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }
    }
}

