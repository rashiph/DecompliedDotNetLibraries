namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Web.UI;
    using System.Web.UI.Design;

    [SupportsPreviewControl(true), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    internal class LocalizeDesigner : LiteralDesigner
    {
        private const string DesignTimeHtml = "<span {0}=0></span>";
        private readonly string[] EnabledPropertiesInGrid = new string[] { "ID", "Text" };

        public override string GetDesignTimeHtml(DesignerRegionCollection regions)
        {
            EditableDesignerRegion region = new EditableDesignerRegion(this, "Text") {
                Description = System.Design.SR.GetString("LocalizeDesigner_RegionWatermark")
            };
            region.Properties[typeof(Control)] = base.Component;
            regions.Add(region);
            return string.Format(CultureInfo.InvariantCulture, "<span {0}=0></span>", new object[] { DesignerRegion.DesignerRegionAttributeName });
        }

        public override string GetEditableDesignerRegionContent(EditableDesignerRegion region)
        {
            PropertyDescriptor descriptor = TypeDescriptor.GetProperties(base.Component)["Text"];
            return (string) descriptor.GetValue(base.Component);
        }

        protected override void PostFilterProperties(IDictionary properties)
        {
            base.HideAllPropertiesUnlessExcluded(properties, this.EnabledPropertiesInGrid);
            base.PostFilterAttributes(properties);
        }

        public override void SetEditableDesignerRegionContent(EditableDesignerRegion region, string content)
        {
            string str = content;
            try
            {
                IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                Control[] controlArray = ControlParser.ParseControls(service, content);
                str = string.Empty;
                foreach (Control control in controlArray)
                {
                    LiteralControl control2 = control as LiteralControl;
                    if (control2 != null)
                    {
                        str = str + control2.Text;
                    }
                }
            }
            catch
            {
            }
            TypeDescriptor.GetProperties(base.Component)["Text"].SetValue(base.Component, str);
        }
    }
}

