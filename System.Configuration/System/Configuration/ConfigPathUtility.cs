namespace System.Configuration
{
    using System;

    internal static class ConfigPathUtility
    {
        private const char SeparatorChar = '/';

        internal static string Combine(string parentConfigPath, string childConfigPath)
        {
            if (string.IsNullOrEmpty(parentConfigPath))
            {
                return childConfigPath;
            }
            if (string.IsNullOrEmpty(childConfigPath))
            {
                return parentConfigPath;
            }
            return (parentConfigPath + "/" + childConfigPath);
        }

        internal static string GetName(string configPath)
        {
            if (string.IsNullOrEmpty(configPath))
            {
                return configPath;
            }
            int num = configPath.LastIndexOf('/');
            if (num == -1)
            {
                return configPath;
            }
            return configPath.Substring(num + 1);
        }

        internal static string[] GetParts(string configPath)
        {
            return configPath.Split(new char[] { '/' });
        }

        internal static bool IsValid(string configPath)
        {
            if (string.IsNullOrEmpty(configPath))
            {
                return false;
            }
            int num = -1;
            for (int i = 0; i <= configPath.Length; i++)
            {
                char ch;
                if (i < configPath.Length)
                {
                    ch = configPath[i];
                }
                else
                {
                    ch = '/';
                }
                if (ch == '\\')
                {
                    return false;
                }
                if (ch == '/')
                {
                    if (i == (num + 1))
                    {
                        return false;
                    }
                    if ((i == (num + 2)) && (configPath[num + 1] == '.'))
                    {
                        return false;
                    }
                    if (((i == (num + 3)) && (configPath[num + 1] == '.')) && (configPath[num + 2] == '.'))
                    {
                        return false;
                    }
                    num = i;
                }
            }
            return true;
        }
    }
}

