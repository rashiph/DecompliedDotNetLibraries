namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Data;
    using System.Drawing;
    using System.Globalization;
    using System.Web.UI.WebControls;

    internal sealed class DataGridAutoFormat : BaseAutoFormat<DataGrid>
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
        private string editItemBackColor;
        private int editItemFont;
        private string editItemForeColor;
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
        private int pagerAlign;
        private string pagerBackColor;
        private int pagerFont;
        private string pagerForeColor;
        private int pagerMode;
        private string selectedItemBackColor;
        private int selectedItemFont;
        private string selectedItemForeColor;

        public DataGridAutoFormat(string schemeName, string schemes) : base(schemeName, schemes)
        {
            this.borderStyle = -1;
            this.gridLines = -1;
            this.cellPadding = -1;
        }

        protected override void Apply(DataGrid grid)
        {
            grid.HeaderStyle.ForeColor = ColorTranslator.FromHtml(this.headerForeColor);
            grid.HeaderStyle.BackColor = ColorTranslator.FromHtml(this.headerBackColor);
            grid.HeaderStyle.Font.Bold = (this.headerFont & 1) != 0;
            grid.HeaderStyle.Font.Italic = (this.headerFont & 2) != 0;
            grid.HeaderStyle.Font.ClearDefaults();
            grid.FooterStyle.ForeColor = ColorTranslator.FromHtml(this.footerForeColor);
            grid.FooterStyle.BackColor = ColorTranslator.FromHtml(this.footerBackColor);
            grid.FooterStyle.Font.Bold = (this.footerFont & 1) != 0;
            grid.FooterStyle.Font.Italic = (this.footerFont & 2) != 0;
            grid.FooterStyle.Font.ClearDefaults();
            grid.BorderWidth = new Unit(this.borderWidth, CultureInfo.InvariantCulture);
            switch (this.gridLines)
            {
                case 0:
                    grid.GridLines = GridLines.None;
                    break;

                case 1:
                    grid.GridLines = GridLines.Horizontal;
                    break;

                case 2:
                    grid.GridLines = GridLines.Vertical;
                    break;

                default:
                    grid.GridLines = GridLines.Both;
                    break;
            }
            if ((this.borderStyle >= 0) && (this.borderStyle <= 9))
            {
                grid.BorderStyle = (BorderStyle) this.borderStyle;
            }
            else
            {
                grid.BorderStyle = BorderStyle.NotSet;
            }
            grid.BorderColor = ColorTranslator.FromHtml(this.borderColor);
            grid.CellPadding = this.cellPadding;
            grid.CellSpacing = this.cellSpacing;
            grid.ForeColor = ColorTranslator.FromHtml(this.foreColor);
            grid.BackColor = ColorTranslator.FromHtml(this.backColor);
            grid.ItemStyle.ForeColor = ColorTranslator.FromHtml(this.itemForeColor);
            grid.ItemStyle.BackColor = ColorTranslator.FromHtml(this.itemBackColor);
            grid.ItemStyle.Font.Bold = (this.itemFont & 1) != 0;
            grid.ItemStyle.Font.Italic = (this.itemFont & 2) != 0;
            grid.ItemStyle.Font.ClearDefaults();
            grid.AlternatingItemStyle.ForeColor = ColorTranslator.FromHtml(this.alternatingItemForeColor);
            grid.AlternatingItemStyle.BackColor = ColorTranslator.FromHtml(this.alternatingItemBackColor);
            grid.AlternatingItemStyle.Font.Bold = (this.alternatingItemFont & 1) != 0;
            grid.AlternatingItemStyle.Font.Italic = (this.alternatingItemFont & 2) != 0;
            grid.AlternatingItemStyle.Font.ClearDefaults();
            grid.SelectedItemStyle.ForeColor = ColorTranslator.FromHtml(this.selectedItemForeColor);
            grid.SelectedItemStyle.BackColor = ColorTranslator.FromHtml(this.selectedItemBackColor);
            grid.SelectedItemStyle.Font.Bold = (this.selectedItemFont & 1) != 0;
            grid.SelectedItemStyle.Font.Italic = (this.selectedItemFont & 2) != 0;
            grid.SelectedItemStyle.Font.ClearDefaults();
            grid.PagerStyle.ForeColor = ColorTranslator.FromHtml(this.pagerForeColor);
            grid.PagerStyle.BackColor = ColorTranslator.FromHtml(this.pagerBackColor);
            grid.PagerStyle.Font.Bold = (this.pagerFont & 1) != 0;
            grid.PagerStyle.Font.Italic = (this.pagerFont & 2) != 0;
            grid.PagerStyle.HorizontalAlign = (HorizontalAlign) this.pagerAlign;
            grid.PagerStyle.Font.ClearDefaults();
            grid.PagerStyle.Mode = (PagerMode) this.pagerMode;
            grid.EditItemStyle.ForeColor = ColorTranslator.FromHtml(this.editItemForeColor);
            grid.EditItemStyle.BackColor = ColorTranslator.FromHtml(this.editItemBackColor);
            grid.EditItemStyle.Font.Bold = (this.editItemFont & 1) != 0;
            grid.EditItemStyle.Font.Italic = (this.editItemFont & 2) != 0;
            grid.EditItemStyle.Font.ClearDefaults();
        }

        protected override void Initialize(DataRow schemeData)
        {
            this.foreColor = BaseAutoFormat<DataGrid>.GetStringProperty("ForeColor", schemeData);
            this.backColor = BaseAutoFormat<DataGrid>.GetStringProperty("BackColor", schemeData);
            this.borderColor = BaseAutoFormat<DataGrid>.GetStringProperty("BorderColor", schemeData);
            this.borderWidth = BaseAutoFormat<DataGrid>.GetStringProperty("BorderWidth", schemeData);
            this.borderStyle = BaseAutoFormat<DataGrid>.GetIntProperty("BorderStyle", -1, schemeData);
            this.cellSpacing = BaseAutoFormat<DataGrid>.GetIntProperty("CellSpacing", schemeData);
            this.cellPadding = BaseAutoFormat<DataGrid>.GetIntProperty("CellPadding", -1, schemeData);
            this.gridLines = BaseAutoFormat<DataGrid>.GetIntProperty("GridLines", -1, schemeData);
            this.itemForeColor = BaseAutoFormat<DataGrid>.GetStringProperty("ItemForeColor", schemeData);
            this.itemBackColor = BaseAutoFormat<DataGrid>.GetStringProperty("ItemBackColor", schemeData);
            this.itemFont = BaseAutoFormat<DataGrid>.GetIntProperty("ItemFont", schemeData);
            this.alternatingItemForeColor = BaseAutoFormat<DataGrid>.GetStringProperty("AltItemForeColor", schemeData);
            this.alternatingItemBackColor = BaseAutoFormat<DataGrid>.GetStringProperty("AltItemBackColor", schemeData);
            this.alternatingItemFont = BaseAutoFormat<DataGrid>.GetIntProperty("AltItemFont", schemeData);
            this.selectedItemForeColor = BaseAutoFormat<DataGrid>.GetStringProperty("SelItemForeColor", schemeData);
            this.selectedItemBackColor = BaseAutoFormat<DataGrid>.GetStringProperty("SelItemBackColor", schemeData);
            this.selectedItemFont = BaseAutoFormat<DataGrid>.GetIntProperty("SelItemFont", schemeData);
            this.headerForeColor = BaseAutoFormat<DataGrid>.GetStringProperty("HeaderForeColor", schemeData);
            this.headerBackColor = BaseAutoFormat<DataGrid>.GetStringProperty("HeaderBackColor", schemeData);
            this.headerFont = BaseAutoFormat<DataGrid>.GetIntProperty("HeaderFont", schemeData);
            this.footerForeColor = BaseAutoFormat<DataGrid>.GetStringProperty("FooterForeColor", schemeData);
            this.footerBackColor = BaseAutoFormat<DataGrid>.GetStringProperty("FooterBackColor", schemeData);
            this.footerFont = BaseAutoFormat<DataGrid>.GetIntProperty("FooterFont", schemeData);
            this.pagerForeColor = BaseAutoFormat<DataGrid>.GetStringProperty("PagerForeColor", schemeData);
            this.pagerBackColor = BaseAutoFormat<DataGrid>.GetStringProperty("PagerBackColor", schemeData);
            this.pagerFont = BaseAutoFormat<DataGrid>.GetIntProperty("PagerFont", schemeData);
            this.pagerAlign = BaseAutoFormat<DataGrid>.GetIntProperty("PagerAlign", schemeData);
            this.pagerMode = BaseAutoFormat<DataGrid>.GetIntProperty("PagerMode", schemeData);
            this.editItemForeColor = BaseAutoFormat<DataGrid>.GetStringProperty("EditItemForeColor", schemeData);
            this.editItemBackColor = BaseAutoFormat<DataGrid>.GetStringProperty("EditItemBackColor", schemeData);
            this.editItemFont = BaseAutoFormat<DataGrid>.GetIntProperty("EditItemFont", schemeData);
        }
    }
}

