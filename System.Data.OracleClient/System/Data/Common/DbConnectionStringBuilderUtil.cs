namespace System.Data.Common
{
    using System;
    using System.Globalization;

    internal static class DbConnectionStringBuilderUtil
    {
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
                    throw System.Data.Common.ADP.ConvertFailed(value.GetType(), typeof(bool), exception);
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
                throw System.Data.Common.ADP.ConvertFailed(value.GetType(), typeof(int), exception);
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
                    throw System.Data.Common.ADP.ConvertFailed(value.GetType(), typeof(bool), exception);
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
                throw System.Data.Common.ADP.ConvertFailed(value.GetType(), typeof(string), exception);
            }
            return str;
        }
    }
}

