namespace System.Workflow.Activities
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;

    [SRDescription("StateFinalizationActivityDescription"), ToolboxBitmap(typeof(StateFinalizationActivity), "Resources.StateFinalizationActivity.png"), ComVisible(false), ToolboxItem(typeof(ActivityToolboxItem)), Designer(typeof(StateFinalizationDesigner), typeof(IDesigner)), ActivityValidator(typeof(StateFinalizationValidator)), SRCategory("Standard")]
    public sealed class StateFinalizationActivity : SequenceActivity
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public StateFinalizationActivity()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public StateFinalizationActivity(string name) : base(name)
        {
        }
    }
}

