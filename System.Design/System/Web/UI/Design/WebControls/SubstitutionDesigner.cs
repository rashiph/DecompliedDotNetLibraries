namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Security.Permissions;
    using System.Web.UI.Design;

    [SupportsPreviewControl(true), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class SubstitutionDesigner : ControlDesigner
    {
        public override string GetDesignTimeHtml()
        {
            return this.GetEmptyDesignTimeHtml();
        }
    }
}

