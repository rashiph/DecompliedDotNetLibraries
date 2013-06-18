namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Data;
    using System.Drawing;
    using System.Globalization;
    using System.Web.UI.WebControls;

    internal sealed class GridViewAutoFormat : BaseAutoFormat<GridView>
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
        private int pagerButtons;
        private int pagerFont;
        private string pagerForeColor;
        private string selectedItemBackColor;
        private int selectedItemFont;
        private string selectedItemForeColor;
        private string sortedAscendingCellBackColor;
        private string sortedAscendingHeaderBackColor;
        private string sortedDescendingCellBackColor;
        private string sortedDescendingHeaderBackColor;

        public GridViewAutoFormat(string schemeName, string schemes) : base(schemeName, schemes)
        {
            this.borderStyle = -1;
            this.gridLines = -1;
            this.cellPadding = -1;
            base.Style.Width = 260;
            base.Style.Height = 240;
        }

        protected override void Apply(GridView grid)
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
            grid.RowStyle.ForeColor = ColorTranslator.FromHtml(this.itemForeColor);
            grid.RowStyle.BackColor = ColorTranslator.FromHtml(this.itemBackColor);
            grid.RowStyle.Font.Bold = (this.itemFont & 1) != 0;
            grid.RowStyle.Font.Italic = (this.itemFont & 2) != 0;
            grid.RowStyle.Font.ClearDefaults();
            grid.AlternatingRowStyle.ForeColor = ColorTranslator.FromHtml(this.alternatingItemForeColor);
            grid.AlternatingRowStyle.BackColor = ColorTranslator.FromHtml(this.alternatingItemBackColor);
            grid.AlternatingRowStyle.Font.Bold = (this.alternatingItemFont & 1) != 0;
            grid.AlternatingRowStyle.Font.Italic = (this.alternatingItemFont & 2) != 0;
            grid.AlternatingRowStyle.Font.ClearDefaults();
            grid.SelectedRowStyle.ForeColor = ColorTranslator.FromHtml(this.selectedItemForeColor);
            grid.SelectedRowStyle.BackColor = ColorTranslator.FromHtml(this.selectedItemBackColor);
            grid.SelectedRowStyle.Font.Bold = (this.selectedItemFont & 1) != 0;
            grid.SelectedRowStyle.Font.Italic = (this.selectedItemFont & 2) != 0;
            grid.SelectedRowStyle.Font.ClearDefaults();
            grid.PagerStyle.ForeColor = ColorTranslator.FromHtml(this.pagerForeColor);
            grid.PagerStyle.BackColor = ColorTranslator.FromHtml(this.pagerBackColor);
            grid.PagerStyle.Font.Bold = (this.pagerFont & 1) != 0;
            grid.PagerStyle.Font.Italic = (this.pagerFont & 2) != 0;
            grid.PagerStyle.HorizontalAlign = (HorizontalAlign) this.pagerAlign;
            grid.PagerStyle.Font.ClearDefaults();
            grid.PagerSettings.Mode = (PagerButtons) this.pagerButtons;
            grid.EditRowStyle.ForeColor = ColorTranslator.FromHtml(this.editItemForeColor);
            grid.EditRowStyle.BackColor = ColorTranslator.FromHtml(this.editItemBackColor);
            grid.EditRowStyle.Font.Bold = (this.editItemFont & 1) != 0;
            grid.EditRowStyle.Font.Italic = (this.editItemFont & 2) != 0;
            grid.EditRowStyle.Font.ClearDefaults();
            grid.SortedAscendingCellStyle.BackColor = ColorTranslator.FromHtml(this.sortedAscendingCellBackColor);
            grid.SortedDescendingCellStyle.BackColor = ColorTranslator.FromHtml(this.sortedDescendingCellBackColor);
            grid.SortedAscendingHeaderStyle.BackColor = ColorTranslator.FromHtml(this.sortedAscendingHeaderBackColor);
            grid.SortedDescendingHeaderStyle.BackColor = ColorTranslator.FromHtml(this.sortedDescendingHeaderBackColor);
        }

        protected override void Initialize(DataRow schemeData)
        {
            this.foreColor = BaseAutoFormat<GridView>.GetStringProperty("ForeColor", schemeData);
            this.backColor = BaseAutoFormat<GridView>.GetStringProperty("BackColor", schemeData);
            this.borderColor = BaseAutoFormat<GridView>.GetStringProperty("BorderColor", schemeData);
            this.borderWidth = BaseAutoFormat<GridView>.GetStringProperty("BorderWidth", schemeData);
            this.borderStyle = BaseAutoFormat<GridView>.GetIntProperty("BorderStyle", -1, schemeData);
            this.cellSpacing = BaseAutoFormat<GridView>.GetIntProperty("CellSpacing", schemeData);
            this.cellPadding = BaseAutoFormat<GridView>.GetIntProperty("CellPadding", -1, schemeData);
            this.gridLines = BaseAutoFormat<GridView>.GetIntProperty("GridLines", -1, schemeData);
            this.itemForeColor = BaseAutoFormat<GridView>.GetStringProperty("ItemForeColor", schemeData);
            this.itemBackColor = BaseAutoFormat<GridView>.GetStringProperty("ItemBackColor", schemeData);
            this.itemFont = BaseAutoFormat<GridView>.GetIntProperty("ItemFont", schemeData);
            this.alternatingItemForeColor = BaseAutoFormat<GridView>.GetStringProperty("AltItemForeColor", schemeData);
            this.alternatingItemBackColor = BaseAutoFormat<GridView>.GetStringProperty("AltItemBackColor", schemeData);
            this.alternatingItemFont = BaseAutoFormat<GridView>.GetIntProperty("AltItemFont", schemeData);
            this.selectedItemForeColor = BaseAutoFormat<GridView>.GetStringProperty("SelItemForeColor", schemeData);
            this.selectedItemBackColor = BaseAutoFormat<GridView>.GetStringProperty("SelItemBackColor", schemeData);
            this.selectedItemFont = BaseAutoFormat<GridView>.GetIntProperty("SelItemFont", schemeData);
            this.headerForeColor = BaseAutoFormat<GridView>.GetStringProperty("HeaderForeColor", schemeData);
            this.headerBackColor = BaseAutoFormat<GridView>.GetStringProperty("HeaderBackColor", schemeData);
            this.headerFont = BaseAutoFormat<GridView>.GetIntProperty("HeaderFont", schemeData);
            this.footerForeColor = BaseAutoFormat<GridView>.GetStringProperty("FooterForeColor", schemeData);
            this.footerBackColor = BaseAutoFormat<GridView>.GetStringProperty("FooterBackColor", schemeData);
            this.footerFont = BaseAutoFormat<GridView>.GetIntProperty("FooterFont", schemeData);
            this.pagerForeColor = BaseAutoFormat<GridView>.GetStringProperty("PagerForeColor", schemeData);
            this.pagerBackColor = BaseAutoFormat<GridView>.GetStringProperty("PagerBackColor", schemeData);
            this.pagerFont = BaseAutoFormat<GridView>.GetIntProperty("PagerFont", schemeData);
            this.pagerAlign = BaseAutoFormat<GridView>.GetIntProperty("PagerAlign", schemeData);
            this.pagerButtons = BaseAutoFormat<GridView>.GetIntProperty("PagerButtons", 1, schemeData);
            this.editItemForeColor = BaseAutoFormat<GridView>.GetStringProperty("EditItemForeColor", schemeData);
            this.editItemBackColor = BaseAutoFormat<GridView>.GetStringProperty("EditItemBackColor", schemeData);
            this.editItemFont = BaseAutoFormat<GridView>.GetIntProperty("EditItemFont", schemeData);
            this.sortedAscendingCellBackColor = BaseAutoFormat<GridView>.GetStringProperty("SortedAscendingCellBackColor", schemeData);
            this.sortedDescendingCellBackColor = BaseAutoFormat<GridView>.GetStringProperty("SortedDescendingCellBackColor", schemeData);
            this.sortedAscendingHeaderBackColor = BaseAutoFormat<GridView>.GetStringProperty("SortedAscendingHeaderBackColor", schemeData);
            this.sortedDescendingHeaderBackColor = BaseAutoFormat<GridView>.GetStringProperty("SortedDescendingHeaderBackColor", schemeData);
        }
    }
}

