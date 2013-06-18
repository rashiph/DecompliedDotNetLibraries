namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Web;
    using System.Web.UI;

    [ToolboxData("<{0}:RangeValidator runat=\"server\" ErrorMessage=\"RangeValidator\"></{0}:RangeValidator>")]
    public class RangeValidator : BaseCompareValidator
    {
        protected override void AddAttributesToRender(HtmlTextWriter writer)
        {
            base.AddAttributesToRender(writer);
            if (base.RenderUplevel)
            {
                string clientID = this.ClientID;
                HtmlTextWriter writer2 = base.EnableLegacyRendering ? writer : null;
                base.AddExpandoAttribute(writer2, clientID, "evaluationfunction", "RangeValidatorEvaluateIsValid", false);
                string maximumValue = this.MaximumValue;
                string minimumValue = this.MinimumValue;
                if (base.CultureInvariantValues)
                {
                    maximumValue = base.ConvertCultureInvariantToCurrentCultureFormat(maximumValue, base.Type);
                    minimumValue = base.ConvertCultureInvariantToCurrentCultureFormat(minimumValue, base.Type);
                }
                base.AddExpandoAttribute(writer2, clientID, "maximumvalue", maximumValue);
                base.AddExpandoAttribute(writer2, clientID, "minimumvalue", minimumValue);
            }
        }

        protected override bool ControlPropertiesValid()
        {
            this.ValidateValues();
            return base.ControlPropertiesValid();
        }

        protected override bool EvaluateIsValid()
        {
            string controlValidationValue = base.GetControlValidationValue(base.ControlToValidate);
            if (controlValidationValue.Trim().Length == 0)
            {
                return true;
            }
            if (((base.Type == ValidationDataType.Date) && !this.DetermineRenderUplevel()) && !base.IsInStandardDateFormat(controlValidationValue))
            {
                controlValidationValue = base.ConvertToShortDateString(controlValidationValue);
            }
            return (BaseCompareValidator.Compare(controlValidationValue, false, this.MinimumValue, base.CultureInvariantValues, ValidationCompareOperator.GreaterThanEqual, base.Type) && BaseCompareValidator.Compare(controlValidationValue, false, this.MaximumValue, base.CultureInvariantValues, ValidationCompareOperator.LessThanEqual, base.Type));
        }

        private void ValidateValues()
        {
            string maximumValue = this.MaximumValue;
            if (!BaseCompareValidator.CanConvert(maximumValue, base.Type, base.CultureInvariantValues))
            {
                throw new HttpException(System.Web.SR.GetString("Validator_value_bad_type", new string[] { maximumValue, "MaximumValue", this.ID, PropertyConverter.EnumToString(typeof(ValidationDataType), base.Type) }));
            }
            string minimumValue = this.MinimumValue;
            if (!BaseCompareValidator.CanConvert(minimumValue, base.Type, base.CultureInvariantValues))
            {
                throw new HttpException(System.Web.SR.GetString("Validator_value_bad_type", new string[] { minimumValue, "MinimumValue", this.ID, PropertyConverter.EnumToString(typeof(ValidationDataType), base.Type) }));
            }
            if (BaseCompareValidator.Compare(minimumValue, base.CultureInvariantValues, maximumValue, base.CultureInvariantValues, ValidationCompareOperator.GreaterThan, base.Type))
            {
                throw new HttpException(System.Web.SR.GetString("Validator_range_overalap", new string[] { maximumValue, minimumValue, this.ID }));
            }
        }

        [Themeable(false), WebSysDescription("RangeValidator_MaximumValue"), WebCategory("Behavior"), DefaultValue("")]
        public string MaximumValue
        {
            get
            {
                object obj2 = this.ViewState["MaximumValue"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["MaximumValue"] = value;
            }
        }

        [WebSysDescription("RangeValidator_MinmumValue"), WebCategory("Behavior"), Themeable(false), DefaultValue("")]
        public string MinimumValue
        {
            get
            {
                object obj2 = this.ViewState["MinimumValue"];
                if (obj2 != null)
                {
                    return (string) obj2;
                }
                return string.Empty;
            }
            set
            {
                this.ViewState["MinimumValue"] = value;
            }
        }
    }
}

