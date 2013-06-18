namespace System.Web.UI.Design
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Web.UI;

    internal static class ControlHelper
    {
        internal static Control FindControl(IServiceProvider serviceProvider, Control control, string controlIdToFind)
        {
            if (string.IsNullOrEmpty(controlIdToFind))
            {
                throw new ArgumentNullException("controlIdToFind");
            }
            while (control != null)
            {
                if ((control.Site == null) || (control.Site.Container == null))
                {
                    return null;
                }
                IComponent component = control.Site.Container.Components[controlIdToFind];
                if (component != null)
                {
                    return (component as Control);
                }
                IDesignerHost service = (IDesignerHost) control.Site.GetService(typeof(IDesignerHost));
                if (service == null)
                {
                    return null;
                }
                ControlDesigner designer = service.GetDesigner(control) as ControlDesigner;
                if (((designer == null) || (designer.View == null)) || (designer.View.NamingContainerDesigner == null))
                {
                    return null;
                }
                control = designer.View.NamingContainerDesigner.Component as Control;
            }
            if (serviceProvider != null)
            {
                IDesignerHost host2 = (IDesignerHost) serviceProvider.GetService(typeof(IDesignerHost));
                if (host2 != null)
                {
                    IContainer container = host2.Container;
                    if (container != null)
                    {
                        return (container.Components[controlIdToFind] as Control);
                    }
                }
            }
            return null;
        }

        internal static IList<IComponent> GetAllComponents(IComponent component, IsValidComponentDelegate componentFilter)
        {
            List<IComponent> list = new List<IComponent>();
            while (component != null)
            {
                IList<IComponent> componentsInContainer = GetComponentsInContainer(component, componentFilter);
                list.AddRange(componentsInContainer);
                IDesignerHost service = (IDesignerHost) component.Site.GetService(typeof(IDesignerHost));
                ControlDesigner designer = service.GetDesigner(component) as ControlDesigner;
                component = null;
                if (((designer != null) && (designer.View != null)) && (designer.View.NamingContainerDesigner != null))
                {
                    component = designer.View.NamingContainerDesigner.Component;
                }
            }
            return list;
        }

        private static IList<IComponent> GetComponentsInContainer(IComponent component, IsValidComponentDelegate componentFilter)
        {
            List<IComponent> list = new List<IComponent>();
            if ((component.Site != null) && (component.Site.Container != null))
            {
                foreach (IComponent component2 in component.Site.Container.Components)
                {
                    if (componentFilter(component2) && !Marshal.IsComObject(component2))
                    {
                        PropertyDescriptor descriptor = TypeDescriptor.GetProperties(component2)["Modifiers"];
                        if (descriptor != null)
                        {
                            MemberAttributes attributes = (MemberAttributes) descriptor.GetValue(component2);
                            if ((attributes & MemberAttributes.AccessMask) == MemberAttributes.Private)
                            {
                                continue;
                            }
                        }
                        list.Add(component2);
                    }
                }
            }
            return list;
        }

        internal delegate bool IsValidComponentDelegate(IComponent component);
    }
}

