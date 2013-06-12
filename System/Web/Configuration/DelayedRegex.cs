namespace System.Web.Configuration
{
    using System;
    using System.Text.RegularExpressions;

    internal class DelayedRegex
    {
        private Regex _regex = null;
        private string _regstring;

        internal DelayedRegex(string s)
        {
            this._regstring = s;
        }

        internal void EnsureRegex()
        {
            string pattern = this._regstring;
            if (this._regex == null)
            {
                this._regex = new Regex(pattern);
                this._regstring = null;
            }
        }

        internal int GroupNumberFromName(string name)
        {
            this.EnsureRegex();
            return this._regex.GroupNumberFromName(name);
        }

        internal System.Text.RegularExpressions.Match Match(string s)
        {
            this.EnsureRegex();
            return this._regex.Match(s);
        }
    }
}

