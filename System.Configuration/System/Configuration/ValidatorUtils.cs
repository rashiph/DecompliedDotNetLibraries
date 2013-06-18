namespace System.Configuration
{
    using System;
    using System.Globalization;

    internal static class ValidatorUtils
    {
        public static void HelperParamValidation(object value, Type allowedType)
        {
            if ((value != null) && (value.GetType() != allowedType))
            {
                throw new ArgumentException(System.Configuration.SR.GetString("Validator_value_type_invalid"), string.Empty);
            }
        }

        private static void ValidateRangeImpl<T>(T value, T min, T max, bool exclusiveRange) where T: IComparable<T>
        {
            IComparable<T> comparable = value;
            bool flag = false;
            if (comparable.CompareTo(min) >= 0)
            {
                flag = true;
            }
            if (flag && (comparable.CompareTo(max) > 0))
            {
                flag = false;
            }
            if (!(flag ^ exclusiveRange))
            {
                string format = null;
                if (min.Equals(max))
                {
                    if (exclusiveRange)
                    {
                        format = System.Configuration.SR.GetString("Validation_scalar_range_violation_not_different");
                    }
                    else
                    {
                        format = System.Configuration.SR.GetString("Validation_scalar_range_violation_not_equal");
                    }
                }
                else if (exclusiveRange)
                {
                    format = System.Configuration.SR.GetString("Validation_scalar_range_violation_not_outside_range");
                }
                else
                {
                    format = System.Configuration.SR.GetString("Validation_scalar_range_violation_not_in_range");
                }
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, format, new object[] { min.ToString(), max.ToString() }));
            }
        }

        private static void ValidateResolution(string resolutionAsString, long value, long resolution)
        {
            if ((value % resolution) != 0L)
            {
                throw new ArgumentException(System.Configuration.SR.GetString("Validator_scalar_resolution_violation", new object[] { resolutionAsString }));
            }
        }

        public static void ValidateScalar(TimeSpan value, TimeSpan min, TimeSpan max, long resolutionInSeconds, bool exclusiveRange)
        {
            ValidateRangeImpl<TimeSpan>(value, min, max, exclusiveRange);
            if (resolutionInSeconds > 0L)
            {
                ValidateResolution(TimeSpan.FromSeconds((double) resolutionInSeconds).ToString(), value.Ticks, resolutionInSeconds * 0x989680L);
            }
        }

        public static void ValidateScalar<T>(T value, T min, T max, T resolution, bool exclusiveRange) where T: IComparable<T>
        {
            ValidateRangeImpl<T>(value, min, max, exclusiveRange);
            ValidateResolution(resolution.ToString(), Convert.ToInt64(value, CultureInfo.InvariantCulture), Convert.ToInt64(resolution, CultureInfo.InvariantCulture));
        }
    }
}

