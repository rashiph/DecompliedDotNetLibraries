namespace System.Web.RegularExpressions
{
    using System.Text.RegularExpressions;

    internal class BrowserCapsRefRegexFactory23 : RegexRunnerFactory
    {
        public override RegexRunner CreateInstance()
        {
            return new BrowserCapsRefRegexRunner23();
        }
    }
}

