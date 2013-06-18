namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Runtime;
    using System.Workflow.Activities.Common;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.Runtime;

    [SRDescription("WebServiceFaultActivityDescription"), DefaultEvent("SendingFault"), SRCategory("Standard"), ToolboxBitmap(typeof(WebServiceFaultActivity), "Resources.WebServiceOut.png"), Designer(typeof(WebServiceFaultDesigner), typeof(IDesigner)), ActivityValidator(typeof(WebServiceFaultValidator))]
    public sealed class WebServiceFaultActivity : Activity, IPropertyValueProvider
    {
        public static readonly DependencyProperty FaultProperty = DependencyProperty.Register("Fault", typeof(Exception), typeof(WebServiceFaultActivity));
        public static readonly DependencyProperty InputActivityNameProperty = DependencyProperty.Register("InputActivityName", typeof(string), typeof(WebServiceFaultActivity), new PropertyMetadata("", DependencyPropertyOptions.Metadata));
        public static readonly DependencyProperty SendingFaultEvent = DependencyProperty.Register("SendingFault", typeof(EventHandler), typeof(WebServiceFaultActivity));

        [SRDescription("OnBeforeFaultingDescr"), MergableProperty(false), SRCategory("Handlers")]
        public event EventHandler SendingFault
        {
            add
            {
                base.AddHandler(SendingFaultEvent, value);
            }
            remove
            {
                base.RemoveHandler(SendingFaultEvent, value);
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public WebServiceFaultActivity()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public WebServiceFaultActivity(string name) : base(name)
        {
        }

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            if (this.Fault == null)
            {
                throw new InvalidOperationException(SR.GetString(CultureInfo.CurrentCulture, "Error_PropertyNotSet", new object[] { FaultProperty.Name }));
            }
            WorkflowQueuingService service = executionContext.GetService<WorkflowQueuingService>();
            base.RaiseEvent(SendingFaultEvent, this, EventArgs.Empty);
            WebServiceInputActivity activityByName = base.GetActivityByName(this.InputActivityName) as WebServiceInputActivity;
            IComparable queueName = new EventQueueName(activityByName.InterfaceType, activityByName.MethodName, activityByName.QualifiedName);
            IMethodResponseMessage message = null;
            WorkflowQueue workflowQueue = service.GetWorkflowQueue(queueName);
            if (workflowQueue.Count != 0)
            {
                message = workflowQueue.Dequeue() as IMethodResponseMessage;
            }
            message.SendException(this.Fault);
            return ActivityExecutionStatus.Closed;
        }

        protected override void Initialize(IServiceProvider provider)
        {
            if (base.Parent == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_MustHaveParent"));
            }
            base.Initialize(provider);
        }

        ICollection IPropertyValueProvider.GetPropertyValues(ITypeDescriptorContext context)
        {
            StringCollection strings = new StringCollection();
            if (context.PropertyDescriptor.Name == "InputActivityName")
            {
                foreach (Activity activity in WebServiceActivityHelpers.GetPreceedingActivities(this))
                {
                    if (activity is WebServiceInputActivity)
                    {
                        strings.Add(activity.QualifiedName);
                    }
                }
            }
            return strings;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible), MergableProperty(false), DefaultValue((string) null), Browsable(true), SRCategory("Properties")]
        public Exception Fault
        {
            get
            {
                return (base.GetValue(FaultProperty) as Exception);
            }
            set
            {
                base.SetValue(FaultProperty, value);
            }
        }

        [SRDescription("ReceiveActivityNameDescription"), DefaultValue(""), SRCategory("Activity"), MergableProperty(false), TypeConverter(typeof(PropertyValueProviderTypeConverter)), RefreshProperties(RefreshProperties.All)]
        public string InputActivityName
        {
            get
            {
                return (base.GetValue(InputActivityNameProperty) as string);
            }
            set
            {
                base.SetValue(InputActivityNameProperty, value);
            }
        }
    }
}

