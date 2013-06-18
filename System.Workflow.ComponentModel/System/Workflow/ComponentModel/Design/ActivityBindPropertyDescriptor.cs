namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Drawing.Design;
    using System.Reflection;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    internal class ActivityBindPropertyDescriptor : DynamicPropertyDescriptor
    {
        private object propertyOwner;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal ActivityBindPropertyDescriptor(IServiceProvider serviceProvider, PropertyDescriptor realPropertyDescriptor, object propertyOwner) : base(serviceProvider, realPropertyDescriptor)
        {
            this.propertyOwner = propertyOwner;
        }

        internal static bool CreateField(ITypeDescriptorContext context, ActivityBind activityBind, bool throwOnError)
        {
            if (!string.IsNullOrEmpty(activityBind.Path))
            {
                Type toType = PropertyDescriptorUtils.GetBaseType(context.PropertyDescriptor, context.Instance, context);
                Activity component = PropertyDescriptorUtils.GetComponent(context) as Activity;
                if ((component != null) && (toType != null))
                {
                    component = Helpers.ParseActivityForBind(component, activityBind.Name);
                    if (component == Helpers.GetRootActivity(component))
                    {
                        bool ignoreCase = CompilerHelpers.GetSupportedLanguage(context) == SupportedLanguages.VB;
                        Type dataSourceClass = Helpers.GetDataSourceClass(component, context);
                        if (dataSourceClass != null)
                        {
                            string path = activityBind.Path;
                            int length = path.IndexOfAny(new char[] { '.', '/', '[' });
                            if (length != -1)
                            {
                                path = path.Substring(0, length);
                            }
                            MemberInfo info = FindMatchingMember(path, dataSourceClass, ignoreCase);
                            if (info != null)
                            {
                                Type fromType = null;
                                bool isPrivate = false;
                                if (info is FieldInfo)
                                {
                                    isPrivate = ((FieldInfo) info).IsPrivate;
                                    fromType = ((FieldInfo) info).FieldType;
                                }
                                else if (info is PropertyInfo)
                                {
                                    MethodInfo getMethod = ((PropertyInfo) info).GetGetMethod();
                                    MethodInfo setMethod = ((PropertyInfo) info).GetSetMethod();
                                    isPrivate = ((getMethod != null) && getMethod.IsPrivate) || ((setMethod != null) && setMethod.IsPrivate);
                                }
                                else if (info is MethodInfo)
                                {
                                    isPrivate = ((MethodInfo) info).IsPrivate;
                                }
                                if (length != -1)
                                {
                                    PathWalker walker = new PathWalker();
                                    PathMemberInfoEventArgs finalEventArgs = null;
                                    walker.MemberFound = (EventHandler<PathMemberInfoEventArgs>) Delegate.Combine(walker.MemberFound, delegate (object sender, PathMemberInfoEventArgs eventArgs) {
                                        finalEventArgs = eventArgs;
                                    });
                                    if (!walker.TryWalkPropertyPath(dataSourceClass, activityBind.Path))
                                    {
                                        if (throwOnError)
                                        {
                                            throw new InvalidOperationException(SR.GetString("Error_MemberWithSameNameExists", new object[] { activityBind.Path, dataSourceClass.FullName }));
                                        }
                                        return false;
                                    }
                                    fromType = BindHelpers.GetMemberType(finalEventArgs.MemberInfo);
                                }
                                if (((info.DeclaringType == dataSourceClass) || !isPrivate) && ((info is FieldInfo) && TypeProvider.IsAssignable(toType, fromType)))
                                {
                                    return true;
                                }
                                if (throwOnError)
                                {
                                    throw new InvalidOperationException(SR.GetString("Error_MemberWithSameNameExists", new object[] { activityBind.Path, dataSourceClass.FullName }));
                                }
                                return false;
                            }
                            Activity activity2 = null;
                            if (string.Compare(component.Name, path, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0)
                            {
                                activity2 = component;
                            }
                            else if ((component is CompositeActivity) && (component is CompositeActivity))
                            {
                                foreach (Activity activity3 in Helpers.GetAllNestedActivities(component as CompositeActivity))
                                {
                                    if (string.Compare(activity3.Name, path, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0)
                                    {
                                        activity2 = activity3;
                                    }
                                }
                            }
                            if (activity2 != null)
                            {
                                if (TypeProvider.IsAssignable(toType, activity2.GetType()))
                                {
                                    return true;
                                }
                                if (throwOnError)
                                {
                                    throw new InvalidOperationException(SR.GetString("Error_MemberWithSameNameExists", new object[] { activityBind.Path, dataSourceClass.FullName }));
                                }
                                return false;
                            }
                            IMemberCreationService service = context.GetService(typeof(IMemberCreationService)) as IMemberCreationService;
                            if (service != null)
                            {
                                IDesignerHost host = context.GetService(typeof(IDesignerHost)) as IDesignerHost;
                                if (host != null)
                                {
                                    service.CreateField(host.RootComponentClassName, activityBind.Path, toType, null, MemberAttributes.Public, null, false);
                                    return true;
                                }
                                if (throwOnError)
                                {
                                    throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(IDesignerHost).FullName }));
                                }
                            }
                            else if (throwOnError)
                            {
                                throw new InvalidOperationException(SR.GetString("General_MissingService", new object[] { typeof(IMemberCreationService).FullName }));
                            }
                        }
                    }
                }
                else
                {
                    if ((component == null) && throwOnError)
                    {
                        throw new InvalidOperationException(SR.GetString("Error_InvalidActivityIdentifier", new object[] { activityBind.Name }));
                    }
                    if ((toType == null) && throwOnError)
                    {
                        throw new InvalidOperationException(SR.GetString("Error_PropertyTypeNotDefined", new object[] { context.PropertyDescriptor.Name, typeof(ActivityBind).Name, typeof(IDynamicPropertyTypeProvider).Name }));
                    }
                }
            }
            return false;
        }

        internal static MemberInfo FindMatchingMember(string name, Type ownerType, bool ignoreCase)
        {
            foreach (MemberInfo info2 in ownerType.GetMembers(BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
            {
                if (info2.Name.Equals(name, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                {
                    return info2;
                }
            }
            return null;
        }

        internal static IList<MemberInfo> GetBindableMembers(object obj, ITypeDescriptorContext context)
        {
            List<MemberInfo> list = new List<MemberInfo>();
            IDesignerHost service = context.GetService(typeof(IDesignerHost)) as IDesignerHost;
            Activity activity = (service != null) ? (service.RootComponent as Activity) : null;
            Type type = (obj == activity) ? Helpers.GetDataSourceClass(activity, context) : obj.GetType();
            Type toType = PropertyDescriptorUtils.GetBaseType(context.PropertyDescriptor, context.Instance, context);
            if ((type != null) && (toType != null))
            {
                DependencyProperty property = DependencyProperty.FromName(context.PropertyDescriptor.Name, context.PropertyDescriptor.ComponentType);
                bool flag = (property != null) && property.IsEvent;
                BindingFlags bindingAttr = BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
                if (obj == activity)
                {
                    bindingAttr |= BindingFlags.NonPublic;
                }
                foreach (MemberInfo info in type.GetMembers(bindingAttr))
                {
                    object[] customAttributes = info.GetCustomAttributes(typeof(DebuggerNonUserCodeAttribute), false);
                    if (((customAttributes == null) || (customAttributes.Length <= 0)) || !(customAttributes[0] is DebuggerNonUserCodeAttribute))
                    {
                        object[] objArray2 = info.GetCustomAttributes(typeof(BrowsableAttribute), false);
                        if (objArray2.Length > 0)
                        {
                            bool browsable = false;
                            BrowsableAttribute attribute = objArray2[0] as BrowsableAttribute;
                            if (attribute != null)
                            {
                                browsable = attribute.Browsable;
                            }
                            else
                            {
                                try
                                {
                                    AttributeInfoAttribute attribute2 = objArray2[0] as AttributeInfoAttribute;
                                    if ((attribute2 != null) && (attribute2.AttributeInfo.ArgumentValues.Count > 0))
                                    {
                                        browsable = (bool) attribute2.AttributeInfo.GetArgumentValueAs(context, 0, typeof(bool));
                                    }
                                }
                                catch
                                {
                                }
                            }
                            if (!browsable)
                            {
                                continue;
                            }
                        }
                        if ((info.DeclaringType != typeof(object)) || (!string.Equals(info.Name, "Equals", StringComparison.Ordinal) && !string.Equals(info.Name, "ReferenceEquals", StringComparison.Ordinal)))
                        {
                            bool flag3 = false;
                            bool flag4 = false;
                            bool isAssembly = false;
                            if (flag && (info is EventInfo))
                            {
                                EventInfo info2 = info as EventInfo;
                                MethodInfo addMethod = info2.GetAddMethod();
                                MethodInfo removeMethod = info2.GetRemoveMethod();
                                flag4 = ((((addMethod != null) && addMethod.IsFamily) || ((removeMethod != null) && removeMethod.IsFamily)) || ((addMethod != null) && addMethod.IsPublic)) || ((removeMethod != null) && removeMethod.IsPublic);
                                isAssembly = ((addMethod != null) && addMethod.IsAssembly) || ((removeMethod != null) && removeMethod.IsAssembly);
                                flag3 = TypeProvider.IsAssignable(toType, info2.EventHandlerType);
                            }
                            else if (info is FieldInfo)
                            {
                                FieldInfo info5 = info as FieldInfo;
                                flag4 = info5.IsFamily || info5.IsPublic;
                                isAssembly = info5.IsAssembly;
                                flag3 = TypeProvider.IsAssignable(toType, info5.FieldType);
                            }
                            else if (info is PropertyInfo)
                            {
                                PropertyInfo info6 = info as PropertyInfo;
                                MethodInfo getMethod = info6.GetGetMethod();
                                MethodInfo setMethod = info6.GetSetMethod();
                                flag4 = ((((getMethod != null) && getMethod.IsFamily) || ((setMethod != null) && setMethod.IsFamily)) || ((getMethod != null) && getMethod.IsPublic)) || ((setMethod != null) && setMethod.IsPublic);
                                isAssembly = ((getMethod != null) && getMethod.IsAssembly) || ((setMethod != null) && setMethod.IsAssembly);
                                flag3 = (getMethod != null) && TypeProvider.IsAssignable(toType, info6.PropertyType);
                            }
                            if (((info.DeclaringType != type) && !flag4) && ((info.DeclaringType.Assembly != null) || !isAssembly))
                            {
                                flag3 = false;
                            }
                            if (flag3)
                            {
                                list.Add(info);
                            }
                        }
                    }
                }
            }
            return list.AsReadOnly();
        }

        public override object GetEditor(Type editorBaseType)
        {
            object editor = base.GetEditor(editorBaseType);
            if (!(editorBaseType == typeof(UITypeEditor)) || this.IsReadOnly)
            {
                return editor;
            }
            object obj3 = (this.PropertyOwner != null) ? this.GetValue(this.PropertyOwner) : null;
            bool propertiesSupported = base.RealPropertyDescriptor.Converter.GetPropertiesSupported((this.PropertyOwner != null) ? new System.Workflow.ComponentModel.Design.TypeDescriptorContext(base.ServiceProvider, base.RealPropertyDescriptor, this.PropertyOwner) : null);
            if (!(obj3 is ActivityBind) && ((editor != null) || propertiesSupported))
            {
                return editor;
            }
            return new BindUITypeEditor();
        }

        public override object GetValue(object component)
        {
            object binding = null;
            DependencyObject obj3 = component as DependencyObject;
            DependencyProperty dependencyProperty = DependencyProperty.FromName(this.Name, this.ComponentType);
            if (((obj3 != null) && (dependencyProperty != null)) && obj3.IsBindingSet(dependencyProperty))
            {
                binding = obj3.GetBinding(dependencyProperty);
            }
            if (!(binding is ActivityBind))
            {
                binding = base.GetValue(component);
            }
            return binding;
        }

        internal static bool IsBindableProperty(PropertyDescriptor propertyDescriptor)
        {
            if (propertyDescriptor.PropertyType == typeof(ActivityBind))
            {
                return true;
            }
            if (propertyDescriptor.Converter is ActivityBindTypeConverter)
            {
                return true;
            }
            DependencyProperty property = DependencyProperty.FromName(propertyDescriptor.Name, propertyDescriptor.ComponentType);
            return ((typeof(DependencyObject).IsAssignableFrom(propertyDescriptor.ComponentType) && (property != null)) && !property.DefaultMetadata.IsMetaProperty);
        }

        public override void SetValue(object component, object value)
        {
            object obj2 = this.GetValue(component);
            ActivityBind bind = value as ActivityBind;
            DependencyObject obj3 = component as DependencyObject;
            DependencyProperty dependencyProperty = DependencyProperty.FromName(this.Name, this.ComponentType);
            if (((obj3 != null) && (dependencyProperty != null)) && (bind != null))
            {
                using (new ComponentChangeDispatcher(base.ServiceProvider, obj3, this))
                {
                    if (dependencyProperty.IsEvent && (base.ServiceProvider != null))
                    {
                        IEventBindingService service = base.ServiceProvider.GetService(typeof(IEventBindingService)) as IEventBindingService;
                        if ((service != null) && (service.GetEvent(base.RealPropertyDescriptor) != null))
                        {
                            base.RealPropertyDescriptor.SetValue(component, null);
                        }
                    }
                    obj3.SetBinding(dependencyProperty, bind);
                    base.OnValueChanged(obj3, EventArgs.Empty);
                    goto Label_00F8;
                }
            }
            if (((obj3 != null) && (dependencyProperty != null)) && obj3.IsBindingSet(dependencyProperty))
            {
                using (new ComponentChangeDispatcher(base.ServiceProvider, obj3, this))
                {
                    obj3.RemoveProperty(dependencyProperty);
                    base.OnValueChanged(obj3, EventArgs.Empty);
                }
            }
            base.SetValue(component, value);
        Label_00F8:
            if ((obj2 != value) && (((obj2 is ActivityBind) && !(value is ActivityBind)) || (!(obj2 is ActivityBind) && (value is ActivityBind))))
            {
                TypeDescriptor.Refresh(component);
            }
        }

        public override AttributeCollection Attributes
        {
            get
            {
                List<Attribute> list = new List<Attribute>();
                foreach (Attribute attribute in base.Attributes)
                {
                    list.Add(attribute);
                }
                object editor = base.RealPropertyDescriptor.GetEditor(typeof(UITypeEditor));
                object obj3 = (this.PropertyOwner != null) ? this.GetValue(this.PropertyOwner) : null;
                bool propertiesSupported = base.RealPropertyDescriptor.Converter.GetPropertiesSupported((this.PropertyOwner != null) ? new System.Workflow.ComponentModel.Design.TypeDescriptorContext(base.ServiceProvider, base.RealPropertyDescriptor, this.PropertyOwner) : null);
                if ((((editor == null) && !propertiesSupported) || (obj3 is ActivityBind)) && !this.IsReadOnly)
                {
                    list.Add(new EditorAttribute(typeof(BindUITypeEditor), typeof(UITypeEditor)));
                }
                return new AttributeCollection(list.ToArray());
            }
        }

        public override TypeConverter Converter
        {
            get
            {
                TypeConverter converter = base.Converter;
                if (typeof(ActivityBindTypeConverter).IsAssignableFrom(converter.GetType()))
                {
                    return converter;
                }
                return new ActivityBindTypeConverter();
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return base.RealPropertyDescriptor.IsReadOnly;
            }
        }

        internal object PropertyOwner
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.propertyOwner;
            }
        }
    }
}

