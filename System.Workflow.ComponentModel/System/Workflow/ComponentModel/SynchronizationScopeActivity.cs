namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Runtime;
    using System.Workflow.ComponentModel.Design;

    [SupportsSynchronization, Designer(typeof(SequenceDesigner), typeof(IDesigner)), SRDescription("SynchronizationScopeActivityDescription"), ToolboxItem(typeof(ActivityToolboxItem)), ToolboxBitmap(typeof(SynchronizationScopeActivity), "Resources.Sequence.png")]
    public sealed class SynchronizationScopeActivity : CompositeActivity, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SynchronizationScopeActivity()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public SynchronizationScopeActivity(string name) : base(name)
        {
        }

        protected internal override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)
        {
            return SequenceHelper.Cancel(this, executionContext);
        }

        protected internal override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            return SequenceHelper.Execute(this, executionContext);
        }

        protected internal override void OnActivityChangeRemove(ActivityExecutionContext executionContext, Activity removedActivity)
        {
            SequenceHelper.OnActivityChangeRemove(this, executionContext, removedActivity);
        }

        protected internal override void OnWorkflowChangesCompleted(ActivityExecutionContext executionContext)
        {
            SequenceHelper.OnWorkflowChangesCompleted(this, executionContext);
        }

        void IActivityEventListener<ActivityExecutionStatusChangedEventArgs>.OnEvent(object sender, ActivityExecutionStatusChangedEventArgs e)
        {
            SequenceHelper.OnEvent(this, sender, e);
        }

        [TypeConverter(typeof(SynchronizationHandlesTypeConverter)), Editor(typeof(SynchronizationHandlesEditor), typeof(UITypeEditor)), SRDescription("SynchronizationHandlesDesc"), SRDisplayName("SynchronizationHandles")]
        public ICollection<string> SynchronizationHandles
        {
            get
            {
                return (base.GetValue(Activity.SynchronizationHandlesProperty) as ICollection<string>);
            }
            set
            {
                base.SetValue(Activity.SynchronizationHandlesProperty, value);
            }
        }
    }
}

