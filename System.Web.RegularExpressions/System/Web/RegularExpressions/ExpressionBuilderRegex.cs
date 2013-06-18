namespace System.Web.RegularExpressions
{
    using System;
    using System.Collections;
    using System.Text.RegularExpressions;

    internal class ExpressionBuilderRegex : Regex
    {
        public ExpressionBuilderRegex()
        {
            base.pattern = @"\G\s*<%\s*\$\s*(?<code>.*)?%>\s*\z";
            base.roptions = RegexOptions.Singleline | RegexOptions.Multiline;
            base.factory = new ExpressionBuilderRegexFactory16();
            base.capnames = new Hashtable();
            base.capnames.Add("0", 0);
            base.capnames.Add("code", 1);
            base.capslist = new string[] { "0", "code" };
            base.capsize = 2;
            base.InitializeReferences();
        }
    }
}

