namespace System.Configuration
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Property)]
    public class ConfigurationValidatorAttribute : Attribute
    {
        private Type _validator;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ConfigurationValidatorAttribute()
        {
        }

        public ConfigurationValidatorAttribute(Type validator)
        {
            if (validator == null)
            {
                throw new ArgumentNullException("validator");
            }
            if (!typeof(ConfigurationValidatorBase).IsAssignableFrom(validator))
            {
                throw new ArgumentException(System.Configuration.SR.GetString("Validator_Attribute_param_not_validator", new object[] { "ConfigurationValidatorBase" }));
            }
            this._validator = validator;
        }

        public virtual ConfigurationValidatorBase ValidatorInstance
        {
            get
            {
                return (ConfigurationValidatorBase) System.Configuration.TypeUtil.CreateInstanceWithReflectionPermission(this._validator);
            }
        }

        public Type ValidatorType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._validator;
            }
        }
    }
}

