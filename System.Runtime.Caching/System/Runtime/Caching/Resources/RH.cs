namespace System.Runtime.Caching.Resources
{
    using System;
    using System.Globalization;

    internal static class RH
    {
        public static string Format(string resource, params object[] args)
        {
            return string.Format(CultureInfo.CurrentCulture, resource, args);
        }
    }
}

