namespace System.Web.UI.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Web.Compilation;
    using System.Web.UI;

    internal static class ControlLocalizer
    {
        private const char filterDelimiter = ':';
        private const string LocalizationResourceExpressionPrefix = "resources";
        private const char objectDelimiter = '.';
        private const char OMDelimiter = '.';

        private static bool IsPropertyLocalizable(PropertyDescriptor propertyDescriptor)
        {
            DesignerSerializationVisibilityAttribute attribute = (DesignerSerializationVisibilityAttribute) propertyDescriptor.Attributes[typeof(DesignerSerializationVisibilityAttribute)];
            if ((attribute != null) && (attribute.Visibility == DesignerSerializationVisibility.Hidden))
            {
                return false;
            }
            LocalizableAttribute attribute2 = (LocalizableAttribute) propertyDescriptor.Attributes[typeof(LocalizableAttribute)];
            return ((attribute2 != null) && attribute2.IsLocalizable);
        }

        public static string LocalizeControl(Control control, IDesignTimeResourceWriter resourceWriter, out string newInnerContent)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }
            if (resourceWriter == null)
            {
                throw new ArgumentNullException("resourceWriter");
            }
            if (control.Site == null)
            {
                throw new InvalidOperationException();
            }
            IDesignerHost service = (IDesignerHost) control.Site.GetService(typeof(IDesignerHost));
            IDesignerHost parseTimeDesignerHost = new LocalizationDesignerHost(service);
            Control control2 = (service.GetDesigner(control) as ControlDesigner).CreateClonedControl(parseTimeDesignerHost, false);
            ((IControlDesignerAccessor) control2).SetOwnerControl(control);
            bool shouldLocalizeInnerContent = ShouldLocalizeInnerContents(control.Site, control);
            string str = LocalizeControl(control2, parseTimeDesignerHost, resourceWriter, shouldLocalizeInnerContent);
            if (shouldLocalizeInnerContent)
            {
                newInnerContent = ControlSerializer.SerializeInnerContents(control2, parseTimeDesignerHost);
                return str;
            }
            newInnerContent = null;
            return str;
        }

        private static string LocalizeControl(Control control, IServiceProvider serviceProvider, IDesignTimeResourceWriter resourceWriter, bool shouldLocalizeInnerContent)
        {
            ResourceExpressionEditor expressionEditor = (ResourceExpressionEditor) ExpressionEditor.GetExpressionEditor("resources", serviceProvider);
            IControlBuilderAccessor accessor = control;
            ControlBuilder controlBuilder = accessor.ControlBuilder;
            ObjectPersistData objectPersistData = controlBuilder.GetObjectPersistData();
            string resourceKey = controlBuilder.GetResourceKey();
            string b = LocalizeObject(serviceProvider, control, objectPersistData, expressionEditor, resourceWriter, resourceKey, string.Empty, control, string.Empty, shouldLocalizeInnerContent, false, false);
            if (!string.Equals(resourceKey, b, StringComparison.OrdinalIgnoreCase))
            {
                controlBuilder.SetResourceKey(b);
            }
            if (objectPersistData != null)
            {
                foreach (PropertyEntry entry in objectPersistData.AllPropertyEntries)
                {
                    BoundPropertyEntry entry2 = entry as BoundPropertyEntry;
                    if ((entry2 != null) && !entry2.Generated)
                    {
                        string[] strArray = entry2.Name.Split(new char[] { '.' });
                        if (strArray.Length > 1)
                        {
                            object component = control;
                            foreach (string str3 in strArray)
                            {
                                PropertyDescriptor descriptor = TypeDescriptor.GetProperties(component)[str3];
                                if (descriptor == null)
                                {
                                    break;
                                }
                                PersistenceModeAttribute attribute = descriptor.Attributes[typeof(PersistenceModeAttribute)] as PersistenceModeAttribute;
                                if (attribute != PersistenceModeAttribute.Attribute)
                                {
                                    if (string.Equals(entry2.ExpressionPrefix, "resources", StringComparison.OrdinalIgnoreCase))
                                    {
                                        System.Web.Compilation.ResourceExpressionFields parsedExpressionData = entry2.ParsedExpressionData as System.Web.Compilation.ResourceExpressionFields;
                                        if ((parsedExpressionData != null) && string.IsNullOrEmpty(parsedExpressionData.ClassKey))
                                        {
                                            object obj3 = expressionEditor.EvaluateExpression(entry2.Expression, entry2.ParsedExpressionData, entry2.PropertyInfo.PropertyType, serviceProvider);
                                            if (obj3 == null)
                                            {
                                                object obj4;
                                                obj3 = ControlDesigner.GetComplexProperty(control, entry2.Name, out obj4).GetValue(obj4);
                                            }
                                            resourceWriter.AddResource(parsedExpressionData.ResourceKey, obj3);
                                        }
                                    }
                                    break;
                                }
                                component = descriptor.GetValue(component);
                            }
                        }
                    }
                }
            }
            return b;
        }

        private static string LocalizeObject(IServiceProvider serviceProvider, object obj, ObjectPersistData persistData, ResourceExpressionEditor resEditor, IDesignTimeResourceWriter resourceWriter, string resourceKey, string objectModelName, object topLevelObject, string filter, bool shouldLocalizeInnerContent, bool isComplexProperty, bool implicitlyLocalizeComplexProperty)
        {
            bool flag;
            if (isComplexProperty)
            {
                flag = implicitlyLocalizeComplexProperty;
            }
            else
            {
                flag = (persistData == null) || persistData.Localize;
            }
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(obj);
            for (int i = 0; i < properties.Count; i++)
            {
                try
                {
                    PropertyDescriptor propertyDescriptor = properties[i];
                    if (string.Equals(propertyDescriptor.Name, "Controls", StringComparison.Ordinal))
                    {
                        Control control = obj as Control;
                        if ((control != null) && shouldLocalizeInnerContent)
                        {
                            if (!ParseChildren(control.GetType()))
                            {
                                foreach (Control control2 in control.Controls)
                                {
                                    IControlBuilderAccessor accessor = control2;
                                    ControlBuilder controlBuilder = accessor.ControlBuilder;
                                    if (controlBuilder != null)
                                    {
                                        string str = controlBuilder.GetResourceKey();
                                        string b = LocalizeObject(serviceProvider, control2, controlBuilder.GetObjectPersistData(), resEditor, resourceWriter, str, string.Empty, control2, string.Empty, true, false, false);
                                        if (!string.Equals(str, b, StringComparison.OrdinalIgnoreCase))
                                        {
                                            controlBuilder.SetResourceKey(b);
                                        }
                                    }
                                }
                            }
                            continue;
                        }
                    }
                    PersistenceModeAttribute attribute = (PersistenceModeAttribute) propertyDescriptor.Attributes[typeof(PersistenceModeAttribute)];
                    string str3 = (objectModelName.Length > 0) ? (objectModelName + '.' + propertyDescriptor.Name) : propertyDescriptor.Name;
                    if ((attribute.Mode == PersistenceMode.Attribute) && (propertyDescriptor.SerializationVisibility == DesignerSerializationVisibility.Content))
                    {
                        resourceKey = LocalizeObject(serviceProvider, propertyDescriptor.GetValue(obj), persistData, resEditor, resourceWriter, resourceKey, str3, topLevelObject, filter, true, true, flag);
                    }
                    else if ((attribute.Mode == PersistenceMode.Attribute) || (propertyDescriptor.PropertyType == typeof(string)))
                    {
                        bool flag2 = false;
                        bool flag3 = false;
                        object obj2 = null;
                        string name = string.Empty;
                        if (persistData != null)
                        {
                            PropertyEntry filteredProperty = persistData.GetFilteredProperty(string.Empty, str3);
                            if (filteredProperty is BoundPropertyEntry)
                            {
                                BoundPropertyEntry entry2 = (BoundPropertyEntry) filteredProperty;
                                if (!entry2.Generated)
                                {
                                    if (string.Equals(entry2.ExpressionPrefix, "resources", StringComparison.OrdinalIgnoreCase))
                                    {
                                        System.Web.Compilation.ResourceExpressionFields parsedExpressionData = entry2.ParsedExpressionData as System.Web.Compilation.ResourceExpressionFields;
                                        if ((parsedExpressionData != null) && string.IsNullOrEmpty(parsedExpressionData.ClassKey))
                                        {
                                            name = parsedExpressionData.ResourceKey;
                                            obj2 = resEditor.EvaluateExpression(entry2.Expression, entry2.ParsedExpressionData, propertyDescriptor.PropertyType, serviceProvider);
                                            if (obj2 != null)
                                            {
                                                flag3 = true;
                                            }
                                            flag2 = true;
                                        }
                                    }
                                }
                                else
                                {
                                    flag2 = true;
                                }
                            }
                            else
                            {
                                flag2 = flag && IsPropertyLocalizable(propertyDescriptor);
                            }
                        }
                        else
                        {
                            flag2 = flag && IsPropertyLocalizable(propertyDescriptor);
                        }
                        if (flag2)
                        {
                            if (!flag3)
                            {
                                obj2 = propertyDescriptor.GetValue(obj);
                            }
                            if (name.Length == 0)
                            {
                                if (string.IsNullOrEmpty(resourceKey))
                                {
                                    resourceKey = resourceWriter.CreateResourceKey(null, topLevelObject);
                                }
                                name = resourceKey + '.' + str3;
                                if (filter.Length != 0)
                                {
                                    name = filter + ':' + name;
                                }
                            }
                            resourceWriter.AddResource(name, obj2);
                        }
                        if (persistData != null)
                        {
                            foreach (PropertyEntry entry3 in persistData.GetPropertyAllFilters(str3))
                            {
                                if (entry3.Filter.Length > 0)
                                {
                                    if (entry3 is SimplePropertyEntry)
                                    {
                                        if (flag && IsPropertyLocalizable(propertyDescriptor))
                                        {
                                            if (name.Length == 0)
                                            {
                                                if (string.IsNullOrEmpty(resourceKey))
                                                {
                                                    resourceKey = resourceWriter.CreateResourceKey(null, topLevelObject);
                                                }
                                                name = resourceKey + '.' + str3;
                                            }
                                            string str5 = entry3.Filter + ':' + name;
                                            resourceWriter.AddResource(str5, ((SimplePropertyEntry) entry3).Value);
                                        }
                                    }
                                    else if (!(entry3 is ComplexPropertyEntry) && (entry3 is BoundPropertyEntry))
                                    {
                                        BoundPropertyEntry entry4 = (BoundPropertyEntry) entry3;
                                        if (!entry4.Generated && string.Equals(entry4.ExpressionPrefix, "resources", StringComparison.OrdinalIgnoreCase))
                                        {
                                            System.Web.Compilation.ResourceExpressionFields fields2 = entry4.ParsedExpressionData as System.Web.Compilation.ResourceExpressionFields;
                                            if ((fields2 != null) && string.IsNullOrEmpty(fields2.ClassKey))
                                            {
                                                object obj3 = resEditor.EvaluateExpression(entry4.Expression, entry4.ParsedExpressionData, entry3.PropertyInfo.PropertyType, serviceProvider);
                                                if (obj3 == null)
                                                {
                                                    obj3 = string.Empty;
                                                }
                                                resourceWriter.AddResource(fields2.ResourceKey, obj3);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else if (shouldLocalizeInnerContent)
                    {
                        if (typeof(ICollection).IsAssignableFrom(propertyDescriptor.PropertyType))
                        {
                            if (persistData != null)
                            {
                                foreach (ComplexPropertyEntry entry5 in persistData.GetPropertyAllFilters(propertyDescriptor.Name))
                                {
                                    foreach (ComplexPropertyEntry entry6 in entry5.Builder.GetObjectPersistData().CollectionItems)
                                    {
                                        ControlBuilder builder = entry6.Builder;
                                        object obj4 = builder.BuildObject();
                                        string str6 = builder.GetResourceKey();
                                        string str7 = LocalizeObject(serviceProvider, obj4, builder.GetObjectPersistData(), resEditor, resourceWriter, str6, string.Empty, obj4, string.Empty, true, false, false);
                                        if (!string.Equals(str6, str7, StringComparison.OrdinalIgnoreCase))
                                        {
                                            builder.SetResourceKey(str7);
                                        }
                                    }
                                }
                            }
                        }
                        else if (typeof(ITemplate).IsAssignableFrom(propertyDescriptor.PropertyType))
                        {
                            if (persistData != null)
                            {
                                foreach (TemplatePropertyEntry entry7 in persistData.GetPropertyAllFilters(propertyDescriptor.Name))
                                {
                                    TemplateBuilder builder3 = (TemplateBuilder) entry7.Builder;
                                    IDesignerHost designerHost = (IDesignerHost) serviceProvider.GetService(typeof(IDesignerHost));
                                    Control[] controlArray = ControlParser.ParseControls(designerHost, builder3.Text);
                                    for (int j = 0; j < controlArray.Length; j++)
                                    {
                                        if (!(controlArray[j] is LiteralControl) && !(controlArray[j] is DesignerDataBoundLiteralControl))
                                        {
                                            LocalizeControl(controlArray[j], serviceProvider, resourceWriter, true);
                                        }
                                    }
                                    StringBuilder builder4 = new StringBuilder();
                                    for (int k = 0; k < controlArray.Length; k++)
                                    {
                                        if (controlArray[k] is LiteralControl)
                                        {
                                            builder4.Append(((LiteralControl) controlArray[k]).Text);
                                        }
                                        else
                                        {
                                            builder4.Append(ControlPersister.PersistControl(controlArray[k], designerHost));
                                        }
                                    }
                                    builder3.Text = builder4.ToString();
                                }
                            }
                        }
                        else if (persistData != null)
                        {
                            object obj5 = propertyDescriptor.GetValue(obj);
                            ObjectPersistData objectPersistData = null;
                            ComplexPropertyEntry entry8 = (ComplexPropertyEntry) persistData.GetFilteredProperty(string.Empty, propertyDescriptor.Name);
                            if (entry8 != null)
                            {
                                objectPersistData = entry8.Builder.GetObjectPersistData();
                            }
                            resourceKey = LocalizeObject(serviceProvider, obj5, objectPersistData, resEditor, resourceWriter, resourceKey, str3, topLevelObject, string.Empty, true, true, flag);
                            foreach (ComplexPropertyEntry entry9 in persistData.GetPropertyAllFilters(propertyDescriptor.Name))
                            {
                                if (entry9.Filter.Length > 0)
                                {
                                    ControlBuilder builder5 = entry9.Builder;
                                    objectPersistData = builder5.GetObjectPersistData();
                                    obj5 = builder5.BuildObject();
                                    resourceKey = LocalizeObject(serviceProvider, obj5, objectPersistData, resEditor, resourceWriter, resourceKey, str3, topLevelObject, entry9.Filter, true, true, flag);
                                }
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    if (serviceProvider != null)
                    {
                        IComponentDesignerDebugService service = serviceProvider.GetService(typeof(IComponentDesignerDebugService)) as IComponentDesignerDebugService;
                        if (service != null)
                        {
                            service.Fail(exception.Message);
                        }
                    }
                }
            }
            return resourceKey;
        }

        private static bool ParseChildren(Type controlType)
        {
            object[] customAttributes = controlType.GetCustomAttributes(typeof(ParseChildrenAttribute), true);
            return (((customAttributes != null) && (customAttributes.Length > 0)) && ((ParseChildrenAttribute) customAttributes[0]).ChildrenAsProperties);
        }

        private static bool ShouldLocalizeInnerContents(IServiceProvider serviceProvider, object obj)
        {
            Control component = obj as Control;
            if (component == null)
            {
                return false;
            }
            IDesignerHost service = (IDesignerHost) serviceProvider.GetService(typeof(IDesignerHost));
            if (service == null)
            {
                return false;
            }
            ControlDesigner designer = service.GetDesigner(component) as ControlDesigner;
            if ((designer != null) && !designer.ReadOnlyInternal)
            {
                return false;
            }
            return true;
        }

        private sealed class LocalizationDesignerHost : IDesignerHost, IServiceContainer, IServiceProvider
        {
            private LocalizationFilterResolutionService _localizationFilterService;
            private IDesignerHost _parentHost;

            event EventHandler IDesignerHost.Activated
            {
                add
                {
                    this._parentHost.Activated += value;
                }
                remove
                {
                    this._parentHost.Activated -= value;
                }
            }

            event EventHandler IDesignerHost.Deactivated
            {
                add
                {
                    this._parentHost.Deactivated += value;
                }
                remove
                {
                    this._parentHost.Deactivated -= value;
                }
            }

            event EventHandler IDesignerHost.LoadComplete
            {
                add
                {
                    this._parentHost.LoadComplete += value;
                }
                remove
                {
                    this._parentHost.LoadComplete -= value;
                }
            }

            event DesignerTransactionCloseEventHandler IDesignerHost.TransactionClosed
            {
                add
                {
                    this._parentHost.TransactionClosed += value;
                }
                remove
                {
                    this._parentHost.TransactionClosed -= value;
                }
            }

            event DesignerTransactionCloseEventHandler IDesignerHost.TransactionClosing
            {
                add
                {
                    this._parentHost.TransactionClosing += value;
                }
                remove
                {
                    this._parentHost.TransactionClosing -= value;
                }
            }

            event EventHandler IDesignerHost.TransactionOpened
            {
                add
                {
                    this._parentHost.TransactionOpened += value;
                }
                remove
                {
                    this._parentHost.TransactionOpened -= value;
                }
            }

            event EventHandler IDesignerHost.TransactionOpening
            {
                add
                {
                    this._parentHost.TransactionOpening += value;
                }
                remove
                {
                    this._parentHost.TransactionOpening -= value;
                }
            }

            internal LocalizationDesignerHost(IDesignerHost parentHost)
            {
                this._parentHost = parentHost;
            }

            void IDesignerHost.Activate()
            {
            }

            IComponent IDesignerHost.CreateComponent(Type componentType)
            {
                return this._parentHost.CreateComponent(componentType);
            }

            IComponent IDesignerHost.CreateComponent(Type componentType, string name)
            {
                return this._parentHost.CreateComponent(componentType, name);
            }

            DesignerTransaction IDesignerHost.CreateTransaction()
            {
                return this._parentHost.CreateTransaction();
            }

            DesignerTransaction IDesignerHost.CreateTransaction(string description)
            {
                return this._parentHost.CreateTransaction(description);
            }

            void IDesignerHost.DestroyComponent(IComponent component)
            {
                this._parentHost.DestroyComponent(component);
            }

            IDesigner IDesignerHost.GetDesigner(IComponent component)
            {
                return this._parentHost.GetDesigner(component);
            }

            Type IDesignerHost.GetType(string typeName)
            {
                return this._parentHost.GetType(typeName);
            }

            void IServiceContainer.AddService(Type serviceType, ServiceCreatorCallback callback)
            {
                this._parentHost.AddService(serviceType, callback);
            }

            void IServiceContainer.AddService(Type serviceType, object serviceInstance)
            {
                this._parentHost.AddService(serviceType, serviceInstance);
            }

            void IServiceContainer.AddService(Type serviceType, ServiceCreatorCallback callback, bool promote)
            {
                this._parentHost.AddService(serviceType, callback, promote);
            }

            void IServiceContainer.AddService(Type serviceType, object serviceInstance, bool promote)
            {
                this._parentHost.AddService(serviceType, serviceInstance, promote);
            }

            void IServiceContainer.RemoveService(Type serviceType)
            {
                this._parentHost.RemoveService(serviceType);
            }

            void IServiceContainer.RemoveService(Type serviceType, bool promote)
            {
                this._parentHost.RemoveService(serviceType, promote);
            }

            object IServiceProvider.GetService(Type serviceType)
            {
                if (!(serviceType == typeof(IFilterResolutionService)))
                {
                    return this._parentHost.GetService(serviceType);
                }
                if (this._localizationFilterService == null)
                {
                    IFilterResolutionService realFilterService = (IFilterResolutionService) this._parentHost.GetService(typeof(IFilterResolutionService));
                    if (realFilterService == null)
                    {
                        throw new InvalidOperationException(System.Design.SR.GetString("ControlLocalizer_RequiresFilterService"));
                    }
                    this._localizationFilterService = new LocalizationFilterResolutionService(realFilterService);
                }
                return this._localizationFilterService;
            }

            IContainer IDesignerHost.Container
            {
                get
                {
                    return this._parentHost.Container;
                }
            }

            bool IDesignerHost.InTransaction
            {
                get
                {
                    return this._parentHost.InTransaction;
                }
            }

            bool IDesignerHost.Loading
            {
                get
                {
                    return this._parentHost.Loading;
                }
            }

            IComponent IDesignerHost.RootComponent
            {
                get
                {
                    return this._parentHost.RootComponent;
                }
            }

            string IDesignerHost.RootComponentClassName
            {
                get
                {
                    return this._parentHost.RootComponentClassName;
                }
            }

            string IDesignerHost.TransactionDescription
            {
                get
                {
                    return this._parentHost.TransactionDescription;
                }
            }

            private sealed class LocalizationFilterResolutionService : IFilterResolutionService
            {
                private IFilterResolutionService _realFilterService;

                internal LocalizationFilterResolutionService(IFilterResolutionService realFilterService)
                {
                    this._realFilterService = realFilterService;
                }

                int IFilterResolutionService.CompareFilters(string filter1, string filter2)
                {
                    return this._realFilterService.CompareFilters(filter1, filter2);
                }

                bool IFilterResolutionService.EvaluateFilter(string filterName)
                {
                    if (((filterName != null) && (filterName.Length != 0)) && !string.Equals(filterName, "default", StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }
                    return true;
                }
            }
        }
    }
}

