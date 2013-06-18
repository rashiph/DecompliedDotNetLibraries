namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.Runtime.Collections;
    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Windows.Markup;

    [ContentProperty("Branches")]
    public sealed class Pick : NativeActivity
    {
        private Collection<Activity> branchBodies;
        private Collection<PickBranch> branches;
        private const string pickStateProperty = "System.Activities.Statements.Pick.PickState";
        private Variable<PickState> pickStateVariable = new Variable<PickState>();

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (this.branchBodies == null)
            {
                this.branchBodies = new Collection<Activity>();
            }
            else
            {
                this.branchBodies.Clear();
            }
            foreach (PickBranch branch in this.Branches)
            {
                if (branch.Trigger == null)
                {
                    metadata.AddValidationError(System.Activities.SR.PickBranchRequiresTrigger(branch.DisplayName));
                }
                PickBranchBody item = new PickBranchBody {
                    Action = branch.Action,
                    DisplayName = branch.DisplayName,
                    Trigger = branch.Trigger,
                    Variables = branch.Variables
                };
                this.branchBodies.Add(item);
            }
            metadata.SetChildrenCollection(this.branchBodies);
            metadata.AddImplementationVariable(this.pickStateVariable);
        }

        protected override void Cancel(NativeActivityContext context)
        {
            context.CancelChildren();
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (this.branchBodies.Count != 0)
            {
                PickState state = new PickState();
                this.pickStateVariable.Set(context, state);
                state.TriggerCompletionBookmark = context.CreateBookmark(new BookmarkCallback(this.OnTriggerComplete));
                context.Properties.Add("System.Activities.Statements.Pick.PickState", state);
                CompletionCallback onCompleted = new CompletionCallback(this.OnBranchComplete);
                for (int i = this.branchBodies.Count - 1; i >= 0; i--)
                {
                    context.ScheduleActivity(this.branchBodies[i], onCompleted);
                }
            }
        }

        private void OnBranchComplete(NativeActivityContext context, System.Activities.ActivityInstance completedInstance)
        {
            PickState pickState = this.pickStateVariable.Get(context);
            ReadOnlyCollection<System.Activities.ActivityInstance> children = context.GetChildren();
            switch (completedInstance.State)
            {
                case ActivityInstanceState.Closed:
                    pickState.HasBranchCompletedSuccessfully = true;
                    break;

                case ActivityInstanceState.Canceled:
                case ActivityInstanceState.Faulted:
                    if ((context.IsCancellationRequested && (children.Count == 0)) && !pickState.HasBranchCompletedSuccessfully)
                    {
                        context.MarkCanceled();
                        context.RemoveAllBookmarks();
                    }
                    break;
            }
            if ((children.Count == 1) && (pickState.ExecuteActionBookmark != null))
            {
                this.ResumeExecutionActionBookmark(pickState, context);
            }
        }

        private void OnTriggerComplete(NativeActivityContext context, Bookmark bookmark, object state)
        {
            PickState pickState = this.pickStateVariable.Get(context);
            string str = (string) state;
            ReadOnlyCollection<System.Activities.ActivityInstance> children = context.GetChildren();
            bool flag = true;
            for (int i = 0; i < children.Count; i++)
            {
                System.Activities.ActivityInstance activityInstance = children[i];
                if (activityInstance.Id != str)
                {
                    context.CancelChild(activityInstance);
                    flag = false;
                }
            }
            if (flag)
            {
                this.ResumeExecutionActionBookmark(pickState, context);
            }
        }

        private void ResumeExecutionActionBookmark(PickState pickState, NativeActivityContext context)
        {
            context.ResumeBookmark(pickState.ExecuteActionBookmark, null);
            pickState.ExecuteActionBookmark = null;
        }

        public Collection<PickBranch> Branches
        {
            get
            {
                if (this.branches == null)
                {
                    ValidatingCollection<PickBranch> validatings = new ValidatingCollection<PickBranch> {
                        OnAddValidationCallback = delegate (PickBranch item) {
                            if (item == null)
                            {
                                throw FxTrace.Exception.ArgumentNull("item");
                            }
                        }
                    };
                    this.branches = validatings;
                }
                return this.branches;
            }
        }

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        private class PickBranchBody : NativeActivity
        {
            protected override void CacheMetadata(NativeActivityMetadata metadata)
            {
                Collection<Activity> collection = null;
                if (this.Trigger != null)
                {
                    ActivityUtilities.Add<Activity>(ref collection, this.Trigger);
                }
                if (this.Action != null)
                {
                    ActivityUtilities.Add<Activity>(ref collection, this.Action);
                }
                metadata.SetChildrenCollection(collection);
                metadata.SetVariablesCollection(this.Variables);
            }

            protected override void Execute(NativeActivityContext context)
            {
                context.ScheduleActivity(this.Trigger, new CompletionCallback(this.OnTriggerCompleted));
            }

            private void OnExecuteAction(NativeActivityContext context, Bookmark bookmark, object state)
            {
                if (this.Action != null)
                {
                    context.ScheduleActivity(this.Action);
                }
            }

            private void OnTriggerCompleted(NativeActivityContext context, System.Activities.ActivityInstance completedInstance)
            {
                Pick.PickState state = (Pick.PickState) context.Properties.Find("System.Activities.Statements.Pick.PickState");
                if ((completedInstance.State == ActivityInstanceState.Closed) && (state.TriggerCompletionBookmark != null))
                {
                    context.ResumeBookmark(state.TriggerCompletionBookmark, context.ActivityInstanceId);
                    state.TriggerCompletionBookmark = null;
                    state.ExecuteActionBookmark = context.CreateBookmark(new BookmarkCallback(this.OnExecuteAction));
                }
                else if (!context.IsCancellationRequested)
                {
                    context.CreateBookmark();
                }
            }

            public Activity Action { get; set; }

            protected override bool CanInduceIdle
            {
                get
                {
                    return true;
                }
            }

            public Activity Trigger { get; set; }

            public Collection<Variable> Variables { get; set; }
        }

        [DataContract]
        private class PickState
        {
            [DataMember(EmitDefaultValue=false)]
            public Bookmark ExecuteActionBookmark { get; set; }

            [DataMember(EmitDefaultValue=false)]
            public bool HasBranchCompletedSuccessfully { get; set; }

            [DataMember(EmitDefaultValue=false)]
            public Bookmark TriggerCompletionBookmark { get; set; }
        }
    }
}

