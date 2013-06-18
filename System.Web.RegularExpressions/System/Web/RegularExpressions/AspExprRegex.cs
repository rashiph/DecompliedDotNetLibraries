namespace System.Web.RegularExpressions
{
    using System;
    using System.Collections;
    using System.Text.RegularExpressions;

    public class AspExprRegex : Regex
    {
        public AspExprRegex()
        {
            base.pattern = @"\G<%\s*?=(?<code>.*?)?%>";
            base.roptions = RegexOptions.Singleline | RegexOptions.Multiline;
            base.factory = new AspExprRegexFactory5();
            base.capnames = new Hashtable();
            base.capnames.Add("0", 0);
            base.capnames.Add("code", 1);
            base.capslist = new string[] { "0", "code" };
            base.capsize = 2;
            base.InitializeReferences();
        }
    }
}

