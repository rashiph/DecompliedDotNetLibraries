namespace System.Web.RegularExpressions
{
    using System;
    using System.Collections;
    using System.Text.RegularExpressions;

    internal class EvalExpressionRegex : Regex
    {
        public EvalExpressionRegex()
        {
            base.pattern = @"^\s*eval\s*\((?<params>.*)\)\s*\z";
            base.roptions = RegexOptions.CultureInvariant | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.IgnoreCase;
            base.factory = new EvalExpressionRegexFactory22();
            base.capnames = new Hashtable();
            base.capnames.Add("params", 1);
            base.capnames.Add("0", 0);
            base.capslist = new string[] { "0", "params" };
            base.capsize = 2;
            base.InitializeReferences();
        }
    }
}

