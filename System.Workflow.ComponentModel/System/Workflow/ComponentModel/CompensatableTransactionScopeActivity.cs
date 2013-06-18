namespace System.Workflow.ComponentModel
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Workflow.ComponentModel.Design;

    [Designer(typeof(CompensatableTransactionScopeActivityDesigner), typeof(IDesigner)), ToolboxBitmap(typeof(CompensatableTransactionScopeActivity), "Resources.Sequence.png"), SRDescription("CompensatableTransactionalContextActivityDescription"), SupportsTransaction, ToolboxItem(typeof(ActivityToolboxItem)), PersistOnClose]
    public sealed class CompensatableTransactionScopeActivity : CompositeActivity, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>, ICompensatableActivity
    {
        internal static readonly DependencyProperty TransactionOptionsProperty = DependencyProperty.Register("TransactionOptions", typeof(WorkflowTransactionOptions), typeof(CompensatableTransactionScopeActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata, new Attribute[] { new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content) }));

        public CompensatableTransactionScopeActivity()
        {
            base.SetValueBase(TransactionOptionsProperty, new WorkflowTransactionOptions());
        }

        public CompensatableTransactionScopeActivity(string name) : base(name)
        {
            base.SetValueBase(TransactionOptionsProperty, new WorkflowTransactionOptions());
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

        ActivityExecutionStatus ICompensatableActivity.Compensate(ActivityExecutionContext executionContext)
        {
            return ActivityExecutionStatus.Closed;
        }

        [ReadOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), SRDescription("TransactionDesc"), MergableProperty(false)]
        public WorkflowTransactionOptions TransactionOptions
        {
            get
            {
                return (WorkflowTransactionOptions) base.GetValue(TransactionOptionsProperty);
            }
            set
            {
                base.SetValue(TransactionOptionsProperty, value);
            }
        }
    }
}

