namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    [ActivityDesignerTheme(typeof(WebServiceDesignerTheme))]
    internal sealed class WebServiceReceiveDesigner : ActivityDesigner
    {
        protected override void OnActivityChanged(ActivityChangedEventArgs e)
        {
            base.OnActivityChanged(e);
            if (e.Member != null)
            {
                if (e.Member.Name == "InterfaceType")
                {
                    if (base.Activity.Site != null)
                    {
                        Type newValue = e.NewValue as Type;
                        if (newValue != null)
                        {
                            new InterfaceTypeFilterProvider(base.Activity.Site).CanFilterType(newValue, true);
                        }
                        Activity activity1 = e.Activity;
                        PropertyDescriptor descriptor = TypeDescriptor.GetProperties(base.Activity)["MethodName"];
                        if (descriptor != null)
                        {
                            descriptor.SetValue(base.Activity, string.Empty);
                        }
                    }
                }
                else if (e.Member.Name == "MethodName")
                {
                    (e.Activity as WebServiceInputActivity).ParameterBindings.Clear();
                }
                if ((e.Member.Name == "InterfaceType") || (e.Member.Name == "MethodName"))
                {
                    TypeDescriptor.Refresh(e.Activity);
                }
                foreach (Activity activity in WebServiceActivityHelpers.GetSucceedingActivities(base.Activity))
                {
                    if ((activity is WebServiceOutputActivity) && (((WebServiceOutputActivity) activity).InputActivityName == base.Activity.QualifiedName))
                    {
                        TypeDescriptor.Refresh(activity);
                    }
                }
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            (base.Activity as WebServiceInputActivity).GetParameterPropertyDescriptors(properties);
            if (properties.Contains("InterfaceType"))
            {
                properties["InterfaceType"] = new WebServiceInterfacePropertyDescriptor(base.Activity.Site, properties["InterfaceType"] as PropertyDescriptor);
            }
        }
    }
}

