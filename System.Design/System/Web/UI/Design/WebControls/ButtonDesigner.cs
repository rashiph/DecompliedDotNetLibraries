namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Security.Permissions;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;

    [SupportsPreviewControl(true), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class ButtonDesigner : ControlDesigner
    {
        public override string GetDesignTimeHtml()
        {
            Button viewControl = (Button) base.ViewControl;
            string text = viewControl.Text;
            bool flag = text.Trim().Length == 0;
            if (flag)
            {
                viewControl.Text = "[" + viewControl.ID + "]";
            }
            string designTimeHtml = base.GetDesignTimeHtml();
            if (flag)
            {
                viewControl.Text = text;
            }
            return designTimeHtml;
        }
    }
}

