namespace System.Web.RegularExpressions
{
    using System.Text.RegularExpressions;

    internal class AspExprRegexFactory5 : RegexRunnerFactory
    {
        public override RegexRunner CreateInstance()
        {
            return new AspExprRegexRunner5();
        }
    }
}

