namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Workflow.Activities.Common;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    [ActivityDesignerTheme(typeof(InvokeMethodDesignerTheme))]
    internal class CallExternalMethodActivityDesigner : ActivityDesigner
    {
        private void AddRemoveCorrelationToken(Type interfaceType, IDictionary properties, object corrRefProperty)
        {
            if (interfaceType != null)
            {
                object[] customAttributes = interfaceType.GetCustomAttributes(typeof(CorrelationProviderAttribute), false);
                object[] objArray2 = interfaceType.GetCustomAttributes(typeof(CorrelationParameterAttribute), false);
                if ((customAttributes.Length != 0) || (objArray2.Length != 0))
                {
                    if (!properties.Contains("CorrelationToken"))
                    {
                        properties.Add("CorrelationToken", corrRefProperty);
                    }
                    return;
                }
            }
            if (properties.Contains("CorrelationToken"))
            {
                properties.Remove("CorrelationToken");
            }
        }

        protected override void OnActivityChanged(ActivityChangedEventArgs e)
        {
            base.OnActivityChanged(e);
            if (e.Member != null)
            {
                if ((e.Member.Name == "InterfaceType") && (base.Activity.Site != null))
                {
                    Type newValue = e.NewValue as Type;
                    if (newValue != null)
                    {
                        new ExternalDataExchangeInterfaceTypeFilterProvider(base.Activity.Site).CanFilterType(newValue, true);
                    }
                    Activity activity = e.Activity;
                    System.Workflow.Activities.Common.PropertyDescriptorUtils.SetPropertyValue(base.Activity.Site, TypeDescriptor.GetProperties(base.Activity)["MethodName"], base.Activity, string.Empty);
                    if (((IExtendedUIService) base.Activity.Site.GetService(typeof(IExtendedUIService))) == null)
                    {
                        throw new Exception(SR.GetString("General_MissingService", new object[] { typeof(IExtendedUIService).FullName }));
                    }
                }
                if ((e.Member.Name == "MethodName") && (e.Activity is CallExternalMethodActivity))
                {
                    (e.Activity as CallExternalMethodActivity).ParameterBindings.Clear();
                }
                if (((e.Member.Name == "InterfaceType") || (e.Member.Name == "MethodName")) || (e.Member.Name == "CorrelationToken"))
                {
                    TypeDescriptor.Refresh(e.Activity);
                }
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            CallExternalMethodActivity activity = base.Activity as CallExternalMethodActivity;
            object corrRefProperty = properties["CorrelationToken"];
            this.AddRemoveCorrelationToken(activity.InterfaceType, properties, corrRefProperty);
            Type interfaceType = activity.InterfaceType;
            if (interfaceType != null)
            {
                this.AddRemoveCorrelationToken(interfaceType, properties, corrRefProperty);
                activity.GetParameterPropertyDescriptors(properties);
            }
        }
    }
}

