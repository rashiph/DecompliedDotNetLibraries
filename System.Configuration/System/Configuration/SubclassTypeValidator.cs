namespace System.Configuration
{
    using System;

    public sealed class SubclassTypeValidator : ConfigurationValidatorBase
    {
        private Type _base;

        public SubclassTypeValidator(Type baseClass)
        {
            if (baseClass == null)
            {
                throw new ArgumentNullException("baseClass");
            }
            this._base = baseClass;
        }

        public override bool CanValidate(Type type)
        {
            return (type == typeof(Type));
        }

        public override void Validate(object value)
        {
            if (value != null)
            {
                if (!(value is Type))
                {
                    ValidatorUtils.HelperParamValidation(value, typeof(Type));
                }
                if (!this._base.IsAssignableFrom((Type) value))
                {
                    throw new ArgumentException(System.Configuration.SR.GetString("Subclass_validator_error", new object[] { ((Type) value).FullName, this._base.FullName }));
                }
            }
        }
    }
}

