namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.ServiceModel;

    internal class StandardRuntimeEnumValidator : ConfigurationValidatorBase
    {
        private Type enumType;

        public StandardRuntimeEnumValidator(Type enumType)
        {
            this.enumType = enumType;
        }

        public override bool CanValidate(Type type)
        {
            return type.IsEnum;
        }

        public override void Validate(object value)
        {
            if (!Enum.IsDefined(this.enumType, value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int) value, this.enumType));
            }
        }
    }
}

