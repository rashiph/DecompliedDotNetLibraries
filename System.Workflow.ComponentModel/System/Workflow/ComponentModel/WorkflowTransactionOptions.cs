namespace System.Workflow.ComponentModel
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Transactions;
    using System.Workflow.ComponentModel.Serialization;

    [DesignerSerializer(typeof(DependencyObjectCodeDomSerializer), typeof(CodeDomSerializer)), Browsable(true)]
    public sealed class WorkflowTransactionOptions : DependencyObject
    {
        public static readonly DependencyProperty IsolationLevelProperty = DependencyProperty.Register("IsolationLevel", typeof(System.Transactions.IsolationLevel), typeof(WorkflowTransactionOptions), new PropertyMetadata(System.Transactions.IsolationLevel.Serializable, DependencyPropertyOptions.Metadata));
        public static readonly DependencyProperty TimeoutDurationProperty = DependencyProperty.Register("TimeoutDuration", typeof(TimeSpan), typeof(WorkflowTransactionOptions), new PropertyMetadata(new TimeSpan(0, 0, 30), DependencyPropertyOptions.Metadata));

        [SRDescription("IsolationLevelDescr"), MergableProperty(false), SRCategory("Activity")]
        public System.Transactions.IsolationLevel IsolationLevel
        {
            get
            {
                return (System.Transactions.IsolationLevel) base.GetValue(IsolationLevelProperty);
            }
            set
            {
                base.SetValue(IsolationLevelProperty, value);
            }
        }

        [TypeConverter(typeof(TimeoutDurationConverter)), MergableProperty(false), DefaultValue(typeof(TimeSpan), "0:0:30"), SRDescription("TimeoutDescr"), SRCategory("Activity")]
        public TimeSpan TimeoutDuration
        {
            get
            {
                return (TimeSpan) base.GetValue(TimeoutDurationProperty);
            }
            set
            {
                base.SetValue(TimeoutDurationProperty, value);
            }
        }
    }
}

