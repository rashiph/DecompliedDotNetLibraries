namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.Runtime.Collections;
    using System.Windows.Markup;

    [ContentProperty("Activities")]
    public sealed class Sequence : NativeActivity
    {
        private Collection<Activity> activities;
        private Variable<int> lastIndexHint = new Variable<int>();
        private CompletionCallback onChildComplete;
        private Collection<Variable> variables;

        public Sequence()
        {
            this.onChildComplete = new CompletionCallback(this.InternalExecute);
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            metadata.SetChildrenCollection(this.Activities);
            metadata.SetVariablesCollection(this.Variables);
            metadata.AddImplementationVariable(this.lastIndexHint);
        }

        protected override void Execute(NativeActivityContext context)
        {
            if ((this.activities != null) && (this.Activities.Count > 0))
            {
                Activity activity = this.Activities[0];
                context.ScheduleActivity(activity, this.onChildComplete);
            }
        }

        private void InternalExecute(NativeActivityContext context, System.Activities.ActivityInstance completedInstance)
        {
            int index = this.lastIndexHint.Get(context);
            if ((index >= this.Activities.Count) || (this.Activities[index] != completedInstance.Activity))
            {
                index = this.Activities.IndexOf(completedInstance.Activity);
            }
            int num2 = index + 1;
            if (num2 != this.Activities.Count)
            {
                Activity activity = this.Activities[num2];
                context.ScheduleActivity(activity, this.onChildComplete);
                this.lastIndexHint.Set(context, num2);
            }
        }

        [DependsOn("Variables")]
        public Collection<Activity> Activities
        {
            get
            {
                if (this.activities == null)
                {
                    ValidatingCollection<Activity> validatings = new ValidatingCollection<Activity> {
                        OnAddValidationCallback = delegate (Activity item) {
                            if (item == null)
                            {
                                throw FxTrace.Exception.ArgumentNull("item");
                            }
                        }
                    };
                    this.activities = validatings;
                }
                return this.activities;
            }
        }

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

