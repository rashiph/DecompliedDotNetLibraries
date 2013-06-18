namespace System.Web.RegularExpressions
{
    using System;
    using System.Collections;
    using System.Text.RegularExpressions;

    internal class BindExpressionRegex : Regex
    {
        public BindExpressionRegex()
        {
            base.pattern = @"^\s*bind\s*\((?<params>.*)\)\s*\z";
            base.roptions = RegexOptions.CultureInvariant | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnoreCase;
            base.factory = new BindExpressionRegexFactory17();
            base.capnames = new Hashtable();
            base.capnames.Add("params", 1);
            base.capnames.Add("0", 0);
            base.capslist = new string[] { "0", "params" };
            base.capsize = 2;
            base.InitializeReferences();
        }
    }
}

