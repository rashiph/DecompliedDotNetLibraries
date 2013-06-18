namespace System.Web.RegularExpressions
{
    using System.Text.RegularExpressions;

    internal class NonWordRegexFactory21 : RegexRunnerFactory
    {
        public override RegexRunner CreateInstance()
        {
            return new NonWordRegexRunner21();
        }
    }
}

