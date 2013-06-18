namespace System.Web.RegularExpressions
{
    using System.Text.RegularExpressions;

    internal class DataBindRegexFactory15 : RegexRunnerFactory
    {
        public override RegexRunner CreateInstance()
        {
            return new DataBindRegexRunner15();
        }
    }
}

