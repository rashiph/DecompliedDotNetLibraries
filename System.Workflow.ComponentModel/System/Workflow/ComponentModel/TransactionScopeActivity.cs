namespace System.Workflow.ComponentModel
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Workflow.ComponentModel.Design;

    [SupportsTransaction, SRDescription("TransactionalContextActivityDescription"), ToolboxItem(typeof(ActivityToolboxItem)), ToolboxBitmap(typeof(TransactionScopeActivity), "Resources.Sequence.png"), Designer(typeof(TransactionScopeActivityDesigner), typeof(IDesigner)), PersistOnClose]
    public sealed class TransactionScopeActivity : CompositeActivity, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
    {
        internal static readonly DependencyProperty TransactionOptionsProperty = DependencyProperty.Register("TransactionOptions", typeof(WorkflowTransactionOptions), typeof(TransactionScopeActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata, new Attribute[] { new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content) }));
        internal static readonly string TransactionScopeActivityIsolationHandle = "A1DAF1E7-E9E7-4df2-B88F-3A92E1D744F2";

        public TransactionScopeActivity()
        {
            base.SetValueBase(TransactionOptionsProperty, new WorkflowTransactionOptions());
        }

        public TransactionScopeActivity(string name) : base(name)
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

        [ReadOnly(true), MergableProperty(false), SRDescription("TransactionDesc"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
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

