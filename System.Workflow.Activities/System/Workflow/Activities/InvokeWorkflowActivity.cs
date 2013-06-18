namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;

    [ActivityValidator(typeof(InvokeWorkflowValidator)), DefaultEvent("Invoking"), SRDescription("InvokeWorkflowActivityDescription"), ToolboxItem(typeof(ActivityToolboxItem)), ToolboxBitmap(typeof(InvokeWorkflowActivity), "Resources.Service.bmp")]
    public sealed class InvokeWorkflowActivity : Activity, ITypeFilterProvider
    {
        public static readonly DependencyProperty InstanceIdProperty = DependencyProperty.Register("InstanceId", typeof(Guid), typeof(InvokeWorkflowActivity), new PropertyMetadata(Guid.Empty));
        public static readonly DependencyProperty InvokingEvent = DependencyProperty.Register("Invoking", typeof(EventHandler), typeof(InvokeWorkflowActivity));
        public static readonly DependencyProperty ParameterBindingsProperty = DependencyProperty.Register("ParameterBindings", typeof(WorkflowParameterBindingCollection), typeof(InvokeWorkflowActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata | DependencyPropertyOptions.ReadOnly, new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content) }));
        internal static readonly ArrayList ReservedParameterNames = new ArrayList(new string[] { "Name", "Enabled", "Description", "TargetWorkflow", "Invoking", "ParameterBindings" });
        public static readonly DependencyProperty TargetWorkflowProperty = DependencyProperty.Register("TargetWorkflow", typeof(Type), typeof(InvokeWorkflowActivity), new PropertyMetadata(null, DependencyPropertyOptions.Metadata));

        [SRDescription("InitializeCaleeDescr"), MergableProperty(false), SRCategory("Handlers")]
        public event EventHandler Invoking
        {
            add
            {
                base.AddHandler(InvokingEvent, value);
            }
            remove
            {
                base.RemoveHandler(InvokingEvent, value);
            }
        }

        public InvokeWorkflowActivity()
        {
            base.SetReadOnlyPropertyValue(ParameterBindingsProperty, new WorkflowParameterBindingCollection(this));
        }

        public InvokeWorkflowActivity(string name) : base(name)
        {
            base.SetReadOnlyPropertyValue(ParameterBindingsProperty, new WorkflowParameterBindingCollection(this));
        }

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            base.RaiseEvent(InvokingEvent, this, EventArgs.Empty);
            Dictionary<string, object> namedArgumentValues = new Dictionary<string, object>();
            foreach (WorkflowParameterBinding binding in this.ParameterBindings)
            {
                namedArgumentValues.Add(binding.ParameterName, binding.Value);
            }
            IStartWorkflow service = executionContext.GetService(typeof(IStartWorkflow)) as IStartWorkflow;
            if (service == null)
            {
                throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(IStartWorkflow).FullName }));
            }
            Guid guid = service.StartWorkflow(this.TargetWorkflow, namedArgumentValues);
            if (guid == Guid.Empty)
            {
                throw new InvalidOperationException(SR.GetString("Error_FailedToStartTheWorkflow"));
            }
            this.SetInstanceId(guid);
            return ActivityExecutionStatus.Closed;
        }

        internal void SetInstanceId(Guid value)
        {
            base.SetValue(InstanceIdProperty, value);
        }

        bool ITypeFilterProvider.CanFilterType(Type type, bool throwOnError)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            bool flag = (TypeProvider.IsAssignable(typeof(Activity), type) && (type != typeof(Activity))) && !type.IsAbstract;
            if (flag)
            {
                IDesignerHost service = this.Site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if ((service != null) && (string.Compare(service.RootComponentClassName, type.FullName, StringComparison.Ordinal) == 0))
                {
                    if (throwOnError)
                    {
                        throw new InvalidOperationException(SR.GetString("Error_CantInvokeSelf"));
                    }
                    flag = false;
                }
            }
            if (throwOnError && !flag)
            {
                throw new Exception(SR.GetString("Error_TypeIsNotRootActivity", new object[] { "TargetWorkflow" }));
            }
            return flag;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public Guid InstanceId
        {
            get
            {
                return (Guid) base.GetValue(InstanceIdProperty);
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public WorkflowParameterBindingCollection ParameterBindings
        {
            get
            {
                return (base.GetValue(ParameterBindingsProperty) as WorkflowParameterBindingCollection);
            }
        }

        string ITypeFilterProvider.FilterDescription
        {
            get
            {
                return SR.GetString("FilterDescription_InvokeWorkflow");
            }
        }

        [SRDescription("TargetWorkflowDescr"), Editor(typeof(TypeBrowserEditor), typeof(UITypeEditor)), SRCategory("Activity"), DefaultValue((string) null)]
        public Type TargetWorkflow
        {
            get
            {
                return (base.GetValue(TargetWorkflowProperty) as Type);
            }
            set
            {
                base.SetValue(TargetWorkflowProperty, value);
            }
        }
    }
}

