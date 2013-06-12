namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI;

    [ToolboxData("<{0}:RegularExpressionValidator runat=\"server\" ErrorMessage=\"RegularExpressionValidator\"></{0}:RegularExpressionValidator>")]
    public class RegularExpressionValidator : BaseValidator
    {
        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            base.AddAttributesToRender(writer);
            if (base.RenderUplevel)
            {
                string clientID = this.ClientID;
                HtmlTextWriter writer2 = base.EnableLegacyRendering ? writer : null;
                base.AddExpandoAttribute(writer2, clientID, "evaluationfunction", "RegularExpressionValidatorEvaluateIsValid", false);
                if (this.ValidationExpression.Length > 0)
                {
                    base.AddExpandoAttribute(writer2, clientID, "validationexpression", this.ValidationExpression);
                }
            }
        }

        protected override bool EvaluateIsValid()
        {
            string controlValidationValue = base.GetControlValidationValue(base.ControlToValidate);
            if ((controlValidationValue == null) || (controlValidationValue.Trim().Length == 0))
            {
                return true;
            }
            try
            {
                Match match = Regex.Match(controlValidationValue, this.ValidationExpression);
                return ((match.Success && (match.Index == 0)) && (match.Length == controlValidationValue.Length));
            }
            catch
            {
                return true;
            }
        }

        [WebCategory("Behavior"), WebSysDescription("RegularExpressionValidator_ValidationExpression"), Themeable(false), DefaultValue(""), Editor("System.Web.UI.Design.WebControls.RegexTypeEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string ValidationExpression
        {
            get
            {
                object obj2 = this.ViewState["ValidationExpression"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                try
                {
                    Regex.IsMatch(string.Empty, value);
                }
                catch (Exception exception)
                {
                    throw new HttpException(System.Web.SR.GetString("Validator_bad_regex", new object[] { value }), exception);
                }
                this.ViewState["ValidationExpression"] = value;
            }
        }
    }
}

