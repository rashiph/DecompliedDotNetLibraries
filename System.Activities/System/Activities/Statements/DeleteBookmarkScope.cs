namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;

    public sealed class DeleteBookmarkScope : NativeActivity
    {
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            RuntimeArgument argument = new RuntimeArgument("Scope", typeof(BookmarkScope), ArgumentDirection.In);
            metadata.Bind(this.Scope, argument);
            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { argument });
        }

        protected override void Execute(NativeActivityContext context)
        {
            BookmarkScope scope = this.Scope.Get(context);
            if (scope == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CannotUnregisterNullBookmarkScope));
            }
            if (scope.Equals(context.DefaultBookmarkScope))
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.CannotUnregisterDefaultBookmarkScope));
            }
            context.UnregisterBookmarkScope(scope);
        }

        public InArgument<BookmarkScope> Scope { get; set; }
    }
}

