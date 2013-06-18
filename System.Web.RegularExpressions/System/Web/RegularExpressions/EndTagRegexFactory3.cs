namespace System.Web.RegularExpressions
{
    using System.Text.RegularExpressions;

    internal class EndTagRegexFactory3 : RegexRunnerFactory
    {
        public override RegexRunner CreateInstance()
        {
            return new EndTagRegexRunner3();
        }
    }
}

