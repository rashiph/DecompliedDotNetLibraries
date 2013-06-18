namespace System.Configuration
{
    using System;
    using System.Globalization;
    using System.Runtime;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class TimeSpanValidatorAttribute : ConfigurationValidatorAttribute
    {
        private bool _excludeRange;
        private TimeSpan _max = TimeSpan.MaxValue;
        private TimeSpan _min = TimeSpan.MinValue;
        public const string TimeSpanMaxValue = "10675199.02:48:05.4775807";
        public const string TimeSpanMinValue = "-10675199.02:48:05.4775808";

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

        public TimeSpan MaxValue
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._max;
            }
        }

        public string MaxValueString
        {
            get
            {
                return this._max.ToString();
            }
            set
            {
                TimeSpan span = TimeSpan.Parse(value, CultureInfo.InvariantCulture);
                if (this._min > span)
                {
                    throw new ArgumentOutOfRangeException("value", System.Configuration.SR.GetString("Validator_min_greater_than_max"));
                }
                this._max = span;
            }
        }

        public TimeSpan MinValue
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._min;
            }
        }

        public string MinValueString
        {
            get
            {
                return this._min.ToString();
            }
            set
            {
                TimeSpan span = TimeSpan.Parse(value, CultureInfo.InvariantCulture);
                if (this._max < span)
                {
                    throw new ArgumentOutOfRangeException("value", System.Configuration.SR.GetString("Validator_min_greater_than_max"));
                }
                this._min = span;
            }
        }

        public override ConfigurationValidatorBase ValidatorInstance
        {
            get
            {
                return new TimeSpanValidator(this._min, this._max, this._excludeRange);
            }
        }
    }
}

