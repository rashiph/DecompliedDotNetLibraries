namespace System.Web.RegularExpressions
{
    using System;
    using System.Collections;
    using System.Text.RegularExpressions;

    public class DirectiveRegex : Regex
    {
        public DirectiveRegex()
        {
            base.pattern = "\\G<%\\s*@(\\s*(?<attrname>\\w[\\w:]*(?=\\W))(\\s*(?<equal>=)\\s*\"(?<attrval>[^\"]*)\"|\\s*(?<equal>=)\\s*'(?<attrval>[^']*)'|\\s*(?<equal>=)\\s*(?<attrval>[^\\s\"'%>]*)|(?<equal>)(?<attrval>\\s*?)))*\\s*?%>";
            base.roptions = RegexOptions.Singleline | RegexOptions.Multiline;
            base.factory = new DirectiveRegexFactory2();
            base.capnames = new Hashtable();
            base.capnames.Add("attrval", 5);
            base.capnames.Add("0", 0);
            base.capnames.Add("attrname", 3);
            base.capnames.Add("2", 2);
            base.capnames.Add("1", 1);
            base.capnames.Add("equal", 4);
            base.capslist = new string[] { "0", "1", "2", "attrname", "equal", "attrval" };
            base.capsize = 6;
            base.InitializeReferences();
        }
    }
}

