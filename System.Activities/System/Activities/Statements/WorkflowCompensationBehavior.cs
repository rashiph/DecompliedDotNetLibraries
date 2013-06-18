namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;

    internal sealed class WorkflowCompensationBehavior : NativeActivity
    {
        private Variable<CompensationToken> currentCompensationToken;

        public WorkflowCompensationBehavior()
        {
            Variable<CompensationToken> variable = new Variable<CompensationToken> {
                Name = "currentCompensationToken"
            };
            this.currentCompensationToken = variable;
            System.Activities.Statements.DefaultCompensation compensation = new System.Activities.Statements.DefaultCompensation {
                Target = new InArgument<CompensationToken>(this.currentCompensationToken)
            };
            this.DefaultCompensation = compensation;
            System.Activities.Statements.DefaultConfirmation confirmation = new System.Activities.Statements.DefaultConfirmation {
                Target = new InArgument<CompensationToken>(this.currentCompensationToken)
            };
            this.DefaultConfirmation = confirmation;
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            metadata.SetImplementationChildrenCollection(new Collection<Activity> { this.DefaultCompensation, this.DefaultConfirmation });
            metadata.SetImplementationVariablesCollection(new Collection<Variable> { this.currentCompensationToken });
        }

        protected override void Cancel(NativeActivityContext context)
        {
            context.CancelChildren();
        }

        protected override void Execute(NativeActivityContext context)
        {
            Bookmark bookmark = context.CreateBookmark(new BookmarkCallback(this.OnMainRootComplete), BookmarkOptions.NonBlocking);
            context.RegisterMainRootCompleteCallback(bookmark);
            CompensationExtension extension = context.GetExtension<CompensationExtension>();
            extension.WorkflowCompensation = context.CreateBookmark(new BookmarkCallback(this.OnCompensate));
            extension.WorkflowConfirmation = context.CreateBookmark(new BookmarkCallback(this.OnConfirm));
            context.ResumeBookmark(extension.WorkflowCompensationScheduled, null);
        }

        private void OnCompensate(NativeActivityContext context, Bookmark bookmark, object value)
        {
            CompensationExtension extension = context.GetExtension<CompensationExtension>();
            CompensationToken token = (CompensationToken) value;
            this.currentCompensationToken.Set(context, token);
            if (extension.Get(token.CompensationId).ExecutionTracker.Count > 0)
            {
                context.ScheduleActivity(this.DefaultCompensation, new CompletionCallback(this.OnCompensationComplete));
            }
            else
            {
                this.OnCompensationComplete(context, null);
            }
        }

        private void OnCompensationComplete(NativeActivityContext context, System.Activities.ActivityInstance completedInstance)
        {
            CompensationExtension extension = context.GetExtension<CompensationExtension>();
            context.RemoveBookmark(extension.WorkflowConfirmation);
        }

        private void OnConfirm(NativeActivityContext context, Bookmark bookmark, object value)
        {
            CompensationExtension extension = context.GetExtension<CompensationExtension>();
            CompensationToken token = (CompensationToken) value;
            this.currentCompensationToken.Set(context, token);
            if (extension.Get(token.CompensationId).ExecutionTracker.Count > 0)
            {
                context.ScheduleActivity(this.DefaultConfirmation, new CompletionCallback(this.OnConfirmationComplete));
            }
            else
            {
                this.OnConfirmationComplete(context, null);
            }
        }

        private void OnConfirmationComplete(NativeActivityContext context, System.Activities.ActivityInstance completedInstance)
        {
            CompensationExtension extension = context.GetExtension<CompensationExtension>();
            context.RemoveBookmark(extension.WorkflowCompensation);
        }

        private void OnMainRootComplete(NativeActivityContext context, Bookmark bookmark, object value)
        {
            CompensationExtension extension = context.GetExtension<CompensationExtension>();
            CompensationTokenData tokenData = extension.Get(0L);
            switch (((ActivityInstanceState) value))
            {
                case ActivityInstanceState.Closed:
                    context.ResumeBookmark(extension.WorkflowConfirmation, new CompensationToken(tokenData));
                    return;

                case ActivityInstanceState.Canceled:
                    context.ResumeBookmark(extension.WorkflowCompensation, new CompensationToken(tokenData));
                    return;

                case ActivityInstanceState.Faulted:
                    context.RemoveBookmark(extension.WorkflowConfirmation);
                    context.RemoveBookmark(extension.WorkflowCompensation);
                    break;
            }
        }

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        private Activity DefaultCompensation { get; set; }

        private Activity DefaultConfirmation { get; set; }
    }
}

