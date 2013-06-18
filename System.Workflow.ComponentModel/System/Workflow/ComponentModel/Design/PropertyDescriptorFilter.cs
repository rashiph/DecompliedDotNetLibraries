namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Workflow.ComponentModel;

    internal static class PropertyDescriptorFilter
    {
        internal static void FilterProperties(IServiceProvider serviceProvider, object propertyOwner, IDictionary props)
        {
            InternalFilterProperties(serviceProvider, propertyOwner, props);
            if (propertyOwner != null)
            {
                foreach (PropertyDescriptor descriptor in GetPropertiesForEvents(serviceProvider, propertyOwner))
                {
                    if (!props.Contains(descriptor.Name))
                    {
                        props.Add(descriptor.Name, descriptor);
                    }
                }
            }
        }

        internal static PropertyDescriptorCollection FilterProperties(IServiceProvider serviceProvider, object propertyOwner, PropertyDescriptorCollection props)
        {
            Hashtable hashtable = new Hashtable();
            foreach (PropertyDescriptor descriptor in props)
            {
                if (!hashtable.ContainsKey(descriptor.Name))
                {
                    hashtable.Add(descriptor.Name, descriptor);
                }
            }
            FilterProperties(serviceProvider, propertyOwner, hashtable);
            PropertyDescriptor[] array = new PropertyDescriptor[hashtable.Count];
            hashtable.Values.CopyTo(array, 0);
            return new PropertyDescriptorCollection(array);
        }

        internal static PropertyDescriptorCollection GetPropertiesForEvents(IServiceProvider serviceProvider, object eventOwner)
        {
            List<PropertyDescriptor> list = new List<PropertyDescriptor>();
            IEventBindingService service = serviceProvider.GetService(typeof(IEventBindingService)) as IEventBindingService;
            if (service != null)
            {
                foreach (EventDescriptor descriptor in TypeDescriptor.GetEvents(eventOwner))
                {
                    if (descriptor.IsBrowsable)
                    {
                        PropertyDescriptor eventProperty = service.GetEventProperty(descriptor);
                        if (!(eventProperty is ActivityBindPropertyDescriptor) && ActivityBindPropertyDescriptor.IsBindableProperty(eventProperty))
                        {
                            list.Add(new ActivityBindPropertyDescriptor(serviceProvider, eventProperty, eventOwner));
                        }
                        else
                        {
                            list.Add(eventProperty);
                        }
                    }
                }
            }
            return new PropertyDescriptorCollection(list.ToArray());
        }

        private static void InternalFilterProperties(IServiceProvider serviceProvider, object propertyOwner, IDictionary properties)
        {
            Hashtable hashtable = new Hashtable();
            foreach (object obj2 in properties.Keys)
            {
                PropertyDescriptor actualPropDesc = properties[obj2] as PropertyDescriptor;
                if (string.Equals(actualPropDesc.Name, "Name", StringComparison.Ordinal) && typeof(Activity).IsAssignableFrom(actualPropDesc.ComponentType))
                {
                    Activity activity = propertyOwner as Activity;
                    if ((activity != null) && (activity == Helpers.GetRootActivity(activity)))
                    {
                        hashtable[obj2] = new NamePropertyDescriptor(serviceProvider, actualPropDesc);
                    }
                    else
                    {
                        hashtable[obj2] = new IDPropertyDescriptor(serviceProvider, actualPropDesc);
                    }
                }
                else if (!(actualPropDesc is ActivityBindPropertyDescriptor) && ActivityBindPropertyDescriptor.IsBindableProperty(actualPropDesc))
                {
                    if (typeof(Type).IsAssignableFrom(actualPropDesc.PropertyType) && !(actualPropDesc is ParameterInfoBasedPropertyDescriptor))
                    {
                        actualPropDesc = new TypePropertyDescriptor(serviceProvider, actualPropDesc);
                    }
                    hashtable[obj2] = new ActivityBindPropertyDescriptor(serviceProvider, actualPropDesc, propertyOwner);
                }
                else if (typeof(Type).IsAssignableFrom(actualPropDesc.PropertyType))
                {
                    hashtable[obj2] = new TypePropertyDescriptor(serviceProvider, actualPropDesc);
                }
                else
                {
                    hashtable[obj2] = new DynamicPropertyDescriptor(serviceProvider, actualPropDesc);
                }
            }
            foreach (object obj3 in hashtable.Keys)
            {
                properties[obj3] = hashtable[obj3];
            }
        }
    }
}

