namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;

    internal sealed class InternalConfirm : NativeActivity
    {
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            RuntimeArgument argument = new RuntimeArgument("Target", typeof(CompensationToken), ArgumentDirection.In);
            metadata.Bind(this.Target, argument);
            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { argument });
        }

        protected override void Cancel(NativeActivityContext context)
        {
        }

        protected override void Execute(NativeActivityContext context)
        {
            CompensationExtension extension = context.GetExtension<CompensationExtension>();
            CompensationToken token = this.Target.Get(context);
            CompensationTokenData data = extension.Get(token.CompensationId);
            data.BookmarkTable[CompensationBookmarkName.Confirmed] = context.CreateBookmark(new BookmarkCallback(this.OnConfirmed));
            data.CompensationState = CompensationState.Confirming;
            extension.NotifyMessage(context, data.CompensationId, CompensationBookmarkName.OnConfirmation);
        }

        private void OnConfirmed(NativeActivityContext context, Bookmark bookmark, object value)
        {
            CompensationExtension extension = context.GetExtension<CompensationExtension>();
            CompensationToken token = this.Target.Get(context);
            CompensationTokenData compensationToken = extension.Get(token.CompensationId);
            compensationToken.CompensationState = CompensationState.Confirmed;
            if (TD.CompensationStateIsEnabled())
            {
                TD.CompensationState(compensationToken.DisplayName, compensationToken.CompensationState.ToString());
            }
            if (compensationToken.ParentCompensationId != 0L)
            {
                extension.Get(compensationToken.ParentCompensationId).ExecutionTracker.Remove(compensationToken);
            }
            else
            {
                extension.Get(0L).ExecutionTracker.Remove(compensationToken);
            }
            compensationToken.RemoveBookmark(context, CompensationBookmarkName.Confirmed);
            extension.Remove(token.CompensationId);
        }

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        public InArgument<CompensationToken> Target { get; set; }
    }
}

