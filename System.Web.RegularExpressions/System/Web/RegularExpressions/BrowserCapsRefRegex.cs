namespace System.Web.RegularExpressions
{
    using System;
    using System.Collections;
    using System.Text.RegularExpressions;

    internal class BrowserCapsRefRegex : Regex
    {
        public BrowserCapsRefRegex()
        {
            base.pattern = @"\$(?:\{(?<name>\w+)\})";
            base.roptions = RegexOptions.Singleline | RegexOptions.Multiline;
            base.factory = new BrowserCapsRefRegexFactory23();
            base.capnames = new Hashtable();
            base.capnames.Add("name", 1);
            base.capnames.Add("0", 0);
            base.capslist = new string[] { "0", "name" };
            base.capsize = 2;
            base.InitializeReferences();
        }
    }
}

