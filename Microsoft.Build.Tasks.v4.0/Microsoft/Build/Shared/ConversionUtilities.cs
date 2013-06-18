namespace Microsoft.Build.Shared
{
    using System;
    using System.Globalization;

    internal static class ConversionUtilities
    {
        internal static bool CanConvertStringToBool(string parameterValue)
        {
            if (!ValidBooleanTrue(parameterValue))
            {
                return ValidBooleanFalse(parameterValue);
            }
            return true;
        }

        internal static double ConvertDecimalOrHexToDouble(string number)
        {
            if (ValidDecimalNumber(number))
            {
                return ConvertDecimalToDouble(number);
            }
            if (ValidHexNumber(number))
            {
                return ConvertHexToDouble(number);
            }
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(false, "Cannot numeric evaluate");
            return 0.0;
        }

        internal static double ConvertDecimalToDouble(string number)
        {
            return double.Parse(number, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, (IFormatProvider) CultureInfo.InvariantCulture.NumberFormat);
        }

        internal static double ConvertHexToDouble(string number)
        {
            return (double) int.Parse(number.Substring(2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture.NumberFormat);
        }

        internal static bool ConvertStringToBool(string parameterValue)
        {
            if (ValidBooleanTrue(parameterValue))
            {
                return true;
            }
            if (!ValidBooleanFalse(parameterValue))
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgument(false, "Shared.CannotConvertStringToBool", parameterValue);
            }
            return false;
        }

        private static bool ValidBooleanFalse(string parameterValue)
        {
            if ((((string.Compare(parameterValue, "false", StringComparison.OrdinalIgnoreCase) != 0) && (string.Compare(parameterValue, "off", StringComparison.OrdinalIgnoreCase) != 0)) && ((string.Compare(parameterValue, "no", StringComparison.OrdinalIgnoreCase) != 0) && (string.Compare(parameterValue, "!true", StringComparison.OrdinalIgnoreCase) != 0))) && (string.Compare(parameterValue, "!on", StringComparison.OrdinalIgnoreCase) != 0))
            {
                return (string.Compare(parameterValue, "!yes", StringComparison.OrdinalIgnoreCase) == 0);
            }
            return true;
        }

        private static bool ValidBooleanTrue(string parameterValue)
        {
            if ((((string.Compare(parameterValue, "true", StringComparison.OrdinalIgnoreCase) != 0) && (string.Compare(parameterValue, "on", StringComparison.OrdinalIgnoreCase) != 0)) && ((string.Compare(parameterValue, "yes", StringComparison.OrdinalIgnoreCase) != 0) && (string.Compare(parameterValue, "!false", StringComparison.OrdinalIgnoreCase) != 0))) && (string.Compare(parameterValue, "!off", StringComparison.OrdinalIgnoreCase) != 0))
            {
                return (string.Compare(parameterValue, "!no", StringComparison.OrdinalIgnoreCase) == 0);
            }
            return true;
        }

        private static bool ValidDecimalNumber(string number)
        {
            double num;
            return double.TryParse(number, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, (IFormatProvider) CultureInfo.InvariantCulture.NumberFormat, out num);
        }

        internal static bool ValidDecimalOrHexNumber(string number)
        {
            if (!ValidDecimalNumber(number))
            {
                return ValidHexNumber(number);
            }
            return true;
        }

        private static bool ValidHexNumber(string number)
        {
            int num;
            bool flag = false;
            if (((number.Length < 3) || (number[0] != '0')) || ((number[1] != 'x') && (number[1] != 'X')))
            {
                return flag;
            }
            return int.TryParse(number.Substring(2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture.NumberFormat, out num);
        }
    }
}

