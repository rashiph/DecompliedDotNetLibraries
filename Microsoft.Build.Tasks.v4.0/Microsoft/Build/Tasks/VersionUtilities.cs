namespace Microsoft.Build.Tasks
{
    using System;

    internal static class VersionUtilities
    {
        internal static Version ConvertToVersion(string version)
        {
            Version result = null;
            if ((version.Length > 0) && ((version[0] == 'v') || (version[0] == 'V')))
            {
                version = version.Substring(1);
            }
            if (!Version.TryParse(version, out result))
            {
                return null;
            }
            return result;
        }
    }
}

