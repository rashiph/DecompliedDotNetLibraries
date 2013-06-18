namespace Microsoft.Build.Shared
{
    using Microsoft.Build.Framework;
    using System;
    using System.Globalization;
    using System.Runtime;
    using System.Text;

    internal static class EventArgsFormatting
    {
        private static readonly string[] newLines = new string[] { "\r\n", "\n" };

        internal static string EscapeCarriageReturn(string stringWithCarriageReturn)
        {
            if (!string.IsNullOrEmpty(stringWithCarriageReturn))
            {
                return stringWithCarriageReturn.Replace("\r", @"\r");
            }
            return stringWithCarriageReturn;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal static string FormatEventMessage(BuildErrorEventArgs e)
        {
            return FormatEventMessage(e, false);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal static string FormatEventMessage(BuildWarningEventArgs e)
        {
            return FormatEventMessage(e, false);
        }

        internal static string FormatEventMessage(BuildErrorEventArgs e, bool removeCarriageReturn)
        {
            ErrorUtilities.VerifyThrowArgumentNull(e, "e");
            return FormatEventMessage("error", e.Subcategory, removeCarriageReturn ? EscapeCarriageReturn(e.Message) : e.Message, e.Code, e.File, null, e.LineNumber, e.EndLineNumber, e.ColumnNumber, e.EndColumnNumber, e.ThreadId);
        }

        internal static string FormatEventMessage(BuildWarningEventArgs e, bool removeCarriageReturn)
        {
            ErrorUtilities.VerifyThrowArgumentNull(e, "e");
            return FormatEventMessage("warning", e.Subcategory, removeCarriageReturn ? EscapeCarriageReturn(e.Message) : e.Message, e.Code, e.File, null, e.LineNumber, e.EndLineNumber, e.ColumnNumber, e.EndColumnNumber, e.ThreadId);
        }

        internal static string FormatEventMessage(BuildErrorEventArgs e, bool removeCarriageReturn, bool showProjectFile)
        {
            ErrorUtilities.VerifyThrowArgumentNull(e, "e");
            return FormatEventMessage("error", e.Subcategory, removeCarriageReturn ? EscapeCarriageReturn(e.Message) : e.Message, e.Code, e.File, showProjectFile ? e.ProjectFile : null, e.LineNumber, e.EndLineNumber, e.ColumnNumber, e.EndColumnNumber, e.ThreadId);
        }

        internal static string FormatEventMessage(BuildWarningEventArgs e, bool removeCarriageReturn, bool showProjectFile)
        {
            ErrorUtilities.VerifyThrowArgumentNull(e, "e");
            return FormatEventMessage("warning", e.Subcategory, removeCarriageReturn ? EscapeCarriageReturn(e.Message) : e.Message, e.Code, e.File, showProjectFile ? e.ProjectFile : null, e.LineNumber, e.EndLineNumber, e.ColumnNumber, e.EndColumnNumber, e.ThreadId);
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal static string FormatEventMessage(string category, string subcategory, string message, string code, string file, int lineNumber, int endLineNumber, int columnNumber, int endColumnNumber, int threadId)
        {
            return FormatEventMessage(category, subcategory, message, code, file, null, lineNumber, endLineNumber, columnNumber, endColumnNumber, threadId);
        }

        internal static string FormatEventMessage(string category, string subcategory, string message, string code, string file, string projectFile, int lineNumber, int endLineNumber, int columnNumber, int endColumnNumber, int threadId)
        {
            StringBuilder builder = new StringBuilder();
            if ((file == null) || (file.Length == 0))
            {
                builder.Append("MSBUILD : ");
            }
            else
            {
                builder.Append("{1}");
                if (lineNumber == 0)
                {
                    builder.Append(" : ");
                }
                else if (columnNumber == 0)
                {
                    if (endLineNumber == 0)
                    {
                        builder.Append("({2}): ");
                    }
                    else
                    {
                        builder.Append("({2}-{7}): ");
                    }
                }
                else if (endLineNumber == 0)
                {
                    if (endColumnNumber == 0)
                    {
                        builder.Append("({2},{3}): ");
                    }
                    else
                    {
                        builder.Append("({2},{3}-{8}): ");
                    }
                }
                else if (endColumnNumber == 0)
                {
                    builder.Append("({2}-{7},{3}): ");
                }
                else
                {
                    builder.Append("({2},{3},{7},{8}): ");
                }
            }
            if ((subcategory != null) && (subcategory.Length != 0))
            {
                builder.Append("{9} ");
            }
            builder.Append("{4} ");
            if (code == null)
            {
                builder.Append(": ");
            }
            else
            {
                builder.Append("{5}: ");
            }
            if (message != null)
            {
                builder.Append("{6}");
            }
            if ((projectFile != null) && !string.Equals(projectFile, file))
            {
                builder.Append(" [{10}]");
            }
            if (message == null)
            {
                message = string.Empty;
            }
            string format = builder.ToString();
            string[] strArray = SplitStringOnNewLines(message);
            StringBuilder builder2 = new StringBuilder();
            for (int i = 0; i < strArray.Length; i++)
            {
                builder2.Append(string.Format(CultureInfo.CurrentCulture, format, new object[] { threadId, file, lineNumber, columnNumber, category, code, strArray[i], endLineNumber, endColumnNumber, subcategory, projectFile }));
                if (i < (strArray.Length - 1))
                {
                    builder2.AppendLine();
                }
            }
            return builder2.ToString();
        }

        private static string[] SplitStringOnNewLines(string s)
        {
            return s.Split(newLines, StringSplitOptions.None);
        }
    }
}

