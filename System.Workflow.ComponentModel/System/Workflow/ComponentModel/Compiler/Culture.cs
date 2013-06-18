namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;

    internal static class Culture
    {
        private static string[] cultureInfoStrings;

        internal static ItemCultureInfo GetItemCultureInfo(string name)
        {
            ItemCultureInfo info;
            info.culture = null;
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(name);
            string extension = Path.GetExtension(fileNameWithoutExtension);
            bool flag = false;
            if ((extension != null) && (extension.Length > 1))
            {
                extension = extension.Substring(1);
                flag = IsValidCultureString(extension);
            }
            if (flag)
            {
                if ((info.culture == null) || (info.culture.Length == 0))
                {
                    info.culture = extension;
                }
                string str3 = Path.GetExtension(name);
                string str4 = Path.GetFileNameWithoutExtension(fileNameWithoutExtension);
                string directoryName = Path.GetDirectoryName(name);
                string str6 = str4 + str3;
                info.cultureNeutralFilename = Path.Combine(directoryName, str6);
                return info;
            }
            info.cultureNeutralFilename = name;
            return info;
        }

        private static bool IsValidCultureString(string cultureString)
        {
            if (cultureInfoStrings == null)
            {
                CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
                cultureInfoStrings = new string[cultures.Length];
                for (int i = 0; i < cultures.Length; i++)
                {
                    cultureInfoStrings[i] = cultures[i].ToString().ToLowerInvariant();
                }
                Array.Sort<string>(cultureInfoStrings);
            }
            bool flag = true;
            if (Array.BinarySearch<string>(cultureInfoStrings, cultureString.ToLowerInvariant()) < 0)
            {
                flag = false;
            }
            return flag;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct ItemCultureInfo
        {
            internal string culture;
            internal string cultureNeutralFilename;
        }
    }
}

