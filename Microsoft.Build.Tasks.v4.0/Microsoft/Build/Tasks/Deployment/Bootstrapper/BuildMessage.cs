namespace Microsoft.Build.Tasks.Deployment.Bootstrapper
{
    using Microsoft.Build.Shared;
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;

    public class BuildMessage : IBuildMessage
    {
        private string helpCode;
        private int helpId;
        private string helpKeyword;
        private string message;
        private static readonly Regex msbuildMessageCodePattern = new Regex(@"(\d+)$");
        private BuildMessageSeverity severity;

        private BuildMessage(BuildMessageSeverity severity, string message, string helpKeyword, string helpCode)
        {
            this.severity = severity;
            this.message = message;
            this.helpKeyword = helpKeyword;
            this.helpCode = helpCode;
            if (!string.IsNullOrEmpty(this.helpCode))
            {
                Match match = msbuildMessageCodePattern.Match(this.helpCode);
                if (match.Success)
                {
                    this.helpId = int.Parse(match.Value, CultureInfo.InvariantCulture);
                }
            }
        }

        internal static BuildMessage CreateMessage(BuildMessageSeverity severity, string resourceName, params object[] args)
        {
            string str;
            string str2;
            return new BuildMessage(severity, Microsoft.Build.Shared.ResourceUtilities.FormatResourceString(out str, out str2, resourceName, args), str2, str);
        }

        internal string HelpCode
        {
            get
            {
                return this.helpCode;
            }
        }

        public int HelpId
        {
            get
            {
                return this.helpId;
            }
        }

        public string HelpKeyword
        {
            get
            {
                return this.helpKeyword;
            }
        }

        public string Message
        {
            get
            {
                return this.message;
            }
        }

        public BuildMessageSeverity Severity
        {
            get
            {
                return this.severity;
            }
        }
    }
}

