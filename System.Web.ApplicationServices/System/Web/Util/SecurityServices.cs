namespace System.Web.Util
{
    using System;
    using System.Globalization;
    using System.Web;

    internal static class SecurityServices
    {
        internal static void CheckForEmptyOrWhiteSpaceParameter(ref string param, string paramName)
        {
            if (param != null)
            {
                param = param.Trim();
                CheckForEmptyParameter(param, paramName);
            }
        }

        internal static void CheckForEmptyParameter(string param, string paramName)
        {
            if (param.Length < 1)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ApplicationServicesStrings.Parameter_can_not_be_empty, new object[] { paramName }), paramName);
            }
        }

        internal static void CheckPasswordParameter(string param, string paramName)
        {
            if (param == null)
            {
                throw new ArgumentNullException(paramName);
            }
            CheckForEmptyParameter(param, paramName);
        }
    }
}

