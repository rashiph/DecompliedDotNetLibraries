namespace System.Web.RegularExpressions
{
    using System;
    using System.Collections;
    using System.Text.RegularExpressions;

    public class TagRegex : Regex
    {
        public TagRegex()
        {
            base.pattern = "\\G<(?<tagname>[\\w:\\.]+)(\\s+(?<attrname>\\w[-\\w:]*)(\\s*=\\s*\"(?<attrval>[^\"]*)\"|\\s*=\\s*'(?<attrval>[^']*)'|\\s*=\\s*(?<attrval><%#.*?%>)|\\s*=\\s*(?<attrval>[^\\s=\"'/>]*)|(?<attrval>\\s*?)))*\\s*(?<empty>/)?>";
            base.roptions = RegexOptions.Singleline | RegexOptions.Multiline;
            base.factory = new TagRegexFactory1();
            base.capnames = new Hashtable();
            base.capnames.Add("0", 0);
            base.capnames.Add("attrname", 4);
            base.capnames.Add("empty", 6);
            base.capnames.Add("tagname", 3);
            base.capnames.Add("attrval", 5);
            base.capnames.Add("2", 2);
            base.capnames.Add("1", 1);
            base.capslist = new string[] { "0", "1", "2", "tagname", "attrname", "attrval", "empty" };
            base.capsize = 7;
            base.InitializeReferences();
        }
    }
}

