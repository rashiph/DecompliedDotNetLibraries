namespace System.Web.RegularExpressions
{
    using System.Text.RegularExpressions;

    internal class DatabindExprRegexFactory6 : RegexRunnerFactory
    {
        public override RegexRunner CreateInstance()
        {
            return new DatabindExprRegexRunner6();
        }
    }
}

