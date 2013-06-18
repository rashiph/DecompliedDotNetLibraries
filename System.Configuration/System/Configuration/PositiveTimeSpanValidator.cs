namespace System.Configuration
{
    using System;

    public class PositiveTimeSpanValidator : ConfigurationValidatorBase
    {
        public override bool CanValidate(Type type)
        {
            return (type == typeof(TimeSpan));
        }

        public override void Validate(object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (((TimeSpan) value) <= TimeSpan.Zero)
            {
                throw new ArgumentException(System.Configuration.SR.GetString("Validator_timespan_value_must_be_positive"));
            }
        }
    }
}

