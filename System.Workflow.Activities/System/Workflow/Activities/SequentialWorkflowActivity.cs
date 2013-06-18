namespace System.Workflow.Activities
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    [SRDisplayName("SequentialWorkflow"), ToolboxItem(false), Designer(typeof(SequentialWorkflowDesigner), typeof(IRootDesigner)), Designer(typeof(SequentialWorkflowDesigner), typeof(IDesigner)), ToolboxBitmap(typeof(SequentialWorkflowActivity), "Resources.SequentialWorkflow.bmp"), SRCategory("Standard")]
    public class SequentialWorkflowActivity : SequenceActivity
    {
        public static readonly DependencyProperty CompletedEvent = DependencyProperty.Register("Completed", typeof(EventHandler), typeof(SequentialWorkflowActivity));
        public static readonly DependencyProperty InitializedEvent = DependencyProperty.Register("Initialized", typeof(EventHandler), typeof(SequentialWorkflowActivity));

        [SRDescription("OnCompletedDescr"), SRCategory("Handlers"), MergableProperty(false)]
        public event EventHandler Completed
        {
            add
            {
                base.AddHandler(CompletedEvent, value);
            }
            remove
            {
                base.RemoveHandler(CompletedEvent, value);
            }
        }

        [SRDescription("OnInitializedDescr"), SRCategory("Handlers"), MergableProperty(false)]
        public event EventHandler Initialized
        {
            add
            {
                base.AddHandler(InitializedEvent, value);
            }
            remove
            {
                base.RemoveHandler(InitializedEvent, value);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SequentialWorkflowActivity()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SequentialWorkflowActivity(string name) : base(name)
        {
        }

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            base.RaiseEvent(InitializedEvent, this, EventArgs.Empty);
            return base.Execute(executionContext);
        }

        protected sealed override void OnSequenceComplete(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            base.RaiseEvent(CompletedEvent, this, EventArgs.Empty);
        }

        [DefaultValue((string) null), SRCategory("Conditions"), SRDescription("DynamicUpdateConditionDescr")]
        public ActivityCondition DynamicUpdateCondition
        {
            get
            {
                return (WorkflowChanges.GetCondition(this) as ActivityCondition);
            }
            set
            {
                WorkflowChanges.SetCondition(this, value);
            }
        }
    }
}

