namespace System.Web.UI.Design.WebControls.WebParts
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Security.Permissions;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.Design.WebControls;
    using System.Web.UI.WebControls.WebParts;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public abstract class PartDesigner : CompositeControlDesigner
    {
        internal PartDesigner()
        {
        }

        private static ControlDesigner GetDesigner(Control control)
        {
            ControlDesigner designer = null;
            ISite site = control.Site;
            if (site != null)
            {
                IDesignerHost service = (IDesignerHost) site.GetService(typeof(IDesignerHost));
                designer = service.GetDesigner(control) as ControlDesigner;
            }
            return designer;
        }

        internal static Control GetViewControl(Control control)
        {
            ControlDesigner designer = GetDesigner(control);
            if (designer != null)
            {
                return designer.ViewControl;
            }
            return control;
        }

        public override void Initialize(IComponent component)
        {
            ControlDesigner.VerifyInitializeArgument(component, typeof(Part));
            base.Initialize(component);
        }

        protected override bool UsePreviewControl
        {
            get
            {
                return true;
            }
        }
    }
}

