namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.Runtime;

    [SRDescription("DelayActivityDescription"), SRCategory("Standard"), ToolboxItem(typeof(ActivityToolboxItem)), Designer(typeof(DelayDesigner), typeof(IDesigner)), ToolboxBitmap(typeof(DelayActivity), "Resources.Delay.png"), DefaultEvent("InitializeTimeoutDuration"), ActivityValidator(typeof(DelayActivity.DelayActivityValidator))]
    public sealed class DelayActivity : Activity, IEventActivity, IActivityEventListener<QueueEventArgs>
    {
        public static readonly DependencyProperty InitializeTimeoutDurationEvent = DependencyProperty.Register("InitializeTimeoutDuration", typeof(EventHandler), typeof(DelayActivity));
        private static DependencyProperty IsInEventActivityModeProperty = DependencyProperty.Register("IsInEventActivityMode", typeof(bool), typeof(DelayActivity), new PropertyMetadata(false));
        private static readonly DependencyProperty QueueNameProperty = DependencyProperty.Register("QueueName", typeof(IComparable), typeof(DelayActivity));
        private static DependencyProperty SubscriptionIDProperty = DependencyProperty.Register("SubscriptionID", typeof(Guid), typeof(DelayActivity), new PropertyMetadata(Guid.NewGuid()));
        public static readonly DependencyProperty TimeoutDurationProperty = DependencyProperty.Register("TimeoutDuration", typeof(TimeSpan), typeof(DelayActivity), new PropertyMetadata(new TimeSpan(0, 0, 0)));

        [SRCategory("Handlers"), SRDescription("TimeoutInitializerDescription"), MergableProperty(false)]
        public event EventHandler InitializeTimeoutDuration
        {
            add
            {
                base.AddHandler(InitializeTimeoutDurationEvent, value);
            }
            remove
            {
                base.RemoveHandler(InitializeTimeoutDurationEvent, value);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public DelayActivity()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public DelayActivity(string name) : base(name)
        {
        }

        protected override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            if (!this.IsInEventActivityMode && (this.SubscriptionID != Guid.Empty))
            {
                ((IEventActivity) this).Unsubscribe(executionContext, this);
            }
            return ActivityExecutionStatus.Closed;
        }

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            if (this.IsInEventActivityMode)
            {
                return ActivityExecutionStatus.Closed;
            }
            ((IEventActivity) this).Subscribe(executionContext, this);
            this.IsInEventActivityMode = false;
            return ActivityExecutionStatus.Executing;
        }

        protected sealed override ActivityExecutionStatus HandleFault(ActivityExecutionContext executionContext, Exception exception)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }
            ActivityExecutionStatus status = this.Cancel(executionContext);
            if (status == ActivityExecutionStatus.Canceling)
            {
                return ActivityExecutionStatus.Faulting;
            }
            return status;
        }

        protected override void Initialize(IServiceProvider provider)
        {
            base.Initialize(provider);
            base.SetValue(QueueNameProperty, Guid.NewGuid());
        }

        protected override void OnClosed(IServiceProvider provider)
        {
            base.RemoveProperty(SubscriptionIDProperty);
            base.RemoveProperty(IsInEventActivityModeProperty);
        }

        void IEventActivity.Subscribe(ActivityExecutionContext parentContext, IActivityEventListener<QueueEventArgs> parentEventHandler)
        {
            if (parentContext == null)
            {
                throw new ArgumentNullException("parentContext");
            }
            if (parentEventHandler == null)
            {
                throw new ArgumentNullException("parentEventHandler");
            }
            this.IsInEventActivityMode = true;
            base.RaiseEvent(InitializeTimeoutDurationEvent, this, EventArgs.Empty);
            TimeSpan timeoutDuration = this.TimeoutDuration;
            DateTime expiresAt = DateTime.UtcNow + timeoutDuration;
            WorkflowQueuingService service = parentContext.GetService<WorkflowQueuingService>();
            IComparable queueName = ((IEventActivity) this).QueueName;
            TimerEventSubscription item = new TimerEventSubscription((Guid) queueName, base.WorkflowInstanceId, expiresAt);
            service.CreateWorkflowQueue(queueName, false).RegisterForQueueItemAvailable(parentEventHandler, base.QualifiedName);
            this.SubscriptionID = item.SubscriptionId;
            Activity parent = this;
            while (parent.Parent != null)
            {
                parent = parent.Parent;
            }
            ((TimerEventSubscriptionCollection) parent.GetValue(TimerEventSubscriptionCollection.TimerCollectionProperty)).Add(item);
        }

        void IEventActivity.Unsubscribe(ActivityExecutionContext parentContext, IActivityEventListener<QueueEventArgs> parentEventHandler)
        {
            if (parentContext == null)
            {
                throw new ArgumentNullException("parentContext");
            }
            if (parentEventHandler == null)
            {
                throw new ArgumentNullException("parentEventHandler");
            }
            WorkflowQueuingService service = parentContext.GetService<WorkflowQueuingService>();
            WorkflowQueue workflowQueue = null;
            try
            {
                workflowQueue = service.GetWorkflowQueue(this.SubscriptionID);
            }
            catch
            {
            }
            if ((workflowQueue != null) && (workflowQueue.Count != 0))
            {
                workflowQueue.Dequeue();
            }
            Activity parent = parentContext.Activity;
            while (parent.Parent != null)
            {
                parent = parent.Parent;
            }
            ((TimerEventSubscriptionCollection) parent.GetValue(TimerEventSubscriptionCollection.TimerCollectionProperty)).Remove(this.SubscriptionID);
            if (workflowQueue != null)
            {
                workflowQueue.UnregisterForQueueItemAvailable(parentEventHandler);
                service.DeleteWorkflowQueue(this.SubscriptionID);
            }
            this.SubscriptionID = Guid.Empty;
        }

        void IActivityEventListener<QueueEventArgs>.OnEvent(object sender, QueueEventArgs e)
        {
            if (sender == null)
            {
                throw new ArgumentNullException("sender");
            }
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            ActivityExecutionContext context = sender as ActivityExecutionContext;
            if (context == null)
            {
                throw new ArgumentException(SR.Error_SenderMustBeActivityExecutionContext, "sender");
            }
            if (base.ExecutionStatus != ActivityExecutionStatus.Closed)
            {
                WorkflowQueuingService service = context.GetService<WorkflowQueuingService>();
                service.GetWorkflowQueue(e.QueueName).Dequeue();
                service.DeleteWorkflowQueue(e.QueueName);
                context.CloseActivity();
            }
        }

        private bool IsInEventActivityMode
        {
            get
            {
                return (bool) base.GetValue(IsInEventActivityModeProperty);
            }
            set
            {
                base.SetValue(IsInEventActivityModeProperty, value);
            }
        }

        private Guid SubscriptionID
        {
            get
            {
                return (Guid) base.GetValue(SubscriptionIDProperty);
            }
            set
            {
                base.SetValue(SubscriptionIDProperty, value);
            }
        }

        IComparable IEventActivity.QueueName
        {
            get
            {
                return (IComparable) base.GetValue(QueueNameProperty);
            }
        }

        [TypeConverter(typeof(TimeoutDurationConverter)), MergableProperty(false), SRDescription("TimeoutDurationDescription")]
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

        private class DelayActivityValidator : ActivityValidator
        {
            public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
            {
                ValidationErrorCollection errors = new ValidationErrorCollection();
                DelayActivity activity = obj as DelayActivity;
                if (activity == null)
                {
                    throw new InvalidOperationException();
                }
                if (activity.TimeoutDuration.Ticks < 0L)
                {
                    errors.Add(new ValidationError(SR.GetString("Error_NegativeValue", new object[] { activity.TimeoutDuration.ToString(), "TimeoutDuration" }), 0x531));
                }
                errors.AddRange(base.Validate(manager, obj));
                return errors;
            }
        }

        private sealed class TimeoutDurationConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return ((destinationType == typeof(string)) || base.CanConvertTo(context, destinationType));
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                object zero = TimeSpan.Zero;
                string str = value as string;
                if (!string.IsNullOrEmpty(str))
                {
                    try
                    {
                        zero = TimeSpan.Parse(str, CultureInfo.InvariantCulture);
                        if (zero != null)
                        {
                            TimeSpan span = (TimeSpan) zero;
                            if (span.Ticks < 0L)
                            {
                                throw new Exception(string.Format(CultureInfo.CurrentCulture, SR.GetString("Error_NegativeValue"), new object[] { value.ToString(), "TimeoutDuration" }));
                            }
                        }
                    }
                    catch
                    {
                        throw new Exception(SR.GetString("InvalidTimespanFormat", new object[] { str }));
                    }
                }
                return zero;
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if ((destinationType == typeof(string)) && (value is TimeSpan))
                {
                    TimeSpan span = (TimeSpan) value;
                    return span.ToString();
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                ArrayList values = new ArrayList();
                values.Add(new TimeSpan(0, 0, 0));
                values.Add(new TimeSpan(0, 1, 0));
                values.Add(new TimeSpan(0, 30, 0));
                values.Add(new TimeSpan(1, 0, 0));
                values.Add(new TimeSpan(12, 0, 0));
                values.Add(new TimeSpan(1, 0, 0, 0));
                return new TypeConverter.StandardValuesCollection(values);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }
    }
}

