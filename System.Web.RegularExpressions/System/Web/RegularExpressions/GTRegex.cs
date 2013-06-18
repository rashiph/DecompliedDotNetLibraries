namespace System.Web.RegularExpressions
{
    using System;
    using System.Text.RegularExpressions;

    public class GTRegex : Regex
    {
        public GTRegex()
        {
            base.pattern = "[^%]>";
            base.roptions = RegexOptions.Singleline | RegexOptions.Multiline;
            base.factory = new GTRegexFactory10();
            base.capsize = 1;
            base.InitializeReferences();
        }
    }
}

