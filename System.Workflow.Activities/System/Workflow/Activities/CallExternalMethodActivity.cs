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
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Workflow.Activities.Common;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.Runtime;

    [SRCategory("Base"), ActivityValidator(typeof(CallExternalMethodActivityValidator)), DefaultEvent("MethodInvoking"), Designer(typeof(CallExternalMethodActivityDesigner), typeof(IDesigner)), SRDescription("CallExternalMethodActivityDescription")]
    public class CallExternalMethodActivity : Activity, System.Workflow.Activities.Common.IPropertyValueProvider, IDynamicPropertyTypeProvider
    {
        public static readonly DependencyProperty CorrelationTokenProperty = DependencyProperty.Register("CorrelationToken", typeof(System.Workflow.Runtime.CorrelationToken), typeof(CallExternalMethodActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata));
        public static readonly DependencyProperty InterfaceTypeProperty = DependencyProperty.Register("InterfaceType", typeof(Type), typeof(CallExternalMethodActivity), new PropertyMetadata(null, DependencyPropertyOptions.Metadata, new Attribute[] { new ValidationOptionAttribute(ValidationOption.Required) }));
        public static readonly DependencyProperty MethodInvokingEvent = DependencyProperty.Register("MethodInvoking", typeof(EventHandler), typeof(CallExternalMethodActivity));
        public static readonly DependencyProperty MethodNameProperty = DependencyProperty.Register("MethodName", typeof(string), typeof(CallExternalMethodActivity), new PropertyMetadata("", DependencyPropertyOptions.Metadata, new Attribute[] { new ValidationOptionAttribute(ValidationOption.Required) }));
        public static readonly DependencyProperty ParameterBindingsProperty = DependencyProperty.Register("ParameterBindings", typeof(WorkflowParameterBindingCollection), typeof(CallExternalMethodActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata | DependencyPropertyOptions.ReadOnly, new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content) }));
        internal static readonly ArrayList ReservedParameterNames = new ArrayList(new string[] { "Name", "Enabled", "Description", "MethodName", "MethodInvoking", "InterfaceType" });

        [MergableProperty(false), SRCategory("Handlers"), SRDescription("OnBeforeMethodInvokeDescr")]
        public event EventHandler MethodInvoking
        {
            add
            {
                base.AddHandler(MethodInvokingEvent, value);
            }
            remove
            {
                base.RemoveHandler(MethodInvokingEvent, value);
            }
        }

        public CallExternalMethodActivity()
        {
            base.SetReadOnlyPropertyValue(ParameterBindingsProperty, new WorkflowParameterBindingCollection(this));
        }

        public CallExternalMethodActivity(string name) : base(name)
        {
            base.SetReadOnlyPropertyValue(ParameterBindingsProperty, new WorkflowParameterBindingCollection(this));
        }

        protected sealed override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            if (this.InterfaceType == null)
            {
                throw new ArgumentException(SR.GetString("Error_MissingInterfaceType"), "executionContext");
            }
            Type interfaceType = this.InterfaceType;
            string methodName = this.MethodName;
            object service = executionContext.GetService(interfaceType);
            if (service == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_ServiceNotFound", new object[] { this.InterfaceType }));
            }
            base.RaiseEvent(MethodInvokingEvent, this, EventArgs.Empty);
            this.OnMethodInvoking(EventArgs.Empty);
            MethodInfo method = interfaceType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
            ParameterModifier[] parameterModifiers = null;
            object[] messageArgs = InvokeHelper.GetParameters(method, this.ParameterBindings, out parameterModifiers);
            WorkflowParameterBinding binding = null;
            if (this.ParameterBindings.Contains("(ReturnValue)"))
            {
                binding = this.ParameterBindings["(ReturnValue)"];
            }
            CorrelationService.InvalidateCorrelationToken(this, interfaceType, methodName, messageArgs);
            object source = interfaceType.InvokeMember(this.MethodName, BindingFlags.InvokeMethod, new ExternalDataExchangeBinder(), service, messageArgs, parameterModifiers, null, null);
            if (binding != null)
            {
                binding.Value = InvokeHelper.CloneOutboundValue(source, new BinaryFormatter(), "(ReturnValue)");
            }
            InvokeHelper.SaveOutRefParameters(messageArgs, method, this.ParameterBindings);
            this.OnMethodInvoked(EventArgs.Empty);
            return ActivityExecutionStatus.Closed;
        }

        internal void GetParameterPropertyDescriptors(IDictionary properties)
        {
            if (this.Site != null)
            {
                if (((ITypeProvider) this.Site.GetService(typeof(ITypeProvider))) == null)
                {
                    throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(ITypeProvider).FullName }));
                }
                if (base.GetType() == typeof(CallExternalMethodActivity))
                {
                    Type interfaceType = this.InterfaceType;
                    if (interfaceType != null)
                    {
                        MethodInfo method = interfaceType.GetMethod(this.MethodName);
                        if (method != null)
                        {
                            ArrayList list = new ArrayList(method.GetParameters());
                            if (!(method.ReturnType == typeof(void)))
                            {
                                list.Add(method.ReturnParameter);
                            }
                            foreach (ParameterInfo info2 in list)
                            {
                                if (info2.ParameterType != null)
                                {
                                    PropertyDescriptor descriptor = new System.Workflow.Activities.Common.ParameterInfoBasedPropertyDescriptor(typeof(CallExternalMethodActivity), info2, true, new Attribute[] { DesignOnlyAttribute.Yes });
                                    properties[descriptor.Name] = descriptor;
                                }
                            }
                        }
                    }
                }
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
            string methodName = this.MethodName;
            if (methodName == null)
            {
                throw new InvalidOperationException(SR.GetString("MethodNameMissing", new object[] { base.Name }));
            }
            MethodInfo method = interfaceType.GetMethod(methodName);
            if (method == null)
            {
                throw new InvalidOperationException(SR.GetString("MethodInfoMissing", new object[] { this.MethodName, this.InterfaceType.Name }));
            }
            InvokeHelper.InitializeParameters(method, this.ParameterBindings);
            base.InitializeProperties();
        }

        protected virtual void OnMethodInvoked(EventArgs e)
        {
        }

        protected virtual void OnMethodInvoking(EventArgs e)
        {
        }

        ICollection System.Workflow.Activities.Common.IPropertyValueProvider.GetPropertyValues(ITypeDescriptorContext context)
        {
            StringCollection strings = new StringCollection();
            if (this.InterfaceType != null)
            {
                if (!(context.PropertyDescriptor.Name == "MethodName"))
                {
                    return strings;
                }
                foreach (MethodInfo info in this.InterfaceType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (!info.IsSpecialName)
                    {
                        strings.Add(info.Name);
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
                System.Workflow.Activities.Common.ParameterInfoBasedPropertyDescriptor descriptor = properties[propertyName] as System.Workflow.Activities.Common.ParameterInfoBasedPropertyDescriptor;
                if (descriptor != null)
                {
                    return descriptor.ParameterType;
                }
            }
            return null;
        }

        [SRCategory("Activity"), SRDescription("CorrelationSetDescr"), RefreshProperties(RefreshProperties.All), MergableProperty(false), TypeConverter(typeof(CorrelationTokenTypeConverter)), DefaultValue((string) null)]
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

        [RefreshProperties(RefreshProperties.All), DefaultValue((string) null), SRCategory("Activity"), SRDescription("HelperExternalDataExchangeDesc"), Editor(typeof(TypeBrowserEditor), typeof(UITypeEditor)), TypeFilterProvider(typeof(ExternalDataExchangeInterfaceTypeFilterProvider))]
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

        [RefreshProperties(RefreshProperties.All), SRDescription("ExternalMethodNameDescr"), DefaultValue(""), SRCategory("Activity"), TypeConverter(typeof(System.Workflow.Activities.Common.PropertyValueProviderTypeConverter)), MergableProperty(false)]
        public virtual string MethodName
        {
            get
            {
                return (base.GetValue(MethodNameProperty) as string);
            }
            set
            {
                base.SetValue(MethodNameProperty, value);
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

