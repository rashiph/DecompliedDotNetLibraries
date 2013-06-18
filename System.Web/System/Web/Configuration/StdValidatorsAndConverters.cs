namespace System.Web.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;

    internal static class StdValidatorsAndConverters
    {
        private static TypeConverter s_infiniteTimeSpanConverter;
        private static ConfigurationValidatorBase s_nonEmptyStringValidator;
        private static ConfigurationValidatorBase s_nonZeroPositiveIntegerValidator;
        private static ConfigurationValidatorBase s_positiveIntegerValidator;
        private static ConfigurationValidatorBase s_positiveTimeSpanValidator;
        private static TypeConverter s_timeSpanMinutesConverter;
        private static TypeConverter s_timeSpanMinutesOrInfiniteConverter;
        private static TypeConverter s_timeSpanSecondsConverter;
        private static TypeConverter s_timeSpanSecondsOrInfiniteConverter;
        private static TypeConverter s_versionConverter;
        private static TypeConverter s_whiteSpaceTrimStringConverter;

        internal static TypeConverter InfiniteTimeSpanConverter
        {
            get
            {
                if (s_infiniteTimeSpanConverter == null)
                {
                    s_infiniteTimeSpanConverter = new System.Configuration.InfiniteTimeSpanConverter();
                }
                return s_infiniteTimeSpanConverter;
            }
        }

        internal static ConfigurationValidatorBase NonEmptyStringValidator
        {
            get
            {
                if (s_nonEmptyStringValidator == null)
                {
                    s_nonEmptyStringValidator = new StringValidator(1);
                }
                return s_nonEmptyStringValidator;
            }
        }

        internal static ConfigurationValidatorBase NonZeroPositiveIntegerValidator
        {
            get
            {
                if (s_nonZeroPositiveIntegerValidator == null)
                {
                    s_nonZeroPositiveIntegerValidator = new IntegerValidator(1, 0x7fffffff);
                }
                return s_nonZeroPositiveIntegerValidator;
            }
        }

        internal static ConfigurationValidatorBase PositiveIntegerValidator
        {
            get
            {
                if (s_positiveIntegerValidator == null)
                {
                    s_positiveIntegerValidator = new IntegerValidator(0, 0x7fffffff);
                }
                return s_positiveIntegerValidator;
            }
        }

        internal static ConfigurationValidatorBase PositiveTimeSpanValidator
        {
            get
            {
                if (s_positiveTimeSpanValidator == null)
                {
                    s_positiveTimeSpanValidator = new System.Configuration.PositiveTimeSpanValidator();
                }
                return s_positiveTimeSpanValidator;
            }
        }

        internal static TypeConverter TimeSpanMinutesConverter
        {
            get
            {
                if (s_timeSpanMinutesConverter == null)
                {
                    s_timeSpanMinutesConverter = new System.Configuration.TimeSpanMinutesConverter();
                }
                return s_timeSpanMinutesConverter;
            }
        }

        internal static TypeConverter TimeSpanMinutesOrInfiniteConverter
        {
            get
            {
                if (s_timeSpanMinutesOrInfiniteConverter == null)
                {
                    s_timeSpanMinutesOrInfiniteConverter = new System.Configuration.TimeSpanMinutesOrInfiniteConverter();
                }
                return s_timeSpanMinutesOrInfiniteConverter;
            }
        }

        internal static TypeConverter TimeSpanSecondsConverter
        {
            get
            {
                if (s_timeSpanSecondsConverter == null)
                {
                    s_timeSpanSecondsConverter = new System.Configuration.TimeSpanSecondsConverter();
                }
                return s_timeSpanSecondsConverter;
            }
        }

        internal static TypeConverter TimeSpanSecondsOrInfiniteConverter
        {
            get
            {
                if (s_timeSpanSecondsOrInfiniteConverter == null)
                {
                    s_timeSpanSecondsOrInfiniteConverter = new System.Configuration.TimeSpanSecondsOrInfiniteConverter();
                }
                return s_timeSpanSecondsOrInfiniteConverter;
            }
        }

        internal static TypeConverter VersionConverter
        {
            get
            {
                if (s_versionConverter == null)
                {
                    s_versionConverter = new System.Web.Configuration.VersionConverter();
                }
                return s_versionConverter;
            }
        }

        internal static TypeConverter WhiteSpaceTrimStringConverter
        {
            get
            {
                if (s_whiteSpaceTrimStringConverter == null)
                {
                    s_whiteSpaceTrimStringConverter = new System.Configuration.WhiteSpaceTrimStringConverter();
                }
                return s_whiteSpaceTrimStringConverter;
            }
        }
    }
}

