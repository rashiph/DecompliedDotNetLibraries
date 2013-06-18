namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Data;
    using System.Drawing;
    using System.Globalization;
    using System.Web.UI.WebControls;

    internal sealed class WizardAutoFormat : BaseAutoFormat<Wizard>
    {
        private Color BackColor;
        private Color BorderColor;
        private System.Web.UI.WebControls.BorderStyle BorderStyle;
        private Unit BorderWidth;
        private string FontName;
        private FontUnit FontSize;
        private Color HeaderStyleBackColor;
        private Color HeaderStyleBorderColor;
        private System.Web.UI.WebControls.BorderStyle HeaderStyleBorderStyle;
        private Unit HeaderStyleBorderWidth;
        private bool HeaderStyleFontBold;
        private FontUnit HeaderStyleFontSize;
        private Color HeaderStyleForeColor;
        private HorizontalAlign HeaderStyleHorizontalAlign;
        private Color NavigationButtonStyleBackColor;
        private Color NavigationButtonStyleBorderColor;
        private System.Web.UI.WebControls.BorderStyle NavigationButtonStyleBorderStyle;
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
        private System.Web.UI.WebControls.BorderStyle StepStyleBorderStyle;
        private Unit StepStyleBorderWidth;
        private FontUnit StepStyleFontSize;
        private Color StepStyleForeColor;

        public WizardAutoFormat(string schemeName, string schemes) : base(schemeName, schemes)
        {
            base.Style.Width = 350;
            base.Style.Height = 200;
        }

        protected override void Apply(Wizard wizard)
        {
            wizard.Font.Name = this.FontName;
            wizard.Font.Size = this.FontSize;
            wizard.BackColor = this.BackColor;
            wizard.BorderColor = this.BorderColor;
            wizard.BorderWidth = this.BorderWidth;
            wizard.BorderStyle = this.BorderStyle;
            wizard.Font.ClearDefaults();
            wizard.NavigationButtonStyle.BorderWidth = this.NavigationButtonStyleBorderWidth;
            wizard.NavigationButtonStyle.Font.Name = this.NavigationButtonStyleFontName;
            wizard.NavigationButtonStyle.Font.Size = this.NavigationButtonStyleFontSize;
            wizard.NavigationButtonStyle.BorderStyle = this.NavigationButtonStyleBorderStyle;
            wizard.NavigationButtonStyle.BorderColor = this.NavigationButtonStyleBorderColor;
            wizard.NavigationButtonStyle.ForeColor = this.NavigationButtonStyleForeColor;
            wizard.NavigationButtonStyle.BackColor = this.NavigationButtonStyleBackColor;
            wizard.NavigationButtonStyle.Font.ClearDefaults();
            wizard.StepStyle.BorderWidth = this.StepStyleBorderWidth;
            wizard.StepStyle.BorderStyle = this.StepStyleBorderStyle;
            wizard.StepStyle.BorderColor = this.StepStyleBorderColor;
            wizard.StepStyle.ForeColor = this.StepStyleForeColor;
            wizard.StepStyle.BackColor = this.StepStyleBackColor;
            wizard.StepStyle.Font.Size = this.StepStyleFontSize;
            wizard.StepStyle.Font.ClearDefaults();
            wizard.SideBarButtonStyle.Font.Underline = this.SideBarButtonStyleFontUnderline;
            wizard.SideBarButtonStyle.Font.Name = this.SideBarButtonStyleFontName;
            wizard.SideBarButtonStyle.ForeColor = this.SideBarButtonStyleForeColor;
            wizard.SideBarButtonStyle.BorderWidth = this.SideBarButtonStyleBorderWidth;
            wizard.SideBarButtonStyle.BackColor = this.SideBarButtonStyleBackColor;
            wizard.SideBarButtonStyle.Font.ClearDefaults();
            wizard.HeaderStyle.ForeColor = this.HeaderStyleForeColor;
            wizard.HeaderStyle.BorderColor = this.HeaderStyleBorderColor;
            wizard.HeaderStyle.BackColor = this.HeaderStyleBackColor;
            wizard.HeaderStyle.Font.Size = this.HeaderStyleFontSize;
            wizard.HeaderStyle.Font.Bold = this.HeaderStyleFontBold;
            wizard.HeaderStyle.BorderWidth = this.HeaderStyleBorderWidth;
            wizard.HeaderStyle.HorizontalAlign = this.HeaderStyleHorizontalAlign;
            wizard.HeaderStyle.BorderStyle = this.HeaderStyleBorderStyle;
            wizard.HeaderStyle.Font.ClearDefaults();
            wizard.SideBarStyle.BackColor = this.SideBarStyleBackColor;
            wizard.SideBarStyle.VerticalAlign = this.SideBarStyleVerticalAlign;
            wizard.SideBarStyle.Font.Size = this.SideBarStyleFontSize;
            wizard.SideBarStyle.Font.Underline = this.SideBarStyleFontUnderline;
            wizard.SideBarStyle.Font.Strikeout = this.SideBarStyleFontStrikeout;
            wizard.SideBarStyle.BorderWidth = this.SideBarStyleBorderWidth;
            wizard.SideBarStyle.Font.ClearDefaults();
        }

        protected override void Initialize(DataRow schemeData)
        {
            if (schemeData != null)
            {
                this.FontName = BaseAutoFormat<Wizard>.GetStringProperty("FontName", schemeData);
                this.FontSize = new FontUnit(BaseAutoFormat<Wizard>.GetStringProperty("FontSize", schemeData), CultureInfo.InvariantCulture);
                this.BackColor = ColorTranslator.FromHtml(BaseAutoFormat<Wizard>.GetStringProperty("BackColor", schemeData));
                this.BorderColor = ColorTranslator.FromHtml(BaseAutoFormat<Wizard>.GetStringProperty("BorderColor", schemeData));
                this.BorderWidth = new Unit(BaseAutoFormat<Wizard>.GetStringProperty("BorderWidth", schemeData), CultureInfo.InvariantCulture);
                this.SideBarStyleBackColor = ColorTranslator.FromHtml(BaseAutoFormat<Wizard>.GetStringProperty("SideBarStyleBackColor", schemeData));
                this.SideBarStyleVerticalAlign = (VerticalAlign) BaseAutoFormat<Wizard>.GetIntProperty("SideBarStyleVerticalAlign", schemeData);
                this.BorderStyle = (System.Web.UI.WebControls.BorderStyle) BaseAutoFormat<Wizard>.GetIntProperty("BorderStyle", schemeData);
                this.NavigationButtonStyleBorderWidth = new Unit(BaseAutoFormat<Wizard>.GetStringProperty("NavigationButtonStyleBorderWidth", schemeData), CultureInfo.InvariantCulture);
                this.NavigationButtonStyleFontName = BaseAutoFormat<Wizard>.GetStringProperty("NavigationButtonStyleFontName", schemeData);
                this.NavigationButtonStyleFontSize = new FontUnit(BaseAutoFormat<Wizard>.GetStringProperty("NavigationButtonStyleFontSize", schemeData), CultureInfo.InvariantCulture);
                this.NavigationButtonStyleBorderStyle = (System.Web.UI.WebControls.BorderStyle) BaseAutoFormat<Wizard>.GetIntProperty("NavigationButtonStyleBorderStyle", schemeData);
                this.NavigationButtonStyleBorderColor = ColorTranslator.FromHtml(BaseAutoFormat<Wizard>.GetStringProperty("NavigationButtonStyleBorderColor", schemeData));
                this.NavigationButtonStyleForeColor = ColorTranslator.FromHtml(BaseAutoFormat<Wizard>.GetStringProperty("NavigationButtonStyleForeColor", schemeData));
                this.NavigationButtonStyleBackColor = ColorTranslator.FromHtml(BaseAutoFormat<Wizard>.GetStringProperty("NavigationButtonStyleBackColor", schemeData));
                this.StepStyleBorderWidth = new Unit(BaseAutoFormat<Wizard>.GetStringProperty("StepStyleBorderWidth", schemeData), CultureInfo.InvariantCulture);
                this.StepStyleBorderStyle = (System.Web.UI.WebControls.BorderStyle) BaseAutoFormat<Wizard>.GetIntProperty("StepStyleBorderStyle", schemeData);
                this.StepStyleBorderColor = ColorTranslator.FromHtml(BaseAutoFormat<Wizard>.GetStringProperty("StepStyleBorderColor", schemeData));
                this.StepStyleForeColor = ColorTranslator.FromHtml(BaseAutoFormat<Wizard>.GetStringProperty("StepStyleForeColor", schemeData));
                this.StepStyleBackColor = ColorTranslator.FromHtml(BaseAutoFormat<Wizard>.GetStringProperty("StepStyleBackColor", schemeData));
                this.StepStyleFontSize = new FontUnit(BaseAutoFormat<Wizard>.GetStringProperty("StepStyleFontSize", schemeData), CultureInfo.InvariantCulture);
                this.SideBarButtonStyleFontUnderline = BaseAutoFormat<Wizard>.GetBooleanProperty("SideBarButtonStyleFontUnderline", schemeData);
                this.SideBarButtonStyleFontName = BaseAutoFormat<Wizard>.GetStringProperty("SideBarButtonStyleFontName", schemeData);
                this.SideBarButtonStyleForeColor = ColorTranslator.FromHtml(BaseAutoFormat<Wizard>.GetStringProperty("SideBarButtonStyleForeColor", schemeData));
                this.SideBarButtonStyleBorderWidth = new Unit(BaseAutoFormat<Wizard>.GetStringProperty("SideBarButtonStyleBorderWidth", schemeData), CultureInfo.InvariantCulture);
                this.SideBarButtonStyleBackColor = ColorTranslator.FromHtml(BaseAutoFormat<Wizard>.GetStringProperty("SideBarButtonStyleBackColor", schemeData));
                this.HeaderStyleForeColor = ColorTranslator.FromHtml(BaseAutoFormat<Wizard>.GetStringProperty("HeaderStyleForeColor", schemeData));
                this.HeaderStyleBorderColor = ColorTranslator.FromHtml(BaseAutoFormat<Wizard>.GetStringProperty("HeaderStyleBorderColor", schemeData));
                this.HeaderStyleBackColor = ColorTranslator.FromHtml(BaseAutoFormat<Wizard>.GetStringProperty("HeaderStyleBackColor", schemeData));
                this.HeaderStyleFontSize = new FontUnit(BaseAutoFormat<Wizard>.GetStringProperty("HeaderStyleFontSize", schemeData), CultureInfo.InvariantCulture);
                this.HeaderStyleFontBold = BaseAutoFormat<Wizard>.GetBooleanProperty("HeaderStyleFontBold", schemeData);
                this.HeaderStyleBorderWidth = new Unit(BaseAutoFormat<Wizard>.GetStringProperty("HeaderStyleBorderWidth", schemeData), CultureInfo.InvariantCulture);
                this.HeaderStyleHorizontalAlign = (HorizontalAlign) BaseAutoFormat<Wizard>.GetIntProperty("HeaderStyleHorizontalAlign", schemeData);
                this.HeaderStyleBorderStyle = (System.Web.UI.WebControls.BorderStyle) BaseAutoFormat<Wizard>.GetIntProperty("HeaderStyleBorderStyle", schemeData);
                this.SideBarStyleBackColor = ColorTranslator.FromHtml(BaseAutoFormat<Wizard>.GetStringProperty("SideBarStyleBackColor", schemeData));
                this.SideBarStyleVerticalAlign = (VerticalAlign) BaseAutoFormat<Wizard>.GetIntProperty("SideBarStyleVerticalAlign", schemeData);
                this.SideBarStyleFontSize = new FontUnit(BaseAutoFormat<Wizard>.GetStringProperty("SideBarStyleFontSize", schemeData), CultureInfo.InvariantCulture);
                this.SideBarStyleFontUnderline = BaseAutoFormat<Wizard>.GetBooleanProperty("SideBarStyleFontUnderline", schemeData);
                this.SideBarStyleFontStrikeout = BaseAutoFormat<Wizard>.GetBooleanProperty("SideBarStyleFontStrikeout", schemeData);
                this.SideBarStyleBorderWidth = new Unit(BaseAutoFormat<Wizard>.GetStringProperty("SideBarStyleBorderWidth", schemeData), CultureInfo.InvariantCulture);
            }
        }
    }
}

