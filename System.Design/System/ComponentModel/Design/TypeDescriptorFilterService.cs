namespace System.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;

    internal sealed class TypeDescriptorFilterService : ITypeDescriptorFilterService
    {
        internal TypeDescriptorFilterService()
        {
        }

        private IDesigner GetDesigner(IComponent component)
        {
            ISite site = component.Site;
            if (site != null)
            {
                IDesignerHost service = site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (service != null)
                {
                    return service.GetDesigner(component);
                }
            }
            return null;
        }

        bool ITypeDescriptorFilterService.FilterAttributes(IComponent component, IDictionary attributes)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            if (attributes == null)
            {
                throw new ArgumentNullException("attributes");
            }
            IDesigner designer = this.GetDesigner(component);
            if (designer is IDesignerFilter)
            {
                ((IDesignerFilter) designer).PreFilterAttributes(attributes);
                ((IDesignerFilter) designer).PostFilterAttributes(attributes);
            }
            return (designer != null);
        }

        bool ITypeDescriptorFilterService.FilterEvents(IComponent component, IDictionary events)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            if (events == null)
            {
                throw new ArgumentNullException("events");
            }
            IDesigner designer = this.GetDesigner(component);
            if (designer is IDesignerFilter)
            {
                ((IDesignerFilter) designer).PreFilterEvents(events);
                ((IDesignerFilter) designer).PostFilterEvents(events);
            }
            return (designer != null);
        }

        bool ITypeDescriptorFilterService.FilterProperties(IComponent component, IDictionary properties)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            if (properties == null)
            {
                throw new ArgumentNullException("properties");
            }
            IDesigner designer = this.GetDesigner(component);
            if (designer is IDesignerFilter)
            {
                ((IDesignerFilter) designer).PreFilterProperties(properties);
                ((IDesignerFilter) designer).PostFilterProperties(properties);
            }
            return (designer != null);
        }
    }
}

