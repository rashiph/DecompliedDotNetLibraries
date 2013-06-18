namespace Microsoft.Build.Tasks
{
    using System;
    using System.Globalization;

    internal static class CultureStringUtilities
    {
        private static string[] cultureInfoStrings;

        internal static bool IsValidCultureString(string cultureString)
        {
            PopulateCultureInfoArray();
            bool flag = true;
            if (Array.BinarySearch<string>(cultureInfoStrings, cultureString, StringComparer.OrdinalIgnoreCase) < 0)
            {
                flag = false;
            }
            return flag;
        }

        internal static void PopulateCultureInfoArray()
        {
            if (cultureInfoStrings == null)
            {
                CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
                cultureInfoStrings = new string[cultures.Length];
                for (int i = 0; i < cultures.Length; i++)
                {
                    cultureInfoStrings[i] = cultures[i].Name;
                }
                Array.Sort<string>(cultureInfoStrings, StringComparer.OrdinalIgnoreCase);
            }
        }
    }
}

