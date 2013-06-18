namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    public sealed class If : NativeActivity
    {
        public If()
        {
        }

        public If(Activity<bool> condition) : this()
        {
            if (condition == null)
            {
                throw FxTrace.Exception.ArgumentNull("condition");
            }
            this.Condition = new InArgument<bool>(condition);
        }

        public If(InArgument<bool> condition) : this()
        {
            if (condition == null)
            {
                throw FxTrace.Exception.ArgumentNull("condition");
            }
            this.Condition = condition;
        }

        public If(Expression<Func<ActivityContext, bool>> condition) : this()
        {
            if (condition == null)
            {
                throw FxTrace.Exception.ArgumentNull("condition");
            }
            this.Condition = new InArgument<bool>(condition);
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            RuntimeArgument argument = new RuntimeArgument("Condition", typeof(bool), ArgumentDirection.In, true);
            metadata.Bind(this.Condition, argument);
            metadata.SetArgumentsCollection(new Collection<RuntimeArgument> { argument });
            Collection<Activity> collection = null;
            if (this.Then != null)
            {
                ActivityUtilities.Add<Activity>(ref collection, this.Then);
            }
            if (this.Else != null)
            {
                ActivityUtilities.Add<Activity>(ref collection, this.Else);
            }
            metadata.SetChildrenCollection(collection);
        }

        protected override void Execute(NativeActivityContext context)
        {
            if (this.Condition.Get(context))
            {
                if (this.Then != null)
                {
                    context.ScheduleActivity(this.Then);
                }
            }
            else if (this.Else != null)
            {
                context.ScheduleActivity(this.Else);
            }
        }

        [RequiredArgument, DefaultValue((string) null)]
        public InArgument<bool> Condition { get; set; }

        [DefaultValue((string) null), DependsOn("Then")]
        public Activity Else { get; set; }

        [DependsOn("Condition"), DefaultValue((string) null)]
        public Activity Then { get; set; }
    }
}

