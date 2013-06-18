namespace System.Configuration
{
    using System;
    using System.Runtime;

    public sealed class ConfigurationElementProperty
    {
        private ConfigurationValidatorBase _validator;

        public ConfigurationElementProperty(ConfigurationValidatorBase validator)
        {
            if (validator == null)
            {
                throw new ArgumentNullException("validator");
            }
            this._validator = validator;
        }

        public ConfigurationValidatorBase Validator
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._validator;
            }
        }
    }
}

