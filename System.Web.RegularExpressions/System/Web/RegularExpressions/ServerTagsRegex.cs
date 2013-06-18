namespace System.Web.RegularExpressions
{
    using System;
    using System.Text.RegularExpressions;

    public class ServerTagsRegex : Regex
    {
        public ServerTagsRegex()
        {
            base.pattern = "<%(?![#$])(([^%]*)%)*?>";
            base.roptions = RegexOptions.Singleline | RegexOptions.Multiline;
            base.factory = new ServerTagsRegexFactory12();
            base.capsize = 3;
            base.InitializeReferences();
        }
    }
}

