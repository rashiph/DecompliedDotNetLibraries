namespace System.Web.RegularExpressions
{
    using System;
    using System.Collections;
    using System.Text.RegularExpressions;

    public class EndTagRegex : Regex
    {
        public EndTagRegex()
        {
            base.pattern = @"\G</(?<tagname>[\w:\.]+)\s*>";
            base.roptions = RegexOptions.Singleline | RegexOptions.Multiline;
            base.factory = new EndTagRegexFactory3();
            base.capnames = new Hashtable();
            base.capnames.Add("tagname", 1);
            base.capnames.Add("0", 0);
            base.capslist = new string[] { "0", "tagname" };
            base.capsize = 2;
            base.InitializeReferences();
        }
    }
}

