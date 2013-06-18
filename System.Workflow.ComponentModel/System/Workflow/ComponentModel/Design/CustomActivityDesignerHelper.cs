namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Drawing.Design;
    using System.Reflection;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    internal static class CustomActivityDesignerHelper
    {
        private static void AddNewProperties(List<CustomProperty> propCollection, System.Type customActivityType, IServiceProvider serviceProvider, List<CustomProperty> existingProps)
        {
            IMemberCreationService service = serviceProvider.GetService(typeof(IMemberCreationService)) as IMemberCreationService;
            if (service == null)
            {
                throw new Exception(SR.GetString("General_MissingService", new object[] { typeof(IMemberCreationService).FullName }));
            }
            ITypeProvider provider = serviceProvider.GetService(typeof(ITypeProvider)) as ITypeProvider;
            if (provider == null)
            {
                throw new Exception(SR.GetString("General_MissingService", new object[] { typeof(ITypeProvider).FullName }));
            }
            foreach (CustomProperty property in propCollection)
            {
                bool flag = (property.oldPropertyName == null) || (property.oldPropertyType == null);
                if (!flag)
                {
                    if (!property.IsEvent)
                    {
                        flag = customActivityType.GetProperty(property.oldPropertyName, provider.GetType(property.oldPropertyType)) == null;
                    }
                    else
                    {
                        flag = customActivityType.GetEvent(property.oldPropertyName) == null;
                    }
                }
                if (flag)
                {
                    AttributeInfo[] attributes = CreateCustomPropertyAttributeArray(property, serviceProvider);
                    if (property.IsEvent)
                    {
                        service.CreateEvent(customActivityType.FullName, property.Name, provider.GetType(property.Type), attributes, property.GenerateDependencyProperty);
                    }
                    else
                    {
                        service.CreateProperty(customActivityType.FullName, property.Name, provider.GetType(property.Type), attributes, property.GenerateDependencyProperty, false, false, null, false);
                    }
                }
                else
                {
                    CustomProperty oldProperty = null;
                    foreach (CustomProperty property3 in existingProps)
                    {
                        if ((property3.Name == property.oldPropertyName) && (property3.Type == property.oldPropertyType))
                        {
                            oldProperty = property3;
                        }
                    }
                    if ((oldProperty == null) || ArePropertiesDifferent(property, oldProperty))
                    {
                        AttributeInfo[] infoArray2 = CreateCustomPropertyAttributeArray(property, serviceProvider);
                        CreateCustomPropertyAttributeArray(oldProperty, serviceProvider);
                        System.Type newEventType = provider.GetType(property.Type, false);
                        System.Type type = provider.GetType(property.oldPropertyType, false);
                        if (newEventType != null)
                        {
                            if (property.IsEvent)
                            {
                                service.UpdateEvent(customActivityType.FullName, property.oldPropertyName, type, property.Name, newEventType, infoArray2, property.GenerateDependencyProperty, false);
                            }
                            else
                            {
                                service.UpdateProperty(customActivityType.FullName, property.oldPropertyName, type, property.Name, newEventType, infoArray2, property.GenerateDependencyProperty, false);
                            }
                        }
                    }
                }
            }
        }

        private static bool ArePropertiesDifferent(CustomProperty property, CustomProperty oldProperty)
        {
            if ((((property.Name == oldProperty.Name) && (property.Type == oldProperty.Type)) && ((property.Browseable == oldProperty.Browseable) && (property.Category == oldProperty.Category))) && (((property.Description == oldProperty.Description) && (property.DesignerSerializationVisibility == oldProperty.DesignerSerializationVisibility)) && ((property.Hidden == oldProperty.Hidden) && (property.UITypeEditor == oldProperty.UITypeEditor))))
            {
                return false;
            }
            return true;
        }

        private static CustomProperty CreateCustomProperty(IServiceProvider serviceProvider, System.Type customActivityType, MemberInfo member, System.Type propertyType)
        {
            CustomProperty property = new CustomProperty(serviceProvider) {
                Name = member.Name,
                IsEvent = member is EventInfo
            };
            if (propertyType == typeof(ActivityBind))
            {
                property.GenerateDependencyProperty = false;
                property.Type = typeof(ActivityBind).FullName;
            }
            else
            {
                FieldInfo field = customActivityType.GetField(member.Name + (property.IsEvent ? "Event" : "Property"), BindingFlags.Public | BindingFlags.Static);
                if ((field != null) && (field.FieldType == typeof(DependencyProperty)))
                {
                    property.GenerateDependencyProperty = true;
                }
                else
                {
                    property.GenerateDependencyProperty = false;
                }
                property.Type = propertyType.FullName;
            }
            property.oldPropertyName = member.Name;
            property.oldPropertyType = propertyType.FullName;
            object[] customAttributes = member.GetCustomAttributes(typeof(FlagsAttribute), true);
            if ((customAttributes != null) && (customAttributes.Length > 0))
            {
                property.Hidden = true;
            }
            foreach (object obj2 in member.GetCustomAttributes(false))
            {
                AttributeInfoAttribute attribute = obj2 as AttributeInfoAttribute;
                AttributeInfo info2 = (attribute != null) ? attribute.AttributeInfo : null;
                if (info2 != null)
                {
                    try
                    {
                        if ((info2.AttributeType == typeof(BrowsableAttribute)) && (info2.ArgumentValues.Count > 0))
                        {
                            property.Browseable = (bool) info2.GetArgumentValueAs(serviceProvider, 0, typeof(bool));
                        }
                        else if ((info2.AttributeType == typeof(CategoryAttribute)) && (info2.ArgumentValues.Count > 0))
                        {
                            property.Category = info2.GetArgumentValueAs(serviceProvider, 0, typeof(string)) as string;
                        }
                        else if ((info2.AttributeType == typeof(DescriptionAttribute)) && (info2.ArgumentValues.Count > 0))
                        {
                            property.Description = info2.GetArgumentValueAs(serviceProvider, 0, typeof(string)) as string;
                        }
                        else if ((info2.AttributeType == typeof(DesignerSerializationVisibilityAttribute)) && (info2.ArgumentValues.Count > 0))
                        {
                            property.DesignerSerializationVisibility = (DesignerSerializationVisibility) info2.GetArgumentValueAs(serviceProvider, 0, typeof(DesignerSerializationVisibility));
                        }
                        else if ((info2.AttributeType == typeof(EditorAttribute)) && (info2.ArgumentValues.Count > 1))
                        {
                            System.Type type = info2.GetArgumentValueAs(serviceProvider, 1, typeof(System.Type)) as System.Type;
                            if (type == typeof(UITypeEditor))
                            {
                                System.Type type2 = info2.GetArgumentValueAs(serviceProvider, 0, typeof(System.Type)) as System.Type;
                                if (type2 != null)
                                {
                                    property.UITypeEditor = type2.FullName;
                                }
                                if (string.IsNullOrEmpty(property.UITypeEditor))
                                {
                                    property.UITypeEditor = info2.GetArgumentValueAs(serviceProvider, 0, typeof(string)) as string;
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
            return property;
        }

        private static AttributeInfo[] CreateCustomPropertyAttributeArray(CustomProperty property, IServiceProvider serviceProvider)
        {
            if ((property == null) || property.Hidden)
            {
                return new AttributeInfo[0];
            }
            List<AttributeInfo> list = new List<AttributeInfo>();
            if (property.Category != null)
            {
                list.Add(new AttributeInfo(typeof(CategoryAttribute), new string[0], new object[] { new CodePrimitiveExpression(property.Category) }));
            }
            if (property.Description != null)
            {
                list.Add(new AttributeInfo(typeof(DescriptionAttribute), new string[0], new object[] { new CodePrimitiveExpression(property.Description) }));
            }
            if (!string.IsNullOrEmpty(property.UITypeEditor))
            {
                list.Add(new AttributeInfo(typeof(EditorAttribute), new string[0], new object[] { new CodeTypeOfExpression(property.UITypeEditor), new CodeTypeOfExpression(typeof(UITypeEditor)) }));
            }
            list.Add(new AttributeInfo(typeof(BrowsableAttribute), new string[0], new object[] { new CodePrimitiveExpression(property.Browseable) }));
            list.Add(new AttributeInfo(typeof(DesignerSerializationVisibilityAttribute), new string[0], new object[] { new CodeSnippetExpression(typeof(DesignerSerializationVisibility).Name + "." + property.DesignerSerializationVisibility.ToString()) }));
            return list.ToArray();
        }

        public static System.Type GetCustomActivityType(IServiceProvider serviceProvider)
        {
            IDesignerHost service = serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (service == null)
            {
                throw new Exception(SR.GetString("General_MissingService", new object[] { typeof(IDesignerHost).FullName }));
            }
            string rootComponentClassName = service.RootComponentClassName;
            if (string.IsNullOrEmpty(rootComponentClassName))
            {
                return null;
            }
            ITypeProvider provider = serviceProvider.GetService(typeof(ITypeProvider)) as ITypeProvider;
            if (provider == null)
            {
                throw new Exception(SR.GetString("General_MissingService", new object[] { typeof(ITypeProvider).FullName }));
            }
            return provider.GetType(rootComponentClassName, false);
        }

        internal static List<CustomProperty> GetCustomProperties(IServiceProvider serviceProvider)
        {
            WorkflowDesignerLoader service = serviceProvider.GetService(typeof(IDesignerLoaderService)) as WorkflowDesignerLoader;
            if (service != null)
            {
                service.Flush();
            }
            System.Type customActivityType = GetCustomActivityType(serviceProvider);
            if (customActivityType == null)
            {
                return null;
            }
            List<CustomProperty> list = new List<CustomProperty>();
            foreach (PropertyInfo info in customActivityType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (info.PropertyType != null)
                {
                    list.Add(CreateCustomProperty(serviceProvider, customActivityType, info, info.PropertyType));
                }
            }
            foreach (EventInfo info2 in customActivityType.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (info2.EventHandlerType != null)
                {
                    CustomProperty item = CreateCustomProperty(serviceProvider, customActivityType, info2, info2.EventHandlerType);
                    item.IsEvent = true;
                    list.Add(item);
                }
            }
            return list;
        }

        private static void RemoveDeletedProperties(List<CustomProperty> propCollection, System.Type customActivityType, IServiceProvider serviceProvider)
        {
            IMemberCreationService service = serviceProvider.GetService(typeof(IMemberCreationService)) as IMemberCreationService;
            if (service == null)
            {
                throw new Exception(SR.GetString("General_MissingService", new object[] { typeof(IMemberCreationService).FullName }));
            }
            foreach (PropertyInfo info in customActivityType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                bool flag = false;
                foreach (CustomProperty property in propCollection)
                {
                    if ((info.Name == property.oldPropertyName) && (info.PropertyType.FullName == property.oldPropertyType))
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    service.RemoveProperty(customActivityType.FullName, info.Name, info.PropertyType);
                }
            }
            foreach (EventInfo info2 in customActivityType.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                bool flag2 = false;
                foreach (CustomProperty property2 in propCollection)
                {
                    if ((info2.Name == property2.oldPropertyName) && (info2.EventHandlerType.FullName == property2.oldPropertyType))
                    {
                        flag2 = true;
                        break;
                    }
                }
                if ((!flag2 && (info2.Name != null)) && (info2.EventHandlerType != null))
                {
                    service.RemoveEvent(customActivityType.FullName, info2.Name, info2.EventHandlerType);
                }
            }
        }

        public static void SetBaseTypeName(string typeName, IServiceProvider serviceProvider)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw new ArgumentNullException("typeName");
            }
            IDesignerHost host = serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (host == null)
            {
                throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(IDesignerHost).FullName }));
            }
            IMemberCreationService service = serviceProvider.GetService(typeof(IMemberCreationService)) as IMemberCreationService;
            if (service == null)
            {
                throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(IMemberCreationService).FullName }));
            }
            System.Type fromType = ValidateBaseType(typeName, serviceProvider);
            if (host.RootComponent.GetType() != fromType)
            {
                if (!TypeProvider.IsAssignable(typeof(CompositeActivity), fromType))
                {
                    PropertyDescriptor descriptor = TypeDescriptor.GetProperties(host.RootComponent)["SupportsEvents"];
                    if ((descriptor != null) && ((bool) descriptor.GetValue(host.RootComponent)))
                    {
                        descriptor.SetValue(host.RootComponent, false);
                    }
                    PropertyDescriptor descriptor2 = TypeDescriptor.GetProperties(host.RootComponent)["SupportsExceptions"];
                    if ((descriptor2 != null) && ((bool) descriptor2.GetValue(host.RootComponent)))
                    {
                        descriptor2.SetValue(host.RootComponent, false);
                    }
                }
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(host.RootComponent);
                if ((host.RootComponent is CompositeActivity) && (((CompositeActivity) host.RootComponent).Activities.Count > 0))
                {
                    IUIService service2 = serviceProvider.GetService(typeof(IUIService)) as IUIService;
                    if ((service2 != null) && (DialogResult.OK != service2.ShowMessage(SR.GetString("NoChildActivities_Message"), SR.GetString("NoChildActivities_Caption"), MessageBoxButtons.OKCancel)))
                    {
                        return;
                    }
                    List<Activity> list = new List<Activity>(((CompositeActivity) host.RootComponent).Activities);
                    CompositeActivityDesigner designer = host.GetDesigner(host.RootComponent) as CompositeActivityDesigner;
                    if (designer != null)
                    {
                        designer.RemoveActivities(list.AsReadOnly());
                    }
                }
                foreach (PropertyDescriptor descriptor3 in properties)
                {
                    if ((!descriptor3.Name.Equals("BaseActivityType", StringComparison.Ordinal) && !descriptor3.Name.Equals("Name", StringComparison.Ordinal)) && descriptor3.CanResetValue(host.RootComponent))
                    {
                        descriptor3.ResetValue(host.RootComponent);
                    }
                }
                PropertyDescriptor oldPropertyDescriptor = properties["BaseActivityType"];
                PropertyDescriptor member = TypeDescriptor.CreateProperty(oldPropertyDescriptor.ComponentType, oldPropertyDescriptor, new Attribute[] { DesignerSerializationVisibilityAttribute.Visible });
                IComponentChangeService service3 = serviceProvider.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                if (service3 != null)
                {
                    service3.OnComponentChanging(host.RootComponent, member);
                }
                ((Activity) host.RootComponent).UserData[UserDataKeys.NewBaseType] = fromType;
                service.UpdateBaseType(host.RootComponentClassName, fromType);
                if (service3 != null)
                {
                    service3.OnComponentChanged(host.RootComponent, member, member.GetValue(host.RootComponent), typeName);
                }
                Application.RaiseIdle(new EventArgs());
            }
        }

        internal static void SetCustomProperties(List<CustomProperty> customProperties, IServiceProvider serviceProvider)
        {
            if (customProperties == null)
            {
                throw new ArgumentNullException("customProperties");
            }
            System.Type customActivityType = GetCustomActivityType(serviceProvider);
            if (customActivityType != null)
            {
                List<CustomProperty> existingProps = GetCustomProperties(serviceProvider);
                RemoveDeletedProperties(customProperties, customActivityType, serviceProvider);
                AddNewProperties(customProperties, customActivityType, serviceProvider, existingProps);
            }
        }

        private static System.Type ValidateBaseType(string typeName, IServiceProvider serviceProvider)
        {
            if ((typeName == null) || (typeName.Length <= 0))
            {
                return null;
            }
            ITypeProvider service = (ITypeProvider) serviceProvider.GetService(typeof(ITypeProvider));
            if (service == null)
            {
                throw new Exception(SR.GetString("General_MissingService", new object[] { typeof(ITypeProvider).FullName }));
            }
            System.Type fromType = service.GetType(typeName);
            if (fromType == null)
            {
                throw new Exception(SR.GetString("Error_TypeNotResolved", new object[] { typeName }));
            }
            IDesignerHost host = serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (host == null)
            {
                throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(IDesignerHost).FullName }));
            }
            System.Type type = service.GetType(host.RootComponentClassName);
            if (((fromType is DesignTimeType) && (type != null)) && (type.Assembly == fromType.Assembly))
            {
                throw new InvalidOperationException(SR.GetString("Error_CantUseCurrentProjectTypeAsBase"));
            }
            if (!TypeProvider.IsAssignable(typeof(Activity), fromType))
            {
                throw new InvalidOperationException(SR.GetString("Error_BaseTypeMustBeActivity"));
            }
            return fromType;
        }
    }
}

