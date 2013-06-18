namespace Microsoft.Build.Utilities
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;

    internal static class CanonicalError
    {
        private static Regex filenameLocationFromOrigin = new Regex(@"^(\d+>)?(?<FILENAME>.*)\((?<LOCATION>[\,,0-9,-]*)\)\s*$", RegexOptions.IgnoreCase);
        private static Regex lineColColFromLocation = new Regex("^(?<LINE>[0-9]*),(?<COLUMN>[0-9]*)-(?<ENDCOLUMN>[0-9]*)$", RegexOptions.IgnoreCase);
        private static Regex lineColFromLocation = new Regex("^(?<LINE>[0-9]*),(?<COLUMN>[0-9]*)$", RegexOptions.IgnoreCase);
        private static Regex lineColLineColFromLocation = new Regex("^(?<LINE>[0-9]*),(?<COLUMN>[0-9]*),(?<ENDLINE>[0-9]*),(?<ENDCOLUMN>[0-9]*)$", RegexOptions.IgnoreCase);
        private static Regex lineFromLocation = new Regex("^(?<LINE>[0-9]*)$", RegexOptions.IgnoreCase);
        private static Regex lineLineFromLocation = new Regex("^(?<LINE>[0-9]*)-(?<ENDLINE>[0-9]*)$", RegexOptions.IgnoreCase);
        private static Regex originCategoryCodeTextExpression = new Regex(@"^\s*(((?<ORIGIN>(((\d+>)?[a-zA-Z]?:[^:]*)|([^:]*))):)|())(?<SUBCATEGORY>(()|([^:]*? )))(?<CATEGORY>(error|warning))( \s*(?<CODE>[^: ]*))?\s*:(?<TEXT>.*)$", RegexOptions.IgnoreCase);

        private static int ConvertToIntWithDefault(string value)
        {
            int num;
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out num) && (num >= 0))
            {
                return num;
            }
            return 0;
        }

        internal static Parts Parse(string message)
        {
            Parts parts;
            Match match;
            string str2;
            string str = string.Empty;
            if (message.Length > 400)
            {
                str = message.Substring(400);
                message = message.Substring(0, 400);
            }
            if ((message.IndexOf("warning", StringComparison.OrdinalIgnoreCase) != -1) || (message.IndexOf("error", StringComparison.OrdinalIgnoreCase) != -1))
            {
                parts = new Parts();
                match = originCategoryCodeTextExpression.Match(message);
                if (!match.Success)
                {
                    return null;
                }
                str2 = match.Groups["ORIGIN"].Value.Trim();
                string strA = match.Groups["CATEGORY"].Value.Trim();
                parts.code = match.Groups["CODE"].Value.Trim();
                parts.text = match.Groups["TEXT"].Value.Trim() + str;
                parts.subcategory = match.Groups["SUBCATEGORY"].Value.Trim();
                if (string.Compare(strA, "error", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    parts.category = Parts.Category.Error;
                    goto Label_0138;
                }
                if (string.Compare(strA, "warning", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    parts.category = Parts.Category.Warning;
                    goto Label_0138;
                }
            }
            return null;
        Label_0138:
            match = filenameLocationFromOrigin.Match(str2);
            if (match.Success)
            {
                string input = match.Groups["LOCATION"].Value.Trim();
                parts.origin = match.Groups["FILENAME"].Value.Trim();
                if (input.Length > 0)
                {
                    match = lineFromLocation.Match(input);
                    if (match.Success)
                    {
                        parts.line = ConvertToIntWithDefault(match.Groups["LINE"].Value.Trim());
                        return parts;
                    }
                    match = lineLineFromLocation.Match(input);
                    if (match.Success)
                    {
                        parts.line = ConvertToIntWithDefault(match.Groups["LINE"].Value.Trim());
                        parts.endLine = ConvertToIntWithDefault(match.Groups["ENDLINE"].Value.Trim());
                        return parts;
                    }
                    match = lineColFromLocation.Match(input);
                    if (match.Success)
                    {
                        parts.line = ConvertToIntWithDefault(match.Groups["LINE"].Value.Trim());
                        parts.column = ConvertToIntWithDefault(match.Groups["COLUMN"].Value.Trim());
                        return parts;
                    }
                    match = lineColColFromLocation.Match(input);
                    if (match.Success)
                    {
                        parts.line = ConvertToIntWithDefault(match.Groups["LINE"].Value.Trim());
                        parts.column = ConvertToIntWithDefault(match.Groups["COLUMN"].Value.Trim());
                        parts.endColumn = ConvertToIntWithDefault(match.Groups["ENDCOLUMN"].Value.Trim());
                        return parts;
                    }
                    match = lineColLineColFromLocation.Match(input);
                    if (match.Success)
                    {
                        parts.line = ConvertToIntWithDefault(match.Groups["LINE"].Value.Trim());
                        parts.column = ConvertToIntWithDefault(match.Groups["COLUMN"].Value.Trim());
                        parts.endLine = ConvertToIntWithDefault(match.Groups["ENDLINE"].Value.Trim());
                        parts.endColumn = ConvertToIntWithDefault(match.Groups["ENDCOLUMN"].Value.Trim());
                    }
                }
                return parts;
            }
            parts.origin = str2;
            return parts;
        }

        internal sealed class Parts
        {
            internal Category category;
            internal string code;
            internal int column;
            internal int endColumn;
            internal int endLine;
            internal int line;
            internal const int numberNotSpecified = 0;
            internal string origin;
            internal string subcategory;
            internal string text;

            internal Parts()
            {
            }

            internal enum Category
            {
                Warning,
                Error
            }
        }
    }
}

