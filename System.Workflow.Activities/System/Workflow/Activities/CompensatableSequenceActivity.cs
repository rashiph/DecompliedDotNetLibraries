namespace System.Workflow.Activities
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    [ToolboxItem(typeof(ActivityToolboxItem)), SRDescription("CompensatableSequenceActivityDescription"), Designer(typeof(System.Workflow.Activities.SequenceDesigner), typeof(IDesigner)), ToolboxBitmap(typeof(CompensatableSequenceActivity), "Resources.Sequence.png"), SRCategory("Standard")]
    public sealed class CompensatableSequenceActivity : SequenceActivity, ICompensatableActivity
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CompensatableSequenceActivity()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public CompensatableSequenceActivity(string name) : base(name)
        {
        }

        ActivityExecutionStatus ICompensatableActivity.Compensate(ActivityExecutionContext executionContext)
        {
            return ActivityExecutionStatus.Closed;
        }
    }
}

