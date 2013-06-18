namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [ContentProperty("Cases")]
    public sealed class Switch<T> : NativeActivity
    {
        private IDictionary<T, Activity> cases;

        public Switch()
        {
        }

        public Switch(Activity<T> expression) : this()
        {
            if (expression == null)
            {
                throw FxTrace.Exception.ArgumentNull("expression");
            }
            this.Expression = new InArgument<T>(expression);
        }

        public Switch(InArgument<T> expression) : this()
        {
            if (expression == null)
            {
                throw FxTrace.Exception.ArgumentNull("expression");
            }
            this.Expression = expression;
        }

        public Switch(Expression<Func<ActivityContext, T>> expression) : this()
        {
            if (expression == null)
            {
                throw FxTrace.Exception.ArgumentNull("expression");
            }
            this.Expression = new InArgument<T>(expression);
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            RuntimeArgument argument = new RuntimeArgument("Expression", typeof(T), ArgumentDirection.In, true);
            metadata.Bind(this.Expression, argument);
            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { argument });
            Collection<Activity> children = new Collection<Activity>();
            foreach (Activity activity in this.Cases.Values)
            {
                children.Add(activity);
            }
            if (this.Default != null)
            {
                children.Add(this.Default);
            }
            metadata.SetChildrenCollection(children);
        }

        protected override void Execute(NativeActivityContext context)
        {
            T key = this.Expression.Get(context);
            Activity activity = null;
            if (!this.Cases.TryGetValue(key, out activity))
            {
                if (this.Default != null)
                {
                    activity = this.Default;
                }
                else if (TD.SwitchCaseNotFoundIsEnabled())
                {
                    TD.SwitchCaseNotFound(base.DisplayName);
                }
            }
            if (activity != null)
            {
                context.ScheduleActivity(activity);
            }
        }

        public IDictionary<T, Activity> Cases
        {
            get
            {
                if (this.cases == null)
                {
                    this.cases = new CasesDictionary<T, Activity>();
                }
                return this.cases;
            }
        }

        [DefaultValue((string) null)]
        public Activity Default { get; set; }

        [RequiredArgument, DefaultValue((string) null)]
        public InArgument<T> Expression { get; set; }
    }
}

