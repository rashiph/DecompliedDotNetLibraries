namespace Microsoft.Build.Tasks
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    internal static class Culture
    {
        internal static ItemCultureInfo GetItemCultureInfo(string name, string dependentUponFilename)
        {
            ItemCultureInfo info;
            info.culture = null;
            string path = (dependentUponFilename == null) ? string.Empty : dependentUponFilename;
            if (string.Compare(Path.GetFileNameWithoutExtension(path), Path.GetFileNameWithoutExtension(name), StringComparison.OrdinalIgnoreCase) == 0)
            {
                info.cultureNeutralFilename = name;
                return info;
            }
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(name);
            string extension = Path.GetExtension(fileNameWithoutExtension);
            bool flag = false;
            if ((extension != null) && (extension.Length > 1))
            {
                extension = extension.Substring(1);
                flag = CultureStringUtilities.IsValidCultureString(extension);
            }
            if (flag)
            {
                info.culture = extension;
                string str4 = Path.GetExtension(name);
                string str5 = Path.GetFileNameWithoutExtension(fileNameWithoutExtension);
                string directoryName = Path.GetDirectoryName(name);
                string str7 = str5 + str4;
                info.cultureNeutralFilename = Path.Combine(directoryName, str7);
                return info;
            }
            info.cultureNeutralFilename = name;
            return info;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct ItemCultureInfo
        {
            internal string culture;
            internal string cultureNeutralFilename;
        }
    }
}

