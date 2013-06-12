namespace System.Net
{
    using System;
    using System.Text.RegularExpressions;

    [Serializable]
    internal class DelayedRegex
    {
        private Regex _AsRegex;
        private string _AsString;

        internal DelayedRegex(string regexString)
        {
            if (regexString == null)
            {
                throw new ArgumentNullException("regexString");
            }
            this._AsString = regexString;
        }

        internal DelayedRegex(Regex regex)
        {
            if (regex == null)
            {
                throw new ArgumentNullException("regex");
            }
            this._AsRegex = regex;
        }

        public override string ToString()
        {
            if (this._AsString == null)
            {
                return (this._AsString = this._AsRegex.ToString());
            }
            return this._AsString;
        }

        internal Regex AsRegex
        {
            get
            {
                if (this._AsRegex == null)
                {
                    this._AsRegex = new Regex(this._AsString + "[/]?", RegexOptions.CultureInvariant | RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
                }
                return this._AsRegex;
            }
        }
    }
}

