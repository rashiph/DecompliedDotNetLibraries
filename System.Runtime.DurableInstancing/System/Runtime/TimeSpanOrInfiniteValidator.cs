namespace System.Runtime
{
    using System;
    using System.Configuration;

    internal class TimeSpanOrInfiniteValidator : TimeSpanValidator
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TimeSpanOrInfiniteValidator(TimeSpan minValue, TimeSpan maxValue) : base(minValue, maxValue)
        {
        }

        public override void Validate(object value)
        {
            if ((value.GetType() != typeof(TimeSpan)) || (((TimeSpan) value) != TimeSpan.MaxValue))
            {
                base.Validate(value);
            }
        }
    }
}

