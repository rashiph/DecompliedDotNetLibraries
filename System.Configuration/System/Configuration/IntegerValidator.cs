namespace System.Configuration
{
    using System;
    using System.Runtime;

    public class IntegerValidator : ConfigurationValidatorBase
    {
        private ValidationFlags _flags;
        private int _maxValue;
        private int _minValue;
        private int _resolution;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public IntegerValidator(int minValue, int maxValue) : this(minValue, maxValue, false, 1)
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public IntegerValidator(int minValue, int maxValue, bool rangeIsExclusive) : this(minValue, maxValue, rangeIsExclusive, 1)
        {
        }

        public IntegerValidator(int minValue, int maxValue, bool rangeIsExclusive, int resolution)
        {
            this._minValue = -2147483648;
            this._maxValue = 0x7fffffff;
            this._resolution = 1;
            if (resolution <= 0)
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
            return (type == typeof(int));
        }

        public override void Validate(object value)
        {
            ValidatorUtils.HelperParamValidation(value, typeof(int));
            ValidatorUtils.ValidateScalar<int>((int) value, this._minValue, this._maxValue, this._resolution, this._flags == ValidationFlags.ExclusiveRange);
        }

        private enum ValidationFlags
        {
            None,
            ExclusiveRange
        }
    }
}

