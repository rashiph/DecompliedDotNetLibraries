namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Data;
    using System.Drawing;
    using System.Globalization;
    using System.Web.UI.WebControls;

    internal sealed class SiteMapPathAutoFormat : BaseAutoFormat<SiteMapPath>
    {
        private Color _currentNodeStyleForeColor;
        private string _fontName;
        private FontUnit _fontSize;
        private bool _nodeStyleFontBold;
        private Color _nodeStyleForeColor;
        private string _pathSeparator;
        private bool _pathSeparatorStyleFontBold;
        private Color _pathSeparatorStyleForeColor;
        private bool _rootNodeStyleFontBold;
        private Color _rootNodeStyleForeColor;

        public SiteMapPathAutoFormat(string schemeName, string schemes) : base(schemeName, schemes)
        {
            base.Style.Width = 400;
            base.Style.Height = 100;
        }

        protected override void Apply(SiteMapPath siteMapPath)
        {
            siteMapPath.Font.Name = this._fontName;
            siteMapPath.Font.Size = this._fontSize;
            siteMapPath.Font.ClearDefaults();
            siteMapPath.NodeStyle.Font.Bold = this._nodeStyleFontBold;
            siteMapPath.NodeStyle.ForeColor = this._nodeStyleForeColor;
            siteMapPath.NodeStyle.Font.ClearDefaults();
            siteMapPath.RootNodeStyle.Font.Bold = this._rootNodeStyleFontBold;
            siteMapPath.RootNodeStyle.ForeColor = this._rootNodeStyleForeColor;
            siteMapPath.RootNodeStyle.Font.ClearDefaults();
            siteMapPath.CurrentNodeStyle.ForeColor = this._currentNodeStyleForeColor;
            siteMapPath.PathSeparatorStyle.Font.Bold = this._pathSeparatorStyleFontBold;
            siteMapPath.PathSeparatorStyle.ForeColor = this._pathSeparatorStyleForeColor;
            siteMapPath.PathSeparatorStyle.Font.ClearDefaults();
            if ((this._pathSeparator != null) && (this._pathSeparator.Length == 0))
            {
                this._pathSeparator = null;
            }
            siteMapPath.PathSeparator = this._pathSeparator;
        }

        protected override void Initialize(DataRow schemeData)
        {
            if (schemeData != null)
            {
                this._fontName = BaseAutoFormat<SiteMapPath>.GetStringProperty("FontName", schemeData);
                this._fontSize = new FontUnit(BaseAutoFormat<SiteMapPath>.GetStringProperty("FontSize", schemeData), CultureInfo.InvariantCulture);
                this._pathSeparator = BaseAutoFormat<SiteMapPath>.GetStringProperty("PathSeparator", schemeData);
                this._nodeStyleFontBold = BaseAutoFormat<SiteMapPath>.GetBooleanProperty("NodeStyleFontBold", schemeData);
                this._nodeStyleForeColor = ColorTranslator.FromHtml(BaseAutoFormat<SiteMapPath>.GetStringProperty("NodeStyleForeColor", schemeData));
                this._rootNodeStyleFontBold = BaseAutoFormat<SiteMapPath>.GetBooleanProperty("RootNodeStyleFontBold", schemeData);
                this._rootNodeStyleForeColor = ColorTranslator.FromHtml(BaseAutoFormat<SiteMapPath>.GetStringProperty("RootNodeStyleForeColor", schemeData));
                this._currentNodeStyleForeColor = ColorTranslator.FromHtml(BaseAutoFormat<SiteMapPath>.GetStringProperty("CurrentNodeStyleForeColor", schemeData));
                this._pathSeparatorStyleFontBold = BaseAutoFormat<SiteMapPath>.GetBooleanProperty("PathSeparatorStyleFontBold", schemeData);
                this._pathSeparatorStyleForeColor = ColorTranslator.FromHtml(BaseAutoFormat<SiteMapPath>.GetStringProperty("PathSeparatorStyleForeColor", schemeData));
            }
        }
    }
}

