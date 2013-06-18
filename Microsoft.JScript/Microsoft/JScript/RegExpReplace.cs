namespace Microsoft.JScript
{
    using System;
    using System.Text.RegularExpressions;

    internal abstract class RegExpReplace
    {
        internal Match lastMatch = null;

        internal RegExpReplace()
        {
        }

        internal abstract string Evaluate(Match match);
    }
}

