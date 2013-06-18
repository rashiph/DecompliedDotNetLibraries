namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Activities.Expressions;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.Collections;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [ContentProperty("Body")]
    public sealed class DoWhile : NativeActivity
    {
        private CompletionCallback onBodyComplete;
        private CompletionCallback<bool> onConditionComplete;
        private Collection<Variable> variables;

        public DoWhile()
        {
        }

        public DoWhile(Activity<bool> condition) : this()
        {
            if (condition == null)
            {
                throw FxTrace.Exception.ArgumentNull("condition");
            }
            this.Condition = condition;
        }

        public DoWhile(Expression<Func<ActivityContext, bool>> condition) : this()
        {
            if (condition == null)
            {
                throw FxTrace.Exception.ArgumentNull("condition");
            }
            this.Condition = new LambdaValue<bool>(condition);
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            metadata.SetVariablesCollection(this.Variables);
            if (this.Condition == null)
            {
                metadata.AddValidationError(System.Activities.SR.DoWhileRequiresCondition(base.DisplayName));
            }
            else
            {
                metadata.AddChild(this.Condition);
            }
            metadata.AddChild(this.Body);
        }

        protected override void Execute(NativeActivityContext context)
        {
            this.OnConditionComplete(context, null, true);
        }

        private void OnBodyComplete(NativeActivityContext context, System.Activities.ActivityInstance completedInstance)
        {
            this.ScheduleCondition(context);
        }

        private void OnConditionComplete(NativeActivityContext context, System.Activities.ActivityInstance completedInstance, bool result)
        {
            if (result)
            {
                if (this.Body != null)
                {
                    if (this.onBodyComplete == null)
                    {
                        this.onBodyComplete = new CompletionCallback(this.OnBodyComplete);
                    }
                    context.ScheduleActivity(this.Body, this.onBodyComplete);
                }
                else
                {
                    this.ScheduleCondition(context);
                }
            }
        }

        private void ScheduleCondition(NativeActivityContext context)
        {
            if (this.onConditionComplete == null)
            {
                this.onConditionComplete = new CompletionCallback<bool>(this.OnConditionComplete);
            }
            context.ScheduleActivity<bool>(this.Condition, this.onConditionComplete, null);
        }

        [DependsOn("Condition"), DefaultValue((string) null)]
        public Activity Body { get; set; }

        [DefaultValue((string) null), DependsOn("Variables")]
        public Activity<bool> Condition { get; set; }

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

