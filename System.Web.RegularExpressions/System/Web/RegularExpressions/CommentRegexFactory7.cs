namespace System.Web.RegularExpressions
{
    using System.Text.RegularExpressions;

    internal class CommentRegexFactory7 : RegexRunnerFactory
    {
        public override RegexRunner CreateInstance()
        {
            return new CommentRegexRunner7();
        }
    }
}

