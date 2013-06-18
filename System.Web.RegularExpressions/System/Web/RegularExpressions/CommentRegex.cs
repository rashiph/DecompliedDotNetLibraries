namespace System.Web.RegularExpressions
{
    using System;
    using System.Text.RegularExpressions;

    public class CommentRegex : Regex
    {
        public CommentRegex()
        {
            base.pattern = @"\G<%--(([^-]*)-)*?-%>";
            base.roptions = RegexOptions.Singleline | RegexOptions.Multiline;
            base.factory = new CommentRegexFactory7();
            base.capsize = 3;
            base.InitializeReferences();
        }
    }
}

