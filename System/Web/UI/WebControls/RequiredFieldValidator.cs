namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    [ToolboxData("<{0}:RequiredFieldValidator runat=\"server\" ErrorMessage=\"RequiredFieldValidator\"></{0}:RequiredFieldValidator>")]
    public class RequiredFieldValidator : BaseValidator
    {
        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            base.AddAttributesToRender(writer);
            if (base.RenderUplevel)
            {
                string clientID = this.ClientID;
                HtmlTextWriter writer2 = base.EnableLegacyRendering ? writer : null;
                base.AddExpandoAttribute(writer2, clientID, "evaluationfunction", "RequiredFieldValidatorEvaluateIsValid", false);
                base.AddExpandoAttribute(writer2, clientID, "initialvalue", this.InitialValue);
            }
        }

        protected override bool EvaluateIsValid()
        {
            string controlValidationValue = base.GetControlValidationValue(base.ControlToValidate);
            return ((controlValidationValue == null) || !controlValidationValue.Trim().Equals(this.InitialValue.Trim()));
        }

        [WebSysDescription("RequiredFieldValidator_InitialValue"), WebCategory("Behavior"), Themeable(false), DefaultValue("")]
        public string InitialValue
        {
            get
            {
                object obj2 = this.ViewState["InitialValue"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["InitialValue"] = value;
            }
        }
    }
}

