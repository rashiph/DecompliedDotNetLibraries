namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.Collections;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [ContentProperty("Body")]
    public sealed class CompensableActivity : NativeActivity<CompensationToken>
    {
        private Variable<long> compensationId = new Variable<long>();
        private System.Activities.Statements.CompensationParticipant compensationParticipant;
        private Variable<long> currentCompensationId = new Variable<long>();
        private Variable<CompensationToken> currentCompensationToken = new Variable<CompensationToken>();
        private static Constraint noCompensableActivityInSecondaryRoot = NoCompensableActivityInSecondaryRoot();
        private Collection<Variable> variables;

        private void AppCompletionCleanup(NativeActivityContext context, CompensationExtension compensationExtension, CompensationTokenData compensationToken)
        {
            if (compensationToken.ParentCompensationId != 0L)
            {
                compensationExtension.Get(compensationToken.ParentCompensationId).ExecutionTracker.Remove(compensationToken);
            }
            else
            {
                compensationExtension.Get(0L).ExecutionTracker.Remove(compensationToken);
            }
            compensationToken.RemoveBookmark(context, CompensationBookmarkName.Canceled);
            compensationToken.RemoveBookmark(context, CompensationBookmarkName.Compensated);
            compensationExtension.Remove(compensationToken.CompensationId);
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            metadata.SetVariablesCollection(this.Variables);
            metadata.SetImplementationVariablesCollection(new Collection<Variable> { this.currentCompensationId, this.currentCompensationToken, this.compensationId });
            if (this.Body != null)
            {
                metadata.SetChildrenCollection(new Collection<Activity> { this.Body });
            }
            if (this.CompensationHandler != null)
            {
                metadata.AddImportedChild(this.CompensationHandler);
            }
            if (this.ConfirmationHandler != null)
            {
                metadata.AddImportedChild(this.ConfirmationHandler);
            }
            if (this.CancellationHandler != null)
            {
                metadata.AddImportedChild(this.CancellationHandler);
            }
            Collection<Activity> implementationChildren = new Collection<Activity>();
            if (!base.IsSingletonActivityDeclared("Activities.Compensation.WorkflowImplicitCompensationBehavior"))
            {
                WorkflowCompensationBehavior activity = new WorkflowCompensationBehavior();
                base.DeclareSingletonActivity("Activities.Compensation.WorkflowImplicitCompensationBehavior", activity);
                implementationChildren.Add(activity);
                metadata.AddDefaultExtensionProvider<CompensationExtension>(new Func<CompensationExtension>(this.CreateCompensationExtension));
            }
            this.CompensationParticipant = null;
            implementationChildren.Add(this.CompensationParticipant);
            metadata.SetImplementationChildrenCollection(implementationChildren);
        }

        protected override void Cancel(NativeActivityContext context)
        {
            context.CancelChildren();
        }

        private CompensationExtension CreateCompensationExtension()
        {
            return new CompensationExtension();
        }

        protected override void Execute(NativeActivityContext context)
        {
            CompensationExtension compensationExtension = context.GetExtension<CompensationExtension>();
            if (compensationExtension.IsWorkflowCompensationBehaviorScheduled)
            {
                this.ScheduleBody(context, compensationExtension);
            }
            else
            {
                compensationExtension.SetupWorkflowCompensationBehavior(context, new BookmarkCallback(this.OnWorkflowCompensationBehaviorScheduled), base.GetSingletonActivity("Activities.Compensation.WorkflowImplicitCompensationBehavior"));
            }
        }

        internal override IList<Constraint> InternalGetConstraints()
        {
            return new List<Constraint>(1) { noCompensableActivityInSecondaryRoot };
        }

        private static Constraint NoCompensableActivityInSecondaryRoot()
        {
            DelegateInArgument<ValidationContext> argument = new DelegateInArgument<ValidationContext> {
                Name = "validationContext"
            };
            DelegateInArgument<CompensableActivity> argument2 = new DelegateInArgument<CompensableActivity> {
                Name = "element"
            };
            Variable<bool> assertFlag = new Variable<bool> {
                Name = "assertFlag",
                Default = 1
            };
            Variable<IEnumerable<Activity>> elements = new Variable<IEnumerable<Activity>> {
                Name = "elements"
            };
            Variable<int> index = new Variable<int> {
                Name = "index"
            };
            Constraint<CompensableActivity> constraint = new Constraint<CompensableActivity>();
            ActivityAction<CompensableActivity, ValidationContext> action = new ActivityAction<CompensableActivity, ValidationContext> {
                Argument1 = argument2,
                Argument2 = argument
            };
            Sequence sequence = new Sequence {
                Variables = { assertFlag, elements, index }
            };
            Assign<IEnumerable<Activity>> item = new Assign<IEnumerable<Activity>> {
                To = elements
            };
            GetParentChain chain = new GetParentChain {
                ValidationContext = argument
            };
            item.Value = chain;
            sequence.Activities.Add(item);
            While @while = new While(env => assertFlag.Get(env) && (index.Get(env) < elements.Get(env).Count<Activity>()));
            Sequence sequence2 = new Sequence();
            If @if = new If(env => elements.Get(env).ElementAt<Activity>(index.Get(env)).GetType() == typeof(System.Activities.Statements.CompensationParticipant));
            Assign<bool> assign2 = new Assign<bool> {
                To = assertFlag,
                Value = 0
            };
            @if.Then = assign2;
            sequence2.Activities.Add(@if);
            Assign<int> assign3 = new Assign<int> {
                To = index,
                Value = new InArgument<int>(env => index.Get(env) + 1)
            };
            sequence2.Activities.Add(assign3);
            @while.Body = sequence2;
            sequence.Activities.Add(@while);
            AssertValidation validation = new AssertValidation {
                Assertion = new InArgument<bool>(assertFlag),
                Message = new InArgument<string>(System.Activities.SR.NoCAInSecondaryRoot)
            };
            sequence.Activities.Add(validation);
            action.Handler = sequence;
            constraint.Body = action;
            return constraint;
        }

        private void OnBodyExecutionComplete(NativeActivityContext context, System.Activities.ActivityInstance completedInstance)
        {
            CompensationExtension compensationExtension = context.GetExtension<CompensationExtension>();
            CompensationTokenData token = compensationExtension.Get(this.currentCompensationId.Get(context));
            if (completedInstance.State == ActivityInstanceState.Closed)
            {
                token.CompensationState = CompensationState.Completed;
                if (TD.CompensationStateIsEnabled())
                {
                    TD.CompensationState(token.DisplayName, token.CompensationState.ToString());
                }
                if (context.IsCancellationRequested)
                {
                    token.CompensationState = CompensationState.Compensating;
                }
            }
            else if ((completedInstance.State == ActivityInstanceState.Canceled) || (completedInstance.State == ActivityInstanceState.Faulted))
            {
                token.CompensationState = CompensationState.Canceling;
            }
            this.ScheduleSecondaryRoot(context, compensationExtension, token);
        }

        private void OnCanceledOrCompensated(NativeActivityContext context, Bookmark bookmark, object value)
        {
            CompensationExtension compensationExtension = context.GetExtension<CompensationExtension>();
            long compensationId = (long) value;
            CompensationTokenData compensationToken = compensationExtension.Get(compensationId);
            switch (compensationToken.CompensationState)
            {
                case CompensationState.Compensating:
                    compensationToken.CompensationState = CompensationState.Compensated;
                    break;

                case CompensationState.Canceling:
                    compensationToken.CompensationState = CompensationState.Canceled;
                    break;
            }
            if (TD.CompensationStateIsEnabled())
            {
                TD.CompensationState(compensationToken.DisplayName, compensationToken.CompensationState.ToString());
            }
            this.AppCompletionCleanup(context, compensationExtension, compensationToken);
            context.MarkCanceled();
        }

        private void OnSecondaryRootScheduled(NativeActivityContext context, Bookmark bookmark, object value)
        {
            CompensationExtension extension = context.GetExtension<CompensationExtension>();
            long compensationId = (long) value;
            CompensationTokenData data = extension.Get(compensationId);
            if (data.CompensationState == CompensationState.Canceling)
            {
                data.BookmarkTable[CompensationBookmarkName.Canceled] = context.CreateBookmark(new BookmarkCallback(this.OnCanceledOrCompensated));
                extension.NotifyMessage(context, data.CompensationId, CompensationBookmarkName.OnCancellation);
            }
            else if (data.CompensationState == CompensationState.Compensating)
            {
                data.BookmarkTable[CompensationBookmarkName.Compensated] = context.CreateBookmark(new BookmarkCallback(this.OnCanceledOrCompensated));
                extension.NotifyMessage(context, data.CompensationId, CompensationBookmarkName.OnCompensation);
            }
        }

        private void OnWorkflowCompensationBehaviorScheduled(NativeActivityContext context, Bookmark bookmark, object value)
        {
            CompensationExtension compensationExtension = context.GetExtension<CompensationExtension>();
            this.ScheduleBody(context, compensationExtension);
        }

        private void ScheduleBody(NativeActivityContext context, CompensationExtension compensationExtension)
        {
            CompensationToken token = null;
            long parentCompensationId = 0L;
            token = (CompensationToken) context.Properties.Find("System.Compensation.CompensationToken");
            if (token != null)
            {
                if (compensationExtension.Get(token.CompensationId).IsTokenValidInSecondaryRoot)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(System.Activities.SR.NoCAInSecondaryRoot));
                }
                parentCompensationId = token.CompensationId;
            }
            CompensationTokenData tokenData = new CompensationTokenData(compensationExtension.GetNextId(), parentCompensationId) {
                CompensationState = CompensationState.Active,
                DisplayName = base.DisplayName
            };
            CompensationToken property = new CompensationToken(tokenData);
            context.Properties.Add("System.Compensation.CompensationToken", property);
            this.currentCompensationId.Set(context, property.CompensationId);
            this.currentCompensationToken.Set(context, property);
            compensationExtension.Add(property.CompensationId, tokenData);
            if (TD.CompensationStateIsEnabled())
            {
                TD.CompensationState(tokenData.DisplayName, tokenData.CompensationState.ToString());
            }
            if (this.Body != null)
            {
                context.ScheduleActivity(this.Body, new CompletionCallback(this.OnBodyExecutionComplete));
            }
            else
            {
                tokenData.CompensationState = CompensationState.Completed;
                if (TD.CompensationStateIsEnabled())
                {
                    TD.CompensationState(tokenData.DisplayName, tokenData.CompensationState.ToString());
                }
                this.ScheduleSecondaryRoot(context, compensationExtension, tokenData);
            }
        }

        private void ScheduleSecondaryRoot(NativeActivityContext context, CompensationExtension compensationExtension, CompensationTokenData token)
        {
            if (token.ParentCompensationId != 0L)
            {
                compensationExtension.Get(token.ParentCompensationId).ExecutionTracker.Add(token);
            }
            else
            {
                compensationExtension.Get(0L).ExecutionTracker.Add(token);
            }
            if ((base.Result != null) && (token.CompensationState == CompensationState.Completed))
            {
                base.Result.Set(context, this.currentCompensationToken.Get(context));
            }
            token.BookmarkTable[CompensationBookmarkName.OnSecondaryRootScheduled] = context.CreateBookmark(new BookmarkCallback(this.OnSecondaryRootScheduled));
            this.compensationId.Set(context, token.CompensationId);
            context.ScheduleSecondaryRoot(this.CompensationParticipant, context.Environment);
        }

        [DefaultValue((string) null), DependsOn("Variables")]
        public Activity Body { get; set; }

        [DependsOn("Body"), DefaultValue((string) null)]
        public Activity CancellationHandler { get; set; }

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        [DefaultValue((string) null), DependsOn("CancellationHandler")]
        public Activity CompensationHandler { get; set; }

        private System.Activities.Statements.CompensationParticipant CompensationParticipant
        {
            get
            {
                if (this.compensationParticipant == null)
                {
                    this.compensationParticipant = new System.Activities.Statements.CompensationParticipant(this.compensationId);
                    if (this.CompensationHandler != null)
                    {
                        this.compensationParticipant.CompensationHandler = this.CompensationHandler;
                    }
                    if (this.ConfirmationHandler != null)
                    {
                        this.compensationParticipant.ConfirmationHandler = this.ConfirmationHandler;
                    }
                    if (this.CancellationHandler != null)
                    {
                        this.compensationParticipant.CancellationHandler = this.CancellationHandler;
                    }
                }
                return this.compensationParticipant;
            }
            set
            {
                this.compensationParticipant = value;
            }
        }

        [DependsOn("CompensationHandler"), DefaultValue((string) null)]
        public Activity ConfirmationHandler { get; set; }

        public Collection<Variable> Variables
        {
            get
            {
                if (this.variables == null)
                {
                    ValidatingCollection<Variable> validatings = new ValidatingCollection<Variable> {
                        OnAddValidationCallback = delegate (Variable item) {
                            if (item == null)
                            {
                                throw FxTrace.Exception.ArgumentNull("item");
                            }
                        }
                    };
                    this.variables = validatings;
                }
                return this.variables;
            }
        }
    }
}

