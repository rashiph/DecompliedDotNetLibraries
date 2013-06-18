namespace System.Web.Util
{
    using System;
    using System.Text.RegularExpressions;

    internal class Wildcard
    {
        internal bool _caseInsensitive;
        internal string _pattern;
        internal Regex _regex;
        protected static Regex backslashRegex = new Regex(@"(?=[\\:])");
        protected static Regex commaRegex = new Regex(",");
        protected static Regex metaRegex = new Regex(@"[\+\{\\\[\|\(\)\.\^\$]");
        protected static Regex questRegex = new Regex(@"\?");
        protected static Regex slashRegex = new Regex("(?=/)");
        protected static Regex starRegex = new Regex(@"\*");

        internal Wildcard(string pattern, bool caseInsensitive)
        {
            this._pattern = pattern;
            this._caseInsensitive = caseInsensitive;
        }

        protected void EnsureRegex()
        {
            if (this._regex == null)
            {
                this._regex = this.RegexFromWildcard(this._pattern, this._caseInsensitive);
            }
        }

        internal bool IsMatch(string input)
        {
            this.EnsureRegex();
            return this._regex.IsMatch(input);
        }

        protected virtual Regex RegexFromWildcard(string pattern, bool caseInsensitive)
        {
            RegexOptions none = RegexOptions.None;
            if ((pattern.Length > 0) && (pattern[0] == '*'))
            {
                none = RegexOptions.RightToLeft | RegexOptions.Singleline;
            }
            else
            {
                none = RegexOptions.Singleline;
            }
            if (caseInsensitive)
            {
                none |= RegexOptions.CultureInvariant | RegexOptions.IgnoreCase;
            }
            pattern = metaRegex.Replace(pattern, @"\$0");
            pattern = questRegex.Replace(pattern, ".");
            pattern = starRegex.Replace(pattern, ".*");
            pattern = commaRegex.Replace(pattern, @"\z|\A");
            return new Regex(@"\A" + pattern + @"\z", none);
        }
    }
}

