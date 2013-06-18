namespace System.Web.UI.Design.WebControls.WebParts
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Design;
    using System.Globalization;
    using System.IO;
    using System.Security.Permissions;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls.WebParts;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    internal class DesignerEditorPartChrome : EditorPartChrome
    {
        private ViewRendering _partViewRendering;

        public DesignerEditorPartChrome(EditorZone zone) : base(zone)
        {
        }

        public ViewRendering GetViewRendering(Control control)
        {
            string str2;
            DesignerRegionCollection regions;
            EditorPart part = control as EditorPart;
            if (part == null)
            {
                return new ViewRendering(ControlDesigner.CreateErrorDesignTimeHtml(System.Design.SR.GetString("EditorZoneDesigner_OnlyEditorParts"), null, control), new DesignerRegionCollection());
            }
            try
            {
                IDictionary data = new HybridDictionary(1);
                data["Zone"] = base.Zone;
                ((IControlDesignerAccessor) part).SetDesignModeState(data);
                this._partViewRendering = ControlDesigner.GetViewRendering(part);
                regions = this._partViewRendering.Regions;
                StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
                this.RenderEditorPart(new DesignTimeHtmlTextWriter(writer), (EditorPart) PartDesigner.GetViewControl(part));
                str2 = writer.ToString();
            }
            catch (Exception exception)
            {
                str2 = ControlDesigner.CreateErrorDesignTimeHtml(System.Design.SR.GetString("ControlDesigner_UnhandledException"), exception, control);
                regions = new DesignerRegionCollection();
            }
            return new ViewRendering(str2, regions);
        }

        protected override void RenderPartContents(HtmlTextWriter writer, EditorPart editorPart)
        {
            writer.Write(this._partViewRendering.Content);
        }
    }
}

