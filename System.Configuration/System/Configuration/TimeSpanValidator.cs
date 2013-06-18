namespace System.Configuration
{
    using System;

    public class TimeSpanValidator : ConfigurationValidatorBase
    {
        private ValidationFlags _flags;
        private TimeSpan _maxValue;
        private TimeSpan _minValue;
        private long _resolution;

        public TimeSpanValidator(TimeSpan minValue, TimeSpan maxValue) : this(minValue, maxValue, false, 0L)
        {
        }

        public TimeSpanValidator(TimeSpan minValue, TimeSpan maxValue, bool rangeIsExclusive) : this(minValue, maxValue, rangeIsExclusive, 0L)
        {
        }

        public TimeSpanValidator(TimeSpan minValue, TimeSpan maxValue, bool rangeIsExclusive, long resolutionInSeconds)
        {
            this._minValue = TimeSpan.MinValue;
            this._maxValue = TimeSpan.MaxValue;
            if (resolutionInSeconds < 0L)
            {
                throw new ArgumentOutOfRangeException("resolutionInSeconds");
            }
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException("minValue", System.Configuration.SR.GetString("Validator_min_greater_than_max"));
            }
            this._minValue = minValue;
            this._maxValue = maxValue;
            this._resolution = resolutionInSeconds;
            this._flags = rangeIsExclusive ? ValidationFlags.ExclusiveRange : ValidationFlags.None;
        }

        public override bool CanValidate(Type type)
        {
            return (type == typeof(TimeSpan));
        }

        public override void Validate(object value)
        {
            ValidatorUtils.HelperParamValidation(value, typeof(TimeSpan));
            ValidatorUtils.ValidateScalar((TimeSpan) value, this._minValue, this._maxValue, this._resolution, this._flags == ValidationFlags.ExclusiveRange);
        }

        private enum ValidationFlags
        {
            None,
            ExclusiveRange
        }
    }
}

