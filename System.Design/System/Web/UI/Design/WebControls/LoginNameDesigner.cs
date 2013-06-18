namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Design;
    using System.Security.Permissions;
    using System.Web.UI.Design;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class LoginNameDesigner : ControlDesigner
    {
        protected override string GetErrorDesignTimeHtml(Exception e)
        {
            return base.CreatePlaceHolderDesignTimeHtml(System.Design.SR.GetString("Control_ErrorRenderingShort") + "<br />" + e.Message);
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

