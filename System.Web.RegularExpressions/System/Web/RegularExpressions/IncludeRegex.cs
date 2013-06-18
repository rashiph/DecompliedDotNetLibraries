namespace System.Web.RegularExpressions
{
    using System;
    using System.Collections;
    using System.Text.RegularExpressions;

    public class IncludeRegex : Regex
    {
        public IncludeRegex()
        {
            base.pattern = "\\G<!--\\s*#(?i:include)\\s*(?<pathtype>[\\w]+)\\s*=\\s*[\"']?(?<filename>[^\\\"']*?)[\"']?\\s*-->";
            base.roptions = RegexOptions.Singleline | RegexOptions.Multiline;
            base.factory = new IncludeRegexFactory8();
            base.capnames = new Hashtable();
            base.capnames.Add("filename", 2);
            base.capnames.Add("pathtype", 1);
            base.capnames.Add("0", 0);
            base.capslist = new string[] { "0", "pathtype", "filename" };
            base.capsize = 3;
            base.InitializeReferences();
        }
    }
}

