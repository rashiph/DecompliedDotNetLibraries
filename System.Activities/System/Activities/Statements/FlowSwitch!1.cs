namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Markup;

    [ContentProperty("Cases")]
    public sealed class FlowSwitch<T> : FlowNode, IFlowSwitch
    {
        internal IDictionary<T, FlowNode> cases;
        private CompletionCallback<T> onSwitchCompleted;

        public FlowSwitch()
        {
            this.cases = new CasesDictionary<T, FlowNode>();
        }

        internal override void GetChildActivities(ICollection<Activity> children)
        {
            if (this.Expression != null)
            {
                children.Add(this.Expression);
            }
        }

        internal override void GetConnectedNodes(IList<FlowNode> connections)
        {
            foreach (KeyValuePair<T, FlowNode> pair in this.Cases)
            {
                connections.Add(pair.Value);
            }
            if (this.Default != null)
            {
                connections.Add(this.Default);
            }
        }

        private CompletionCallback<T> GetSwitchCompletedCallback(Flowchart parent)
        {
            if (this.onSwitchCompleted == null)
            {
                this.onSwitchCompleted = new CompletionCallback<T>(parent.OnSwitchCompleted<T>);
            }
            return this.onSwitchCompleted;
        }

        internal override void OnOpen(Flowchart owner, NativeActivityMetadata metadata)
        {
            if (this.Expression == null)
            {
                metadata.AddValidationError(System.Activities.SR.FlowSwitchRequiresExpression(owner.DisplayName));
            }
        }

        bool IFlowSwitch.Execute(NativeActivityContext context, Flowchart parent)
        {
            context.ScheduleActivity<T>(this.Expression, this.GetSwitchCompletedCallback(parent), null);
            return false;
        }

        FlowNode IFlowSwitch.GetNextNode(object value)
        {
            FlowNode node;
            T key = (T) value;
            if (this.Cases.TryGetValue(key, out node))
            {
                if (TD.FlowchartSwitchCaseIsEnabled())
                {
                    TD.FlowchartSwitchCase(base.Owner.DisplayName, key.ToString());
                }
                return node;
            }
            if (this.Default != null)
            {
                if (TD.FlowchartSwitchDefaultIsEnabled())
                {
                    TD.FlowchartSwitchDefault(base.Owner.DisplayName);
                }
            }
            else if (TD.FlowchartSwitchCaseNotFoundIsEnabled())
            {
                TD.FlowchartSwitchCaseNotFound(base.Owner.DisplayName);
            }
            return this.Default;
        }

        public IDictionary<T, FlowNode> Cases
        {
            get
            {
                return this.cases;
            }
        }

        [DefaultValue((string) null)]
        public FlowNode Default { get; set; }

        [DefaultValue((string) null)]
        public Activity<T> Expression { get; set; }
    }
}

