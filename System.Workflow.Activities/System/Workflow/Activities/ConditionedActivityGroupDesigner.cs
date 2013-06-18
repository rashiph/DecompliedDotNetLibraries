namespace System.Workflow.Activities
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    [ActivityDesignerTheme(typeof(ConditionedActivityGroupDesignerTheme))]
    internal sealed class ConditionedActivityGroupDesigner : ActivityPreviewDesigner
    {
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        protected override void Initialize(Activity activity)
        {
            base.Initialize(activity);
            IExtenderListService service = (IExtenderListService) base.GetService(typeof(IExtenderListService));
            if (service != null)
            {
                bool flag = false;
                foreach (IExtenderProvider provider in service.GetExtenderProviders())
                {
                    if (provider.GetType() == typeof(ConditionPropertyProviderExtender))
                    {
                        flag = true;
                    }
                }
                if (!flag)
                {
                    IExtenderProviderService service2 = (IExtenderProviderService) base.GetService(typeof(IExtenderProviderService));
                    if (service2 != null)
                    {
                        service2.AddExtenderProvider(new ConditionPropertyProviderExtender());
                    }
                }
            }
        }
    }
}

