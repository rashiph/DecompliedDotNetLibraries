namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel.Design;
    using System.Data;
    using System.Drawing;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.WebControls;

    internal sealed class DataListAutoFormat : BaseAutoFormat<DataList>
    {
        private string alternatingItemBackColor;
        private int alternatingItemFont;
        private string alternatingItemForeColor;
        private string backColor;
        private string borderColor;
        private int borderStyle;
        private string borderWidth;
        private int cellPadding;
        private int cellSpacing;
        private const int FONT_BOLD = 1;
        private const int FONT_ITALIC = 2;
        private string footerBackColor;
        private int footerFont;
        private string footerForeColor;
        private string foreColor;
        private int gridLines;
        private string headerBackColor;
        private int headerFont;
        private string headerForeColor;
        private string itemBackColor;
        private int itemFont;
        private string itemForeColor;
        private string selectedItemBackColor;
        private int selectedItemFont;
        private string selectedItemForeColor;

        public DataListAutoFormat(string schemeName, string schemes) : base(schemeName, schemes)
        {
            this.borderStyle = -1;
            this.gridLines = -1;
            this.cellPadding = -1;
        }

        protected override void Apply(DataList list)
        {
            list.HeaderStyle.ForeColor = ColorTranslator.FromHtml(this.headerForeColor);
            list.HeaderStyle.BackColor = ColorTranslator.FromHtml(this.headerBackColor);
            list.HeaderStyle.Font.Bold = (this.headerFont & 1) != 0;
            list.HeaderStyle.Font.Italic = (this.headerFont & 2) != 0;
            list.HeaderStyle.Font.ClearDefaults();
            list.FooterStyle.ForeColor = ColorTranslator.FromHtml(this.footerForeColor);
            list.FooterStyle.BackColor = ColorTranslator.FromHtml(this.footerBackColor);
            list.FooterStyle.Font.Bold = (this.footerFont & 1) != 0;
            list.FooterStyle.Font.Italic = (this.footerFont & 2) != 0;
            list.FooterStyle.Font.ClearDefaults();
            list.BorderWidth = new Unit(this.borderWidth, CultureInfo.InvariantCulture);
            switch (this.gridLines)
            {
                case 0:
                    list.GridLines = GridLines.None;
                    break;

                case 1:
                    list.GridLines = GridLines.Horizontal;
                    break;

                case 2:
                    list.GridLines = GridLines.Vertical;
                    break;

                case 3:
                    list.GridLines = GridLines.Both;
                    break;

                default:
                    list.GridLines = GridLines.None;
                    break;
            }
            if ((this.borderStyle >= 0) && (this.borderStyle <= 9))
            {
                list.BorderStyle = (BorderStyle) this.borderStyle;
            }
            else
            {
                list.BorderStyle = BorderStyle.NotSet;
            }
            list.BorderColor = ColorTranslator.FromHtml(this.borderColor);
            list.CellPadding = this.cellPadding;
            list.CellSpacing = this.cellSpacing;
            list.ForeColor = ColorTranslator.FromHtml(this.foreColor);
            list.BackColor = ColorTranslator.FromHtml(this.backColor);
            list.ItemStyle.ForeColor = ColorTranslator.FromHtml(this.itemForeColor);
            list.ItemStyle.BackColor = ColorTranslator.FromHtml(this.itemBackColor);
            list.ItemStyle.Font.Bold = (this.itemFont & 1) != 0;
            list.ItemStyle.Font.Italic = (this.itemFont & 2) != 0;
            list.ItemStyle.Font.ClearDefaults();
            list.AlternatingItemStyle.ForeColor = ColorTranslator.FromHtml(this.alternatingItemForeColor);
            list.AlternatingItemStyle.BackColor = ColorTranslator.FromHtml(this.alternatingItemBackColor);
            list.AlternatingItemStyle.Font.Bold = (this.alternatingItemFont & 1) != 0;
            list.AlternatingItemStyle.Font.Italic = (this.alternatingItemFont & 2) != 0;
            list.AlternatingItemStyle.Font.ClearDefaults();
            list.SelectedItemStyle.ForeColor = ColorTranslator.FromHtml(this.selectedItemForeColor);
            list.SelectedItemStyle.BackColor = ColorTranslator.FromHtml(this.selectedItemBackColor);
            list.SelectedItemStyle.Font.Bold = (this.selectedItemFont & 1) != 0;
            list.SelectedItemStyle.Font.Italic = (this.selectedItemFont & 2) != 0;
            list.SelectedItemStyle.Font.ClearDefaults();
        }

        public override Control GetPreviewControl(Control runtimeControl)
        {
            Control previewControl = base.GetPreviewControl(runtimeControl);
            if (previewControl != null)
            {
                IDesignerHost service = (IDesignerHost) runtimeControl.Site.GetService(typeof(IDesignerHost));
                DataList list = previewControl as DataList;
                if ((list == null) || (service == null))
                {
                    return previewControl;
                }
                TemplateBuilder itemTemplate = list.ItemTemplate as TemplateBuilder;
                if (((itemTemplate != null) && (itemTemplate.Text.Length == 0)) || (list.ItemTemplate == null))
                {
                    string templateText = "####";
                    list.ItemTemplate = ControlParser.ParseTemplate(service, templateText);
                    list.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
                }
                list.HorizontalAlign = HorizontalAlign.Center;
                list.Width = new Unit(80.0, UnitType.Percentage);
            }
            return previewControl;
        }

        protected override void Initialize(DataRow schemeData)
        {
            this.foreColor = BaseAutoFormat<DataList>.GetStringProperty("ForeColor", schemeData);
            this.backColor = BaseAutoFormat<DataList>.GetStringProperty("BackColor", schemeData);
            this.borderColor = BaseAutoFormat<DataList>.GetStringProperty("BorderColor", schemeData);
            this.borderWidth = BaseAutoFormat<DataList>.GetStringProperty("BorderWidth", schemeData);
            this.borderStyle = BaseAutoFormat<DataList>.GetIntProperty("BorderStyle", -1, schemeData);
            this.cellSpacing = BaseAutoFormat<DataList>.GetIntProperty("CellSpacing", schemeData);
            this.cellPadding = BaseAutoFormat<DataList>.GetIntProperty("CellPadding", -1, schemeData);
            this.gridLines = BaseAutoFormat<DataList>.GetIntProperty("GridLines", -1, schemeData);
            this.itemForeColor = BaseAutoFormat<DataList>.GetStringProperty("ItemForeColor", schemeData);
            this.itemBackColor = BaseAutoFormat<DataList>.GetStringProperty("ItemBackColor", schemeData);
            this.itemFont = BaseAutoFormat<DataList>.GetIntProperty("ItemFont", schemeData);
            this.alternatingItemForeColor = BaseAutoFormat<DataList>.GetStringProperty("AltItemForeColor", schemeData);
            this.alternatingItemBackColor = BaseAutoFormat<DataList>.GetStringProperty("AltItemBackColor", schemeData);
            this.alternatingItemFont = BaseAutoFormat<DataList>.GetIntProperty("AltItemFont", schemeData);
            this.selectedItemForeColor = BaseAutoFormat<DataList>.GetStringProperty("SelItemForeColor", schemeData);
            this.selectedItemBackColor = BaseAutoFormat<DataList>.GetStringProperty("SelItemBackColor", schemeData);
            this.selectedItemFont = BaseAutoFormat<DataList>.GetIntProperty("SelItemFont", schemeData);
            this.headerForeColor = BaseAutoFormat<DataList>.GetStringProperty("HeaderForeColor", schemeData);
            this.headerBackColor = BaseAutoFormat<DataList>.GetStringProperty("HeaderBackColor", schemeData);
            this.headerFont = BaseAutoFormat<DataList>.GetIntProperty("HeaderFont", schemeData);
            this.footerForeColor = BaseAutoFormat<DataList>.GetStringProperty("FooterForeColor", schemeData);
            this.footerBackColor = BaseAutoFormat<DataList>.GetStringProperty("FooterBackColor", schemeData);
            this.footerFont = BaseAutoFormat<DataList>.GetIntProperty("FooterFont", schemeData);
        }
    }
}

