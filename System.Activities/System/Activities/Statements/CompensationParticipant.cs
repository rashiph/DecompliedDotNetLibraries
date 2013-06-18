namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.Runtime.CompilerServices;

    internal sealed class CompensationParticipant : NativeActivity
    {
        private InArgument<long> compensationId;
        private Variable<CompensationToken> currentCompensationToken;

        public CompensationParticipant(Variable<long> compensationId)
        {
            this.compensationId = compensationId;
            this.currentCompensationToken = new Variable<CompensationToken>();
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
            metadata.SetImplementationVariablesCollection(new Collection<Variable> { this.currentCompensationToken });
            Collection<Activity> children = new Collection<Activity>();
            if (this.CompensationHandler != null)
            {
                children.Add(this.CompensationHandler);
            }
            if (this.ConfirmationHandler != null)
            {
                children.Add(this.ConfirmationHandler);
            }
            if (this.CancellationHandler != null)
            {
                children.Add(this.CancellationHandler);
            }
            metadata.SetChildrenCollection(children);
            Collection<Activity> implementationChildren = new Collection<Activity> {
                this.DefaultCompensation,
                this.DefaultConfirmation
            };
            metadata.SetImplementationChildrenCollection(implementationChildren);
            RuntimeArgument argument = new RuntimeArgument("CompensationId", typeof(long), ArgumentDirection.In);
            metadata.Bind(this.compensationId, argument);
            metadata.AddArgument(argument);
        }

        protected override void Execute(NativeActivityContext context)
        {
            CompensationExtension extension = context.GetExtension<CompensationExtension>();
            long compensationId = this.compensationId.Get(context);
            CompensationTokenData tokenData = extension.Get(compensationId);
            CompensationToken token = new CompensationToken(tokenData);
            this.currentCompensationToken.Set(context, token);
            tokenData.IsTokenValidInSecondaryRoot = true;
            context.Properties.Add("System.Compensation.CompensationToken", token);
            tokenData.BookmarkTable[CompensationBookmarkName.OnConfirmation] = context.CreateBookmark(new BookmarkCallback(this.OnConfirmation));
            tokenData.BookmarkTable[CompensationBookmarkName.OnCompensation] = context.CreateBookmark(new BookmarkCallback(this.OnCompensation));
            tokenData.BookmarkTable[CompensationBookmarkName.OnCancellation] = context.CreateBookmark(new BookmarkCallback(this.OnCancellation));
            Bookmark bookmark = tokenData.BookmarkTable[CompensationBookmarkName.OnSecondaryRootScheduled];
            tokenData.BookmarkTable[CompensationBookmarkName.OnSecondaryRootScheduled] = null;
            context.ResumeBookmark(bookmark, compensationId);
        }

        private void InternalOnCompensationComplete(NativeActivityContext context, CompensationExtension compensationExtension, CompensationTokenData compensationToken)
        {
            switch (compensationToken.CompensationState)
            {
                case CompensationState.Compensating:
                    compensationExtension.NotifyMessage(context, compensationToken.CompensationId, CompensationBookmarkName.Compensated);
                    break;

                case CompensationState.Compensated:
                    break;

                case CompensationState.Canceling:
                    compensationExtension.NotifyMessage(context, compensationToken.CompensationId, CompensationBookmarkName.Canceled);
                    return;

                default:
                    return;
            }
        }

        private void OnCancellation(NativeActivityContext context, Bookmark bookmark, object value)
        {
            CompensationExtension compensationExtension = context.GetExtension<CompensationExtension>();
            long compensationId = (long) value;
            CompensationTokenData tokenData = compensationExtension.Get(compensationId);
            if (TD.CompensationStateIsEnabled())
            {
                TD.CompensationState(tokenData.DisplayName, tokenData.CompensationState.ToString());
            }
            tokenData.RemoveBookmark(context, CompensationBookmarkName.OnCompensation);
            tokenData.RemoveBookmark(context, CompensationBookmarkName.OnConfirmation);
            this.currentCompensationToken.Set(context, new CompensationToken(tokenData));
            if (this.CancellationHandler != null)
            {
                context.ScheduleActivity(this.CancellationHandler, new CompletionCallback(this.OnCancellationHandlerComplete), new FaultCallback(this.OnExceptionFromHandler));
            }
            else if (tokenData.ExecutionTracker.Count > 0)
            {
                context.ScheduleActivity(this.DefaultCompensation, new CompletionCallback(this.OnCompensationComplete));
            }
            else
            {
                this.InternalOnCompensationComplete(context, compensationExtension, tokenData);
            }
        }

        private void OnCancellationHandlerComplete(NativeActivityContext context, System.Activities.ActivityInstance completedInstance)
        {
            CompensationExtension compensationExtension = context.GetExtension<CompensationExtension>();
            CompensationTokenData tokenData = compensationExtension.Get(this.compensationId.Get(context));
            if (completedInstance.State == ActivityInstanceState.Closed)
            {
                this.currentCompensationToken.Set(context, new CompensationToken(tokenData));
                if (tokenData.ExecutionTracker.Count > 0)
                {
                    context.ScheduleActivity(this.DefaultConfirmation, new CompletionCallback(this.OnCompensationComplete));
                }
                else
                {
                    this.InternalOnCompensationComplete(context, compensationExtension, tokenData);
                }
            }
        }

        private void OnCompensation(NativeActivityContext context, Bookmark bookmark, object value)
        {
            CompensationExtension compensationExtension = context.GetExtension<CompensationExtension>();
            long compensationId = (long) value;
            CompensationTokenData tokenData = compensationExtension.Get(compensationId);
            if (TD.CompensationStateIsEnabled())
            {
                TD.CompensationState(tokenData.DisplayName, tokenData.CompensationState.ToString());
            }
            tokenData.RemoveBookmark(context, CompensationBookmarkName.OnCancellation);
            tokenData.RemoveBookmark(context, CompensationBookmarkName.OnConfirmation);
            if (this.CompensationHandler != null)
            {
                context.ScheduleActivity(this.CompensationHandler, new CompletionCallback(this.OnCompensationHandlerComplete), new FaultCallback(this.OnExceptionFromHandler));
            }
            else
            {
                this.currentCompensationToken.Set(context, new CompensationToken(tokenData));
                if (tokenData.ExecutionTracker.Count > 0)
                {
                    context.ScheduleActivity(this.DefaultCompensation, new CompletionCallback(this.OnCompensationComplete));
                }
                else
                {
                    this.InternalOnCompensationComplete(context, compensationExtension, tokenData);
                }
            }
        }

        private void OnCompensationComplete(NativeActivityContext context, System.Activities.ActivityInstance completedInstance)
        {
            CompensationExtension compensationExtension = context.GetExtension<CompensationExtension>();
            CompensationTokenData compensationToken = compensationExtension.Get(this.compensationId.Get(context));
            this.InternalOnCompensationComplete(context, compensationExtension, compensationToken);
        }

        private void OnCompensationHandlerComplete(NativeActivityContext context, System.Activities.ActivityInstance completedInstance)
        {
            CompensationExtension compensationExtension = context.GetExtension<CompensationExtension>();
            CompensationTokenData tokenData = compensationExtension.Get(this.compensationId.Get(context));
            if (completedInstance.State == ActivityInstanceState.Closed)
            {
                this.currentCompensationToken.Set(context, new CompensationToken(tokenData));
                if (tokenData.ExecutionTracker.Count > 0)
                {
                    context.ScheduleActivity(this.DefaultConfirmation, new CompletionCallback(this.OnCompensationComplete));
                }
                else
                {
                    this.InternalOnCompensationComplete(context, compensationExtension, tokenData);
                }
            }
        }

        private void OnConfirmation(NativeActivityContext context, Bookmark bookmark, object value)
        {
            CompensationExtension extension = context.GetExtension<CompensationExtension>();
            long compensationId = (long) value;
            CompensationTokenData tokenData = extension.Get(compensationId);
            if (TD.CompensationStateIsEnabled())
            {
                TD.CompensationState(tokenData.DisplayName, tokenData.CompensationState.ToString());
            }
            tokenData.RemoveBookmark(context, CompensationBookmarkName.OnCancellation);
            tokenData.RemoveBookmark(context, CompensationBookmarkName.OnCompensation);
            if (this.ConfirmationHandler != null)
            {
                context.ScheduleActivity(this.ConfirmationHandler, new CompletionCallback(this.OnConfirmationHandlerComplete), new FaultCallback(this.OnExceptionFromHandler));
            }
            else
            {
                this.currentCompensationToken.Set(context, new CompensationToken(tokenData));
                if (tokenData.ExecutionTracker.Count > 0)
                {
                    context.ScheduleActivity(this.DefaultConfirmation, new CompletionCallback(this.OnConfirmationComplete));
                }
                else
                {
                    extension.NotifyMessage(context, tokenData.CompensationId, CompensationBookmarkName.Confirmed);
                }
            }
        }

        private void OnConfirmationComplete(NativeActivityContext context, System.Activities.ActivityInstance completedInstance)
        {
            CompensationExtension extension = context.GetExtension<CompensationExtension>();
            CompensationTokenData data = extension.Get(this.compensationId.Get(context));
            if (completedInstance.State == ActivityInstanceState.Closed)
            {
                extension.NotifyMessage(context, data.CompensationId, CompensationBookmarkName.Confirmed);
            }
        }

        private void OnConfirmationHandlerComplete(NativeActivityContext context, System.Activities.ActivityInstance completedInstance)
        {
            CompensationExtension extension = context.GetExtension<CompensationExtension>();
            CompensationTokenData tokenData = extension.Get(this.compensationId.Get(context));
            if (completedInstance.State == ActivityInstanceState.Closed)
            {
                this.currentCompensationToken.Set(context, new CompensationToken(tokenData));
                if (tokenData.ExecutionTracker.Count > 0)
                {
                    context.ScheduleActivity(this.DefaultConfirmation, new CompletionCallback(this.OnConfirmationComplete));
                }
                else
                {
                    extension.NotifyMessage(context, tokenData.CompensationId, CompensationBookmarkName.Confirmed);
                }
            }
        }

        private void OnExceptionFromHandler(NativeActivityFaultContext context, Exception propagatedException, System.Activities.ActivityInstance propagatedFrom)
        {
            CompensationTokenData data = context.GetExtension<CompensationExtension>().Get(this.compensationId.Get(context));
            InvalidOperationException reason = null;
            switch (data.CompensationState)
            {
                case CompensationState.Confirming:
                    reason = new InvalidOperationException(System.Activities.SR.ConfirmationHandlerFatalException(data.DisplayName), propagatedException);
                    break;

                case CompensationState.Compensating:
                    reason = new InvalidOperationException(System.Activities.SR.CompensationHandlerFatalException(data.DisplayName), propagatedException);
                    break;

                case CompensationState.Canceling:
                    reason = new InvalidOperationException(System.Activities.SR.CancellationHandlerFatalException(data.DisplayName), propagatedException);
                    break;
            }
            context.Abort(reason);
        }

        public Activity CancellationHandler { get; set; }

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        public Activity CompensationHandler { get; set; }

        public Activity ConfirmationHandler { get; set; }

        private Activity DefaultCompensation { get; set; }

        private Activity DefaultConfirmation { get; set; }
    }
}

