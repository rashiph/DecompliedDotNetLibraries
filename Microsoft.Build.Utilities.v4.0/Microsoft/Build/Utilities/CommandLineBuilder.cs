namespace Microsoft.Build.Utilities
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using System;
    using System.Runtime;
    using System.Text;
    using System.Text.RegularExpressions;

    public class CommandLineBuilder
    {
        private Regex allowedUnquoted;
        private static string allowedUnquotedRegexNoHyphen = @"^[a-z\\/:0-9\._+=]*$";
        private static string allowedUnquotedRegexWithHyphen = @"^[a-z\\/:0-9\._\-+=]*$";
        private StringBuilder commandLine;
        private Regex definitelyNeedQuotes;
        private static string definitelyNeedQuotesRegexNoHyphen = "[|><\\s,;\"]+";
        private static string definitelyNeedQuotesRegexWithHyphen = "[|><\\s,;\\-\"]+";
        private bool quoteHyphens;

        public CommandLineBuilder()
        {
            this.commandLine = new StringBuilder();
        }

        public CommandLineBuilder(bool quoteHyphensOnCommandLine)
        {
            this.commandLine = new StringBuilder();
            this.quoteHyphens = quoteHyphensOnCommandLine;
        }

        public void AppendFileNameIfNotNull(ITaskItem fileItem)
        {
            if (fileItem != null)
            {
                this.VerifyThrowNoEmbeddedDoubleQuotes(string.Empty, fileItem.ItemSpec);
                this.AppendFileNameIfNotNull(fileItem.ItemSpec);
            }
        }

        public void AppendFileNameIfNotNull(string fileName)
        {
            if (fileName != null)
            {
                this.VerifyThrowNoEmbeddedDoubleQuotes(string.Empty, fileName);
                this.AppendSpaceIfNotEmpty();
                this.AppendFileNameWithQuoting(fileName);
            }
        }

        public void AppendFileNamesIfNotNull(ITaskItem[] fileItems, string delimiter)
        {
            ErrorUtilities.VerifyThrowArgumentNull(delimiter, "delimiter");
            if ((fileItems != null) && (fileItems.Length > 0))
            {
                for (int i = 0; i < fileItems.Length; i++)
                {
                    if (fileItems[i] != null)
                    {
                        this.VerifyThrowNoEmbeddedDoubleQuotes(string.Empty, fileItems[i].ItemSpec);
                    }
                }
                this.AppendSpaceIfNotEmpty();
                for (int j = 0; j < fileItems.Length; j++)
                {
                    if (j != 0)
                    {
                        this.AppendTextUnquoted(delimiter);
                    }
                    if (fileItems[j] != null)
                    {
                        this.AppendFileNameWithQuoting(fileItems[j].ItemSpec);
                    }
                }
            }
        }

        public void AppendFileNamesIfNotNull(string[] fileNames, string delimiter)
        {
            ErrorUtilities.VerifyThrowArgumentNull(delimiter, "delimiter");
            if ((fileNames != null) && (fileNames.Length > 0))
            {
                for (int i = 0; i < fileNames.Length; i++)
                {
                    this.VerifyThrowNoEmbeddedDoubleQuotes(string.Empty, fileNames[i]);
                }
                this.AppendSpaceIfNotEmpty();
                for (int j = 0; j < fileNames.Length; j++)
                {
                    if (j != 0)
                    {
                        this.AppendTextUnquoted(delimiter);
                    }
                    this.AppendFileNameWithQuoting(fileNames[j]);
                }
            }
        }

        protected void AppendFileNameWithQuoting(string fileName)
        {
            if (fileName != null)
            {
                this.VerifyThrowNoEmbeddedDoubleQuotes(string.Empty, fileName);
                if ((fileName.Length != 0) && (fileName[0] == '-'))
                {
                    this.AppendTextWithQuoting(@".\" + fileName);
                }
                else
                {
                    this.AppendTextWithQuoting(fileName);
                }
            }
        }

        protected void AppendQuotedTextToBuffer(StringBuilder buffer, string unquotedTextToAppend)
        {
            ErrorUtilities.VerifyThrowArgumentNull(buffer, "buffer");
            if (unquotedTextToAppend != null)
            {
                bool flag = this.IsQuotingRequired(unquotedTextToAppend);
                if (flag)
                {
                    buffer.Append('"');
                }
                int num = 0;
                for (int i = 0; i < unquotedTextToAppend.Length; i++)
                {
                    if ('"' == unquotedTextToAppend[i])
                    {
                        num++;
                    }
                }
                if (num > 0)
                {
                    ErrorUtilities.VerifyThrowArgument((num % 2) == 0, "General.StringsCannotContainOddNumberOfDoubleQuotes", unquotedTextToAppend);
                    unquotedTextToAppend = unquotedTextToAppend.Replace("\\\"", "\\\\\"");
                    unquotedTextToAppend = unquotedTextToAppend.Replace("\"", "\\\"");
                }
                buffer.Append(unquotedTextToAppend);
                if (flag && unquotedTextToAppend.EndsWith(@"\", StringComparison.Ordinal))
                {
                    buffer.Append('\\');
                }
                if (flag)
                {
                    buffer.Append('"');
                }
            }
        }

        protected void AppendSpaceIfNotEmpty()
        {
            if ((this.CommandLine.Length != 0) && (this.CommandLine[this.CommandLine.Length - 1] != ' '))
            {
                this.CommandLine.Append(" ");
            }
        }

        public void AppendSwitch(string switchName)
        {
            ErrorUtilities.VerifyThrowArgumentNull(switchName, "switchName");
            this.AppendSpaceIfNotEmpty();
            this.AppendTextUnquoted(switchName);
        }

        public void AppendSwitchIfNotNull(string switchName, ITaskItem parameter)
        {
            ErrorUtilities.VerifyThrowArgumentNull(switchName, "switchName");
            if (parameter != null)
            {
                this.AppendSwitchIfNotNull(switchName, parameter.ItemSpec);
            }
        }

        public void AppendSwitchIfNotNull(string switchName, string parameter)
        {
            ErrorUtilities.VerifyThrowArgumentNull(switchName, "switchName");
            if (parameter != null)
            {
                this.AppendSwitch(switchName);
                this.AppendTextWithQuoting(parameter);
            }
        }

        public void AppendSwitchIfNotNull(string switchName, ITaskItem[] parameters, string delimiter)
        {
            ErrorUtilities.VerifyThrowArgumentNull(switchName, "switchName");
            ErrorUtilities.VerifyThrowArgumentNull(delimiter, "delimiter");
            if ((parameters != null) && (parameters.Length > 0))
            {
                this.AppendSwitch(switchName);
                bool flag = true;
                foreach (ITaskItem item in parameters)
                {
                    if (!flag)
                    {
                        this.AppendTextUnquoted(delimiter);
                    }
                    flag = false;
                    if (item != null)
                    {
                        this.AppendTextWithQuoting(item.ItemSpec);
                    }
                }
            }
        }

        public void AppendSwitchIfNotNull(string switchName, string[] parameters, string delimiter)
        {
            ErrorUtilities.VerifyThrowArgumentNull(switchName, "switchName");
            ErrorUtilities.VerifyThrowArgumentNull(delimiter, "delimiter");
            if ((parameters != null) && (parameters.Length > 0))
            {
                this.AppendSwitch(switchName);
                bool flag = true;
                foreach (string str in parameters)
                {
                    if (!flag)
                    {
                        this.AppendTextUnquoted(delimiter);
                    }
                    flag = false;
                    this.AppendTextWithQuoting(str);
                }
            }
        }

        public void AppendSwitchUnquotedIfNotNull(string switchName, ITaskItem parameter)
        {
            ErrorUtilities.VerifyThrowArgumentNull(switchName, "switchName");
            if (parameter != null)
            {
                this.AppendSwitchUnquotedIfNotNull(switchName, parameter.ItemSpec);
            }
        }

        public void AppendSwitchUnquotedIfNotNull(string switchName, string parameter)
        {
            ErrorUtilities.VerifyThrowArgumentNull(switchName, "switchName");
            if (parameter != null)
            {
                this.AppendSwitch(switchName);
                this.AppendTextUnquoted(parameter);
            }
        }

        public void AppendSwitchUnquotedIfNotNull(string switchName, ITaskItem[] parameters, string delimiter)
        {
            ErrorUtilities.VerifyThrowArgumentNull(switchName, "switchName");
            ErrorUtilities.VerifyThrowArgumentNull(delimiter, "delimiter");
            if ((parameters != null) && (parameters.Length > 0))
            {
                this.AppendSwitch(switchName);
                bool flag = true;
                foreach (ITaskItem item in parameters)
                {
                    if (!flag)
                    {
                        this.AppendTextUnquoted(delimiter);
                    }
                    flag = false;
                    if (item != null)
                    {
                        this.AppendTextUnquoted(item.ItemSpec);
                    }
                }
            }
        }

        public void AppendSwitchUnquotedIfNotNull(string switchName, string[] parameters, string delimiter)
        {
            ErrorUtilities.VerifyThrowArgumentNull(switchName, "switchName");
            ErrorUtilities.VerifyThrowArgumentNull(delimiter, "delimiter");
            if ((parameters != null) && (parameters.Length > 0))
            {
                this.AppendSwitch(switchName);
                bool flag = true;
                foreach (string str in parameters)
                {
                    if (!flag)
                    {
                        this.AppendTextUnquoted(delimiter);
                    }
                    flag = false;
                    this.AppendTextUnquoted(str);
                }
            }
        }

        public void AppendTextUnquoted(string textToAppend)
        {
            if (textToAppend != null)
            {
                this.CommandLine.Append(textToAppend);
            }
        }

        protected void AppendTextWithQuoting(string textToAppend)
        {
            this.AppendQuotedTextToBuffer(this.CommandLine, textToAppend);
        }

        protected virtual bool IsQuotingRequired(string parameter)
        {
            bool flag = false;
            if (parameter != null)
            {
                bool flag2 = this.AllowedUnquoted.IsMatch(parameter);
                bool flag3 = this.DefinitelyNeedQuotes.IsMatch(parameter);
                flag = !flag2 || flag3;
            }
            return flag;
        }

        public override string ToString()
        {
            return this.CommandLine.ToString();
        }

        protected virtual void VerifyThrowNoEmbeddedDoubleQuotes(string switchName, string parameter)
        {
            if (parameter != null)
            {
                if (string.IsNullOrEmpty(switchName))
                {
                    ErrorUtilities.VerifyThrowArgument(-1 == parameter.IndexOf('"'), "General.QuotesNotAllowedInThisKindOfTaskParameterNoSwitchName", parameter);
                }
                else
                {
                    ErrorUtilities.VerifyThrowArgument(-1 == parameter.IndexOf('"'), "General.QuotesNotAllowedInThisKindOfTaskParameter", switchName, parameter);
                }
            }
        }

        private Regex AllowedUnquoted
        {
            get
            {
                if (this.allowedUnquoted == null)
                {
                    if (this.quoteHyphens)
                    {
                        this.allowedUnquoted = new Regex(allowedUnquotedRegexNoHyphen, RegexOptions.IgnoreCase);
                    }
                    else
                    {
                        this.allowedUnquoted = new Regex(allowedUnquotedRegexWithHyphen, RegexOptions.IgnoreCase);
                    }
                }
                return this.allowedUnquoted;
            }
        }

        protected StringBuilder CommandLine
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.commandLine;
            }
        }

        private Regex DefinitelyNeedQuotes
        {
            get
            {
                if (this.definitelyNeedQuotes == null)
                {
                    if (this.quoteHyphens)
                    {
                        this.definitelyNeedQuotes = new Regex(definitelyNeedQuotesRegexWithHyphen, RegexOptions.None);
                    }
                    else
                    {
                        this.definitelyNeedQuotes = new Regex(definitelyNeedQuotesRegexNoHyphen, RegexOptions.None);
                    }
                }
                return this.definitelyNeedQuotes;
            }
        }

        public int Length
        {
            get
            {
                return this.CommandLine.Length;
            }
        }
    }
}

