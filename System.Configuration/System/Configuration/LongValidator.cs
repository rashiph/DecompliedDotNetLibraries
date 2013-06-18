namespace System.Configuration
{
    using System;

    public class LongValidator : ConfigurationValidatorBase
    {
        private ValidationFlags _flags;
        private long _maxValue;
        private long _minValue;
        private long _resolution;

        public LongValidator(long minValue, long maxValue) : this(minValue, maxValue, false, 1L)
        {
        }

        public LongValidator(long minValue, long maxValue, bool rangeIsExclusive) : this(minValue, maxValue, rangeIsExclusive, 1L)
        {
        }

        public LongValidator(long minValue, long maxValue, bool rangeIsExclusive, long resolution)
        {
            this._minValue = -9223372036854775808L;
            this._maxValue = 0x7fffffffffffffffL;
            this._resolution = 1L;
            if (resolution <= 0L)
            {
                throw new ArgumentOutOfRangeException("resolution");
            }
            if (minValue > maxValue)
            {
                throw new ArgumentOutOfRangeException("minValue", System.Configuration.SR.GetString("Validator_min_greater_than_max"));
            }
            this._minValue = minValue;
            this._maxValue = maxValue;
            this._resolution = resolution;
            this._flags = rangeIsExclusive ? ValidationFlags.ExclusiveRange : ValidationFlags.None;
        }

        public override bool CanValidate(Type type)
        {
            return (type == typeof(long));
        }

        public override void Validate(object value)
        {
            ValidatorUtils.HelperParamValidation(value, typeof(long));
            ValidatorUtils.ValidateScalar<long>((long) value, this._minValue, this._maxValue, this._resolution, this._flags == ValidationFlags.ExclusiveRange);
        }

        private enum ValidationFlags
        {
            None,
            ExclusiveRange
        }
    }
}

