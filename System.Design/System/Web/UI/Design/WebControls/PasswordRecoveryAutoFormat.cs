namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Data;
    using System.Drawing;
    using System.Globalization;
    using System.Web.UI.WebControls;

    internal sealed class PasswordRecoveryAutoFormat : BaseAutoFormat<PasswordRecovery>
    {
        private string backColor;
        private string borderColor;
        private int borderPadding;
        private int borderStyle;
        private string borderWidth;
        private const int FONT_BOLD = 1;
        private const int FONT_ITALIC = 2;
        private string fontName;
        private string fontSize;
        private int instructionTextFont;
        private string instructionTextForeColor;
        private string renderOuterTable;
        private string submitButtonBackColor;
        private string submitButtonBorderColor;
        private int submitButtonBorderStyle;
        private string submitButtonBorderWidth;
        private string submitButtonFontName;
        private string submitButtonFontSize;
        private string submitButtonForeColor;
        private int successTextFont;
        private string successTextForeColor;
        private string textboxFontSize;
        private string titleTextBackColor;
        private int titleTextFont;
        private string titleTextFontSize;
        private string titleTextForeColor;

        public PasswordRecoveryAutoFormat(string schemeName, string schemes) : base(schemeName, schemes)
        {
            this.borderStyle = -1;
            this.borderPadding = 1;
            this.submitButtonBorderStyle = -1;
            base.Style.Width = 500;
            base.Style.Height = 300;
        }

        protected override void Apply(PasswordRecovery passwordRecovery)
        {
            passwordRecovery.BackColor = ColorTranslator.FromHtml(this.backColor);
            passwordRecovery.BorderColor = ColorTranslator.FromHtml(this.borderColor);
            passwordRecovery.BorderWidth = new Unit(this.borderWidth, CultureInfo.InvariantCulture);
            if ((this.borderStyle >= 0) && (this.borderStyle <= 9))
            {
                passwordRecovery.BorderStyle = (BorderStyle) this.borderStyle;
            }
            else
            {
                passwordRecovery.BorderStyle = BorderStyle.NotSet;
            }
            passwordRecovery.Font.Size = new FontUnit(this.fontSize, CultureInfo.InvariantCulture);
            passwordRecovery.Font.Name = this.fontName;
            passwordRecovery.Font.ClearDefaults();
            passwordRecovery.TitleTextStyle.BackColor = ColorTranslator.FromHtml(this.titleTextBackColor);
            passwordRecovery.TitleTextStyle.ForeColor = ColorTranslator.FromHtml(this.titleTextForeColor);
            passwordRecovery.TitleTextStyle.Font.Bold = (this.titleTextFont & 1) != 0;
            passwordRecovery.TitleTextStyle.Font.Size = new FontUnit(this.titleTextFontSize, CultureInfo.InvariantCulture);
            passwordRecovery.TitleTextStyle.Font.ClearDefaults();
            passwordRecovery.BorderPadding = this.borderPadding;
            passwordRecovery.InstructionTextStyle.ForeColor = ColorTranslator.FromHtml(this.instructionTextForeColor);
            passwordRecovery.InstructionTextStyle.Font.Italic = (this.instructionTextFont & 2) != 0;
            passwordRecovery.InstructionTextStyle.Font.ClearDefaults();
            passwordRecovery.TextBoxStyle.Font.Size = new FontUnit(this.textboxFontSize, CultureInfo.InvariantCulture);
            passwordRecovery.TextBoxStyle.Font.ClearDefaults();
            passwordRecovery.SubmitButtonStyle.BackColor = ColorTranslator.FromHtml(this.submitButtonBackColor);
            passwordRecovery.SubmitButtonStyle.ForeColor = ColorTranslator.FromHtml(this.submitButtonForeColor);
            passwordRecovery.SubmitButtonStyle.Font.Size = new FontUnit(this.submitButtonFontSize, CultureInfo.InvariantCulture);
            passwordRecovery.SubmitButtonStyle.Font.Name = this.submitButtonFontName;
            passwordRecovery.SubmitButtonStyle.BorderColor = ColorTranslator.FromHtml(this.submitButtonBorderColor);
            passwordRecovery.SubmitButtonStyle.BorderWidth = new Unit(this.submitButtonBorderWidth, CultureInfo.InvariantCulture);
            if ((this.submitButtonBorderStyle >= 0) && (this.submitButtonBorderStyle <= 9))
            {
                passwordRecovery.SubmitButtonStyle.BorderStyle = (BorderStyle) this.submitButtonBorderStyle;
            }
            else
            {
                passwordRecovery.SubmitButtonStyle.BorderStyle = BorderStyle.NotSet;
            }
            passwordRecovery.SubmitButtonStyle.Font.ClearDefaults();
            passwordRecovery.SuccessTextStyle.ForeColor = ColorTranslator.FromHtml(this.successTextForeColor);
            passwordRecovery.SuccessTextStyle.Font.Bold = (this.successTextFont & 1) != 0;
            passwordRecovery.SuccessTextStyle.Font.ClearDefaults();
            passwordRecovery.RenderOuterTable = bool.Parse(this.renderOuterTable);
        }

        protected override void Initialize(DataRow schemeData)
        {
            this.backColor = BaseAutoFormat<PasswordRecovery>.GetStringProperty("BackColor", schemeData);
            this.borderColor = BaseAutoFormat<PasswordRecovery>.GetStringProperty("BorderColor", schemeData);
            this.borderWidth = BaseAutoFormat<PasswordRecovery>.GetStringProperty("BorderWidth", schemeData);
            this.borderStyle = BaseAutoFormat<PasswordRecovery>.GetIntProperty("BorderStyle", -1, schemeData);
            this.fontSize = BaseAutoFormat<PasswordRecovery>.GetStringProperty("FontSize", schemeData);
            this.fontName = BaseAutoFormat<PasswordRecovery>.GetStringProperty("FontName", schemeData);
            this.titleTextBackColor = BaseAutoFormat<PasswordRecovery>.GetStringProperty("TitleTextBackColor", schemeData);
            this.titleTextForeColor = BaseAutoFormat<PasswordRecovery>.GetStringProperty("TitleTextForeColor", schemeData);
            this.titleTextFont = BaseAutoFormat<PasswordRecovery>.GetIntProperty("TitleTextFont", schemeData);
            this.titleTextFontSize = BaseAutoFormat<PasswordRecovery>.GetStringProperty("TitleTextFontSize", schemeData);
            this.instructionTextForeColor = BaseAutoFormat<PasswordRecovery>.GetStringProperty("InstructionTextForeColor", schemeData);
            this.instructionTextFont = BaseAutoFormat<PasswordRecovery>.GetIntProperty("InstructionTextFont", schemeData);
            this.borderPadding = BaseAutoFormat<PasswordRecovery>.GetIntProperty("BorderPadding", 1, schemeData);
            this.textboxFontSize = BaseAutoFormat<PasswordRecovery>.GetStringProperty("TextboxFontSize", schemeData);
            this.submitButtonBackColor = BaseAutoFormat<PasswordRecovery>.GetStringProperty("SubmitButtonBackColor", schemeData);
            this.submitButtonForeColor = BaseAutoFormat<PasswordRecovery>.GetStringProperty("SubmitButtonForeColor", schemeData);
            this.submitButtonFontSize = BaseAutoFormat<PasswordRecovery>.GetStringProperty("SubmitButtonFontSize", schemeData);
            this.submitButtonFontName = BaseAutoFormat<PasswordRecovery>.GetStringProperty("SubmitButtonFontName", schemeData);
            this.submitButtonBorderColor = BaseAutoFormat<PasswordRecovery>.GetStringProperty("SubmitButtonBorderColor", schemeData);
            this.submitButtonBorderWidth = BaseAutoFormat<PasswordRecovery>.GetStringProperty("SubmitButtonBorderWidth", schemeData);
            this.submitButtonBorderStyle = BaseAutoFormat<PasswordRecovery>.GetIntProperty("SubmitButtonBorderStyle", -1, schemeData);
            this.successTextForeColor = BaseAutoFormat<PasswordRecovery>.GetStringProperty("SuccessTextForeColor", schemeData);
            this.successTextFont = BaseAutoFormat<PasswordRecovery>.GetIntProperty("SuccessTextFont", schemeData);
            this.renderOuterTable = BaseAutoFormat<PasswordRecovery>.GetStringProperty("RenderOuterTable", schemeData);
        }
    }
}

