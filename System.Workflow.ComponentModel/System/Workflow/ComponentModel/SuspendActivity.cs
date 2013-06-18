namespace System.Workflow.ComponentModel
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Runtime;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;

    [ToolboxBitmap(typeof(SuspendActivity), "Resources.Suspend.png"), Designer(typeof(SuspendDesigner), typeof(IDesigner)), ActivityValidator(typeof(SuspendValidator)), SRCategory("Standard"), SRDescription("SuspendActivityDescription"), ToolboxItem(typeof(ActivityToolboxItem))]
    public sealed class SuspendActivity : Activity
    {
        public static readonly DependencyProperty ErrorProperty = DependencyProperty.Register("Error", typeof(string), typeof(SuspendActivity));

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SuspendActivity()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SuspendActivity(string name) : base(name)
        {
        }

        protected internal sealed override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            executionContext.CloseActivity();
            string error = this.Error;
            executionContext.SuspendWorkflowInstance(error);
            return ActivityExecutionStatus.Closed;
        }

        protected internal override void Initialize(IServiceProvider provider)
        {
            if (base.Parent == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_MustHaveParent"));
            }
            base.Initialize(provider);
        }

        [SRDescription("SuspendErrorMessageDescr"), SRCategory("Activity"), DefaultValue((string) null), MergableProperty(false), Browsable(true)]
        public string Error
        {
            get
            {
                return (base.GetValue(ErrorProperty) as string);
            }
            set
            {
                base.SetValue(ErrorProperty, value);
            }
        }
    }
}

