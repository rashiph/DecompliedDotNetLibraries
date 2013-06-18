namespace System.Web.RegularExpressions
{
    using System;
    using System.Text.RegularExpressions;

    public class TextRegex : Regex
    {
        public TextRegex()
        {
            base.pattern = @"\G[^<]+";
            base.roptions = RegexOptions.Singleline | RegexOptions.Multiline;
            base.factory = new TextRegexFactory9();
            base.capsize = 1;
            base.InitializeReferences();
        }
    }
}

