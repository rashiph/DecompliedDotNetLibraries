namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Data;
    using System.Drawing;
    using System.Globalization;
    using System.Web.UI.WebControls;

    internal sealed class LoginAutoFormat : BaseAutoFormat<Login>
    {
        private string _loginButtonBackColor;
        private string _loginButtonBorderColor;
        private int _loginButtonBorderStyle;
        private string _loginButtonBorderWidth;
        private string _loginButtonFontName;
        private string _loginButtonFontSize;
        private string _loginButtonForeColor;
        private string _renderOuterTable;
        private string backColor;
        private string borderColor;
        private int borderPadding;
        private int borderStyle;
        private string borderWidth;
        private const int FONT_BOLD = 1;
        private const int FONT_ITALIC = 2;
        private string fontName;
        private string fontSize;
        private string foreColor;
        private int instructionTextFont;
        private string instructionTextForeColor;
        private string textboxFontSize;
        private int textLayout;
        private string titleTextBackColor;
        private int titleTextFont;
        private string titleTextFontSize;
        private string titleTextForeColor;

        public LoginAutoFormat(string schemeName, string schemes) : base(schemeName, schemes)
        {
            this.borderStyle = -1;
            this._loginButtonBorderStyle = -1;
            base.Style.Width = 300;
            base.Style.Height = 200;
        }

        protected override void Apply(Login login)
        {
            login.BackColor = ColorTranslator.FromHtml(this.backColor);
            login.ForeColor = ColorTranslator.FromHtml(this.foreColor);
            login.BorderColor = ColorTranslator.FromHtml(this.borderColor);
            login.BorderWidth = new Unit(this.borderWidth, CultureInfo.InvariantCulture);
            if ((this.borderStyle >= 0) && (this.borderStyle <= 9))
            {
                login.BorderStyle = (BorderStyle) this.borderStyle;
            }
            else
            {
                login.BorderStyle = BorderStyle.NotSet;
            }
            login.Font.Size = new FontUnit(this.fontSize, CultureInfo.InvariantCulture);
            login.Font.Name = this.fontName;
            login.Font.ClearDefaults();
            login.TitleTextStyle.BackColor = ColorTranslator.FromHtml(this.titleTextBackColor);
            login.TitleTextStyle.ForeColor = ColorTranslator.FromHtml(this.titleTextForeColor);
            login.TitleTextStyle.Font.Bold = (this.titleTextFont & 1) != 0;
            login.TitleTextStyle.Font.Size = new FontUnit(this.titleTextFontSize, CultureInfo.InvariantCulture);
            login.TitleTextStyle.Font.ClearDefaults();
            login.BorderPadding = this.borderPadding;
            if (this.textLayout > 0)
            {
                login.TextLayout = LoginTextLayout.TextOnTop;
            }
            else
            {
                login.TextLayout = LoginTextLayout.TextOnLeft;
            }
            login.InstructionTextStyle.ForeColor = ColorTranslator.FromHtml(this.instructionTextForeColor);
            login.InstructionTextStyle.Font.Italic = (this.instructionTextFont & 2) != 0;
            login.InstructionTextStyle.Font.ClearDefaults();
            login.TextBoxStyle.Font.Size = new FontUnit(this.textboxFontSize, CultureInfo.InvariantCulture);
            login.TextBoxStyle.Font.ClearDefaults();
            login.LoginButtonStyle.BackColor = ColorTranslator.FromHtml(this._loginButtonBackColor);
            login.LoginButtonStyle.ForeColor = ColorTranslator.FromHtml(this._loginButtonForeColor);
            login.LoginButtonStyle.Font.Size = new FontUnit(this._loginButtonFontSize, CultureInfo.InvariantCulture);
            login.LoginButtonStyle.Font.Name = this._loginButtonFontName;
            login.LoginButtonStyle.BorderColor = ColorTranslator.FromHtml(this._loginButtonBorderColor);
            login.LoginButtonStyle.BorderWidth = new Unit(this._loginButtonBorderWidth, CultureInfo.InvariantCulture);
            if ((this._loginButtonBorderStyle >= 0) && (this._loginButtonBorderStyle <= 9))
            {
                login.LoginButtonStyle.BorderStyle = (BorderStyle) this._loginButtonBorderStyle;
            }
            else
            {
                login.LoginButtonStyle.BorderStyle = BorderStyle.NotSet;
            }
            login.LoginButtonStyle.Font.ClearDefaults();
            login.RenderOuterTable = bool.Parse(this._renderOuterTable);
        }

        protected override void Initialize(DataRow schemeData)
        {
            this.backColor = BaseAutoFormat<Login>.GetStringProperty("BackColor", schemeData);
            this.foreColor = BaseAutoFormat<Login>.GetStringProperty("ForeColor", schemeData);
            this.borderColor = BaseAutoFormat<Login>.GetStringProperty("BorderColor", schemeData);
            this.borderWidth = BaseAutoFormat<Login>.GetStringProperty("BorderWidth", schemeData);
            this.borderStyle = BaseAutoFormat<Login>.GetIntProperty("BorderStyle", -1, schemeData);
            this.fontSize = BaseAutoFormat<Login>.GetStringProperty("FontSize", schemeData);
            this.fontName = BaseAutoFormat<Login>.GetStringProperty("FontName", schemeData);
            this.instructionTextForeColor = BaseAutoFormat<Login>.GetStringProperty("InstructionTextForeColor", schemeData);
            this.instructionTextFont = BaseAutoFormat<Login>.GetIntProperty("InstructionTextFont", schemeData);
            this.titleTextBackColor = BaseAutoFormat<Login>.GetStringProperty("TitleTextBackColor", schemeData);
            this.titleTextForeColor = BaseAutoFormat<Login>.GetStringProperty("TitleTextForeColor", schemeData);
            this.titleTextFont = BaseAutoFormat<Login>.GetIntProperty("TitleTextFont", schemeData);
            this.titleTextFontSize = BaseAutoFormat<Login>.GetStringProperty("TitleTextFontSize", schemeData);
            this.borderPadding = BaseAutoFormat<Login>.GetIntProperty("BorderPadding", 1, schemeData);
            this.textLayout = BaseAutoFormat<Login>.GetIntProperty("TextLayout", schemeData);
            this.textboxFontSize = BaseAutoFormat<Login>.GetStringProperty("TextboxFontSize", schemeData);
            this._loginButtonBackColor = BaseAutoFormat<Login>.GetStringProperty("SubmitButtonBackColor", schemeData);
            this._loginButtonForeColor = BaseAutoFormat<Login>.GetStringProperty("SubmitButtonForeColor", schemeData);
            this._loginButtonFontSize = BaseAutoFormat<Login>.GetStringProperty("SubmitButtonFontSize", schemeData);
            this._loginButtonFontName = BaseAutoFormat<Login>.GetStringProperty("SubmitButtonFontName", schemeData);
            this._loginButtonBorderColor = BaseAutoFormat<Login>.GetStringProperty("SubmitButtonBorderColor", schemeData);
            this._loginButtonBorderWidth = BaseAutoFormat<Login>.GetStringProperty("SubmitButtonBorderWidth", schemeData);
            this._loginButtonBorderStyle = BaseAutoFormat<Login>.GetIntProperty("SubmitButtonBorderStyle", -1, schemeData);
            this._renderOuterTable = BaseAutoFormat<Login>.GetStringProperty("RenderOuterTable", schemeData);
        }
    }
}

