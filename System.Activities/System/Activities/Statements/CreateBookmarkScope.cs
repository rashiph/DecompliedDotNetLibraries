namespace System.Activities.Statements
{
    using System;
    using System.Activities;

    public sealed class CreateBookmarkScope : NativeActivity<BookmarkScope>
    {
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
        }

        protected override void Execute(NativeActivityContext context)
        {
            context.SetValue<BookmarkScope>(base.Result, context.CreateBookmarkScope());
        }
    }
}

