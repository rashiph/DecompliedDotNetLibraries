namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;

    internal static class SqlDataSourceCommandParser
    {
        private static bool ConsumeField(string s, int startIndex, List<string> parts)
        {
            string str;
            while ((startIndex < s.Length) && char.IsWhiteSpace(s, startIndex))
            {
                startIndex++;
            }
            startIndex = ConsumeIdentifier(s, startIndex, out str);
            parts.Add(str);
            return ExpectField(s, startIndex, parts);
        }

        private static bool ConsumeFrom(string s, int startIndex, List<string> parts)
        {
            while ((startIndex < s.Length) && char.IsWhiteSpace(s, startIndex))
            {
                startIndex++;
            }
            if ((startIndex + 5) >= s.Length)
            {
                return false;
            }
            return (((string.Compare(s, startIndex, "from", 0, 4, StringComparison.OrdinalIgnoreCase) == 0) && char.IsWhiteSpace(s, startIndex + 4)) && ConsumeTable(s, startIndex + 5, parts));
        }

        private static int ConsumeIdentifier(string s, int startIndex, out string identifier)
        {
            bool flag = false;
            identifier = string.Empty;
            while (startIndex < s.Length)
            {
                if (!flag && (s[startIndex] == '['))
                {
                    flag = true;
                    identifier = identifier + s[startIndex];
                    startIndex++;
                }
                else
                {
                    if (flag && (s[startIndex] == ']'))
                    {
                        flag = false;
                        identifier = identifier + s[startIndex];
                        startIndex++;
                        continue;
                    }
                    if (flag)
                    {
                        identifier = identifier + s[startIndex];
                        startIndex++;
                        continue;
                    }
                    if (char.IsWhiteSpace(s, startIndex) || (s[startIndex] == ','))
                    {
                        return startIndex;
                    }
                    identifier = identifier + s[startIndex];
                    startIndex++;
                }
            }
            return startIndex;
        }

        private static bool ConsumeSelect(string s, int startIndex, List<string> parts)
        {
            if (s.Length < 7)
            {
                return false;
            }
            if (!s.ToLowerInvariant().StartsWith("select", StringComparison.Ordinal))
            {
                return false;
            }
            if (!char.IsWhiteSpace(s, 6))
            {
                return false;
            }
            return ConsumeField(s, startIndex + 7, parts);
        }

        private static bool ConsumeTable(string s, int startIndex, List<string> parts)
        {
            string str;
            while ((startIndex < s.Length) && char.IsWhiteSpace(s, startIndex))
            {
                startIndex++;
            }
            startIndex = ConsumeIdentifier(s, startIndex, out str);
            parts.Add(str);
            return (startIndex == s.Length);
        }

        private static bool ExpectField(string s, int startIndex, List<string> parts)
        {
            while ((startIndex < s.Length) && char.IsWhiteSpace(s, startIndex))
            {
                startIndex++;
            }
            if (startIndex >= (s.Length - 1))
            {
                return false;
            }
            if (s[startIndex] == ',')
            {
                return ConsumeField(s, startIndex + 1, parts);
            }
            return ConsumeFrom(s, startIndex, parts);
        }

        private static string[] GetIdentifierParts(string identifier)
        {
            bool flag = false;
            StringBuilder builder = new StringBuilder();
            ArrayList list = new ArrayList();
            for (int i = 0; i < identifier.Length; i++)
            {
                char c = identifier[i];
                switch (c)
                {
                    case '[':
                        if (!flag)
                        {
                            break;
                        }
                        return null;

                    case ']':
                        if (flag && ((identifier.Length <= (i + 2)) || (identifier[i + 1] == '.')))
                        {
                            goto Label_0069;
                        }
                        return null;

                    case '.':
                    {
                        if (flag)
                        {
                            builder.Append('.');
                        }
                        else
                        {
                            list.Add(builder.ToString());
                            builder.Length = 0;
                        }
                        continue;
                    }
                    default:
                        goto Label_0091;
                }
                flag = true;
                continue;
            Label_0069:
                flag = false;
                continue;
            Label_0091:
                if (!flag)
                {
                    switch (c)
                    {
                        case '#':
                        case '*':
                        case '@':
                        case '_':
                            goto Label_00DB;
                    }
                    if (!char.IsLetter(c) && ((builder.Length <= 0) || ((c != '$') && !char.IsDigit(c))))
                    {
                        return null;
                    }
                }
            Label_00DB:
                builder.Append(c);
            }
            list.Add(builder.ToString());
            return (string[]) list.ToArray(typeof(string));
        }

        public static string GetLastIdentifierPart(string identifier)
        {
            string[] identifierParts = GetIdentifierParts(identifier);
            if ((identifierParts != null) && (identifierParts.Length != 0))
            {
                return identifierParts[identifierParts.Length - 1];
            }
            return null;
        }

        public static string[] ParseSqlString(string sqlString)
        {
            if (string.IsNullOrEmpty(sqlString))
            {
                return null;
            }
            try
            {
                sqlString = sqlString.Trim();
                List<string> parts = new List<string>();
                return (ConsumeSelect(sqlString, 0, parts) ? parts.ToArray() : null);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}

