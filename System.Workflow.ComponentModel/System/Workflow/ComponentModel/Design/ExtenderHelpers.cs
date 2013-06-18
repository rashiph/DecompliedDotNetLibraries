namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Workflow.ComponentModel;

    internal static class ExtenderHelpers
    {
        internal static void FilterDependencyProperties(IServiceProvider serviceProvider, Activity activity)
        {
            IExtenderListService service = serviceProvider.GetService(typeof(IExtenderListService)) as IExtenderListService;
            if (service != null)
            {
                Dictionary<string, DependencyProperty> dictionary = new Dictionary<string, DependencyProperty>();
                foreach (DependencyProperty property in activity.MetaDependencyProperties)
                {
                    dictionary.Add(property.Name, property);
                }
                List<string> list = new List<string>();
                foreach (IExtenderProvider provider in service.GetExtenderProviders())
                {
                    if (!provider.CanExtend(activity))
                    {
                        ProvidePropertyAttribute[] customAttributes = provider.GetType().GetCustomAttributes(typeof(ProvidePropertyAttribute), true) as ProvidePropertyAttribute[];
                        foreach (ProvidePropertyAttribute attribute in customAttributes)
                        {
                            list.Add(attribute.PropertyName);
                        }
                    }
                }
                foreach (string str in list)
                {
                    if (dictionary.ContainsKey(str))
                    {
                        activity.RemoveProperty(dictionary[str]);
                    }
                }
            }
        }
    }
}

