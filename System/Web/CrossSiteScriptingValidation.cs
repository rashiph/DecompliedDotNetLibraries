namespace System.Web
{
    using System;
    using System.CodeDom.Compiler;
    using System.Runtime.InteropServices;

    internal static class CrossSiteScriptingValidation
    {
        private static char[] startingChars = new char[] { '<', '&' };

        private static bool IsAtoZ(char c)
        {
            return (((c >= 'a') && (c <= 'z')) || ((c >= 'A') && (c <= 'Z')));
        }

        internal static bool IsDangerousString(string s, out int matchIndex)
        {
            matchIndex = 0;
            int startIndex = 0;
            while (true)
            {
                int num2 = s.IndexOfAny(startingChars, startIndex);
                if (num2 < 0)
                {
                    return false;
                }
                if (num2 == (s.Length - 1))
                {
                    return false;
                }
                matchIndex = num2;
                char ch = s[num2];
                if (ch != '&')
                {
                    if ((ch == '<') && ((IsAtoZ(s[num2 + 1]) || (s[num2 + 1] == '!')) || ((s[num2 + 1] == '/') || (s[num2 + 1] == '?'))))
                    {
                        return true;
                    }
                }
                else if (s[num2 + 1] == '#')
                {
                    return true;
                }
                startIndex = num2 + 1;
            }
        }

        internal static bool IsDangerousUrl(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return false;
            }
            s = s.Trim();
            int length = s.Length;
            if (((((length > 4) && ((s[0] == 'h') || (s[0] == 'H'))) && ((s[1] == 't') || (s[1] == 'T'))) && (((s[2] == 't') || (s[2] == 'T')) && ((s[3] == 'p') || (s[3] == 'P')))) && ((s[4] == ':') || (((length > 5) && ((s[4] == 's') || (s[4] == 'S'))) && (s[5] == ':'))))
            {
                return false;
            }
            if (s.IndexOf(':') == -1)
            {
                return false;
            }
            return true;
        }

        internal static bool IsValidJavascriptId(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                return CodeGenerator.IsValidLanguageIndependentIdentifier(id);
            }
            return true;
        }
    }
}

