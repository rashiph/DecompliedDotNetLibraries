namespace System.Configuration
{
    using System;

    internal static class ConfigPathUtility
    {
        private const char SeparatorChar = '/';

        internal static string GetParent(string configPath)
        {
            if (string.IsNullOrEmpty(configPath))
            {
                return null;
            }
            int length = configPath.LastIndexOf('/');
            if (length == -1)
            {
                return null;
            }
            return configPath.Substring(0, length);
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

