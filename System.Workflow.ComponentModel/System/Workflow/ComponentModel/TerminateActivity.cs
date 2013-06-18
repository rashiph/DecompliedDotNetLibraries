namespace System.Workflow.ComponentModel
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Runtime;
    using System.Workflow.ComponentModel.Design;

    [ToolboxBitmap(typeof(TerminateActivity), "Resources.Terminate.png"), Designer(typeof(TerminateDesigner), typeof(IDesigner)), SRCategory("Standard"), SRDescription("TerminateActivityDescription"), ToolboxItem(typeof(ActivityToolboxItem))]
    public sealed class TerminateActivity : Activity
    {
        public static readonly DependencyProperty ErrorProperty = DependencyProperty.Register("Error", typeof(string), typeof(TerminateActivity));

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TerminateActivity()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TerminateActivity(string name) : base(name)
        {
        }

        protected internal sealed override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            executionContext.CloseActivity();
            string error = this.Error;
            executionContext.TerminateWorkflowInstance(new WorkflowTerminatedException(error));
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

        [MergableProperty(false), Browsable(true), SRCategory("Activity"), SRDescription("TerminateErrorMessageDescr"), DefaultValue((string) null)]
        public string Error
        {
            get
            {
                return (string) base.GetValue(ErrorProperty);
            }
            set
            {
                base.SetValue(ErrorProperty, value);
            }
        }
    }
}

