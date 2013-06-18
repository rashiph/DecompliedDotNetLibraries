namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Workflow.ComponentModel;

    [ProvideProperty("BaseActivityType", typeof(Activity))]
    internal sealed class CustomActivityPropertyExtender : IExtenderProvider
    {
        [DefaultValue("System.Workflow.ComponentModel.Sequence"), DesignOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), TypeFilterProvider(typeof(BaseClassTypeFilterProvider)), SRCategory("Activity"), Editor(typeof(TypeBrowserEditor), typeof(UITypeEditor)), SRDescription("CustomActivityBaseTypeDesc"), SRDisplayName("BaseActivityType")]
        public string GetBaseActivityType(Activity activity)
        {
            return activity.GetType().FullName;
        }

        public void SetBaseActivityType(Activity activity, string baseActivityTypeName)
        {
            CustomActivityDesignerHelper.SetBaseTypeName(baseActivityTypeName, activity.Site);
        }

        bool IExtenderProvider.CanExtend(object extendee)
        {
            bool flag = false;
            Activity activity = extendee as Activity;
            if (((activity != null) && (activity.Site != null)) && (activity == Helpers.GetRootActivity(activity)))
            {
                ActivityDesigner designer = ActivityDesigner.GetDesigner(activity);
                if ((designer != null) && (designer.ParentDesigner == null))
                {
                    flag = true;
                }
            }
            return flag;
        }
    }
}

