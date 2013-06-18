namespace System.Configuration
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class RegexStringValidatorAttribute : ConfigurationValidatorAttribute
    {
        private string _regex;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public RegexStringValidatorAttribute(string regex)
        {
            this._regex = regex;
        }

        public string Regex
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._regex;
            }
        }

        public override ConfigurationValidatorBase ValidatorInstance
        {
            get
            {
                return new RegexStringValidator(this._regex);
            }
        }
    }
}

