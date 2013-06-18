namespace System.Web.RegularExpressions
{
    using System.Text.RegularExpressions;

    internal class AspEncodedExprRegexFactory24 : RegexRunnerFactory
    {
        public override RegexRunner CreateInstance()
        {
            return new AspEncodedExprRegexRunner24();
        }
    }
}

