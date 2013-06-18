namespace System.Web.RegularExpressions
{
    using System;
    using System.Text.RegularExpressions;

    public class RunatServerRegex : Regex
    {
        public RunatServerRegex()
        {
            base.pattern = @"runat\W*server";
            base.roptions = RegexOptions.CultureInvariant | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnoreCase;
            base.factory = new RunatServerRegexFactory13();
            base.capsize = 1;
            base.InitializeReferences();
        }
    }
}

