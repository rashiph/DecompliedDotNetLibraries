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

    [ToolboxBitmap(typeof(StateInitializationActivity), "Resources.StateInitializationActivity.png"), SRCategory("Standard"), ActivityValidator(typeof(StateInitializationValidator)), ComVisible(false), ToolboxItem(typeof(ActivityToolboxItem)), SRDescription("StateInitializationActivityDescription"), Designer(typeof(StateInitializationDesigner), typeof(IDesigner))]
    public sealed class StateInitializationActivity : SequenceActivity
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public StateInitializationActivity()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public StateInitializationActivity(string name) : base(name)
        {
        }
    }
}

