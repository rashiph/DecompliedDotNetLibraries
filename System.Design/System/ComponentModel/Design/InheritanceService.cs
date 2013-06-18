namespace System.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public class InheritanceService : IInheritanceService, IDisposable
    {
        private InheritanceAttribute addingAttribute;
        private IComponent addingComponent;
        private static TraceSwitch InheritanceServiceSwitch = new TraceSwitch("InheritanceService", "InheritanceService : Debug inheritance scan.");
        private Hashtable inheritedComponents = new Hashtable();

        public void AddInheritedComponents(IComponent component, IContainer container)
        {
            this.AddInheritedComponents(component.GetType(), component, container);
        }

        protected virtual void AddInheritedComponents(Type type, IComponent component, IContainer container)
        {
            if ((type != null) && typeof(IComponent).IsAssignableFrom(type))
            {
                ISite site = component.Site;
                IComponentChangeService service = null;
                INameCreationService service2 = null;
                if (site != null)
                {
                    service2 = (INameCreationService) site.GetService(typeof(INameCreationService));
                    service = (IComponentChangeService) site.GetService(typeof(IComponentChangeService));
                    if (service != null)
                    {
                        service.ComponentAdding += new ComponentEventHandler(this.OnComponentAdding);
                    }
                }
                try
                {
                    while (type != typeof(object))
                    {
                        Type reflectionType = TypeDescriptor.GetReflectionType(type);
                        foreach (FieldInfo info in reflectionType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                        {
                            string name = info.Name;
                            if (typeof(IComponent).IsAssignableFrom(info.FieldType))
                            {
                                object obj2 = info.GetValue(component);
                                if (obj2 != null)
                                {
                                    InheritanceAttribute inheritedReadOnly;
                                    MemberInfo member = info;
                                    object[] customAttributes = info.GetCustomAttributes(typeof(AccessedThroughPropertyAttribute), false);
                                    if ((customAttributes != null) && (customAttributes.Length > 0))
                                    {
                                        AccessedThroughPropertyAttribute attribute = (AccessedThroughPropertyAttribute) customAttributes[0];
                                        PropertyInfo property = reflectionType.GetProperty(attribute.PropertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                                        if ((property != null) && (property.PropertyType == info.FieldType))
                                        {
                                            if (!property.CanRead)
                                            {
                                                continue;
                                            }
                                            member = property.GetGetMethod(true);
                                            name = attribute.PropertyName;
                                        }
                                    }
                                    bool flag = this.IgnoreInheritedMember(member, component);
                                    bool flag2 = false;
                                    if (flag)
                                    {
                                        flag2 = true;
                                    }
                                    else if (member is FieldInfo)
                                    {
                                        FieldInfo info4 = (FieldInfo) member;
                                        flag2 = info4.IsPrivate | info4.IsAssembly;
                                    }
                                    else if (member is MethodInfo)
                                    {
                                        MethodInfo info5 = (MethodInfo) member;
                                        flag2 = info5.IsPrivate | info5.IsAssembly;
                                    }
                                    if (flag2)
                                    {
                                        inheritedReadOnly = InheritanceAttribute.InheritedReadOnly;
                                    }
                                    else
                                    {
                                        inheritedReadOnly = InheritanceAttribute.Inherited;
                                    }
                                    bool flag3 = this.inheritedComponents[obj2] == null;
                                    this.inheritedComponents[obj2] = inheritedReadOnly;
                                    if (!flag && flag3)
                                    {
                                        try
                                        {
                                            this.addingComponent = (IComponent) obj2;
                                            this.addingAttribute = inheritedReadOnly;
                                            if ((service2 == null) || service2.IsValidName(name))
                                            {
                                                try
                                                {
                                                    container.Add((IComponent) obj2, name);
                                                }
                                                catch
                                                {
                                                }
                                            }
                                        }
                                        finally
                                        {
                                            this.addingComponent = null;
                                            this.addingAttribute = null;
                                        }
                                    }
                                }
                            }
                        }
                        type = type.BaseType;
                    }
                }
                finally
                {
                    if (service != null)
                    {
                        service.ComponentAdding -= new ComponentEventHandler(this.OnComponentAdding);
                    }
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && (this.inheritedComponents != null))
            {
                this.inheritedComponents.Clear();
                this.inheritedComponents = null;
            }
        }

        public InheritanceAttribute GetInheritanceAttribute(IComponent component)
        {
            InheritanceAttribute attribute = (InheritanceAttribute) this.inheritedComponents[component];
            if (attribute == null)
            {
                attribute = InheritanceAttribute.Default;
            }
            return attribute;
        }

        protected virtual bool IgnoreInheritedMember(MemberInfo member, IComponent component)
        {
            if (member is FieldInfo)
            {
                FieldInfo info = (FieldInfo) member;
                if (!info.IsPrivate)
                {
                    return info.IsAssembly;
                }
                return true;
            }
            if (member is MethodInfo)
            {
                MethodInfo info2 = (MethodInfo) member;
                if (!info2.IsPrivate)
                {
                    return info2.IsAssembly;
                }
            }
            return true;
        }

        private void OnComponentAdding(object sender, ComponentEventArgs ce)
        {
            if ((this.addingComponent != null) && (this.addingComponent != ce.Component))
            {
                this.inheritedComponents[ce.Component] = InheritanceAttribute.InheritedReadOnly;
                INestedContainer container = sender as INestedContainer;
                if ((container != null) && (container.Owner == this.addingComponent))
                {
                    this.inheritedComponents[ce.Component] = this.addingAttribute;
                }
            }
        }
    }
}

