namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using System;
    using System.Globalization;

    internal static class ConvertUtil
    {
        public static bool ToBoolean(string value)
        {
            return ToBoolean(value, false);
        }

        public static bool ToBoolean(string value, bool defaultValue)
        {
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    return Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                }
                catch (FormatException)
                {
                }
                catch (ArgumentException)
                {
                }
            }
            return defaultValue;
        }
    }
}

