namespace System.Web.RegularExpressions
{
    using System;
    using System.Text.RegularExpressions;

    internal class FormatStringRegex : Regex
    {
        public FormatStringRegex()
        {
            base.pattern = "^(([^\"]*(\"\")?)*)$";
            base.roptions = RegexOptions.Singleline | RegexOptions.Multiline;
            base.factory = new FormatStringRegexFactory19();
            base.capsize = 4;
            base.InitializeReferences();
        }
    }
}

