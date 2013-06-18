namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.Collections;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [ContentProperty("Branches")]
    public sealed class Parallel : NativeActivity
    {
        private Collection<Activity> branches;
        private Variable<bool> hasCompleted;
        private CompletionCallback<bool> onConditionComplete;
        private Collection<Variable> variables;

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            Collection<Activity> children = new Collection<Activity>();
            foreach (Activity activity in this.Branches)
            {
                children.Add(activity);
            }
            if (this.CompletionCondition != null)
            {
                children.Add(this.CompletionCondition);
            }
            metadata.SetChildrenCollection(children);
            metadata.SetVariablesCollection(this.Variables);
            if (this.CompletionCondition != null)
            {
                if (this.hasCompleted == null)
                {
                    this.hasCompleted = new Variable<bool>();
                }
                metadata.AddImplementationVariable(this.hasCompleted);
            }
        }

        protected override void Cancel(NativeActivityContext context)
        {
            if (this.CompletionCondition == null)
            {
                base.Cancel(context);
            }
            else
            {
                context.CancelChildren();
            }
        }

        protected override void Execute(NativeActivityContext context)
        {
            if ((this.branches != null) && (this.Branches.Count != 0))
            {
                CompletionCallback onCompleted = new CompletionCallback(this.OnBranchComplete);
                for (int i = this.Branches.Count - 1; i >= 0; i--)
                {
                    context.ScheduleActivity(this.Branches[i], onCompleted);
                }
            }
        }

        private void OnBranchComplete(NativeActivityContext context, System.Activities.ActivityInstance completedInstance)
        {
            if ((this.CompletionCondition != null) && !this.hasCompleted.Get(context))
            {
                if ((completedInstance.State != ActivityInstanceState.Closed) && context.IsCancellationRequested)
                {
                    context.MarkCanceled();
                    this.hasCompleted.Set(context, true);
                }
                else
                {
                    if (this.onConditionComplete == null)
                    {
                        this.onConditionComplete = new CompletionCallback<bool>(this.OnConditionComplete);
                    }
                    context.ScheduleActivity<bool>(this.CompletionCondition, this.onConditionComplete, null);
                }
            }
        }

        private void OnConditionComplete(NativeActivityContext context, System.Activities.ActivityInstance completedInstance, bool result)
        {
            if (result)
            {
                context.CancelChildren();
                this.hasCompleted.Set(context, true);
            }
        }

        [DependsOn("CompletionCondition")]
        public Collection<Activity> Branches
        {
            get
            {
                if (this.branches == null)
                {
                    ValidatingCollection<Activity> validatings = new ValidatingCollection<Activity> {
                        OnAddValidationCallback = delegate (Activity item) {
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

        [DefaultValue((string) null), DependsOn("Variables")]
        public Activity<bool> CompletionCondition { get; set; }

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

