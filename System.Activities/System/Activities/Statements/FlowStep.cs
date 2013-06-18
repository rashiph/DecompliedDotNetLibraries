namespace System.Activities.Statements
{
    using System;
    using System.Activities;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Windows.Markup;

    [ContentProperty("Action")]
    public sealed class FlowStep : FlowNode
    {
        internal bool Execute(NativeActivityContext context, CompletionCallback onCompleted, out FlowNode nextNode)
        {
            if ((this.Next == null) && TD.FlowchartNextNullIsEnabled())
            {
                TD.FlowchartNextNull(base.Owner.DisplayName);
            }
            if (this.Action == null)
            {
                nextNode = this.Next;
                return true;
            }
            context.ScheduleActivity(this.Action, onCompleted);
            nextNode = null;
            return false;
        }

        internal override void GetChildActivities(ICollection<Activity> children)
        {
            if (this.Action != null)
            {
                children.Add(this.Action);
            }
        }

        internal override void GetConnectedNodes(IList<FlowNode> connections)
        {
            if (this.Next != null)
            {
                connections.Add(this.Next);
            }
        }

        internal override void OnOpen(Flowchart owner, NativeActivityMetadata metadata)
        {
        }

        [DefaultValue((string) null)]
        public Activity Action { get; set; }

        [DependsOn("Action"), DefaultValue((string) null)]
        public FlowNode Next { get; set; }
    }
}

