namespace System.Web.RegularExpressions
{
    using System;
    using System.Text.RegularExpressions;

    public class LTRegex : Regex
    {
        public LTRegex()
        {
            base.pattern = "<[^%]";
            base.roptions = RegexOptions.Singleline | RegexOptions.Multiline;
            base.factory = new LTRegexFactory11();
            base.capsize = 1;
            base.InitializeReferences();
        }
    }
}

