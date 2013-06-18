namespace System.Web.RegularExpressions
{
    using System;
    using System.Text.RegularExpressions;

    internal class NonWordRegex : Regex
    {
        public NonWordRegex()
        {
            base.pattern = @"\W";
            base.roptions = RegexOptions.Singleline | RegexOptions.Multiline;
            base.factory = new NonWordRegexFactory21();
            base.capsize = 1;
            base.InitializeReferences();
        }
    }
}

