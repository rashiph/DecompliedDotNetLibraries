namespace System.Web.RegularExpressions
{
    using System.Text.RegularExpressions;

    internal class SimpleDirectiveRegexFactory14 : RegexRunnerFactory
    {
        public override RegexRunner CreateInstance()
        {
            return new SimpleDirectiveRegexRunner14();
        }
    }
}

