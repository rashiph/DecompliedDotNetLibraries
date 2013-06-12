namespace System.Data.Common
{
    using System;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Runtime.InteropServices;

    internal static class DbConnectionStringBuilderUtil
    {
        private const string ApplicationIntentReadOnlyString = "ReadOnly";
        private const string ApplicationIntentReadWriteString = "ReadWrite";

        internal static string ApplicationIntentToString(ApplicationIntent value)
        {
            if (value == ApplicationIntent.ReadOnly)
            {
                return "ReadOnly";
            }
            return "ReadWrite";
        }

        internal static ApplicationIntent ConvertToApplicationIntent(string keyword, object value)
        {
            ApplicationIntent intent2;
            string str = value as string;
            if (str == null)
            {
                ApplicationIntent intent;
                if (value is ApplicationIntent)
                {
                    intent = (ApplicationIntent) value;
                }
                else
                {
                    if (value.GetType().IsEnum)
                    {
                        throw ADP.ConvertFailed(value.GetType(), typeof(ApplicationIntent), null);
                    }
                    try
                    {
                        intent = (ApplicationIntent) Enum.ToObject(typeof(ApplicationIntent), value);
                    }
                    catch (ArgumentException exception)
                    {
                        throw ADP.ConvertFailed(value.GetType(), typeof(ApplicationIntent), exception);
                    }
                }
                if (!IsValidApplicationIntentValue(intent))
                {
                    throw ADP.InvalidEnumerationValue(typeof(ApplicationIntent), (int) intent);
                }
                return intent;
            }
            if (!TryConvertToApplicationIntent(str, out intent2) && !TryConvertToApplicationIntent(str.Trim(), out intent2))
            {
                throw ADP.InvalidConnectionOptionValue(keyword);
            }
            return intent2;
        }

        internal static bool ConvertToBoolean(object value)
        {
            string x = value as string;
            if (x == null)
            {
                bool flag;
                try
                {
                    flag = ((IConvertible) value).ToBoolean(CultureInfo.InvariantCulture);
                }
                catch (InvalidCastException exception)
                {
                    throw ADP.ConvertFailed(value.GetType(), typeof(bool), exception);
                }
                return flag;
            }
            if (StringComparer.OrdinalIgnoreCase.Equals(x, "true") || StringComparer.OrdinalIgnoreCase.Equals(x, "yes"))
            {
                return true;
            }
            if (!StringComparer.OrdinalIgnoreCase.Equals(x, "false") && !StringComparer.OrdinalIgnoreCase.Equals(x, "no"))
            {
                string str2 = x.Trim();
                if (StringComparer.OrdinalIgnoreCase.Equals(str2, "true") || StringComparer.OrdinalIgnoreCase.Equals(str2, "yes"))
                {
                    return true;
                }
                if (!StringComparer.OrdinalIgnoreCase.Equals(str2, "false") && !StringComparer.OrdinalIgnoreCase.Equals(str2, "no"))
                {
                    return bool.Parse(x);
                }
            }
            return false;
        }

        internal static int ConvertToInt32(object value)
        {
            int num;
            try
            {
                num = ((IConvertible) value).ToInt32(CultureInfo.InvariantCulture);
            }
            catch (InvalidCastException exception)
            {
                throw ADP.ConvertFailed(value.GetType(), typeof(int), exception);
            }
            return num;
        }

        internal static bool ConvertToIntegratedSecurity(object value)
        {
            string x = value as string;
            if (x == null)
            {
                bool flag;
                try
                {
                    flag = ((IConvertible) value).ToBoolean(CultureInfo.InvariantCulture);
                }
                catch (InvalidCastException exception)
                {
                    throw ADP.ConvertFailed(value.GetType(), typeof(bool), exception);
                }
                return flag;
            }
            if ((StringComparer.OrdinalIgnoreCase.Equals(x, "sspi") || StringComparer.OrdinalIgnoreCase.Equals(x, "true")) || StringComparer.OrdinalIgnoreCase.Equals(x, "yes"))
            {
                return true;
            }
            if (!StringComparer.OrdinalIgnoreCase.Equals(x, "false") && !StringComparer.OrdinalIgnoreCase.Equals(x, "no"))
            {
                string str2 = x.Trim();
                if ((StringComparer.OrdinalIgnoreCase.Equals(str2, "sspi") || StringComparer.OrdinalIgnoreCase.Equals(str2, "true")) || StringComparer.OrdinalIgnoreCase.Equals(str2, "yes"))
                {
                    return true;
                }
                if (!StringComparer.OrdinalIgnoreCase.Equals(str2, "false") && !StringComparer.OrdinalIgnoreCase.Equals(str2, "no"))
                {
                    return bool.Parse(x);
                }
            }
            return false;
        }

        internal static string ConvertToString(object value)
        {
            string str;
            try
            {
                str = ((IConvertible) value).ToString(CultureInfo.InvariantCulture);
            }
            catch (InvalidCastException exception)
            {
                throw ADP.ConvertFailed(value.GetType(), typeof(string), exception);
            }
            return str;
        }

        internal static bool IsValidApplicationIntentValue(ApplicationIntent value)
        {
            if (value != ApplicationIntent.ReadOnly)
            {
                return (value == ApplicationIntent.ReadWrite);
            }
            return true;
        }

        internal static bool TryConvertToApplicationIntent(string value, out ApplicationIntent result)
        {
            if (StringComparer.OrdinalIgnoreCase.Equals(value, "ReadOnly"))
            {
                result = ApplicationIntent.ReadOnly;
                return true;
            }
            if (StringComparer.OrdinalIgnoreCase.Equals(value, "ReadWrite"))
            {
                result = ApplicationIntent.ReadWrite;
                return true;
            }
            result = ApplicationIntent.ReadWrite;
            return false;
        }
    }
}

