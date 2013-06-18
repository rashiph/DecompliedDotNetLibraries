namespace System.Web.RegularExpressions
{
    using System.Text.RegularExpressions;

    internal class TagRegexFactory1 : RegexRunnerFactory
    {
        public override RegexRunner CreateInstance()
        {
            return new TagRegexRunner1();
        }
    }
}

