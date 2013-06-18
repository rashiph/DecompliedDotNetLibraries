namespace System.Workflow.Activities
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;

    [ToolboxItem(typeof(ActivityToolboxItem)), ActivityValidator(typeof(SetStateValidator)), SRCategory("Standard"), Designer(typeof(SetStateDesigner), typeof(IDesigner)), ComVisible(false), SRDescription("SetStateActivityDescription"), ToolboxBitmap(typeof(SetStateActivity), "Resources.SetStateActivity.png")]
    public sealed class SetStateActivity : Activity
    {
        public static readonly DependencyProperty TargetStateNameProperty = DependencyProperty.Register("TargetStateName", typeof(string), typeof(SetStateActivity), new PropertyMetadata("", DependencyPropertyOptions.Metadata, new Attribute[] { new ValidationOptionAttribute(ValidationOption.Optional) }));
        internal const string TargetStateNamePropertyName = "TargetStateName";

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SetStateActivity()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SetStateActivity(string name) : base(name)
        {
        }

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            StateMachineExecutionState.Get(StateMachineHelpers.GetRootState(StateMachineHelpers.FindEnclosingState(executionContext.Activity))).NextStateName = this.TargetStateName;
            return ActivityExecutionStatus.Closed;
        }

        [SRDescription("TargetStateDescription"), Editor(typeof(StateDropDownEditor), typeof(UITypeEditor)), DefaultValue((string) null)]
        public string TargetStateName
        {
            get
            {
                return (base.GetValue(TargetStateNameProperty) as string);
            }
            set
            {
                base.SetValue(TargetStateNameProperty, value);
            }
        }
    }
}

