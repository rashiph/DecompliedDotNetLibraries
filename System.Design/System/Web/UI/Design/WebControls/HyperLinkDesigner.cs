namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel.Design;
    using System.Security.Permissions;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;

    [SupportsPreviewControl(true), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class HyperLinkDesigner : TextControlDesigner
    {
        public override string GetDesignTimeHtml()
        {
            string designTimeHtml;
            HyperLink component = (HyperLink) base.Component;
            string text = component.Text;
            string imageUrl = component.ImageUrl;
            string navigateUrl = component.NavigateUrl;
            bool flag = (text.Trim().Length == 0) && (imageUrl.Trim().Length == 0);
            bool flag2 = navigateUrl.Trim().Length == 0;
            bool flag3 = component.HasControls();
            Control[] array = null;
            if (flag)
            {
                if (flag3)
                {
                    array = new Control[component.Controls.Count];
                    component.Controls.CopyTo(array, 0);
                }
                component.Text = "[" + component.ID + "]";
            }
            if (flag2)
            {
                component.NavigateUrl = "url";
            }
            try
            {
                designTimeHtml = base.GetDesignTimeHtml();
            }
            finally
            {
                if (flag)
                {
                    component.Text = text;
                    if (flag3)
                    {
                        foreach (Control control in array)
                        {
                            component.Controls.Add(control);
                        }
                    }
                }
                if (flag2)
                {
                    component.NavigateUrl = navigateUrl;
                }
            }
            return designTimeHtml;
        }

        public override void OnComponentChanged(object sender, ComponentChangedEventArgs ce)
        {
            base.OnComponentChanged(sender, new ComponentChangedEventArgs(ce.Component, null, null, null));
        }
    }
}

