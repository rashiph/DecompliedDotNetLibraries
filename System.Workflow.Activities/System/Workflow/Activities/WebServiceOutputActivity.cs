namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Reflection;
    using System.Workflow.Activities.Common;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.Runtime;

    [SRDescription("WebServiceResponseActivityDescription"), Designer(typeof(WebServiceResponseDesigner), typeof(IDesigner)), ActivityValidator(typeof(WebServiceResponseValidator)), DefaultEvent("SendingOutput"), ToolboxBitmap(typeof(WebServiceOutputActivity), "Resources.WebServiceOut.png"), SRCategory("Standard")]
    public sealed class WebServiceOutputActivity : Activity, IPropertyValueProvider, IDynamicPropertyTypeProvider
    {
        public static readonly DependencyProperty InputActivityNameProperty = DependencyProperty.Register("InputActivityName", typeof(string), typeof(WebServiceOutputActivity), new PropertyMetadata("", DependencyPropertyOptions.Metadata));
        public static readonly DependencyProperty ParameterBindingsProperty = DependencyProperty.Register("ParameterBindings", typeof(WorkflowParameterBindingCollection), typeof(WebServiceOutputActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata | DependencyPropertyOptions.ReadOnly, new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content) }));
        public static readonly DependencyProperty SendingOutputEvent = DependencyProperty.Register("SendingOutput", typeof(EventHandler), typeof(WebServiceOutputActivity));

        [SRCategory("Handlers"), MergableProperty(false), SRDescription("OnBeforeResponseDescr")]
        public event EventHandler SendingOutput
        {
            add
            {
                base.AddHandler(SendingOutputEvent, value);
            }
            remove
            {
                base.RemoveHandler(SendingOutputEvent, value);
            }
        }

        public WebServiceOutputActivity()
        {
            base.SetReadOnlyPropertyValue(ParameterBindingsProperty, new WorkflowParameterBindingCollection(this));
        }

        public WebServiceOutputActivity(string name) : base(name)
        {
            base.SetReadOnlyPropertyValue(ParameterBindingsProperty, new WorkflowParameterBindingCollection(this));
        }

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            WorkflowQueuingService service = executionContext.GetService<WorkflowQueuingService>();
            base.RaiseEvent(SendingOutputEvent, this, EventArgs.Empty);
            WebServiceInputActivity activityByName = base.GetActivityByName(this.InputActivityName) as WebServiceInputActivity;
            if (activityByName == null)
            {
                for (Activity activity2 = base.Parent; activity2 != null; activity2 = base.Parent)
                {
                    string activityQualifiedName = activity2.QualifiedName + "." + this.InputActivityName;
                    activityByName = base.GetActivityByName(activityQualifiedName) as WebServiceInputActivity;
                    if (activityByName != null)
                    {
                        break;
                    }
                }
            }
            if (activityByName == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_CannotResolveWebServiceInput", new object[] { base.QualifiedName, this.InputActivityName }));
            }
            IComparable queueName = new EventQueueName(activityByName.InterfaceType, activityByName.MethodName, activityByName.QualifiedName);
            MethodInfo method = activityByName.InterfaceType.GetMethod(activityByName.MethodName);
            if (!service.Exists(queueName))
            {
                if (method.ReturnType == typeof(void))
                {
                    return ActivityExecutionStatus.Closed;
                }
                bool flag = false;
                foreach (ParameterInfo info2 in method.GetParameters())
                {
                    if (info2.ParameterType.IsByRef || (info2.IsIn && info2.IsOut))
                    {
                        flag = true;
                    }
                }
                if (flag)
                {
                    return ActivityExecutionStatus.Closed;
                }
            }
            if (!service.Exists(queueName))
            {
                throw new InvalidOperationException(SR.GetString("Error_WebServiceInputNotProcessed", new object[] { activityByName.QualifiedName }));
            }
            IMethodResponseMessage message = null;
            WorkflowQueue workflowQueue = service.GetWorkflowQueue(queueName);
            if (workflowQueue.Count != 0)
            {
                message = workflowQueue.Dequeue() as IMethodResponseMessage;
            }
            WorkflowParameterBindingCollection parameterBindings = this.ParameterBindings;
            ArrayList outArgs = new ArrayList();
            if (this.ParameterBindings.Contains("(ReturnValue)"))
            {
                WorkflowParameterBinding binding = this.ParameterBindings["(ReturnValue)"];
                if (binding != null)
                {
                    outArgs.Add(binding.Value);
                }
            }
            foreach (ParameterInfo info3 in method.GetParameters())
            {
                if (info3.ParameterType.IsByRef || (info3.IsIn && info3.IsOut))
                {
                    WorkflowParameterBinding binding2 = parameterBindings[info3.Name];
                    outArgs.Add(binding2.Value);
                }
            }
            message.SendResponse(outArgs);
            return ActivityExecutionStatus.Closed;
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
                if ((this.InputActivityName != null) && !string.IsNullOrEmpty(this.InputActivityName.Trim()))
                {
                    WebServiceInputActivity activity = Helpers.ParseActivity(Helpers.GetRootActivity(this), this.InputActivityName) as WebServiceInputActivity;
                    if (activity != null)
                    {
                        Type interfaceType = null;
                        if (activity.InterfaceType != null)
                        {
                            interfaceType = service.GetType(activity.InterfaceType.AssemblyQualifiedName);
                        }
                        if (interfaceType != null)
                        {
                            MethodInfo interfaceMethod = Helpers.GetInterfaceMethod(interfaceType, activity.MethodName);
                            if ((interfaceMethod != null) && (WebServiceActivityHelpers.ValidateParameterTypes(interfaceMethod).Count == 0))
                            {
                                List<ParameterInfo> list;
                                List<ParameterInfo> list2;
                                WebServiceActivityHelpers.GetParameterInfo(interfaceMethod, out list, out list2);
                                foreach (ParameterInfo info2 in list2)
                                {
                                    PropertyDescriptor descriptor = null;
                                    if (info2.Position == -1)
                                    {
                                        descriptor = new ParameterInfoBasedPropertyDescriptor(typeof(WebServiceOutputActivity), info2, false, new Attribute[] { DesignOnlyAttribute.Yes });
                                    }
                                    else
                                    {
                                        descriptor = new ParameterInfoBasedPropertyDescriptor(typeof(WebServiceOutputActivity), info2, true, new Attribute[] { DesignOnlyAttribute.Yes });
                                    }
                                    if (descriptor != null)
                                    {
                                        properties[descriptor.Name] = descriptor;
                                    }
                                }
                            }
                        }
                    }
                }
            }
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
                ParameterInfoBasedPropertyDescriptor descriptor = properties[propertyName] as ParameterInfoBasedPropertyDescriptor;
                if (descriptor != null)
                {
                    return descriptor.ParameterType;
                }
            }
            return null;
        }

        [TypeConverter(typeof(PropertyValueProviderTypeConverter)), DefaultValue(""), SRDescription("ReceiveActivityNameDescription"), RefreshProperties(RefreshProperties.All), MergableProperty(false), SRCategory("Activity")]
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

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public WorkflowParameterBindingCollection ParameterBindings
        {
            get
            {
                return (base.GetValue(ParameterBindingsProperty) as WorkflowParameterBindingCollection);
            }
        }
    }
}

