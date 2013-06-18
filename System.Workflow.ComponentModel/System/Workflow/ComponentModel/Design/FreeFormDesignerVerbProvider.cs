namespace System.Workflow.ComponentModel.Design
{
    using System;

    internal sealed class FreeFormDesignerVerbProvider : IDesignerVerbProvider
    {
        private void OnZOrderChanged(object sender, EventArgs e)
        {
            ActivityDesignerVerb verb = sender as ActivityDesignerVerb;
            if ((verb != null) && verb.Properties.Contains(DesignerUserDataKeys.ZOrderKey))
            {
                FreeformActivityDesigner parentDesigner = verb.ActivityDesigner.ParentDesigner as FreeformActivityDesigner;
                if (parentDesigner != null)
                {
                    if (((ZOrder) verb.Properties[DesignerUserDataKeys.ZOrderKey]) == ZOrder.Foreground)
                    {
                        parentDesigner.BringToFront(verb.ActivityDesigner);
                    }
                    else if (((ZOrder) verb.Properties[DesignerUserDataKeys.ZOrderKey]) == ZOrder.Background)
                    {
                        parentDesigner.SendToBack(verb.ActivityDesigner);
                    }
                }
            }
        }

        private void OnZOrderStatusUpdate(object sender, EventArgs e)
        {
            ActivityDesignerVerb verb = sender as ActivityDesignerVerb;
            if ((verb != null) && verb.Properties.Contains(DesignerUserDataKeys.ZOrderKey))
            {
                FreeformActivityDesigner parentDesigner = verb.ActivityDesigner.ParentDesigner as FreeformActivityDesigner;
                if (parentDesigner != null)
                {
                    verb.Enabled = parentDesigner.CanUpdateZOrder(verb.ActivityDesigner, (ZOrder) verb.Properties[DesignerUserDataKeys.ZOrderKey]);
                }
            }
        }

        ActivityDesignerVerbCollection IDesignerVerbProvider.GetVerbs(ActivityDesigner activityDesigner)
        {
            ActivityDesignerVerbCollection verbs = new ActivityDesignerVerbCollection();
            if (activityDesigner.ParentDesigner is FreeformActivityDesigner)
            {
                ActivityDesignerVerb verb = new ActivityDesignerVerb(activityDesigner, DesignerVerbGroup.Actions, DR.GetString("BringToFront", new object[0]), new EventHandler(this.OnZOrderChanged), new EventHandler(this.OnZOrderStatusUpdate));
                verb.Properties[DesignerUserDataKeys.ZOrderKey] = ZOrder.Foreground;
                verbs.Add(verb);
                verb = new ActivityDesignerVerb(activityDesigner, DesignerVerbGroup.Actions, DR.GetString("SendToBack", new object[0]), new EventHandler(this.OnZOrderChanged), new EventHandler(this.OnZOrderStatusUpdate));
                verb.Properties[DesignerUserDataKeys.ZOrderKey] = ZOrder.Background;
                verbs.Add(verb);
            }
            return verbs;
        }
    }
}

