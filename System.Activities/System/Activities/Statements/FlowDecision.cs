namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Activities.Expressions;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    public sealed class FlowDecision : FlowNode
    {
        public FlowDecision()
        {
        }

        public FlowDecision(Activity<bool> condition) : this()
        {
            if (condition == null)
            {
                throw FxTrace.Exception.ArgumentNull("condition");
            }
            this.Condition = condition;
        }

        public FlowDecision(Expression<Func<ActivityContext, bool>> condition) : this()
        {
            if (condition == null)
            {
                throw FxTrace.Exception.ArgumentNull("condition");
            }
            this.Condition = new LambdaValue<bool>(condition);
        }

        internal bool Execute(NativeActivityContext context, CompletionCallback<bool> onConditionCompleted)
        {
            context.ScheduleActivity<bool>(this.Condition, onConditionCompleted, null);
            return false;
        }

        internal override void GetChildActivities(ICollection<Activity> children)
        {
            if (this.Condition != null)
            {
                children.Add(this.Condition);
            }
        }

        internal override void GetConnectedNodes(IList<FlowNode> connections)
        {
            if (this.True != null)
            {
                connections.Add(this.True);
            }
            if (this.False != null)
            {
                connections.Add(this.False);
            }
        }

        internal override void OnOpen(Flowchart owner, NativeActivityMetadata metadata)
        {
            if (this.Condition == null)
            {
                metadata.AddValidationError(System.Activities.SR.FlowDecisionRequiresCondition(owner.DisplayName));
            }
        }

        [DefaultValue((string) null)]
        public Activity<bool> Condition { get; set; }

        [DefaultValue((string) null), DependsOn("True")]
        public FlowNode False { get; set; }

        [DefaultValue((string) null), DependsOn("Condition")]
        public FlowNode True { get; set; }
    }
}

