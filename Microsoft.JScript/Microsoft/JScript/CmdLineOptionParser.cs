namespace Microsoft.JScript
{
    using System;

    public class CmdLineOptionParser
    {
        public static string IsArgumentOption(string option, string prefix)
        {
            int length = prefix.Length;
            if ((option.Length < length) || (string.Compare(option, 0, prefix, 0, length, StringComparison.OrdinalIgnoreCase) != 0))
            {
                return null;
            }
            if (option.Length == length)
            {
                return "";
            }
            if (':' != option[length])
            {
                return null;
            }
            return option.Substring(length + 1);
        }

        public static string IsArgumentOption(string option, string shortPrefix, string longPrefix)
        {
            string str = IsArgumentOption(option, shortPrefix);
            if (str == null)
            {
                str = IsArgumentOption(option, longPrefix);
            }
            return str;
        }

        public static object IsBooleanOption(string option, string prefix)
        {
            int length = prefix.Length;
            if ((option.Length >= prefix.Length) && (string.Compare(option, 0, prefix, 0, length, StringComparison.OrdinalIgnoreCase) == 0))
            {
                if (option.Length == length)
                {
                    return true;
                }
                if (option.Length == (length + 1))
                {
                    if ('-' == option[length])
                    {
                        return false;
                    }
                    if ('+' == option[length])
                    {
                        return true;
                    }
                }
            }
            return null;
        }

        public static object IsBooleanOption(string option, string shortPrefix, string longPrefix)
        {
            object obj2 = IsBooleanOption(option, shortPrefix);
            if (obj2 == null)
            {
                obj2 = IsBooleanOption(option, longPrefix);
            }
            return obj2;
        }

        public static bool IsSimpleOption(string option, string prefix)
        {
            if (string.Compare(option, prefix, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return false;
            }
            return true;
        }
    }
}

