namespace System.Configuration
{
    using System;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class IntegerValidatorAttribute : ConfigurationValidatorAttribute
    {
        private bool _excludeRange;
        private int _max = 0x7fffffff;
        private int _min = -2147483648;

        public bool ExcludeRange
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._excludeRange;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._excludeRange = value;
            }
        }

        public int MaxValue
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._max;
            }
            set
            {
                if (this._min > value)
                {
                    throw new ArgumentOutOfRangeException("value", System.Configuration.SR.GetString("Validator_min_greater_than_max"));
                }
                this._max = value;
            }
        }

        public int MinValue
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._min;
            }
            set
            {
                if (this._max < value)
                {
                    throw new ArgumentOutOfRangeException("value", System.Configuration.SR.GetString("Validator_min_greater_than_max"));
                }
                this._min = value;
            }
        }

        public override ConfigurationValidatorBase ValidatorInstance
        {
            get
            {
                return new IntegerValidator(this._min, this._max, this._excludeRange);
            }
        }
    }
}

