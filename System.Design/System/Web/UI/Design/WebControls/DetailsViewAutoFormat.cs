namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Data;
    using System.Drawing;
    using System.Globalization;
    using System.Web.UI.WebControls;

    internal sealed class DetailsViewAutoFormat : BaseAutoFormat<DetailsView>
    {
        private string alternatingRowBackColor;
        private int alternatingRowFont;
        private string alternatingRowForeColor;
        private string backColor;
        private string borderColor;
        private int borderStyle;
        private string borderWidth;
        private int cellPadding;
        private int cellSpacing;
        private string commandRowBackColor;
        private int commandRowFont;
        private string commandRowForeColor;
        private string editRowBackColor;
        private int editRowFont;
        private string editRowForeColor;
        private string fieldHeaderBackColor;
        private int fieldHeaderFont;
        private string fieldHeaderForeColor;
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
        private int itemFont;
        private int pagerAlign;
        private string pagerBackColor;
        private int pagerButtons;
        private int pagerFont;
        private string pagerForeColor;
        private string rowBackColor;
        private string rowForeColor;

        public DetailsViewAutoFormat(string schemeName, string schemes) : base(schemeName, schemes)
        {
            this.borderStyle = -1;
            this.gridLines = -1;
            this.cellPadding = -1;
        }

        protected override void Apply(DetailsView view)
        {
            view.HeaderStyle.ForeColor = ColorTranslator.FromHtml(this.headerForeColor);
            view.HeaderStyle.BackColor = ColorTranslator.FromHtml(this.headerBackColor);
            view.HeaderStyle.Font.Bold = (this.headerFont & 1) != 0;
            view.HeaderStyle.Font.Italic = (this.headerFont & 2) != 0;
            view.HeaderStyle.Font.ClearDefaults();
            view.FooterStyle.ForeColor = ColorTranslator.FromHtml(this.footerForeColor);
            view.FooterStyle.BackColor = ColorTranslator.FromHtml(this.footerBackColor);
            view.FooterStyle.Font.Bold = (this.footerFont & 1) != 0;
            view.FooterStyle.Font.Italic = (this.footerFont & 2) != 0;
            view.FooterStyle.Font.ClearDefaults();
            view.BorderWidth = new Unit(this.borderWidth, CultureInfo.InvariantCulture);
            switch (this.gridLines)
            {
                case 0:
                    view.GridLines = GridLines.None;
                    break;

                case 1:
                    view.GridLines = GridLines.Horizontal;
                    break;

                case 2:
                    view.GridLines = GridLines.Vertical;
                    break;

                case 3:
                    view.GridLines = GridLines.Both;
                    break;

                default:
                    view.GridLines = GridLines.Both;
                    break;
            }
            if ((this.borderStyle >= 0) && (this.borderStyle <= 9))
            {
                view.BorderStyle = (BorderStyle) this.borderStyle;
            }
            else
            {
                view.BorderStyle = BorderStyle.NotSet;
            }
            view.BorderColor = ColorTranslator.FromHtml(this.borderColor);
            view.CellPadding = this.cellPadding;
            view.CellSpacing = this.cellSpacing;
            view.ForeColor = ColorTranslator.FromHtml(this.foreColor);
            view.BackColor = ColorTranslator.FromHtml(this.backColor);
            view.RowStyle.ForeColor = ColorTranslator.FromHtml(this.rowForeColor);
            view.RowStyle.BackColor = ColorTranslator.FromHtml(this.rowBackColor);
            view.RowStyle.Font.Bold = (this.itemFont & 1) != 0;
            view.RowStyle.Font.Italic = (this.itemFont & 2) != 0;
            view.RowStyle.Font.ClearDefaults();
            view.AlternatingRowStyle.ForeColor = ColorTranslator.FromHtml(this.alternatingRowForeColor);
            view.AlternatingRowStyle.BackColor = ColorTranslator.FromHtml(this.alternatingRowBackColor);
            view.AlternatingRowStyle.Font.Bold = (this.alternatingRowFont & 1) != 0;
            view.AlternatingRowStyle.Font.Italic = (this.alternatingRowFont & 2) != 0;
            view.AlternatingRowStyle.Font.ClearDefaults();
            view.CommandRowStyle.ForeColor = ColorTranslator.FromHtml(this.commandRowForeColor);
            view.CommandRowStyle.BackColor = ColorTranslator.FromHtml(this.commandRowBackColor);
            view.CommandRowStyle.Font.Bold = (this.commandRowFont & 1) != 0;
            view.CommandRowStyle.Font.Italic = (this.commandRowFont & 2) != 0;
            view.CommandRowStyle.Font.ClearDefaults();
            view.FieldHeaderStyle.ForeColor = ColorTranslator.FromHtml(this.fieldHeaderForeColor);
            view.FieldHeaderStyle.BackColor = ColorTranslator.FromHtml(this.fieldHeaderBackColor);
            view.FieldHeaderStyle.Font.Bold = (this.fieldHeaderFont & 1) != 0;
            view.FieldHeaderStyle.Font.Italic = (this.fieldHeaderFont & 2) != 0;
            view.FieldHeaderStyle.Font.ClearDefaults();
            view.EditRowStyle.ForeColor = ColorTranslator.FromHtml(this.editRowForeColor);
            view.EditRowStyle.BackColor = ColorTranslator.FromHtml(this.editRowBackColor);
            view.EditRowStyle.Font.Bold = (this.editRowFont & 1) != 0;
            view.EditRowStyle.Font.Italic = (this.editRowFont & 2) != 0;
            view.EditRowStyle.Font.ClearDefaults();
            view.PagerStyle.ForeColor = ColorTranslator.FromHtml(this.pagerForeColor);
            view.PagerStyle.BackColor = ColorTranslator.FromHtml(this.pagerBackColor);
            view.PagerStyle.Font.Bold = (this.pagerFont & 1) != 0;
            view.PagerStyle.Font.Italic = (this.pagerFont & 2) != 0;
            view.PagerStyle.HorizontalAlign = (HorizontalAlign) this.pagerAlign;
            view.PagerStyle.Font.ClearDefaults();
            view.PagerSettings.Mode = (PagerButtons) this.pagerButtons;
        }

        protected override void Initialize(DataRow schemeData)
        {
            this.foreColor = BaseAutoFormat<DetailsView>.GetStringProperty("ForeColor", schemeData);
            this.backColor = BaseAutoFormat<DetailsView>.GetStringProperty("BackColor", schemeData);
            this.borderColor = BaseAutoFormat<DetailsView>.GetStringProperty("BorderColor", schemeData);
            this.borderWidth = BaseAutoFormat<DetailsView>.GetStringProperty("BorderWidth", schemeData);
            this.borderStyle = BaseAutoFormat<DetailsView>.GetIntProperty("BorderStyle", -1, schemeData);
            this.cellSpacing = BaseAutoFormat<DetailsView>.GetIntProperty("CellSpacing", schemeData);
            this.cellPadding = BaseAutoFormat<DetailsView>.GetIntProperty("CellPadding", -1, schemeData);
            this.gridLines = BaseAutoFormat<DetailsView>.GetIntProperty("GridLines", -1, schemeData);
            this.rowForeColor = BaseAutoFormat<DetailsView>.GetStringProperty("RowForeColor", schemeData);
            this.rowBackColor = BaseAutoFormat<DetailsView>.GetStringProperty("RowBackColor", schemeData);
            this.itemFont = BaseAutoFormat<DetailsView>.GetIntProperty("RowFont", schemeData);
            this.alternatingRowForeColor = BaseAutoFormat<DetailsView>.GetStringProperty("AltRowForeColor", schemeData);
            this.alternatingRowBackColor = BaseAutoFormat<DetailsView>.GetStringProperty("AltRowBackColor", schemeData);
            this.alternatingRowFont = BaseAutoFormat<DetailsView>.GetIntProperty("AltRowFont", schemeData);
            this.commandRowForeColor = BaseAutoFormat<DetailsView>.GetStringProperty("CommandRowForeColor", schemeData);
            this.commandRowBackColor = BaseAutoFormat<DetailsView>.GetStringProperty("CommandRowBackColor", schemeData);
            this.commandRowFont = BaseAutoFormat<DetailsView>.GetIntProperty("CommandRowFont", schemeData);
            this.fieldHeaderForeColor = BaseAutoFormat<DetailsView>.GetStringProperty("FieldHeaderForeColor", schemeData);
            this.fieldHeaderBackColor = BaseAutoFormat<DetailsView>.GetStringProperty("FieldHeaderBackColor", schemeData);
            this.fieldHeaderFont = BaseAutoFormat<DetailsView>.GetIntProperty("FieldHeaderFont", schemeData);
            this.editRowForeColor = BaseAutoFormat<DetailsView>.GetStringProperty("EditRowForeColor", schemeData);
            this.editRowBackColor = BaseAutoFormat<DetailsView>.GetStringProperty("EditRowBackColor", schemeData);
            this.editRowFont = BaseAutoFormat<DetailsView>.GetIntProperty("EditRowFont", schemeData);
            this.headerForeColor = BaseAutoFormat<DetailsView>.GetStringProperty("HeaderForeColor", schemeData);
            this.headerBackColor = BaseAutoFormat<DetailsView>.GetStringProperty("HeaderBackColor", schemeData);
            this.headerFont = BaseAutoFormat<DetailsView>.GetIntProperty("HeaderFont", schemeData);
            this.footerForeColor = BaseAutoFormat<DetailsView>.GetStringProperty("FooterForeColor", schemeData);
            this.footerBackColor = BaseAutoFormat<DetailsView>.GetStringProperty("FooterBackColor", schemeData);
            this.footerFont = BaseAutoFormat<DetailsView>.GetIntProperty("FooterFont", schemeData);
            this.pagerForeColor = BaseAutoFormat<DetailsView>.GetStringProperty("PagerForeColor", schemeData);
            this.pagerBackColor = BaseAutoFormat<DetailsView>.GetStringProperty("PagerBackColor", schemeData);
            this.pagerFont = BaseAutoFormat<DetailsView>.GetIntProperty("PagerFont", schemeData);
            this.pagerAlign = BaseAutoFormat<DetailsView>.GetIntProperty("PagerAlign", schemeData);
            this.pagerButtons = BaseAutoFormat<DetailsView>.GetIntProperty("PagerButtons", 1, schemeData);
        }
    }
}

