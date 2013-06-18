namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Security.Permissions;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class HiddenFieldDesigner : ControlDesigner
    {
        public override string GetDesignTimeHtml()
        {
            return base.CreatePlaceHolderDesignTimeHtml();
        }

        public override void Initialize(IComponent component)
        {
            ControlDesigner.VerifyInitializeArgument(component, typeof(HiddenField));
            base.Initialize(component);
        }
    }
}

