namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Reflection;
    using System.Workflow.Activities.Common;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.Runtime;

    [SRDescription("HandleExternalEventActivityDescription"), SRCategory("Base"), DefaultEvent("Invoked"), Designer(typeof(HandleExternalEventActivityDesigner), typeof(IDesigner)), ActivityValidator(typeof(HandleExternalEventActivityValidator))]
    public class HandleExternalEventActivity : Activity, IEventActivity, System.Workflow.Activities.Common.IPropertyValueProvider, IActivityEventListener<QueueEventArgs>, IDynamicPropertyTypeProvider
    {
        private static DependencyProperty ActivitySubscribedProperty = DependencyProperty.Register("ActivitySubscribed", typeof(bool), typeof(HandleExternalEventActivity), new PropertyMetadata(false));
        public static readonly DependencyProperty CorrelationTokenProperty = DependencyProperty.Register("CorrelationToken", typeof(System.Workflow.Runtime.CorrelationToken), typeof(HandleExternalEventActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata));
        public static readonly DependencyProperty EventNameProperty = DependencyProperty.Register("EventName", typeof(string), typeof(HandleExternalEventActivity), new PropertyMetadata("", DependencyPropertyOptions.Metadata, new Attribute[] { new ValidationOptionAttribute(ValidationOption.Required) }));
        public static readonly DependencyProperty InterfaceTypeProperty = DependencyProperty.Register("InterfaceType", typeof(Type), typeof(HandleExternalEventActivity), new PropertyMetadata(null, DependencyPropertyOptions.Metadata, new Attribute[] { new ValidationOptionAttribute(ValidationOption.Required) }));
        public static readonly DependencyProperty InvokedEvent = DependencyProperty.Register("Invoked", typeof(EventHandler<ExternalDataEventArgs>), typeof(HandleExternalEventActivity));
        public static readonly DependencyProperty ParameterBindingsProperty = DependencyProperty.Register("ParameterBindings", typeof(WorkflowParameterBindingCollection), typeof(HandleExternalEventActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata | DependencyPropertyOptions.ReadOnly, new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content) }));
        private static DependencyProperty QueueNameProperty = DependencyProperty.Register("QueueName", typeof(IComparable), typeof(HandleExternalEventActivity));
        internal static readonly ArrayList ReservedParameterNames = new ArrayList(new string[] { "Name", "Enabled", "Description", "EventName", "InterfaceType", "Invoked", "Roles" });
        public static readonly DependencyProperty RolesProperty = DependencyProperty.Register("Roles", typeof(WorkflowRoleCollection), typeof(HandleExternalEventActivity));

        [SRCategory("Handlers"), SRDescription("OnAfterMethodInvokeDescr"), MergableProperty(false)]
        public event EventHandler<ExternalDataEventArgs> Invoked
        {
            add
            {
                base.AddHandler(InvokedEvent, value);
            }
            remove
            {
                base.RemoveHandler(InvokedEvent, value);
            }
        }

        public HandleExternalEventActivity()
        {
            base.SetReadOnlyPropertyValue(ParameterBindingsProperty, new WorkflowParameterBindingCollection(this));
        }

        public HandleExternalEventActivity(string name) : base(name)
        {
            base.SetReadOnlyPropertyValue(ParameterBindingsProperty, new WorkflowParameterBindingCollection(this));
        }

        protected sealed override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            this.UnsubscribeForActivity(executionContext);
            return ActivityExecutionStatus.Closed;
        }

        protected sealed override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            object[] args = null;
            ActivityExecutionStatus status = InboundActivityHelper.ExecuteForActivity(this, executionContext, this.InterfaceType, this.EventName, out args);
            if (status == ActivityExecutionStatus.Closed)
            {
                this.RaiseEvent(args);
                this.UnsubscribeForActivity(executionContext);
                executionContext.CloseActivity();
                return status;
            }
            if (!this.ActivitySubscribed)
            {
                this.ActivitySubscribed = CorrelationService.Subscribe(executionContext, this, this.InterfaceType, this.EventName, this, base.WorkflowInstanceId);
            }
            return ActivityExecutionStatus.Executing;
        }

        internal void GetParameterPropertyDescriptors(IDictionary properties)
        {
            if (this.Site != null)
            {
                if (((ITypeProvider) this.Site.GetService(typeof(ITypeProvider))) == null)
                {
                    throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(ITypeProvider).FullName }));
                }
                Type interfaceType = this.InterfaceType;
                if ((interfaceType != null) && (base.GetType() == typeof(HandleExternalEventActivity)))
                {
                    EventInfo eventInfo = interfaceType.GetEvent(this.EventName);
                    if (eventInfo != null)
                    {
                        Type eventHandlerType = TypeProvider.GetEventHandlerType(eventInfo);
                        if (eventHandlerType != null)
                        {
                            MethodInfo method = eventHandlerType.GetMethod("Invoke");
                            ArrayList list = new ArrayList();
                            if (method != null)
                            {
                                list.AddRange(method.GetParameters());
                                if (!(method.ReturnType == typeof(void)))
                                {
                                    list.Add(method.ReturnParameter);
                                }
                            }
                            foreach (ParameterInfo info3 in list)
                            {
                                PropertyDescriptor descriptor = new System.Workflow.Activities.Common.ParameterInfoBasedPropertyDescriptor(typeof(HandleExternalEventActivity), info3, true, new Attribute[] { DesignOnlyAttribute.Yes });
                                properties[descriptor.Name] = descriptor;
                            }
                        }
                    }
                }
            }
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

        protected sealed override void Initialize(IServiceProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            if ((!base.IsDynamicActivity && !this.IsNestedUnderMultiInstanceContainer) || this.IsInitializingUnderMultiInstanceContainer)
            {
                Type interfaceType = this.InterfaceType;
                string eventName = this.EventName;
                IComparable comparable = null;
                if (CorrelationResolver.IsInitializingMember(interfaceType, eventName, null))
                {
                    comparable = new EventQueueName(interfaceType, eventName);
                }
                base.SetValue(QueueNameProperty, comparable);
                CorrelationService.Initialize(provider, this, interfaceType, eventName, base.WorkflowInstanceId);
            }
        }

        protected override void InitializeProperties()
        {
            ActivityHelpers.InitializeCorrelationTokenCollection(this, this.CorrelationToken);
            Type interfaceType = this.InterfaceType;
            if (interfaceType == null)
            {
                throw new InvalidOperationException(SR.GetString("InterfaceTypeMissing", new object[] { base.Name }));
            }
            string eventName = this.EventName;
            if (eventName == null)
            {
                throw new InvalidOperationException(SR.GetString("EventNameMissing", new object[] { base.Name }));
            }
            EventInfo info = interfaceType.GetEvent(eventName);
            if (info == null)
            {
                throw new InvalidOperationException(SR.GetString("MethodInfoMissing", new object[] { this.EventName, this.InterfaceType.Name }));
            }
            InvokeHelper.InitializeParameters(info.EventHandlerType.GetMethod("Invoke"), this.ParameterBindings);
            base.InitializeProperties();
        }

        protected override void OnClosed(IServiceProvider provider)
        {
            base.RemoveProperty(ActivitySubscribedProperty);
        }

        protected virtual void OnInvoked(EventArgs e)
        {
        }

        private void RaiseEvent(object[] args)
        {
            if (args != null)
            {
                ExternalDataEventArgs e = args[1] as ExternalDataEventArgs;
                this.OnInvoked(e);
                base.RaiseGenericEvent<ExternalDataEventArgs>(InvokedEvent, args[0], e);
            }
            else
            {
                this.OnInvoked(EventArgs.Empty);
                base.RaiseGenericEvent<EventArgs>(InvokedEvent, this, EventArgs.Empty);
            }
        }

        ICollection System.Workflow.Activities.Common.IPropertyValueProvider.GetPropertyValues(ITypeDescriptorContext context)
        {
            StringCollection strings = new StringCollection();
            if (this.InterfaceType != null)
            {
                if (!(context.PropertyDescriptor.Name == "EventName"))
                {
                    return strings;
                }
                foreach (EventInfo info in this.InterfaceType.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    strings.Add(info.Name);
                }
            }
            return strings;
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
            CorrelationService.Subscribe(parentContext, this, this.InterfaceType, this.EventName, parentEventHandler, base.WorkflowInstanceId);
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
            CorrelationService.Unsubscribe(parentContext, this, this.InterfaceType, this.EventName, parentEventHandler);
        }

        void IActivityEventListener<QueueEventArgs>.OnEvent(object sender, QueueEventArgs e)
        {
            ActivityExecutionContext context = (ActivityExecutionContext) sender;
            HandleExternalEventActivity activity = context.Activity as HandleExternalEventActivity;
            if (activity.ExecutionStatus == ActivityExecutionStatus.Executing)
            {
                object[] args = null;
                if (InboundActivityHelper.ExecuteForActivity(this, context, this.InterfaceType, this.EventName, out args) == ActivityExecutionStatus.Closed)
                {
                    this.RaiseEvent(args);
                    this.UnsubscribeForActivity(context);
                    context.CloseActivity();
                }
            }
        }

        AccessTypes IDynamicPropertyTypeProvider.GetAccessType(IServiceProvider serviceProvider, string propertyName)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }
            return AccessTypes.Read;
        }

        Type IDynamicPropertyTypeProvider.GetPropertyType(IServiceProvider serviceProvider, string propertyName)
        {
            if (propertyName == null)
            {
                throw new ArgumentNullException("propertyName");
            }
            Dictionary<string, object> properties = new Dictionary<string, object>();
            this.GetParameterPropertyDescriptors(properties);
            if (properties.ContainsKey(propertyName))
            {
                System.Workflow.Activities.Common.ParameterInfoBasedPropertyDescriptor descriptor = properties[propertyName] as System.Workflow.Activities.Common.ParameterInfoBasedPropertyDescriptor;
                if (descriptor != null)
                {
                    return descriptor.ParameterType;
                }
            }
            return null;
        }

        private void UnsubscribeForActivity(ActivityExecutionContext context)
        {
            if (this.ActivitySubscribed)
            {
                CorrelationService.Unsubscribe(context, this, this.InterfaceType, this.EventName, this);
                this.ActivitySubscribed = false;
            }
        }

        private bool ActivitySubscribed
        {
            get
            {
                return (bool) base.GetValue(ActivitySubscribedProperty);
            }
            set
            {
                base.SetValue(ActivitySubscribedProperty, value);
            }
        }

        [DefaultValue((string) null), SRCategory("Activity"), RefreshProperties(RefreshProperties.All), SRDescription("CorrelationSetDescr"), MergableProperty(false), TypeConverter(typeof(CorrelationTokenTypeConverter))]
        public virtual System.Workflow.Runtime.CorrelationToken CorrelationToken
        {
            get
            {
                return (base.GetValue(CorrelationTokenProperty) as System.Workflow.Runtime.CorrelationToken);
            }
            set
            {
                base.SetValue(CorrelationTokenProperty, value);
            }
        }

        [DefaultValue(""), MergableProperty(false), RefreshProperties(RefreshProperties.All), TypeConverter(typeof(System.Workflow.Activities.Common.PropertyValueProviderTypeConverter)), SRCategory("Activity"), SRDescription("ExternalEventNameDescr")]
        public virtual string EventName
        {
            get
            {
                return (base.GetValue(EventNameProperty) as string);
            }
            set
            {
                base.SetValue(EventNameProperty, value);
            }
        }

        [DefaultValue((string) null), SRCategory("Activity"), SRDescription("HelperExternalDataExchangeDesc"), RefreshProperties(RefreshProperties.All), Editor(typeof(TypeBrowserEditor), typeof(UITypeEditor)), TypeFilterProvider(typeof(ExternalDataExchangeInterfaceTypeFilterProvider))]
        public virtual Type InterfaceType
        {
            get
            {
                return (base.GetValue(InterfaceTypeProperty) as Type);
            }
            set
            {
                base.SetValue(InterfaceTypeProperty, value);
            }
        }

        private bool IsInitializingUnderMultiInstanceContainer
        {
            get
            {
                CompositeActivity parent = base.Parent;
                Activity activity2 = this;
                while (parent != null)
                {
                    if (parent is ReplicatorActivity)
                    {
                        break;
                    }
                    if (!parent.GetActivityByName(activity2.QualifiedName).Equals(activity2))
                    {
                        return false;
                    }
                    activity2 = parent;
                    parent = parent.Parent;
                }
                return ((parent != null) && !parent.GetActivityByName(activity2.QualifiedName).Equals(activity2));
            }
        }

        private bool IsNestedUnderMultiInstanceContainer
        {
            get
            {
                for (CompositeActivity activity = base.Parent; activity != null; activity = activity.Parent)
                {
                    if (activity is ReplicatorActivity)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Browsable(false)]
        public WorkflowParameterBindingCollection ParameterBindings
        {
            get
            {
                return (base.GetValue(ParameterBindingsProperty) as WorkflowParameterBindingCollection);
            }
        }

        [SRCategory("Activity"), DefaultValue((string) null), SRDescription("RoleDescr"), Editor(typeof(BindUITypeEditor), typeof(UITypeEditor))]
        public WorkflowRoleCollection Roles
        {
            get
            {
                return (base.GetValue(RolesProperty) as WorkflowRoleCollection);
            }
            set
            {
                base.SetValue(RolesProperty, value);
            }
        }

        IComparable IEventActivity.QueueName
        {
            get
            {
                IComparable comparable = (IComparable) base.GetValue(QueueNameProperty);
                if (comparable != null)
                {
                    return comparable;
                }
                return CorrelationService.ResolveQueueName(this, this.InterfaceType, this.EventName);
            }
        }
    }
}

