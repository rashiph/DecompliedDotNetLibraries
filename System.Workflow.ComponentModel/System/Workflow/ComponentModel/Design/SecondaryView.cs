namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Design;
    using System.Windows.Forms.Design;
    using System.Workflow.ComponentModel;

    internal sealed class SecondaryView : DesignerView
    {
        private StructuredCompositeActivityDesigner parentDesigner;
        internal static readonly Guid UserDataKey_ActivityType = new Guid("03C4103A-D6E9-46e9-B98E-149E145EC2C9");
        internal static readonly Guid UserDataKey_Designer = new Guid("2B72C7F7-DE4A-4e32-8EB4-9E1ED1C5E84E");

        internal SecondaryView(StructuredCompositeActivityDesigner parentDesigner, int id, string text, Type activityType) : base(id, text, ActivityToolboxItem.GetToolboxImage(activityType))
        {
            this.parentDesigner = parentDesigner;
            base.UserData[UserDataKey_ActivityType] = activityType;
            if (this.parentDesigner.Activity.GetType() == activityType)
            {
                base.UserData[UserDataKey_Designer] = this.parentDesigner;
            }
        }

        public override void OnActivate()
        {
            if (this.AssociatedDesigner == null)
            {
                Type type = base.UserData[UserDataKey_ActivityType] as Type;
                CompositeActivity activity = this.parentDesigner.Activity as CompositeActivity;
                if (((type != null) && (activity != null)) && this.parentDesigner.IsEditable)
                {
                    Activity activity2 = Activator.CreateInstance(type) as Activity;
                    try
                    {
                        CompositeActivityDesigner.InsertActivities(this.parentDesigner, new System.Workflow.ComponentModel.Design.HitTestInfo(this.parentDesigner, HitTestLocations.Designer), new List<Activity>(new Activity[] { activity2 }).AsReadOnly(), SR.GetString("AddingImplicitActivity"));
                    }
                    catch (Exception exception)
                    {
                        if (exception != CheckoutException.Canceled)
                        {
                            IUIService service = this.parentDesigner.Activity.Site.GetService(typeof(IUIService)) as IUIService;
                            if (service != null)
                            {
                                service.ShowError(exception.Message);
                            }
                        }
                    }
                    ActivityDesigner designer = ActivityDesigner.GetDesigner(activity2);
                    base.UserData[UserDataKey_Designer] = designer;
                }
            }
        }

        public override ActivityDesigner AssociatedDesigner
        {
            get
            {
                ActivityDesigner parentDesigner = base.UserData[UserDataKey_Designer] as ActivityDesigner;
                if (parentDesigner == null)
                {
                    Type activityType = base.UserData[UserDataKey_ActivityType] as Type;
                    if (activityType == null)
                    {
                        return parentDesigner;
                    }
                    if (activityType != this.parentDesigner.Activity.GetType())
                    {
                        Activity activity = SecondaryViewProvider.FindActivity(this.parentDesigner, activityType);
                        if (activity != null)
                        {
                            parentDesigner = ActivityDesigner.GetDesigner(activity);
                        }
                    }
                    else
                    {
                        parentDesigner = this.parentDesigner;
                    }
                    base.UserData[UserDataKey_Designer] = parentDesigner;
                }
                return parentDesigner;
            }
        }
    }
}

