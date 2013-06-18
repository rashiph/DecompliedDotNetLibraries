namespace System.Web.RegularExpressions
{
    using System;
    using System.Collections;
    using System.Text.RegularExpressions;

    internal class WebResourceRegex : Regex
    {
        public WebResourceRegex()
        {
            base.pattern = "<%\\s*=\\s*WebResource\\(\"(?<resourceName>[^\"]*)\"\\)\\s*%>";
            base.roptions = RegexOptions.Singleline | RegexOptions.Multiline;
            base.factory = new WebResourceRegexFactory20();
            base.capnames = new Hashtable();
            base.capnames.Add("0", 0);
            base.capnames.Add("resourceName", 1);
            base.capslist = new string[] { "0", "resourceName" };
            base.capsize = 2;
            base.InitializeReferences();
        }
    }
}

