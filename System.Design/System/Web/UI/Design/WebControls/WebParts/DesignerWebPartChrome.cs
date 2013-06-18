namespace System.Web.UI.Design.WebControls.WebParts
{
    using System;
    using System.Design;
    using System.Globalization;
    using System.IO;
    using System.Security.Permissions;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;
    using System.Web.UI.WebControls.WebParts;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    internal class DesignerWebPartChrome : WebPartChrome
    {
        private ViewRendering _partViewRendering;

        public DesignerWebPartChrome(WebPartZoneBase zone) : base(zone, null)
        {
        }

        public ViewRendering GetViewRendering(Control control)
        {
            string str;
            DesignerRegionCollection regions;
            try
            {
                this._partViewRendering = ControlDesigner.GetViewRendering(control);
                regions = this._partViewRendering.Regions;
                WebPart part = control as WebPart;
                if (part == null)
                {
                    part = new DesignerGenericWebPart(PartDesigner.GetViewControl(control));
                }
                StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
                this.RenderWebPart(new DesignTimeHtmlTextWriter(writer), (WebPart) PartDesigner.GetViewControl(part));
                str = writer.ToString();
            }
            catch (Exception exception)
            {
                str = ControlDesigner.CreateErrorDesignTimeHtml(System.Design.SR.GetString("ControlDesigner_UnhandledException"), exception, control);
                regions = new DesignerRegionCollection();
            }
            StringWriter writer2 = new StringWriter(CultureInfo.InvariantCulture);
            DesignTimeHtmlTextWriter writer3 = new DesignTimeHtmlTextWriter(writer2);
            bool flag = base.Zone.LayoutOrientation == Orientation.Horizontal;
            if (flag)
            {
                writer3.AddStyleAttribute("display", "inline-block");
                writer3.AddStyleAttribute(HtmlTextWriterStyle.Height, "100%");
                writer3.RenderBeginTag(HtmlTextWriterTag.Span);
            }
            writer3.Write(str);
            if (flag)
            {
                writer3.RenderEndTag();
            }
            return new ViewRendering(writer2.ToString(), regions);
        }

        protected override void RenderPartContents(HtmlTextWriter writer, WebPart webPart)
        {
            writer.Write(this._partViewRendering.Content);
        }
    }
}

