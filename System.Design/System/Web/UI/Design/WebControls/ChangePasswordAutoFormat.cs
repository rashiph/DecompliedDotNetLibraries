namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Data;
    using System.Drawing;
    using System.Globalization;
    using System.Web.UI.WebControls;

    internal sealed class ChangePasswordAutoFormat : BaseAutoFormat<ChangePassword>
    {
        private string _backColor;
        private string _borderColor;
        private int _borderPadding;
        private int _borderStyle;
        private string _borderWidth;
        private string _buttonBackColor;
        private string _buttonBorderColor;
        private int _buttonBorderStyle;
        private string _buttonBorderWidth;
        private string _buttonFontName;
        private string _buttonFontSize;
        private string _buttonForeColor;
        private string _fontName;
        private string _fontSize;
        private int _instructionTextFont;
        private string _instructionTextForeColor;
        private int _passwordHintFont;
        private string _passwordHintForeColor;
        private string _renderOuterTable;
        private string _textboxFontSize;
        private string _titleTextBackColor;
        private int _titleTextFont;
        private string _titleTextFontSize;
        private string _titleTextForeColor;
        private const int FONT_BOLD = 1;
        private const int FONT_ITALIC = 2;

        public ChangePasswordAutoFormat(string schemeName, string schemes) : base(schemeName, schemes)
        {
            this._borderStyle = -1;
            this._borderPadding = 1;
            this._buttonBorderStyle = -1;
            base.Style.Width = 400;
            base.Style.Height = 250;
        }

        protected override void Apply(ChangePassword changePassword)
        {
            changePassword.BackColor = ColorTranslator.FromHtml(this._backColor);
            changePassword.BorderColor = ColorTranslator.FromHtml(this._borderColor);
            changePassword.BorderWidth = new Unit(this._borderWidth, CultureInfo.InvariantCulture);
            if ((this._borderStyle >= 0) && (this._borderStyle <= 9))
            {
                changePassword.BorderStyle = (BorderStyle) this._borderStyle;
            }
            else
            {
                changePassword.BorderStyle = BorderStyle.NotSet;
            }
            changePassword.Font.Size = new FontUnit(this._fontSize, CultureInfo.InvariantCulture);
            changePassword.Font.Name = this._fontName;
            changePassword.Font.ClearDefaults();
            changePassword.TitleTextStyle.BackColor = ColorTranslator.FromHtml(this._titleTextBackColor);
            changePassword.TitleTextStyle.ForeColor = ColorTranslator.FromHtml(this._titleTextForeColor);
            changePassword.TitleTextStyle.Font.Bold = (this._titleTextFont & 1) != 0;
            changePassword.TitleTextStyle.Font.Size = new FontUnit(this._titleTextFontSize, CultureInfo.InvariantCulture);
            changePassword.TitleTextStyle.Font.ClearDefaults();
            changePassword.BorderPadding = this._borderPadding;
            changePassword.InstructionTextStyle.ForeColor = ColorTranslator.FromHtml(this._instructionTextForeColor);
            changePassword.InstructionTextStyle.Font.Italic = (this._instructionTextFont & 2) != 0;
            changePassword.InstructionTextStyle.Font.ClearDefaults();
            changePassword.TextBoxStyle.Font.Size = new FontUnit(this._textboxFontSize, CultureInfo.InvariantCulture);
            changePassword.TextBoxStyle.Font.ClearDefaults();
            changePassword.ChangePasswordButtonStyle.BackColor = ColorTranslator.FromHtml(this._buttonBackColor);
            changePassword.ChangePasswordButtonStyle.ForeColor = ColorTranslator.FromHtml(this._buttonForeColor);
            changePassword.ChangePasswordButtonStyle.Font.Size = new FontUnit(this._buttonFontSize, CultureInfo.InvariantCulture);
            changePassword.ChangePasswordButtonStyle.Font.Name = this._buttonFontName;
            changePassword.ChangePasswordButtonStyle.BorderColor = ColorTranslator.FromHtml(this._buttonBorderColor);
            changePassword.ChangePasswordButtonStyle.BorderWidth = new Unit(this._buttonBorderWidth, CultureInfo.InvariantCulture);
            if ((this._buttonBorderStyle >= 0) && (this._buttonBorderStyle <= 9))
            {
                changePassword.ChangePasswordButtonStyle.BorderStyle = (BorderStyle) this._buttonBorderStyle;
            }
            else
            {
                changePassword.ChangePasswordButtonStyle.BorderStyle = BorderStyle.NotSet;
            }
            changePassword.ChangePasswordButtonStyle.Font.ClearDefaults();
            changePassword.ContinueButtonStyle.BackColor = ColorTranslator.FromHtml(this._buttonBackColor);
            changePassword.ContinueButtonStyle.ForeColor = ColorTranslator.FromHtml(this._buttonForeColor);
            changePassword.ContinueButtonStyle.Font.Size = new FontUnit(this._buttonFontSize, CultureInfo.InvariantCulture);
            changePassword.ContinueButtonStyle.Font.Name = this._buttonFontName;
            changePassword.ContinueButtonStyle.BorderColor = ColorTranslator.FromHtml(this._buttonBorderColor);
            changePassword.ContinueButtonStyle.BorderWidth = new Unit(this._buttonBorderWidth, CultureInfo.InvariantCulture);
            if ((this._buttonBorderStyle >= 0) && (this._buttonBorderStyle <= 9))
            {
                changePassword.ContinueButtonStyle.BorderStyle = (BorderStyle) this._buttonBorderStyle;
            }
            else
            {
                changePassword.ContinueButtonStyle.BorderStyle = BorderStyle.NotSet;
            }
            changePassword.ContinueButtonStyle.Font.ClearDefaults();
            changePassword.CancelButtonStyle.BackColor = ColorTranslator.FromHtml(this._buttonBackColor);
            changePassword.CancelButtonStyle.ForeColor = ColorTranslator.FromHtml(this._buttonForeColor);
            changePassword.CancelButtonStyle.Font.Size = new FontUnit(this._buttonFontSize, CultureInfo.InvariantCulture);
            changePassword.CancelButtonStyle.Font.Name = this._buttonFontName;
            changePassword.CancelButtonStyle.BorderColor = ColorTranslator.FromHtml(this._buttonBorderColor);
            changePassword.CancelButtonStyle.BorderWidth = new Unit(this._buttonBorderWidth, CultureInfo.InvariantCulture);
            if ((this._buttonBorderStyle >= 0) && (this._buttonBorderStyle <= 9))
            {
                changePassword.CancelButtonStyle.BorderStyle = (BorderStyle) this._buttonBorderStyle;
            }
            else
            {
                changePassword.CancelButtonStyle.BorderStyle = BorderStyle.NotSet;
            }
            changePassword.CancelButtonStyle.Font.ClearDefaults();
            changePassword.PasswordHintStyle.ForeColor = ColorTranslator.FromHtml(this._passwordHintForeColor);
            changePassword.PasswordHintStyle.Font.Italic = (this._passwordHintFont & 2) != 0;
            changePassword.PasswordHintStyle.Font.ClearDefaults();
            changePassword.RenderOuterTable = bool.Parse(this._renderOuterTable);
        }

        protected override void Initialize(DataRow schemeData)
        {
            this._backColor = BaseAutoFormat<ChangePassword>.GetStringProperty("BackColor", schemeData);
            this._borderColor = BaseAutoFormat<ChangePassword>.GetStringProperty("BorderColor", schemeData);
            this._borderWidth = BaseAutoFormat<ChangePassword>.GetStringProperty("BorderWidth", schemeData);
            this._borderStyle = BaseAutoFormat<ChangePassword>.GetIntProperty("BorderStyle", -1, schemeData);
            this._fontSize = BaseAutoFormat<ChangePassword>.GetStringProperty("FontSize", schemeData);
            this._fontName = BaseAutoFormat<ChangePassword>.GetStringProperty("FontName", schemeData);
            this._titleTextBackColor = BaseAutoFormat<ChangePassword>.GetStringProperty("TitleTextBackColor", schemeData);
            this._titleTextForeColor = BaseAutoFormat<ChangePassword>.GetStringProperty("TitleTextForeColor", schemeData);
            this._titleTextFont = BaseAutoFormat<ChangePassword>.GetIntProperty("TitleTextFont", schemeData);
            this._titleTextFontSize = BaseAutoFormat<ChangePassword>.GetStringProperty("TitleTextFontSize", schemeData);
            this._instructionTextForeColor = BaseAutoFormat<ChangePassword>.GetStringProperty("InstructionTextForeColor", schemeData);
            this._instructionTextFont = BaseAutoFormat<ChangePassword>.GetIntProperty("InstructionTextFont", schemeData);
            this._borderPadding = BaseAutoFormat<ChangePassword>.GetIntProperty("BorderPadding", 1, schemeData);
            this._textboxFontSize = BaseAutoFormat<ChangePassword>.GetStringProperty("TextboxFontSize", schemeData);
            this._buttonBackColor = BaseAutoFormat<ChangePassword>.GetStringProperty("ButtonBackColor", schemeData);
            this._buttonForeColor = BaseAutoFormat<ChangePassword>.GetStringProperty("ButtonForeColor", schemeData);
            this._buttonFontSize = BaseAutoFormat<ChangePassword>.GetStringProperty("ButtonFontSize", schemeData);
            this._buttonFontName = BaseAutoFormat<ChangePassword>.GetStringProperty("ButtonFontName", schemeData);
            this._buttonBorderColor = BaseAutoFormat<ChangePassword>.GetStringProperty("ButtonBorderColor", schemeData);
            this._buttonBorderWidth = BaseAutoFormat<ChangePassword>.GetStringProperty("ButtonBorderWidth", schemeData);
            this._buttonBorderStyle = BaseAutoFormat<ChangePassword>.GetIntProperty("ButtonBorderStyle", -1, schemeData);
            this._passwordHintForeColor = BaseAutoFormat<ChangePassword>.GetStringProperty("PasswordHintForeColor", schemeData);
            this._passwordHintFont = BaseAutoFormat<ChangePassword>.GetIntProperty("PasswordHintFont", schemeData);
            this._renderOuterTable = BaseAutoFormat<ChangePassword>.GetStringProperty("RenderOuterTable", schemeData);
        }
    }
}

