namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Data;
    using System.Drawing;
    using System.Globalization;
    using System.Web.UI.WebControls;

    internal sealed class CreateUserWizardAutoFormat : BaseAutoFormat<CreateUserWizard>
    {
        private string backColor;
        private string borderColor;
        private int borderStyle;
        private string borderWidth;
        private const int FONT_BOLD = 1;
        private string fontName;
        private string fontSize;
        private Color HeaderStyleBackColor;
        private Color HeaderStyleBorderColor;
        private BorderStyle HeaderStyleBorderStyle;
        private Unit HeaderStyleBorderWidth;
        private bool HeaderStyleFontBold;
        private FontUnit HeaderStyleFontSize;
        private Color HeaderStyleForeColor;
        private HorizontalAlign HeaderStyleHorizontalAlign;
        private Color NavigationButtonStyleBackColor;
        private Color NavigationButtonStyleBorderColor;
        private BorderStyle NavigationButtonStyleBorderStyle;
        private Unit NavigationButtonStyleBorderWidth;
        private string NavigationButtonStyleFontName;
        private FontUnit NavigationButtonStyleFontSize;
        private Color NavigationButtonStyleForeColor;
        private Color SideBarButtonStyleBackColor;
        private Unit SideBarButtonStyleBorderWidth;
        private string SideBarButtonStyleFontName;
        private bool SideBarButtonStyleFontUnderline;
        private Color SideBarButtonStyleForeColor;
        private Color SideBarStyleBackColor;
        private Unit SideBarStyleBorderWidth;
        private FontUnit SideBarStyleFontSize;
        private bool SideBarStyleFontStrikeout;
        private bool SideBarStyleFontUnderline;
        private VerticalAlign SideBarStyleVerticalAlign;
        private Color StepStyleBackColor;
        private Color StepStyleBorderColor;
        private BorderStyle StepStyleBorderStyle;
        private Unit StepStyleBorderWidth;
        private FontUnit StepStyleFontSize;
        private Color StepStyleForeColor;
        private string titleTextBackColor;
        private int titleTextFont;
        private string titleTextForeColor;

        public CreateUserWizardAutoFormat(string schemeName, string schemes) : base(schemeName, schemes)
        {
            this.borderStyle = -1;
            base.Style.Width = 500;
            base.Style.Height = 400;
        }

        protected override void Apply(CreateUserWizard createUserWizard)
        {
            createUserWizard.StepStyle.Reset();
            createUserWizard.BackColor = ColorTranslator.FromHtml(this.backColor);
            createUserWizard.BorderColor = ColorTranslator.FromHtml(this.borderColor);
            createUserWizard.BorderWidth = new Unit(this.borderWidth, CultureInfo.InvariantCulture);
            if ((this.borderStyle >= 0) && (this.borderStyle <= 9))
            {
                createUserWizard.BorderStyle = (BorderStyle) this.borderStyle;
            }
            else
            {
                createUserWizard.BorderStyle = BorderStyle.NotSet;
            }
            createUserWizard.Font.Size = new FontUnit(this.fontSize, CultureInfo.InvariantCulture);
            createUserWizard.Font.Name = this.fontName;
            createUserWizard.Font.ClearDefaults();
            createUserWizard.TitleTextStyle.BackColor = ColorTranslator.FromHtml(this.titleTextBackColor);
            createUserWizard.TitleTextStyle.ForeColor = ColorTranslator.FromHtml(this.titleTextForeColor);
            createUserWizard.TitleTextStyle.Font.Bold = (this.titleTextFont & 1) != 0;
            createUserWizard.TitleTextStyle.Font.ClearDefaults();
            createUserWizard.StepStyle.BorderWidth = this.StepStyleBorderWidth;
            createUserWizard.StepStyle.BorderStyle = this.StepStyleBorderStyle;
            createUserWizard.StepStyle.BorderColor = this.StepStyleBorderColor;
            createUserWizard.StepStyle.ForeColor = this.StepStyleForeColor;
            createUserWizard.StepStyle.BackColor = this.StepStyleBackColor;
            createUserWizard.StepStyle.Font.Size = this.StepStyleFontSize;
            createUserWizard.StepStyle.Font.ClearDefaults();
            createUserWizard.SideBarButtonStyle.Font.Underline = this.SideBarButtonStyleFontUnderline;
            createUserWizard.SideBarButtonStyle.Font.Name = this.SideBarButtonStyleFontName;
            createUserWizard.SideBarButtonStyle.ForeColor = this.SideBarButtonStyleForeColor;
            createUserWizard.SideBarButtonStyle.BorderWidth = this.SideBarButtonStyleBorderWidth;
            createUserWizard.SideBarButtonStyle.BackColor = this.SideBarButtonStyleBackColor;
            createUserWizard.SideBarButtonStyle.Font.ClearDefaults();
            createUserWizard.NavigationButtonStyle.BorderWidth = this.NavigationButtonStyleBorderWidth;
            createUserWizard.NavigationButtonStyle.Font.Name = this.NavigationButtonStyleFontName;
            createUserWizard.NavigationButtonStyle.Font.Size = this.NavigationButtonStyleFontSize;
            createUserWizard.NavigationButtonStyle.BorderStyle = this.NavigationButtonStyleBorderStyle;
            createUserWizard.NavigationButtonStyle.BorderColor = this.NavigationButtonStyleBorderColor;
            createUserWizard.NavigationButtonStyle.ForeColor = this.NavigationButtonStyleForeColor;
            createUserWizard.NavigationButtonStyle.BackColor = this.NavigationButtonStyleBackColor;
            createUserWizard.NavigationButtonStyle.Font.ClearDefaults();
            createUserWizard.ContinueButtonStyle.BorderWidth = this.NavigationButtonStyleBorderWidth;
            createUserWizard.ContinueButtonStyle.Font.Name = this.NavigationButtonStyleFontName;
            createUserWizard.ContinueButtonStyle.Font.Size = this.NavigationButtonStyleFontSize;
            createUserWizard.ContinueButtonStyle.BorderStyle = this.NavigationButtonStyleBorderStyle;
            createUserWizard.ContinueButtonStyle.BorderColor = this.NavigationButtonStyleBorderColor;
            createUserWizard.ContinueButtonStyle.ForeColor = this.NavigationButtonStyleForeColor;
            createUserWizard.ContinueButtonStyle.BackColor = this.NavigationButtonStyleBackColor;
            createUserWizard.ContinueButtonStyle.Font.ClearDefaults();
            createUserWizard.CreateUserButtonStyle.BorderWidth = this.NavigationButtonStyleBorderWidth;
            createUserWizard.CreateUserButtonStyle.Font.Name = this.NavigationButtonStyleFontName;
            createUserWizard.CreateUserButtonStyle.Font.Size = this.NavigationButtonStyleFontSize;
            createUserWizard.CreateUserButtonStyle.BorderStyle = this.NavigationButtonStyleBorderStyle;
            createUserWizard.CreateUserButtonStyle.BorderColor = this.NavigationButtonStyleBorderColor;
            createUserWizard.CreateUserButtonStyle.ForeColor = this.NavigationButtonStyleForeColor;
            createUserWizard.CreateUserButtonStyle.BackColor = this.NavigationButtonStyleBackColor;
            createUserWizard.CreateUserButtonStyle.Font.ClearDefaults();
            createUserWizard.HeaderStyle.ForeColor = this.HeaderStyleForeColor;
            createUserWizard.HeaderStyle.BorderColor = this.HeaderStyleBorderColor;
            createUserWizard.HeaderStyle.BackColor = this.HeaderStyleBackColor;
            createUserWizard.HeaderStyle.Font.Size = this.HeaderStyleFontSize;
            createUserWizard.HeaderStyle.Font.Bold = this.HeaderStyleFontBold;
            createUserWizard.HeaderStyle.BorderWidth = this.HeaderStyleBorderWidth;
            createUserWizard.HeaderStyle.HorizontalAlign = this.HeaderStyleHorizontalAlign;
            createUserWizard.HeaderStyle.BorderStyle = this.HeaderStyleBorderStyle;
            createUserWizard.HeaderStyle.Font.ClearDefaults();
            createUserWizard.SideBarStyle.BackColor = this.SideBarStyleBackColor;
            createUserWizard.SideBarStyle.VerticalAlign = this.SideBarStyleVerticalAlign;
            createUserWizard.SideBarStyle.Font.Size = this.SideBarStyleFontSize;
            createUserWizard.SideBarStyle.Font.Underline = this.SideBarStyleFontUnderline;
            createUserWizard.SideBarStyle.Font.Strikeout = this.SideBarStyleFontStrikeout;
            createUserWizard.SideBarStyle.BorderWidth = this.SideBarStyleBorderWidth;
            createUserWizard.SideBarStyle.Font.ClearDefaults();
        }

        protected override void Initialize(DataRow schemeData)
        {
            this.backColor = BaseAutoFormat<CreateUserWizard>.GetStringProperty("BackColor", schemeData);
            this.borderColor = BaseAutoFormat<CreateUserWizard>.GetStringProperty("BorderColor", schemeData);
            this.borderWidth = BaseAutoFormat<CreateUserWizard>.GetStringProperty("BorderWidth", schemeData);
            this.borderStyle = BaseAutoFormat<CreateUserWizard>.GetIntProperty("BorderStyle", -1, schemeData);
            this.fontSize = BaseAutoFormat<CreateUserWizard>.GetStringProperty("FontSize", schemeData);
            this.fontName = BaseAutoFormat<CreateUserWizard>.GetStringProperty("FontName", schemeData);
            this.titleTextBackColor = BaseAutoFormat<CreateUserWizard>.GetStringProperty("TitleTextBackColor", schemeData);
            this.titleTextForeColor = BaseAutoFormat<CreateUserWizard>.GetStringProperty("TitleTextForeColor", schemeData);
            this.titleTextFont = BaseAutoFormat<CreateUserWizard>.GetIntProperty("TitleTextFont", schemeData);
            this.NavigationButtonStyleBorderWidth = new Unit(BaseAutoFormat<CreateUserWizard>.GetStringProperty("NavigationButtonStyleBorderWidth", schemeData), CultureInfo.InvariantCulture);
            this.NavigationButtonStyleFontName = BaseAutoFormat<CreateUserWizard>.GetStringProperty("NavigationButtonStyleFontName", schemeData);
            this.NavigationButtonStyleFontSize = new FontUnit(BaseAutoFormat<CreateUserWizard>.GetStringProperty("NavigationButtonStyleFontSize", schemeData), CultureInfo.InvariantCulture);
            this.NavigationButtonStyleBorderStyle = (BorderStyle) BaseAutoFormat<CreateUserWizard>.GetIntProperty("NavigationButtonStyleBorderStyle", schemeData);
            this.NavigationButtonStyleBorderColor = ColorTranslator.FromHtml(BaseAutoFormat<CreateUserWizard>.GetStringProperty("NavigationButtonStyleBorderColor", schemeData));
            this.NavigationButtonStyleForeColor = ColorTranslator.FromHtml(BaseAutoFormat<CreateUserWizard>.GetStringProperty("NavigationButtonStyleForeColor", schemeData));
            this.NavigationButtonStyleBackColor = ColorTranslator.FromHtml(BaseAutoFormat<CreateUserWizard>.GetStringProperty("NavigationButtonStyleBackColor", schemeData));
            this.StepStyleBorderWidth = new Unit(BaseAutoFormat<CreateUserWizard>.GetStringProperty("StepStyleBorderWidth", schemeData), CultureInfo.InvariantCulture);
            this.StepStyleBorderStyle = (BorderStyle) BaseAutoFormat<CreateUserWizard>.GetIntProperty("StepStyleBorderStyle", schemeData);
            this.StepStyleBorderColor = ColorTranslator.FromHtml(BaseAutoFormat<CreateUserWizard>.GetStringProperty("StepStyleBorderColor", schemeData));
            this.StepStyleForeColor = ColorTranslator.FromHtml(BaseAutoFormat<CreateUserWizard>.GetStringProperty("StepStyleForeColor", schemeData));
            this.StepStyleBackColor = ColorTranslator.FromHtml(BaseAutoFormat<CreateUserWizard>.GetStringProperty("StepStyleBackColor", schemeData));
            this.StepStyleFontSize = new FontUnit(BaseAutoFormat<CreateUserWizard>.GetStringProperty("StepStyleFontSize", schemeData), CultureInfo.InvariantCulture);
            this.SideBarButtonStyleFontUnderline = BaseAutoFormat<CreateUserWizard>.GetBooleanProperty("SideBarButtonStyleFontUnderline", schemeData);
            this.SideBarButtonStyleFontName = BaseAutoFormat<CreateUserWizard>.GetStringProperty("SideBarButtonStyleFontName", schemeData);
            this.SideBarButtonStyleForeColor = ColorTranslator.FromHtml(BaseAutoFormat<CreateUserWizard>.GetStringProperty("SideBarButtonStyleForeColor", schemeData));
            this.SideBarButtonStyleBorderWidth = new Unit(BaseAutoFormat<CreateUserWizard>.GetStringProperty("SideBarButtonStyleBorderWidth", schemeData), CultureInfo.InvariantCulture);
            this.SideBarButtonStyleBackColor = ColorTranslator.FromHtml(BaseAutoFormat<CreateUserWizard>.GetStringProperty("SideBarButtonStyleBackColor", schemeData));
            this.HeaderStyleForeColor = ColorTranslator.FromHtml(BaseAutoFormat<CreateUserWizard>.GetStringProperty("HeaderStyleForeColor", schemeData));
            this.HeaderStyleBorderColor = ColorTranslator.FromHtml(BaseAutoFormat<CreateUserWizard>.GetStringProperty("HeaderStyleBorderColor", schemeData));
            this.HeaderStyleBackColor = ColorTranslator.FromHtml(BaseAutoFormat<CreateUserWizard>.GetStringProperty("HeaderStyleBackColor", schemeData));
            this.HeaderStyleFontSize = new FontUnit(BaseAutoFormat<CreateUserWizard>.GetStringProperty("HeaderStyleFontSize", schemeData), CultureInfo.InvariantCulture);
            this.HeaderStyleFontBold = BaseAutoFormat<CreateUserWizard>.GetBooleanProperty("HeaderStyleFontBold", schemeData);
            this.HeaderStyleBorderWidth = new Unit(BaseAutoFormat<CreateUserWizard>.GetStringProperty("HeaderStyleBorderWidth", schemeData), CultureInfo.InvariantCulture);
            this.HeaderStyleHorizontalAlign = (HorizontalAlign) BaseAutoFormat<CreateUserWizard>.GetIntProperty("HeaderStyleHorizontalAlign", schemeData);
            this.HeaderStyleBorderStyle = (BorderStyle) BaseAutoFormat<CreateUserWizard>.GetIntProperty("HeaderStyleBorderStyle", schemeData);
            this.SideBarStyleBackColor = ColorTranslator.FromHtml(BaseAutoFormat<CreateUserWizard>.GetStringProperty("SideBarStyleBackColor", schemeData));
            this.SideBarStyleVerticalAlign = (VerticalAlign) BaseAutoFormat<CreateUserWizard>.GetIntProperty("SideBarStyleVerticalAlign", schemeData);
            this.SideBarStyleFontSize = new FontUnit(BaseAutoFormat<CreateUserWizard>.GetStringProperty("SideBarStyleFontSize", schemeData), CultureInfo.InvariantCulture);
            this.SideBarStyleFontUnderline = BaseAutoFormat<CreateUserWizard>.GetBooleanProperty("SideBarStyleFontUnderline", schemeData);
            this.SideBarStyleFontStrikeout = BaseAutoFormat<CreateUserWizard>.GetBooleanProperty("SideBarStyleFontStrikeout", schemeData);
            this.SideBarStyleBorderWidth = new Unit(BaseAutoFormat<CreateUserWizard>.GetStringProperty("SideBarStyleBorderWidth", schemeData), CultureInfo.InvariantCulture);
        }
    }
}

