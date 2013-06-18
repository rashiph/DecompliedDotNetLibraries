namespace System.Web.Util
{
    using System;
    using System.Text.RegularExpressions;

    internal abstract class WildcardPath : Wildcard
    {
        private Regex _suffix;

        internal WildcardPath(string pattern, bool caseInsensitive) : base(pattern, caseInsensitive)
        {
        }

        protected abstract Regex[][] DirsFromWildcard(string pattern);
        protected void EnsureSuffix()
        {
            if (this._suffix == null)
            {
                this._suffix = this.SuffixFromWildcard(base._pattern, base._caseInsensitive);
            }
        }

        internal bool IsSuffix(string input)
        {
            this.EnsureSuffix();
            return this._suffix.IsMatch(input);
        }

        protected abstract string[] SplitDirs(string input);
        protected abstract Regex SuffixFromWildcard(string pattern, bool caseInsensitive);
    }
}

