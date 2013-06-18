namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Reflection;
    using System.Runtime.Remoting.Messaging;
    using System.Workflow.Activities.Common;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.Runtime;

    [Designer(typeof(WebServiceReceiveDesigner), typeof(IDesigner)), DefaultEvent("InputReceived"), ActivityValidator(typeof(WebServiceReceiveValidator)), ActivityCodeGenerator(typeof(WebServiceCodeGenerator)), SRDescription("WebServiceReceiveActivityDescription"), SRCategory("Standard"), ToolboxBitmap(typeof(WebServiceInputActivity), "Resources.WebServiceIn.png")]
    public sealed class WebServiceInputActivity : Activity, IEventActivity, System.Workflow.Activities.Common.IPropertyValueProvider, IActivityEventListener<QueueEventArgs>, IDynamicPropertyTypeProvider
    {
        public static readonly DependencyProperty ActivitySubscribedProperty = DependencyProperty.Register("ActivitySubscribed", typeof(bool), typeof(WebServiceInputActivity), new PropertyMetadata(false));
        public static readonly DependencyProperty InputReceivedEvent = DependencyProperty.Register("InputReceived", typeof(EventHandler), typeof(WebServiceInputActivity));
        public static readonly DependencyProperty InterfaceTypeProperty = DependencyProperty.Register("InterfaceType", typeof(Type), typeof(WebServiceInputActivity), new PropertyMetadata(null, DependencyPropertyOptions.Metadata));
        public static readonly DependencyProperty IsActivatingProperty = DependencyProperty.Register("IsActivating", typeof(bool), typeof(WebServiceInputActivity), new PropertyMetadata(false, DependencyPropertyOptions.Metadata));
        public static readonly DependencyProperty MethodNameProperty = DependencyProperty.Register("MethodName", typeof(string), typeof(WebServiceInputActivity), new PropertyMetadata("", DependencyPropertyOptions.Metadata));
        public static readonly DependencyProperty ParameterBindingsProperty = DependencyProperty.Register("ParameterBindings", typeof(WorkflowParameterBindingCollection), typeof(WebServiceInputActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata | DependencyPropertyOptions.ReadOnly, new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content) }));
        private static readonly DependencyProperty QueueNameProperty = DependencyProperty.Register("QueueName", typeof(IComparable), typeof(WebServiceInputActivity));
        public static readonly DependencyProperty RolesProperty = DependencyProperty.Register("Roles", typeof(WorkflowRoleCollection), typeof(WebServiceInputActivity));

        [SRDescription("OnAfterReceiveDescr"), MergableProperty(false), SRCategory("Handlers")]
        public event EventHandler InputReceived
        {
            add
            {
                base.AddHandler(InputReceivedEvent, value);
            }
            remove
            {
                base.RemoveHandler(InputReceivedEvent, value);
            }
        }

        public WebServiceInputActivity()
        {
            base.SetReadOnlyPropertyValue(ParameterBindingsProperty, new WorkflowParameterBindingCollection(this));
        }

        public WebServiceInputActivity(string name) : base(name)
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
            ActivityExecutionStatus status = this.ExecuteForActivity(executionContext, this.InterfaceType, this.MethodName);
            if (status == ActivityExecutionStatus.Closed)
            {
                this.UnsubscribeForActivity(executionContext);
                executionContext.CloseActivity();
                return status;
            }
            if (!this.ActivitySubscribed)
            {
                this.ActivitySubscribed = CorrelationService.Subscribe(executionContext, this, this.InterfaceType, this.MethodName, this, base.WorkflowInstanceId);
            }
            return ActivityExecutionStatus.Executing;
        }

        private ActivityExecutionStatus ExecuteForActivity(ActivityExecutionContext context, Type interfaceType, string operation)
        {
            WorkflowQueuingService queueSvcs = (WorkflowQueuingService) context.GetService(typeof(WorkflowQueuingService));
            IComparable queueId = new EventQueueName(interfaceType, operation);
            if (queueId != null)
            {
                WorkflowQueue queue;
                object msg = InboundActivityHelper.DequeueMessage(queueId, queueSvcs, this, out queue);
                if (msg != null)
                {
                    this.ProcessMessage(context, msg, interfaceType, operation);
                    return ActivityExecutionStatus.Closed;
                }
            }
            return ActivityExecutionStatus.Executing;
        }

        internal void GetParameterPropertyDescriptors(IDictionary properties)
        {
            if (this.Site != null)
            {
                ITypeProvider service = (ITypeProvider) this.Site.GetService(typeof(ITypeProvider));
                if (service == null)
                {
                    throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(ITypeProvider).FullName }));
                }
                Type interfaceType = null;
                if (this.InterfaceType != null)
                {
                    interfaceType = service.GetType(this.InterfaceType.AssemblyQualifiedName);
                }
                if ((interfaceType != null) && interfaceType.IsInterface)
                {
                    MethodInfo interfaceMethod = System.Workflow.Activities.Common.Helpers.GetInterfaceMethod(interfaceType, this.MethodName);
                    if ((interfaceMethod != null) && (WebServiceActivityHelpers.ValidateParameterTypes(interfaceMethod).Count == 0))
                    {
                        List<ParameterInfo> list;
                        List<ParameterInfo> list2;
                        WebServiceActivityHelpers.GetParameterInfo(interfaceMethod, out list, out list2);
                        foreach (ParameterInfo info2 in list)
                        {
                            PropertyDescriptor descriptor = new System.Workflow.Activities.Common.ParameterInfoBasedPropertyDescriptor(typeof(WebServiceInputActivity), info2, true, new Attribute[] { DesignOnlyAttribute.Yes });
                            properties[descriptor.Name] = descriptor;
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
            if (base.Parent == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_MustHaveParent"));
            }
            WorkflowQueuingService service = (WorkflowQueuingService) provider.GetService(typeof(WorkflowQueuingService));
            EventQueueName name = new EventQueueName(this.InterfaceType, this.MethodName);
            base.SetValue(QueueNameProperty, name);
            if (!service.Exists(name))
            {
                service.CreateWorkflowQueue(name, true);
            }
        }

        private void ProcessMessage(ActivityExecutionContext context, object msg, Type interfaceType, string operation)
        {
            IMethodMessage message = msg as IMethodMessage;
            if (message == null)
            {
                Exception exception = msg as Exception;
                if (exception != null)
                {
                    throw exception;
                }
                throw new ArgumentNullException("msg");
            }
            CorrelationService.InvalidateCorrelationToken(this, interfaceType, operation, message.Args);
            IdentityContextData data = (IdentityContextData) message.LogicalCallContext.GetData("__identitycontext__");
            InboundActivityHelper.ValidateRoles(this, data.Identity);
            this.ProcessParameters(context, message, interfaceType, operation);
            base.RaiseEvent(InputReceivedEvent, this, EventArgs.Empty);
        }

        private void ProcessParameters(ActivityExecutionContext context, IMethodMessage message, Type interfaceType, string operation)
        {
            WorkflowParameterBindingCollection parameterBindings = this.ParameterBindings;
            if (parameterBindings != null)
            {
                MethodInfo method = interfaceType.GetMethod(operation);
                if (method != null)
                {
                    int num = 0;
                    bool flag = false;
                    foreach (ParameterInfo info2 in method.GetParameters())
                    {
                        if (!info2.ParameterType.IsByRef && (!info2.IsIn || !info2.IsOut))
                        {
                            if (parameterBindings.Contains(info2.Name))
                            {
                                WorkflowParameterBinding binding = parameterBindings[info2.Name];
                                binding.Value = message.Args[num++];
                            }
                        }
                        else
                        {
                            flag = true;
                        }
                    }
                    if ((method.ReturnType != typeof(void)) || flag)
                    {
                        IComparable queueName = new EventQueueName(interfaceType, operation, base.QualifiedName);
                        WorkflowQueuingService service = (WorkflowQueuingService) context.GetService(typeof(WorkflowQueuingService));
                        if (!service.Exists(queueName))
                        {
                            service.CreateWorkflowQueue(queueName, true);
                        }
                        service.GetWorkflowQueue(queueName).Enqueue(message);
                    }
                }
            }
        }

        ICollection System.Workflow.Activities.Common.IPropertyValueProvider.GetPropertyValues(ITypeDescriptorContext context)
        {
            ITypeProvider service = (ITypeProvider) context.GetService(typeof(ITypeProvider));
            if (service == null)
            {
                throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(ITypeProvider).FullName }));
            }
            StringCollection strings = new StringCollection();
            if ((context.PropertyDescriptor.Name == "MethodName") && (this.InterfaceType != null))
            {
                Type type = service.GetType(this.InterfaceType.AssemblyQualifiedName);
                if (type == null)
                {
                    return strings;
                }
                foreach (MethodInfo info in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (!info.IsSpecialName)
                    {
                        strings.Add(info.Name);
                    }
                }
                foreach (Type type2 in type.GetInterfaces())
                {
                    foreach (MethodInfo info2 in type2.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                    {
                        strings.Add(type2.FullName + "." + info2.Name);
                    }
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
            CorrelationService.Subscribe(parentContext, this, this.InterfaceType, this.MethodName, parentEventHandler, base.WorkflowInstanceId);
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
            CorrelationService.Unsubscribe(parentContext, this, this.InterfaceType, this.MethodName, parentEventHandler);
        }

        void IActivityEventListener<QueueEventArgs>.OnEvent(object sender, QueueEventArgs e)
        {
            ActivityExecutionContext context = (ActivityExecutionContext) sender;
            WebServiceInputActivity activity = context.Activity as WebServiceInputActivity;
            if ((activity.ExecutionStatus == ActivityExecutionStatus.Executing) && (this.ExecuteForActivity(context, activity.InterfaceType, activity.MethodName) == ActivityExecutionStatus.Closed))
            {
                this.UnsubscribeForActivity(context);
                context.CloseActivity();
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
                CorrelationService.Unsubscribe(context, this, this.InterfaceType, this.MethodName, this);
                this.ActivitySubscribed = false;
            }
        }

        internal bool ActivitySubscribed
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

        [SRCategory("Activity"), RefreshProperties(RefreshProperties.All), Editor(typeof(TypeBrowserEditor), typeof(UITypeEditor)), DefaultValue((string) null), SRDescription("InterfaceTypeDescription"), TypeFilterProvider(typeof(InterfaceTypeFilterProvider))]
        public Type InterfaceType
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

        [MergableProperty(false), SRDescription("ActivationDescr"), DefaultValue(false), SRCategory("Activity")]
        public bool IsActivating
        {
            get
            {
                return (bool) base.GetValue(IsActivatingProperty);
            }
            set
            {
                base.SetValue(IsActivatingProperty, value);
            }
        }

        [SRCategory("Activity"), TypeConverter(typeof(System.Workflow.Activities.Common.PropertyValueProviderTypeConverter)), SRDescription("WebServiceMethodDescription"), RefreshProperties(RefreshProperties.All), MergableProperty(false), DefaultValue("")]
        public string MethodName
        {
            get
            {
                return (string) base.GetValue(MethodNameProperty);
            }
            set
            {
                base.SetValue(MethodNameProperty, value);
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

        [Editor(typeof(BindUITypeEditor), typeof(UITypeEditor)), SRDescription("RoleDescr"), DefaultValue((string) null), SRCategory("Activity")]
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
                return (IComparable) base.GetValue(QueueNameProperty);
            }
        }
    }
}

