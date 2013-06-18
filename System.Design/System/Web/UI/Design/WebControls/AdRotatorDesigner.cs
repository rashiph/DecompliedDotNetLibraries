namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Security.Permissions;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;

    [SupportsPreviewControl(true), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class AdRotatorDesigner : DataBoundControlDesigner
    {
        public override string GetDesignTimeHtml()
        {
            AdRotator viewControl = (AdRotator) base.ViewControl;
            StringWriter writer = new StringWriter(CultureInfo.CurrentCulture);
            DesignTimeHtmlTextWriter writer2 = new DesignTimeHtmlTextWriter(writer);
            HyperLink link = new HyperLink {
                ID = viewControl.ID,
                NavigateUrl = "",
                Target = viewControl.Target,
                AccessKey = viewControl.AccessKey,
                Enabled = viewControl.Enabled,
                TabIndex = viewControl.TabIndex
            };
            link.Style.Value = viewControl.Style.Value;
            link.RenderBeginTag(writer2);
            Image image = new Image();
            image.ApplyStyle(viewControl.ControlStyle);
            image.ImageUrl = "";
            image.AlternateText = viewControl.ID;
            image.ToolTip = viewControl.ToolTip;
            image.RenderControl(writer2);
            link.RenderEndTag(writer2);
            return writer.ToString();
        }
    }
}

