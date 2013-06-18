namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Security.Permissions;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class ViewDesigner : ContainerControlDesigner
    {
        public ViewDesigner()
        {
            base.FrameStyleInternal.Width = Unit.Percentage(100.0);
        }

        public override string GetDesignTimeHtml()
        {
            return this.GetDesignTimeHtmlHelper(false, null);
        }

        public override string GetDesignTimeHtml(DesignerRegionCollection regions)
        {
            return this.GetDesignTimeHtmlHelper(true, regions);
        }

        private string GetDesignTimeHtmlHelper(bool useRegions, DesignerRegionCollection regions)
        {
            System.Web.UI.WebControls.View component = (System.Web.UI.WebControls.View) base.Component;
            if (!(component.Parent is MultiView))
            {
                return base.CreateInvalidParentDesignTimeHtml(typeof(System.Web.UI.WebControls.View), typeof(MultiView));
            }
            if (useRegions)
            {
                return base.GetDesignTimeHtml(regions);
            }
            return base.GetDesignTimeHtml();
        }

        public override void Initialize(IComponent component)
        {
            ControlDesigner.VerifyInitializeArgument(component, typeof(System.Web.UI.WebControls.View));
            base.Initialize(component);
        }
    }
}

