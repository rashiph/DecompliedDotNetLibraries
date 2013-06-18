namespace System.Web.RegularExpressions
{
    using System.Text.RegularExpressions;

    internal class RunatServerRegexFactory13 : RegexRunnerFactory
    {
        public override RegexRunner CreateInstance()
        {
            return new RunatServerRegexRunner13();
        }
    }
}

