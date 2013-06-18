namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    internal sealed class IDPropertyDescriptor : DynamicPropertyDescriptor
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal IDPropertyDescriptor(IServiceProvider serviceProvider, PropertyDescriptor actualPropDesc) : base(serviceProvider, actualPropDesc)
        {
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override void SetValue(object component, object value)
        {
            Activity activity = component as Activity;
            if (activity != null)
            {
                ISite site = PropertyDescriptorUtils.GetSite(base.ServiceProvider, component);
                if (site == null)
                {
                    throw new Exception(SR.GetString("General_MissingService", new object[] { typeof(ISite).FullName }));
                }
                IIdentifierCreationService service = site.GetService(typeof(IIdentifierCreationService)) as IIdentifierCreationService;
                if (service == null)
                {
                    throw new Exception(SR.GetString("General_MissingService", new object[] { typeof(IIdentifierCreationService).FullName }));
                }
                string identifier = value as string;
                service.ValidateIdentifier(activity, identifier);
                DesignerHelpers.UpdateSiteName(activity, identifier);
                base.SetValue(component, value);
            }
        }
    }
}

