namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Net;
    using System.Reflection;
    using System.Web.Services.Protocols;
    using System.Workflow.Activities.Common;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    [ToolboxItem(typeof(InvokeWebServiceToolboxItem)), ActivityValidator(typeof(InvokeWebServiceValidator)), SRCategory("Standard"), ToolboxBitmap(typeof(InvokeWebServiceActivity), "Resources.WebServiceInOut.png"), SRDescription("InvokeWebServiceActivityDescription"), Designer(typeof(InvokeWebServiceDesigner), typeof(IDesigner))]
    public sealed class InvokeWebServiceActivity : Activity, IPropertyValueProvider, IDynamicPropertyTypeProvider
    {
        public static readonly DependencyProperty InvokedEvent = DependencyProperty.Register("Invoked", typeof(EventHandler<InvokeWebServiceEventArgs>), typeof(InvokeWebServiceActivity));
        public static readonly DependencyProperty InvokingEvent = DependencyProperty.Register("Invoking", typeof(EventHandler<InvokeWebServiceEventArgs>), typeof(InvokeWebServiceActivity));
        public static readonly DependencyProperty MethodNameProperty = DependencyProperty.Register("MethodName", typeof(string), typeof(InvokeWebServiceActivity), new PropertyMetadata("", DependencyPropertyOptions.Metadata));
        public static readonly DependencyProperty ParameterBindingsProperty = DependencyProperty.Register("ParameterBindings", typeof(WorkflowParameterBindingCollection), typeof(InvokeWebServiceActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata | DependencyPropertyOptions.ReadOnly, new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content) }));
        public static readonly DependencyProperty ProxyClassProperty = DependencyProperty.Register("ProxyClass", typeof(Type), typeof(InvokeWebServiceActivity), new PropertyMetadata(null, DependencyPropertyOptions.Metadata));
        internal static readonly ArrayList ReservedParameterNames = new ArrayList(new string[] { "Name", "Enabled", "Description", "MethodName", "ProxyClass", "SessionId", "Invoked", "Invoking" });
        private static DependencyProperty SessionCookieContainerProperty = DependencyProperty.Register("SessionCookieContainer", typeof(CookieContainer), typeof(InvokeWebServiceActivity));
        private static DependencyProperty SessionCookieMapProperty = DependencyProperty.RegisterAttached("SessionCookieMap", typeof(Dictionary<string, CookieContainer>), typeof(InvokeWebServiceActivity));
        public static readonly DependencyProperty SessionIdProperty = DependencyProperty.Register("SessionId", typeof(string), typeof(InvokeWebServiceActivity), new PropertyMetadata("", DependencyPropertyOptions.Metadata));
        private static readonly Guid WebServiceInvoker = new Guid("C3FE5ABC-7D41-4064-810E-42BEF0A855EC");

        [MergableProperty(false), SRDescription("OnAfterMethodInvokeDescr"), SRCategory("Handlers")]
        public event EventHandler<InvokeWebServiceEventArgs> Invoked
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

        [MergableProperty(false), SRCategory("Handlers"), SRDescription("OnBeforeMethodInvokeDescr")]
        public event EventHandler<InvokeWebServiceEventArgs> Invoking
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

        public InvokeWebServiceActivity()
        {
            base.SetReadOnlyPropertyValue(ParameterBindingsProperty, new WorkflowParameterBindingCollection(this));
        }

        public InvokeWebServiceActivity(string name) : base(name)
        {
            base.SetReadOnlyPropertyValue(ParameterBindingsProperty, new WorkflowParameterBindingCollection(this));
        }

        protected sealed override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
            {
                throw new ArgumentNullException("executionContext");
            }
            if ((this.SessionId != "") && (this.SessionId != null))
            {
                this.PopulateSessionCookie();
            }
            object proxyInstance = Activator.CreateInstance(this.ProxyClass);
            HttpWebClientProtocol protocol = proxyInstance as HttpWebClientProtocol;
            protocol.CookieContainer = this.SessionCookieContainer;
            base.RaiseGenericEvent<InvokeWebServiceEventArgs>(InvokingEvent, this, new InvokeWebServiceEventArgs(proxyInstance));
            MethodInfo method = this.ProxyClass.GetMethod(this.MethodName, BindingFlags.Public | BindingFlags.Instance);
            object[] parameters = InvokeHelper.GetParameters(method, this.ParameterBindings);
            WorkflowParameterBinding binding = null;
            if (this.ParameterBindings.Contains("(ReturnValue)"))
            {
                binding = this.ParameterBindings["(ReturnValue)"];
            }
            object obj3 = null;
            try
            {
                obj3 = this.ProxyClass.InvokeMember(this.MethodName, BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance, null, proxyInstance, parameters, CultureInfo.InvariantCulture);
            }
            catch (TargetInvocationException exception)
            {
                if (exception.InnerException != null)
                {
                    throw exception.InnerException;
                }
                throw;
            }
            if (binding != null)
            {
                binding.Value = obj3;
            }
            InvokeHelper.SaveOutRefParameters(parameters, method, this.ParameterBindings);
            base.RaiseGenericEvent<InvokeWebServiceEventArgs>(InvokedEvent, this, new InvokeWebServiceEventArgs(proxyInstance));
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
                Type proxyClass = this.ProxyClass;
                if (proxyClass != null)
                {
                    MethodInfo method = proxyClass.GetMethod(this.MethodName);
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
                                PropertyDescriptor descriptor = null;
                                if (info2.Position == -1)
                                {
                                    descriptor = new ParameterInfoBasedPropertyDescriptor(typeof(InvokeWebServiceActivity), info2, false, new Attribute[] { DesignOnlyAttribute.Yes });
                                }
                                else
                                {
                                    descriptor = new ParameterInfoBasedPropertyDescriptor(typeof(InvokeWebServiceActivity), info2, ReservedParameterNames.Contains(info2.Name), new Attribute[] { DesignOnlyAttribute.Yes });
                                }
                                properties[descriptor.Name] = descriptor;
                            }
                        }
                    }
                }
            }
        }

        private Activity GetRootActivity()
        {
            Activity parent = this;
            while (parent.Parent != null)
            {
                parent = parent.Parent;
            }
            return parent;
        }

        protected override void OnClosed(IServiceProvider provider)
        {
            base.RemoveProperty(SessionCookieContainerProperty);
            base.RemoveProperty(SessionCookieMapProperty);
        }

        private void PopulateSessionCookie()
        {
            if (this.SessionCookieContainer == null)
            {
                CookieContainer container;
                Activity rootActivity = this.GetRootActivity();
                Dictionary<string, CookieContainer> dictionary = (Dictionary<string, CookieContainer>) rootActivity.GetValue(SessionCookieMapProperty);
                if (dictionary == null)
                {
                    dictionary = new Dictionary<string, CookieContainer>();
                    rootActivity.SetValue(SessionCookieMapProperty, dictionary);
                    container = new CookieContainer();
                    dictionary.Add(this.SessionId, container);
                }
                else if (!dictionary.TryGetValue(this.SessionId, out container))
                {
                    container = new CookieContainer();
                    dictionary.Add(this.SessionId, container);
                }
                this.SessionCookieContainer = container;
            }
        }

        ICollection IPropertyValueProvider.GetPropertyValues(ITypeDescriptorContext context)
        {
            ITypeProvider service = (ITypeProvider) context.GetService(typeof(ITypeProvider));
            if (service == null)
            {
                throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(ITypeProvider).FullName }));
            }
            ICollection is2 = null;
            if (context.PropertyDescriptor.Name == "ProxyClass")
            {
                List<Type> list = new List<Type>();
                Type o = service.GetType(typeof(SoapHttpClientProtocol).FullName);
                if (o != null)
                {
                    foreach (Type type2 in service.GetTypes())
                    {
                        if (!type2.Equals(o) && TypeProvider.IsAssignable(o, type2))
                        {
                            list.Add(type2);
                        }
                    }
                }
                return list.ToArray();
            }
            if (!(context.PropertyDescriptor.Name == "MethodName"))
            {
                return is2;
            }
            StringCollection strings = new StringCollection();
            Type proxyClass = this.ProxyClass;
            if (proxyClass != null)
            {
                foreach (MethodInfo info in proxyClass.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    object[] customAttributes = info.GetCustomAttributes(typeof(SoapDocumentMethodAttribute), false);
                    if ((customAttributes == null) || (customAttributes.Length == 0))
                    {
                        customAttributes = info.GetCustomAttributes(typeof(SoapRpcMethodAttribute), false);
                    }
                    if ((customAttributes != null) && (customAttributes.Length > 0))
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
                ParameterInfoBasedPropertyDescriptor descriptor = properties[propertyName] as ParameterInfoBasedPropertyDescriptor;
                if (descriptor != null)
                {
                    return descriptor.ParameterType;
                }
            }
            return null;
        }

        [TypeConverter(typeof(PropertyValueProviderTypeConverter)), MergableProperty(false), SRDescription("MethodNameDescr"), SRCategory("Activity"), RefreshProperties(RefreshProperties.All), DefaultValue("")]
        public string MethodName
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

        [SRDescription("ProxyClassDescr"), TypeConverter(typeof(TypePropertyValueProviderTypeConverter)), MergableProperty(false), DefaultValue((string) null), SRCategory("Activity"), RefreshProperties(RefreshProperties.All)]
        public Type ProxyClass
        {
            get
            {
                return (base.GetValue(ProxyClassProperty) as Type);
            }
            set
            {
                base.SetValue(ProxyClassProperty, value);
            }
        }

        internal CookieContainer SessionCookieContainer
        {
            get
            {
                return (CookieContainer) base.GetValue(SessionCookieContainerProperty);
            }
            set
            {
                base.SetValue(SessionCookieContainerProperty, value);
            }
        }

        [MergableProperty(false), SRDescription("WebServiceSessionIDDescr"), SRCategory("Activity"), DefaultValue("")]
        public string SessionId
        {
            get
            {
                return (base.GetValue(SessionIdProperty) as string);
            }
            set
            {
                base.SetValue(SessionIdProperty, value);
            }
        }
    }
}

