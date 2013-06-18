namespace Microsoft.JScript
{
    using System;
    using System.Text.RegularExpressions;

    internal class ReplaceWithString : RegExpReplace
    {
        private string replaceString;

        internal ReplaceWithString(string replaceString)
        {
            this.replaceString = replaceString;
        }

        internal override string Evaluate(Match match)
        {
            base.lastMatch = match;
            return match.Result(this.replaceString);
        }
    }
}

