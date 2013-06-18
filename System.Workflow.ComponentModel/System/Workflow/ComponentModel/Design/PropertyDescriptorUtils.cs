namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Reflection;
    using System.Workflow.ComponentModel;

    internal static class PropertyDescriptorUtils
    {
        internal static Type GetBaseType(PropertyDescriptor property, object owner, IServiceProvider serviceProvider)
        {
            Type propertyType = null;
            owner.GetType();
            if (owner != null)
            {
                IDynamicPropertyTypeProvider provider = owner as IDynamicPropertyTypeProvider;
                if (provider != null)
                {
                    propertyType = provider.GetPropertyType(serviceProvider, property.Name);
                }
            }
            if (propertyType == null)
            {
                propertyType = property.PropertyType;
            }
            return propertyType;
        }

        internal static IComponent GetComponent(ITypeDescriptorContext context)
        {
            ISite site = (context != null) ? GetSite(context, context.Instance) : null;
            if (site == null)
            {
                return null;
            }
            return site.Component;
        }

        internal static ISite GetSite(IServiceProvider serviceProvider, object component)
        {
            ISite site = null;
            if (component != null)
            {
                if ((component is IComponent) && (((IComponent) component).Site != null))
                {
                    site = ((IComponent) component).Site;
                }
                if (((site == null) && component.GetType().IsArray) && (((component as object[]).Length > 0) && ((component as object[])[0] is IComponent)))
                {
                    site = ((IComponent) (component as object[])[0]).Site;
                }
                if ((site == null) && (serviceProvider != null))
                {
                    IReferenceService service = serviceProvider.GetService(typeof(IReferenceService)) as IReferenceService;
                    if (service != null)
                    {
                        IComponent component2 = service.GetComponent(component);
                        if (component2 != null)
                        {
                            site = component2.Site;
                        }
                    }
                }
            }
            if (site == null)
            {
                site = serviceProvider as ISite;
            }
            return site;
        }

        internal static void SetPropertyValue(IServiceProvider serviceProvider, PropertyDescriptor propertyDescriptor, object component, object value)
        {
            ComponentChangeDispatcher dispatcher = new ComponentChangeDispatcher(serviceProvider, component, propertyDescriptor);
            try
            {
                propertyDescriptor.SetValue(component, value);
            }
            catch (Exception exception)
            {
                if ((exception is TargetInvocationException) && (exception.InnerException != null))
                {
                    throw exception.InnerException;
                }
                throw exception;
            }
            finally
            {
                dispatcher.Dispose();
            }
        }
    }
}

