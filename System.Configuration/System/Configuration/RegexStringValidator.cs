namespace System.Configuration
{
    using System;
    using System.Text.RegularExpressions;

    public class RegexStringValidator : ConfigurationValidatorBase
    {
        private string _expression;
        private Regex _regex;

        public RegexStringValidator(string regex)
        {
            if (string.IsNullOrEmpty(regex))
            {
                throw ExceptionUtil.ParameterNullOrEmpty("regex");
            }
            this._expression = regex;
            this._regex = new Regex(regex, RegexOptions.Compiled);
        }

        public override bool CanValidate(Type type)
        {
            return (type == typeof(string));
        }

        public override void Validate(object value)
        {
            ValidatorUtils.HelperParamValidation(value, typeof(string));
            if ((value != null) && !this._regex.Match((string) value).Success)
            {
                throw new ArgumentException(System.Configuration.SR.GetString("Regex_validator_error", new object[] { this._expression }));
            }
        }
    }
}

