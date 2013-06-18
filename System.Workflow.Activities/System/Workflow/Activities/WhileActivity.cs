namespace System.Workflow.Activities
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;

    [ToolboxBitmap(typeof(WhileActivity), "Resources.While.png"), Designer(typeof(WhileDesigner), typeof(IDesigner)), SRDescription("WhileActivityDescription"), ActivityValidator(typeof(WhileValidator)), ToolboxItem(typeof(ActivityToolboxItem))]
    public sealed class WhileActivity : CompositeActivity, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
    {
        public static readonly DependencyProperty ConditionProperty = DependencyProperty.Register("Condition", typeof(ActivityCondition), typeof(WhileActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata, new Attribute[] { new ValidationOptionAttribute(ValidationOption.Required) }));

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public WhileActivity()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public WhileActivity(string name) : base(name)
        {
        }

        protected override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            if (base.EnabledActivities.Count == 0)
            {
                return ActivityExecutionStatus.Closed;
            }
            Activity activity = base.EnabledActivities[0];
            ActivityExecutionContext context = executionContext.ExecutionContextManager.GetExecutionContext(activity);
            if (context == null)
            {
                return ActivityExecutionStatus.Closed;
            }
            if (context.Activity.ExecutionStatus == ActivityExecutionStatus.Executing)
            {
                context.CancelActivity(context.Activity);
            }
            return ActivityExecutionStatus.Canceling;
        }

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            if (this.TryNextIteration(executionContext))
            {
                return ActivityExecutionStatus.Executing;
            }
            return ActivityExecutionStatus.Closed;
        }

        void IActivityEventListener<ActivityExecutionStatusChangedEventArgs>.OnEvent(object sender, ActivityExecutionStatusChangedEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            if (sender == null)
            {
                throw new ArgumentNullException("sender");
            }
            ActivityExecutionContext context = sender as ActivityExecutionContext;
            if (context == null)
            {
                throw new ArgumentException(SR.Error_SenderMustBeActivityExecutionContext, "sender");
            }
            e.Activity.UnregisterForStatusChange(Activity.ClosedEvent, this);
            ActivityExecutionContextManager executionContextManager = context.ExecutionContextManager;
            executionContextManager.CompleteExecutionContext(executionContextManager.GetExecutionContext(e.Activity));
            if (!this.TryNextIteration(context))
            {
                context.CloseActivity();
            }
        }

        private bool TryNextIteration(ActivityExecutionContext context)
        {
            if (((base.ExecutionStatus == ActivityExecutionStatus.Canceling) || (base.ExecutionStatus == ActivityExecutionStatus.Faulting)) || !this.Condition.Evaluate(this, context))
            {
                return false;
            }
            if (base.EnabledActivities.Count > 0)
            {
                ActivityExecutionContext context2 = context.ExecutionContextManager.CreateExecutionContext(base.EnabledActivities[0]);
                context2.Activity.RegisterForStatusChange(Activity.ClosedEvent, this);
                context2.ExecuteActivity(context2.Activity);
            }
            return true;
        }

        [SRCategory("Conditions"), SRDescription("WhileConditionDescr")]
        public ActivityCondition Condition
        {
            get
            {
                return (base.GetValue(ConditionProperty) as ActivityCondition);
            }
            set
            {
                base.SetValue(ConditionProperty, value);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Activity DynamicActivity
        {
            get
            {
                if (base.EnabledActivities.Count > 0)
                {
                    Activity[] dynamicActivities = base.GetDynamicActivities(base.EnabledActivities[0]);
                    if (dynamicActivities.Length != 0)
                    {
                        return dynamicActivities[0];
                    }
                }
                return null;
            }
        }
    }
}

